using System;
using System.Collections.Generic;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    public enum TurnPhase
    {
        NotStarted,
        TurnActive,
        ActionExecuting,
        Settling,
        GameOver
    }

    public enum GameOverResult
    {
        None,
        Team1Wins,
        Team2Wins,
        Draw
    }

    [DisallowMultipleComponent]
    public sealed class MutinyTurnManager : MonoBehaviour
    {
        public const int InactivitySettlingThreshold = 10; // Exact Nitrome standard: 10 frames = 0.4s

        [Header("Teams")]
        public MutinyTeam Team1;
        public MutinyTeam Team2;
        public MutinyTeam CurrentTeam;

        [Header("State")]
        public TurnPhase CurrentPhase = TurnPhase.NotStarted;
        public GameOverResult GameResult = GameOverResult.None;
        public int InactivityTicks = 0;
        public int TurnCount = 0;

        public event Action<MutinyTeam> OnTurnStarted;
        public event Action<MutinyTeam> OnTurnEnded;
        public event Action<GameOverResult> OnGameOver;

        private float m_TickAccumulator = 0f;
        private List<MutinyCharacter> m_AllCharacters = new List<MutinyCharacter>();
        private bool m_ActionCommittedThisTurn;

        public bool ActionCommittedThisTurn => m_ActionCommittedThisTurn;

        private void Start()
        {
            if (Team1 == null || Team2 == null)
            {
                var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
                if (levelRoot != null)
                {
                    Team1 = levelRoot.Team1;
                    Team2 = levelRoot.Team2;
                }
            }

            if (CurrentPhase == TurnPhase.NotStarted && Team1 != null && Team2 != null)
            {
                StartGame();
            }
        }

        public void Initialize(MutinyTeam team1, MutinyTeam team2)
        {
            Team1 = team1;
            Team2 = team2;
            StartGame();
        }

        public void StartGame()
        {
            if (!HasPlayableTeams())
            {
                CurrentTeam = null;
                CurrentPhase = TurnPhase.NotStarted;
                GameResult = GameOverResult.None;
                Debug.LogError("[MutinyTurnManager] Cannot start: both teams must contain at least one character.", this);
                return;
            }

            TurnCount = 0;
            GameResult = GameOverResult.None;
            InactivityTicks = 0;
            m_ActionCommittedThisTurn = false;

            RefreshCharacterCache();

            // Single-player always starts with the red team. In the original local
            // two-player mode either side starts with an even random chance.
            CurrentTeam = !Team2.IsAiControlled && UnityEngine.Random.value >= 0.5f
                ? Team2
                : Team1;
            CurrentPhase = TurnPhase.TurnActive;

            if (CurrentTeam != null)
            {
                CurrentTeam.StartTurn();
                OnTurnStarted?.Invoke(CurrentTeam);
            }

            Debug.Log($"[MutinyTurnManager] Started with Team 1={Team1.AliveCount}, Team 2={Team2.AliveCount} alive.", this);

            Mutiny.Presentation.MutinyAudioManager.Instance?.PlayMusic("game_music");
        }

        public void RefreshCharacterCache()
        {
            m_AllCharacters.Clear();
            AddTeamCharacters(Team1);
            AddTeamCharacters(Team2);
        }

        public void NotifyActionStarted()
        {
            m_ActionCommittedThisTurn = true;
            InactivityTicks = 0;
            CurrentPhase = TurnPhase.ActionExecuting;
        }

        public void NotifyAmbientActivity()
        {
            InactivityTicks = 0;
        }

        public void PassTurn()
        {
            if (CurrentPhase == TurnPhase.GameOver || CurrentPhase == TurnPhase.NotStarted || CurrentTeam == null)
                return;

            CurrentTeam.FinishTurn();
            NotifyActionStarted();
        }

        private void Update()
        {
            if (CurrentPhase == TurnPhase.GameOver || CurrentPhase == TurnPhase.NotStarted)
                return;

            m_TickAccumulator += Time.deltaTime;
            while (m_TickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_TickAccumulator -= MutinyPhysics.TimeStep;
                AdvanceSimulationTick();
            }
        }

        public void AdvanceSimulationTick()
        {
            bool allAtRest = CheckAllBodiesAtRest();

            if (allAtRest)
            {
                InactivityTicks++;

                if (InactivityTicks >= InactivitySettlingThreshold)
                {
                    InactivityTicks = 0;
                    EvaluateTurnOrGameOver();
                }
            }
            else
            {
                InactivityTicks = 0;
                CurrentPhase = TurnPhase.ActionExecuting;
            }
        }

        public bool CheckAllBodiesAtRest()
        {
            // 1. Check characters
            for (int i = 0; i < m_AllCharacters.Count; i++)
            {
                var ch = m_AllCharacters[i];
                if (ch == null || !ch.IsAlive)
                    continue;

                if (ch.PhysicsBody != null && !ch.PhysicsBody.IsAtRest)
                {
                    return false;
                }
            }

            // 2. Check active weapons / projectiles
            var activeWeapons = FindObjectsByType<MutinyWeapon>();
            for (int i = 0; i < activeWeapons.Length; i++)
            {
                var w = activeWeapons[i];
                if (w != null && w.IsFired && !w.IsFinished)
                {
                    return false;
                }
            }

            // 3. Check active explosions
            var activeExplosions = FindObjectsByType<MutinyExplosion>();
            if (activeExplosions.Length > 0)
            {
                return false;
            }

            return true;
        }

        private void EvaluateTurnOrGameOver()
        {
            // Empty team lists mean the level has not finished initializing, or this is
            // a stale manager from a level being replaced. Neither condition is a draw.
            if (!HasPlayableTeams())
            {
                CurrentTeam = null;
                CurrentPhase = TurnPhase.NotStarted;
                GameResult = GameOverResult.None;
                Debug.LogError("[MutinyTurnManager] Stopped because team character data is unavailable.", this);
                return;
            }

            // 1. Check Win / Loss / Draw conditions
            bool team1Defeated = Team1 == null || Team1.IsDefeated;
            bool team2Defeated = Team2 == null || Team2.IsDefeated;

            if (team1Defeated || team2Defeated)
            {
                CurrentPhase = TurnPhase.GameOver;
                if (team1Defeated && team2Defeated)
                {
                    GameResult = GameOverResult.Draw;
                    Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("die");
                }
                else if (team2Defeated)
                {
                    GameResult = GameOverResult.Team1Wins;
                    Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("ching");

                    var controller = FindAnyObjectByType<MutinyLevelController>();
                    if (controller != null)
                    {
                        Mutiny.Persistence.MutinySaveSystem.UnlockLevel(controller.CurrentLevelIndex + 1);
                    }
                }
                else
                {
                    GameResult = GameOverResult.Team2Wins;
                    Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("fan");
                }

                OnGameOver?.Invoke(GameResult);
                return;
            }

            // Merely having no selected character is the normal state at the start of
            // an original Mutiny turn. A turn may only finish after an action (or an
            // explicit pass) has actually been committed.
            if (!m_ActionCommittedThisTurn)
            {
                CurrentPhase = TurnPhase.TurnActive;
                return;
            }

            // 2. Check if current team finished its turn actions
            if (CurrentTeam != null && CurrentTeam.IsTurnComplete())
            {
                OnTurnEnded?.Invoke(CurrentTeam);
                CurrentTeam.FinishTurn();

                // Switch turn
                CurrentTeam = (CurrentTeam == Team1) ? Team2 : Team1;
                TurnCount++;
                CurrentPhase = TurnPhase.TurnActive;
                m_ActionCommittedThisTurn = false;

                FindAnyObjectByType<MutinyTreasureChestManager>()?.TryDropNew();
                CurrentTeam.StartTurn();
                OnTurnStarted?.Invoke(CurrentTeam);
            }
            else
            {
                // Current character still has remaining actions
                CurrentPhase = TurnPhase.TurnActive;
            }
        }

        private bool HasPlayableTeams()
        {
            return Team1 != null && Team2 != null &&
                   Team1.Characters != null && Team1.Characters.Count > 0 &&
                   Team2.Characters != null && Team2.Characters.Count > 0;
        }

        private void AddTeamCharacters(MutinyTeam team)
        {
            if (team == null || team.Characters == null)
                return;

            for (int i = 0; i < team.Characters.Count; i++)
            {
                MutinyCharacter character = team.Characters[i];
                if (character != null && !m_AllCharacters.Contains(character))
                    m_AllCharacters.Add(character);
            }
        }
    }
}
