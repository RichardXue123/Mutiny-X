using System.Collections;
using System.Collections.Generic;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    public struct AIMove
    {
        public MutinyCharacter Character;
        public string WeaponType;
        public Vector2 LaunchVelocity;
        public float Score;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MutinyTeam))]
    public sealed class MutinyAIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public float ThinkDelay = 0.8f;
        public int TrajectorySamples = 30;

        private MutinyTeam m_Team;
        private MutinyTurnManager m_TurnManager;
        private Coroutine m_TurnCoroutine;

        private void Awake()
        {
            m_Team = GetComponent<MutinyTeam>();
        }

        private void Start()
        {
            m_TurnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (m_TurnManager != null)
            {
                m_TurnManager.OnTurnStarted += HandleTurnStarted;
            }
        }

        private void OnDestroy()
        {
            if (m_TurnManager != null)
            {
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            }
        }

        private void HandleTurnStarted(MutinyTeam activeTeam)
        {
            if (activeTeam == m_Team && m_Team.IsAiControlled && !m_Team.IsDefeated)
            {
                if (m_TurnCoroutine != null)
                {
                    StopCoroutine(m_TurnCoroutine);
                }
                m_TurnCoroutine = StartCoroutine(ExecuteAITurnRoutine());
            }
        }

        private IEnumerator ExecuteAITurnRoutine()
        {
            yield return new WaitForSeconds(ThinkDelay);

            if (m_TurnManager == null || m_TurnManager.CurrentPhase != TurnPhase.TurnActive)
                yield break;

            AIMove bestMove = EvaluateBestMove();
            if (bestMove.Character != null)
            {
                m_Team.SelectCharacter(bestMove.Character);

                yield return new WaitForSeconds(0.4f);

                ExecuteMove(bestMove);
            }
            else
            {
                // Fallback: pass turn if no legal moves
                m_TurnManager.PassTurn();
            }
        }

        public AIMove EvaluateBestMove()
        {
            var bestMove = new AIMove { Score = float.NegativeInfinity };

            // Find enemy characters (Team 1)
            var allChars = FindObjectsByType<MutinyCharacter>();
            var enemies = new List<MutinyCharacter>();
            var allies = new List<MutinyCharacter>();

            for (int i = 0; i < allChars.Length; i++)
            {
                var ch = allChars[i];
                if (ch != null && ch.IsAlive)
                {
                    if (ch.TeamIndex != m_Team.TeamNumber)
                        enemies.Add(ch);
                    else
                        allies.Add(ch);
                }
            }

            if (enemies.Count == 0 || allies.Count == 0)
                return bestMove;

            // Get terrain grid for deterministic trajectory simulation
            string[,] terrainGrid = null;
            int gridW = 50;
            int gridH = 20;

            var controller = FindAnyObjectByType<MutinyLevelController>();
            if (controller != null && controller.LevelXml != null)
            {
                var lvlData = MutinyLevelXmlParser.Parse(controller.LevelXml.text);
                terrainGrid = lvlData.Terrain;
                gridW = lvlData.Width;
                gridH = lvlData.Height;
            }

            float waterPixelY = float.PositiveInfinity;
            var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
            if (levelRoot != null)
            {
                waterPixelY = -levelRoot.WaterLevelY * MutinyPhysics.PixelsPerUnit;
            }

            // Evaluate all alive allies
            for (int a = 0; a < allies.Count; a++)
            {
                var ally = allies[a];
                if (!ally.CanShoot && !ally.CanThrow)
                    continue;

                Vector2 charPosPx = new Vector2(ally.PhysicsBody.State.X, ally.PhysicsBody.State.Y);

                // Try weapons: Cherry Bomb, Dynamite, or self throw
                string weaponToUse = ally.HasWeapon("dynamite") ? "dynamite" : "cherryBomb";

                for (int s = 0; s < TrajectorySamples; s++)
                {
                    // Sample angles arcing upwards
                    float angleDeg = UnityEngine.Random.Range(195f, 345f);
                    float angleRad = angleDeg * Mathf.Deg2Rad;
                    float force = UnityEngine.Random.Range(6f, 20f);

                    Vector2 testVel = new Vector2(Mathf.Cos(angleRad) * force, Mathf.Sin(angleRad) * force);

                    var simBody = PhysicsBodyState.CreateDefault(charPosPx.x, charPosPx.y);
                    simBody.VelocityX = testVel.x;
                    simBody.VelocityY = testVel.y;
                    simBody.Bounce = 0.2f;
                    simBody.Friction = (weaponToUse == "dynamite") ? 1.7f : 0.3f;

                    var trajectory = MutinyPhysics.SimulateTrajectory(simBody, terrainGrid, gridW, gridH, 45);
                    if (trajectory.Count == 0)
                        continue;

                    Vector2 impactPx = trajectory[trajectory.Count - 1];

                    // Score this trajectory
                    float score = ScoreTrajectory(impactPx, enemies, allies, waterPixelY);

                    if (score > bestMove.Score)
                    {
                        bestMove.Score = score;
                        bestMove.Character = ally;
                        bestMove.WeaponType = weaponToUse;
                        bestMove.LaunchVelocity = testVel;
                    }
                }
            }

            return bestMove;
        }

        private float ScoreTrajectory(Vector2 impactPx, List<MutinyCharacter> enemies, List<MutinyCharacter> allies, float waterPixelY)
        {
            float score = 0f;

            // Water penalty
            if (impactPx.y >= waterPixelY)
            {
                score -= 30f;
            }

            // Enemy proximity reward (Flash AS2 Character.aiThink formula)
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector2 enemyPosPx = new Vector2(enemy.PhysicsBody.State.X, enemy.PhysicsBody.State.Y);
                float dist = Vector2.Distance(impactPx, enemyPosPx);

                if (dist < 120f)
                {
                    float ratio = 1f - (dist / 120f);
                    score += ratio * 40f;
                }
            }

            // Friendly fire penalty
            for (int i = 0; i < allies.Count; i++)
            {
                var ally = allies[i];
                if (ally == null || !ally.IsAlive)
                    continue;

                Vector2 allyPosPx = new Vector2(ally.PhysicsBody.State.X, ally.PhysicsBody.State.Y);
                float dist = Vector2.Distance(impactPx, allyPosPx);

                if (dist < 80f)
                {
                    float ratio = 1f - (dist / 80f);
                    score -= ratio * 50f;
                }
            }

            return score;
        }

        private void ExecuteMove(AIMove move)
        {
            var ch = move.Character;
            if (ch == null || !ch.IsAlive)
                return;

            if (!string.IsNullOrEmpty(move.WeaponType) && ch.HasWeapon(move.WeaponType))
            {
                MutinyWeaponFactory.SpawnAndFire(move.WeaponType, ch, move.LaunchVelocity);
            }
            else
            {
                ch.PhysicsBody.SetVelocity(move.LaunchVelocity.x, move.LaunchVelocity.y);
                ch.CanThrow = false;
                ch.CanShoot = false;
                if (m_TurnManager != null)
                {
                    m_TurnManager.NotifyActionStarted();
                }
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("click");
            }
        }
    }
}
