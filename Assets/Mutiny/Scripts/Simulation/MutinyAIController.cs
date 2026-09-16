using System.Collections;
using System.Collections.Generic;
using System;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    public enum AIMoveType
    {
        Pass,
        SelfThrow,
        ShootWeapon
    }

    public struct AIMove
    {
        public AIMoveType MoveType;
        public MutinyCharacter Character;
        public string WeaponType;
        public Vector2 LaunchVelocity;
        public Vector2 TargetPosition;
        public MutinyCharacter TargetCharacter;
        public float Score;
    }

    public enum MutinyAITurnGate
    {
        Cancel,
        Wait,
        Execute
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MutinyTeam))]
    public sealed class MutinyAIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public float ThinkDelay = 0.8f;
        [Tooltip("Retained for existing scenes. Flash uses exactly 50 character throw samples.")]
        public int TrajectorySamples = 50;

        private MutinyTeam m_Team;
        private MutinyTurnManager m_TurnManager;
        private Coroutine m_TurnCoroutine;
        private bool m_LoggedMissingManager;

        private void Awake()
        {
            m_Team = GetComponent<MutinyTeam>();
        }

        private void Start()
        {
            BindTurnManager();
            TryStartCurrentTurn("Start");
        }

        private void Update()
        {
            if (m_TurnManager == null)
                BindTurnManager();

            if (m_TurnCoroutine == null &&
                ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Execute)
            {
                BeginTurnRoutine("watchdog");
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
            MutinyDebugLog.Info("AI",
                $"turn event activeTeam={TeamLabel(activeTeam)} self={TeamLabel(m_Team)} phase={m_TurnManager?.CurrentPhase}", this);
            if (activeTeam == m_Team && m_Team.IsAiControlled && !m_Team.IsDefeated)
                BeginTurnRoutine("turn event");
        }

        private IEnumerator ExecuteAITurnRoutine()
        {
            MutinyDebugLog.Info("AI",
                $"thinking started team={TeamLabel(m_Team)} delay={ThinkDelay:0.00}s", this);
            yield return new WaitForSeconds(ThinkDelay);

            bool loggedWait = false;
            MutinyAITurnGate gate = ResolveTurnGate(m_TurnManager, m_Team);
            while (gate == MutinyAITurnGate.Wait)
            {
                if (!loggedWait)
                {
                    loggedWait = true;
                    MutinyDebugLog.Info("AI",
                        $"waiting for board readiness team={TeamLabel(m_Team)} phase={m_TurnManager.CurrentPhase}", this);
                }
                yield return null;
                gate = ResolveTurnGate(m_TurnManager, m_Team);
            }

            if (gate == MutinyAITurnGate.Cancel)
            {
                MutinyDebugLog.Info("AI", "thinking cancelled because the active turn changed", this);
                m_TurnCoroutine = null;
                yield break;
            }

            AIMove bestMove;
            try
            {
                bestMove = EvaluateBestMove();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
                MutinyDebugLog.Warning("AI", "move evaluation failed; passing the turn", this);
                m_TurnManager.PassTurn();
                m_TurnCoroutine = null;
                yield break;
            }

            if (bestMove.MoveType == AIMoveType.Pass || bestMove.Character == null)
            {
                MutinyDebugLog.Info("AI", "no safe or rewarding action found; passing the turn", this);
                m_TurnManager.PassTurn();
                m_TurnCoroutine = null;
                yield break;
            }

            m_Team.SelectCharacter(bestMove.Character);
            MutinyDebugLog.Info("AI",
                $"selected type={bestMove.MoveType} character={bestMove.Character.name} weapon={bestMove.WeaponType} score={bestMove.Score:0.00} velocity={bestMove.LaunchVelocity}", this);

            yield return new WaitForSeconds(0.35f);

            gate = ResolveTurnGate(m_TurnManager, m_Team);
            while (gate == MutinyAITurnGate.Wait)
            {
                yield return null;
                gate = ResolveTurnGate(m_TurnManager, m_Team);
            }

            if (gate == MutinyAITurnGate.Execute)
                ExecuteMove(bestMove);
            else
                MutinyDebugLog.Info("AI", "selected move cancelled because active turn changed", this);

            m_TurnCoroutine = null;
        }

        public static MutinyAITurnGate ResolveTurnGate(MutinyTurnManager manager, MutinyTeam team)
        {
            if (manager == null || team == null || manager.CurrentTeam != team ||
                !team.IsAiControlled || team.IsDefeated ||
                manager.CurrentPhase == TurnPhase.GameOver ||
                manager.CurrentPhase == TurnPhase.NotStarted)
                return MutinyAITurnGate.Cancel;

            return manager.CurrentPhase == TurnPhase.TurnActive
                ? MutinyAITurnGate.Execute
                : MutinyAITurnGate.Wait;
        }

        public AIMove EvaluateBestMove()
        {
            var bestMove = new AIMove
            {
                MoveType = AIMoveType.Pass,
                // Team.advance starts from -Infinity.  A non-positive move is still
                // mandatory on a newly started AI turn.
                Score = float.NegativeInfinity
            };

            // Flash keeps dead characters in Team.characters when calculating the
            // opposing centroid; individual distance scoring then checks alive.
            var allChars = FindObjectsByType<MutinyCharacter>();
            var enemies = new List<MutinyCharacter>();
            var allies = new List<MutinyCharacter>();

            for (int i = 0; i < allChars.Length; i++)
            {
                var ch = allChars[i];
                if (ch != null && ch.PhysicsBody != null)
                {
                    if (ch.TeamIndex != m_Team.TeamNumber)
                        enemies.Add(ch);
                    else
                        allies.Add(ch);
                }
            }

            if (enemies.Count == 0 || allies.Count == 0 || !ContainsAlive(enemies) || !ContainsAlive(allies))
                return bestMove;

            // Retrieve terrain and water level
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

            // Two-Phase check:
            // If a character was already chosen and moved (CanThrow consumed, CanShoot remaining),
            // only evaluate shooting for this selected character.
            MutinyCharacter activeChar = m_Team.SelectedCharacter;
            if (activeChar != null && activeChar.IsAlive && !activeChar.CanThrow && activeChar.CanShoot)
            {
                int continuationCandidates = 0;
                EvaluateCharacterWeapons(activeChar, enemies, allies, terrainGrid, gridW, gridH, waterPixelY,
                    ref bestMove, ref continuationCandidates);
                MutinyDebugLog.Info("AI",
                    $"continuation candidates={continuationCandidates} best={bestMove.Score:0.000}", this);

                // Team.continueTurn sets aiCanBailOut=true: the selected pirate only
                // fires in phase two when the best weapon candidate is positive.
                if (bestMove.Character == null || bestMove.Score <= 0f)
                    return CreatePassMove();
                return bestMove;
            }

            int candidateCount = 0;
            // Team.advance collects candidates from every character before choosing.
            for (int a = 0; a < allies.Count; a++)
            {
                var ally = allies[a];
                if (ally == null || !ally.IsAlive)
                    continue;

                // 1. Direct weapon attack from current position (Primary Action)
                if (ally.CanShoot)
                {
                    EvaluateCharacterWeapons(ally, enemies, allies, terrainGrid, gridW, gridH, waterPixelY,
                        ref bestMove, ref candidateCount);
                }

                if (ally.CanThrow)
                {
                    EvaluateCharacterSelfThrow(ally, enemies, allies, terrainGrid, gridW, gridH, waterPixelY,
                        ref bestMove, ref candidateCount);
                }
            }

            MutinyDebugLog.Info("AI",
                $"first-action candidates={candidateCount} best={(bestMove.Character == null ? "none" : bestMove.Character.name)} score={bestMove.Score:0.000}", this);
            return bestMove;
        }

        private void EvaluateCharacterWeapons(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            ref AIMove bestMove,
            ref int candidateCount)
        {
            Vector2 shooterPos = new Vector2(shooter.PhysicsBody.State.X, shooter.PhysicsBody.State.Y);

            int samples = Mathf.FloorToInt(shooter.Luck * m_Team.Characters.Count / Mathf.Max(1, m_Team.AliveCount));
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, int> entry in shooter.WeaponInventory)
            {
                string weaponType = entry.Key;
                if (!shooter.HasWeapon(weaponType) || !seen.Add(weaponType))
                    continue;

                if (string.Equals(weaponType, "tidalWave", StringComparison.OrdinalIgnoreCase))
                {
                    float score = ScoreTidalWave(enemies, allies, waterPixelY);
                    ConsiderCandidate(ref bestMove, new AIMove { MoveType = AIMoveType.ShootWeapon, Character = shooter, WeaponType = weaponType, Score = score }, ref candidateCount);
                    continue;
                }

                if (string.Equals(weaponType, "voodooDoll", StringComparison.OrdinalIgnoreCase))
                {
                    EvaluateVoodoo(shooter, enemies, terrainGrid, gridW, gridH, waterPixelY, ref bestMove, ref candidateCount);
                    continue;
                }

                // These weapons have original aiPerform data beyond vx/vy.  Do not
                // substitute a normal projectile until their runtime path is exact.
                if (IsDeferredSpecialWeapon(weaponType))
                {
                    MutinyDebugLog.Info("AI", $"deferred special weapon={weaponType} character={shooter.name}", this);
                    continue;
                }

                if (!MutinyWeaponFactoryCanFire(weaponType))
                    continue;

                for (int s = 0; s < samples; s++)
                {
                    Vector2 velocity = RandomArc(MutinyWeaponFactory.GetTwangMaxForce(weaponType));
                    PhysicsBodyState body = CreateWeaponSimulation(shooterPos, weaponType, velocity);
                    Vector2 impact = SimulateWeaponImpact(body, weaponType, terrainGrid, gridW, gridH, waterPixelY);
                    float score = ScoreGenericWeaponCandidate(weaponType, impact, enemies, allies);
                    ConsiderCandidate(ref bestMove, new AIMove
                    {
                        MoveType = AIMoveType.ShootWeapon,
                        Character = shooter,
                        WeaponType = weaponType,
                        LaunchVelocity = velocity,
                        Score = score
                    }, ref candidateCount);
                }
            }
        }

        private void EvaluateCharacterSelfThrow(
            MutinyCharacter character,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            ref AIMove bestMove,
            ref int candidateCount)
        {
            Vector2 startPos = new Vector2(character.PhysicsBody.State.X, character.PhysicsBody.State.Y);
            Vector2 enemyCentroid = Vector2.zero;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemyCentroid += PositionOf(enemies[i]);
            }
            enemyCentroid /= enemies.Count;

            const int originalSamples = 50;
            for (int s = 0; s < originalSamples; s++)
            {
                Vector2 testVel = RandomArc(20f);

                var simBody = PhysicsBodyState.CreateDefault(startPos.x, startPos.y);
                simBody.VelocityX = testVel.x;
                simBody.VelocityY = testVel.y;
                simBody.Friction = 2.0f; // Character ground friction
                simBody.Bounce = 0.2f;

                Vector2 impactPx = SimulateCharacterLanding(simBody, terrainGrid, gridW, gridH, waterPixelY);
                float score = ScoreSelfThrow(character, startPos, impactPx, enemyCentroid, enemies, allies, waterPixelY);
                ConsiderCandidate(ref bestMove, new AIMove
                {
                    MoveType = AIMoveType.SelfThrow,
                    Character = character,
                    LaunchVelocity = testVel,
                    Score = score
                }, ref candidateCount);
            }
        }

        private static AIMove CreatePassMove()
        {
            return new AIMove { MoveType = AIMoveType.Pass, Score = 0f };
        }

        private static bool ContainsAlive(List<MutinyCharacter> characters)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].IsAlive)
                    return true;
            }
            return false;
        }

        private static Vector2 PositionOf(MutinyCharacter character)
        {
            PhysicsBodyState state = character.PhysicsBody.State;
            return new Vector2(state.X, state.Y);
        }

        private static Vector2 RandomArc(float maxForce)
        {
            // Character.randomThrows and Weapon.randomThrows: 180 + int(random * 180).
            int degrees = 180 + UnityEngine.Random.Range(0, 180);
            float force = UnityEngine.Random.Range(5f, maxForce);
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians) * force, Mathf.Sin(radians) * force);
        }

        private static bool IsDeferredSpecialWeapon(string weaponType)
        {
            return string.Equals(weaponType, "seagull", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "woodenCrate", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "gunpowderBarrel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "anchor", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "cannon", StringComparison.OrdinalIgnoreCase);
        }

        private static bool MutinyWeaponFactoryCanFire(string weaponType)
        {
            return string.Equals(weaponType, "cherryBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "dynamite", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "banana", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "cannonball", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "boulder", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "mine", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "parachuteBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "rumBottle", StringComparison.OrdinalIgnoreCase);
        }

        private static PhysicsBodyState CreateWeaponSimulation(Vector2 start, string weaponType, Vector2 velocity)
        {
            PhysicsBodyState body = PhysicsBodyState.CreateDefault(start.x, start.y);
            body.VelocityX = velocity.x;
            body.VelocityY = velocity.y;
            body.Bounce = 0.2f;
            body.Friction = 0.3f;
            body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 9f;

            if (string.Equals(weaponType, "dynamite", StringComparison.OrdinalIgnoreCase))
            {
                body.Friction = 1.7f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 11f;
            }
            else if (string.Equals(weaponType, "banana", StringComparison.OrdinalIgnoreCase))
            {
                body.Bounce = 0.8f;
                body.Friction = 0.5f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 7f;
            }
            else if (string.Equals(weaponType, "boulder", StringComparison.OrdinalIgnoreCase))
            {
                // MutinyBoulder.Fire halves the released velocity after Weapon.Fire.
                body.VelocityX *= 0.5f;
                body.VelocityY *= 0.5f;
                body.Weight = 1.5f;
                body.Friction = 0.25f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 31f;
            }
            else if (string.Equals(weaponType, "mine", StringComparison.OrdinalIgnoreCase))
            {
                body.Friction = 1.5f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 14f;
            }
            else if (string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase))
            {
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 7f;
            }
            return body;
        }

        private static float ScoreGenericWeaponCandidate(
            string weaponType,
            Vector2 impact,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies)
        {
            float score = -0.01f;
            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distance = Vector2.Distance(impact, PositionOf(enemy));
                if (distance < 70f)
                {
                    score += 1.5f - distance / 70f;
                    score *= 1f + enemy.Evilness;
                }
            }

            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally == null || !ally.IsAlive)
                    continue;

                float distance = Vector2.Distance(impact, PositionOf(ally));
                if (distance < 40f)
                    score -= 1.5f - distance / 40f;
            }

            if (string.Equals(weaponType, "anchor", StringComparison.OrdinalIgnoreCase))
                score *= 0.5f;
            if (string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase))
                score = (score - 0.5f) * 1.2f;
            return score;
        }

        private float ScoreSelfThrow(
            MutinyCharacter character,
            Vector2 start,
            Vector2 landing,
            Vector2 enemyCentroid,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            float waterPixelY)
        {
            float score = -(landing.y - start.y) * 0.003f;
            score += (Mathf.Abs(landing.x - enemyCentroid.x) - Mathf.Abs(start.x - enemyCentroid.x)) / -500f;
            score += (Mathf.Abs(landing.y - enemyCentroid.y) - Mathf.Abs(start.y - enemyCentroid.y)) / -500f;
            if (landing.y >= waterPixelY)
                score -= 2f;

            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distance = Vector2.Distance(landing, PositionOf(enemy));
                float proximity;
                if (distance < 200f)
                {
                    proximity = 0.2f * (1f - distance / 200f);
                    if (distance < 100f)
                    {
                        proximity *= distance / 100f;
                        score -= 3f * (1f - distance / 100f);
                    }
                }
                else
                {
                    proximity = 0.3f * Mathf.Pow(0.75f, distance / 200f);
                }
                score += proximity;
                score *= 1f + enemy.Evilness;
            }

            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally == null || !ally.IsAlive)
                    continue;

                float distance = Vector2.Distance(landing, PositionOf(ally));
                if (ally == character && distance < 80f)
                    score -= 1f - distance / 80f;
                else if (ally != character && distance < 40f)
                    score -= 0.1f * (1f - distance / 40f);
            }

            MutinyTreasureChest[] chests = FindObjectsByType<MutinyTreasureChest>();
            for (int i = 0; i < chests.Length; i++)
            {
                if (chests[i] == null || chests[i].IsFinished)
                    continue;
                if (Vector2.Distance(landing, new Vector2(chests[i].PixelX, chests[i].FloorPixelY)) < 40f)
                    score += 0.5f;
            }

            if (character.HasWeapon("cherryBomb"))
                score = ScoreCherryBombFollowUpAtMoveLanding(score, character, landing, enemies);

            float movement = Vector2.Distance(landing, start);
            score += movement < 300f ? 0.3f * movement / 300f - 0.3f : -0.3f;
            return score;
        }

        private float ScoreCherryBombFollowUpAtMoveLanding(
            float score,
            MutinyCharacter character,
            Vector2 landing,
            List<MutinyCharacter> enemies)
        {
            // Character.aiThink calls cherryBomb.randomThrows(10), but then (as
            // confirmed in pcode) scores the character movement landing every time.
            // Preserve that scoring quirk without instantiating ten simulation objects.
            for (int sample = 0; sample < 10; sample++)
            {
                float followUp = 0f;
                for (int i = 0; i < enemies.Count; i++)
                {
                    MutinyCharacter enemy = enemies[i];
                    if (enemy == null || !enemy.IsAlive)
                        continue;
                    float distance = Vector2.Distance(landing, PositionOf(enemy));
                    if (distance < 100f)
                        followUp += (1f - distance / 100f) * (1f + enemy.Evilness) * 0.5f;
                }
                float selfDistance = Vector2.Distance(landing, PositionOf(character));
                if (selfDistance < 80f)
                    followUp -= 1f - selfDistance / 80f;
                if (followUp > 0f)
                    score += followUp;
            }
            return score;
        }

        private static float ScoreTidalWave(List<MutinyCharacter> enemies, List<MutinyCharacter> allies, float waterPixelY)
        {
            float score = 0f;
            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy != null && enemy.IsAlive && PositionOf(enemy).y >= waterPixelY - 300f)
                    score += enemy.Health / enemy.MaxHealth * 0.5f;
            }
            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally != null && ally.IsAlive && PositionOf(ally).y >= waterPixelY - 300f)
                    score -= 1.5f;
            }
            return score;
        }

        private void EvaluateVoodoo(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            ref AIMove bestMove,
            ref int candidateCount)
        {
            for (int enemyIndex = 0; enemyIndex < enemies.Count; enemyIndex++)
            {
                MutinyCharacter enemy = enemies[enemyIndex];
                if (enemy == null || !enemy.IsAlive)
                    continue;
                Vector2 start = PositionOf(enemy);
                for (int sample = 0; sample < 2; sample++)
                {
                    Vector2 velocity = RandomArc(20f);
                    PhysicsBodyState body = PhysicsBodyState.CreateDefault(start.x, start.y);
                    body.VelocityX = velocity.x;
                    body.VelocityY = velocity.y;
                    body.Friction = 2f;
                    Vector2 landing = SimulateCharacterLanding(body, terrainGrid, gridW, gridH, waterPixelY);
                    float score = landing.y >= waterPixelY
                        ? 1f + UnityEngine.Random.Range(0f, 0.2f)
                        : UnityEngine.Random.Range(0f, 0.2f) - 0.5f;
                    ConsiderCandidate(ref bestMove, new AIMove
                    {
                        MoveType = AIMoveType.ShootWeapon,
                        Character = shooter,
                        WeaponType = "voodooDoll",
                        LaunchVelocity = velocity,
                        TargetCharacter = enemy,
                        Score = score
                    }, ref candidateCount);
                }
            }
        }

        private static void ConsiderCandidate(ref AIMove bestMove, AIMove candidate, ref int candidateCount)
        {
            candidateCount++;
            // Flash Team.advance uses strict >, so the first candidate wins a tie.
            if (bestMove.Character == null || candidate.Score > bestMove.Score)
                bestMove = candidate;
        }

        private static Vector2 SimulateWeaponImpact(
            PhysicsBodyState body,
            string weaponType,
            string[,] grid,
            int gridW,
            int gridH,
            float waterPixelY)
        {
            int maxSteps = (weaponType == "dynamite") ? 80 : 50;

            for (int i = 0; i < maxSteps; i++)
            {
                StepResult res = MutinyPhysics.Step(ref body, grid, gridW, gridH);

                // Water entry
                if (body.Y >= waterPixelY)
                    return new Vector2(body.X, body.Y);

                // Cherry Bomb explodes immediately upon any contact
                if (weaponType == "cherryBomb" &&
                    (res.HitFloor || res.HitCeiling || res.HitLeftWall || res.HitRightWall))
                {
                    return new Vector2(body.X, body.Y);
                }

                // Dynamite rolls until coming to a complete rest
                if (weaponType == "dynamite" && res.IsAtRest)
                {
                    return new Vector2(body.X, body.Y);
                }
            }

            return new Vector2(body.X, body.Y);
        }

        private static Vector2 SimulateCharacterLanding(
            PhysicsBodyState body,
            string[,] grid,
            int gridW,
            int gridH,
            float waterPixelY)
        {
            for (int i = 0; i < 70; i++)
            {
                StepResult res = MutinyPhysics.Step(ref body, grid, gridW, gridH);

                if (body.Y >= waterPixelY)
                    return new Vector2(body.X, body.Y);

                if (res.IsAtRest)
                    return new Vector2(body.X, body.Y);
            }

            return new Vector2(body.X, body.Y);
        }

        private void ExecuteMove(AIMove move)
        {
            var ch = move.Character;
            if (ch == null || !ch.IsAlive)
                return;

            if (move.MoveType == AIMoveType.ShootWeapon && !string.IsNullOrEmpty(move.WeaponType))
            {
                MutinyDebugLog.Info("AI",
                    $"firing weapon={move.WeaponType} character={ch.name} velocity={move.LaunchVelocity}", this);
                if (string.Equals(move.WeaponType, "tidalWave", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon waveWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (waveWeapon is MutinyTidalWave wave)
                    {
                        float waterY = ch.PhysicsBody.WaterPixelY;
                        wave.StartWave(-550f, waterY);
                        ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else if (string.Equals(move.WeaponType, "voodooDoll", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon dollWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (dollWeapon is MutinyVoodooDoll doll)
                    {
                        doll.BindTarget(move.TargetCharacter);
                        doll.Fire(move.LaunchVelocity);
                        ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else
                {
                    MutinyWeaponFactory.SpawnAndFire(move.WeaponType, ch, move.LaunchVelocity);
                }
                ch.CanThrow = false;
                ch.CanShoot = false;
                if (m_TurnManager != null)
                {
                    m_TurnManager.NotifyActionStarted();
                }
            }
            else if (move.MoveType == AIMoveType.SelfThrow)
            {
                MutinyDebugLog.Info("AI",
                    $"jumping character={ch.name} velocity={move.LaunchVelocity}; preserves CanShoot for phase 2", this);
                ch.PhysicsBody.SetVelocity(move.LaunchVelocity.x, move.LaunchVelocity.y);
                ch.MarkSelfThrown("AI");
                ch.CanThrow = false;
                // Authentic Flash parity: CanShoot remains true for phase 2!
                if (m_TurnManager != null)
                {
                    m_TurnManager.NotifyActionStarted();
                }
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("click");
            }
            else
            {
                MutinyDebugLog.Info("AI", $"passing turn character={ch.name}", this);
                if (m_TurnManager != null)
                {
                    m_TurnManager.PassTurn();
                }
            }
        }

        private void BindTurnManager()
        {
            MutinyTurnManager found = FindAnyObjectByType<MutinyTurnManager>();
            if (found == null)
            {
                if (!m_LoggedMissingManager)
                {
                    m_LoggedMissingManager = true;
                    MutinyDebugLog.Warning("AI", $"team={TeamLabel(m_Team)} cannot find turn manager", this);
                }
                return;
            }

            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            m_TurnManager = found;
            m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            m_TurnManager.OnTurnStarted += HandleTurnStarted;
            m_LoggedMissingManager = false;
            MutinyDebugLog.Info("AI", $"bound turn manager team={TeamLabel(m_Team)}", this);
        }

        private void TryStartCurrentTurn(string source)
        {
            if (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Execute)
                BeginTurnRoutine(source);
        }

        private void BeginTurnRoutine(string source)
        {
            if (m_TurnCoroutine != null)
                StopCoroutine(m_TurnCoroutine);
            MutinyDebugLog.Info("AI", $"starting turn routine source={source}", this);
            m_TurnCoroutine = StartCoroutine(ExecuteAITurnRoutine());
        }

        private static string TeamLabel(MutinyTeam team)
        {
            return team == null ? "none" : $"T{team.TeamNumber}";
        }
    }
}
