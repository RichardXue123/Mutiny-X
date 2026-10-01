using System;
using Mutiny.Diagnostics;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Levels
{
    public enum MutinyGameMode
    {
        SinglePlayer,
        LocalTwoPlayer
    }

    [DisallowMultipleComponent]
    public sealed class MutinyLevelController : MonoBehaviour
    {
        [SerializeField]
        private TextAsset m_LevelXml;

        [SerializeField]
        private bool m_BuildOnStart = true;

        [SerializeField, Tooltip("Enter a dedicated preview scene's assigned level in Play Mode. Main always starts at the title menu.")]
        private bool m_StartInGameplay;

        [SerializeField]
        private MutinyLevelRoot m_CurrentLevel;

        // `_root.score` is a one-player session value in the Flash game.  It must
        // outlive a level rebuild, but it is not a saved unlock setting.
        private int m_SinglePlayerScore;
        private int m_LastCompletedLevelScore;
        private int m_AwardedLevelIndex = -1;
        private bool m_HasMenuSessionMode;
        private bool m_ResultRecordedForCurrentLevel;
        private int m_Player1Wins;
        private int m_Player2Wins;
        [SerializeField] private MutinyGameMode m_LevelMode;
        [SerializeField] private bool m_HasLevelIdentity;
        [SerializeField] private int m_CurrentLevelIndex = 1;

        public int CurrentLevelIndex
        {
            get => m_CurrentLevelIndex;
            set => m_CurrentLevelIndex = value;
        }
        public MutinyLevelId CurrentLevelId => new MutinyLevelId(
            m_HasLevelIdentity ? m_LevelMode : ActiveGameMode, CurrentLevelIndex);
        public int OriginalLevelIndex => CurrentLevelId.OriginalNumber;
        public int SinglePlayerScore => m_SinglePlayerScore;
        public int LastCompletedLevelScore => m_LastCompletedLevelScore;
        public MutinyGameMode SessionMode { get; private set; } = MutinyGameMode.SinglePlayer;
        public MutinyGameMode ActiveGameMode => m_HasMenuSessionMode
            ? SessionMode
            : m_HasLevelIdentity ? m_LevelMode
            : m_CurrentLevel != null && m_CurrentLevel.Players != 1
                ? MutinyGameMode.LocalTwoPlayer
                : MutinyGameMode.SinglePlayer;
        public int Player1Wins => m_Player1Wins;
        public int Player2Wins => m_Player2Wins;

        public void ConfigureSession(MutinyGameMode mode, bool resetVersusWins = false)
        {
            SessionMode = mode;
            m_HasMenuSessionMode = true;
            if (resetVersusWins)
            {
                m_Player1Wins = 0;
                m_Player2Wins = 0;
            }
        }

        public void RecordGameResult(GameOverResult result)
        {
            if (ActiveGameMode != MutinyGameMode.LocalTwoPlayer || m_ResultRecordedForCurrentLevel)
                return;
            m_ResultRecordedForCurrentLevel = true;
            if (result == GameOverResult.Team1Wins)
                m_Player1Wins++;
            else if (result == GameOverResult.Team2Wins)
                m_Player2Wins++;
        }

        public TextAsset LevelXml
        {
            get => m_LevelXml;
            set => m_LevelXml = value;
        }

        public MutinyLevelRoot CurrentLevel => m_CurrentLevel;
        public bool StartInGameplay
        {
            get => m_StartInGameplay;
            set => m_StartInGameplay = value;
        }

        // GM-12/13: the override belongs to this root's native AI, not the session.
        public bool TrySetCurrentAiLuck(float? luck, out string error)
        {
            error = null;
            if (luck.HasValue && !MutinyAIController.IsValidGmLuck(luck.Value))
            {
                error = "Luck must be a finite number from 0 to 99999.";
                return false;
            }
            if (m_CurrentLevel == null || !m_CurrentLevel.gameObject.activeInHierarchy ||
                ActiveGameMode != MutinyGameMode.SinglePlayer)
            {
                error = "Requires a loaded single-player level.";
                return false;
            }
            MutinyTeam team = m_CurrentLevel.Team2;
            MutinyAIController ai = team != null ? team.GetComponent<MutinyAIController>() : null;
            if (ai == null || !team.IsAiControlled)
            {
                error = "The current level has no native enemy AI.";
                return false;
            }
            ai.SetLevelLuckOverride(luck);
            return true;
        }

        private void Awake()
        {
            // Older versions of the scene tool attached this controller directly to an
            // already-built level. Adopt that level instead of destroying and rebuilding
            // it during Start (which leaves the old turn manager with dead references).
            if (m_CurrentLevel == null)
            {
                MutinyLevelRoot attachedLevel = GetComponent<MutinyLevelRoot>();
                if (attachedLevel != null)
                {
                    m_CurrentLevel = attachedLevel;
                    m_BuildOnStart = false;
                }
            }

            if (m_LevelXml != null)
            {
                ParseLevelIdentityFromName(m_LevelXml.name);
            }
        }

        private void Start()
        {
            if (m_BuildOnStart && m_CurrentLevel == null && m_LevelXml != null)
            {
                BuildLevel();
            }
            else if (m_CurrentLevel != null)
            {
                // Legacy/baked levels do not pass through BuildLevel at runtime.
                MutinyCameraController.ResetCamerasForLevel(m_CurrentLevel);
            }
        }

        public void LoadLevel(int levelNumber)
        {
            TryLoadLevel(levelNumber);
        }

        public static bool HasNumberedLevelData(int levelNumber)
        {
            return HasLevelData(new MutinyLevelId(MutinyGameMode.SinglePlayer, levelNumber));
        }

        public static bool HasLevelData(MutinyLevelId id) =>
            id.IsValid && Resources.Load<TextAsset>(id.ResourcePath) != null;

        public bool TryLoadLevel(int levelNumber)
        {
            return TryLoadLevel(new MutinyLevelId(SessionMode, levelNumber));
        }

        public bool TryLoadLevel(MutinyLevelId id)
        {
            if (!id.IsValid)
                return false;
            TextAsset xml = Resources.Load<TextAsset>(id.ResourcePath);
            if (xml == null)
            {
                MutinyDebugLog.Warning("Level", $"Runtime level resource not found: {id.AssetName}.xml", this);
                return false;
            }
            // Reject malformed data before replacing the live world or its session.
            try
            {
                MutinyLevelXmlParser.Parse(xml.text, xml.name);
            }
            catch (FormatException exception)
            {
                MutinyDebugLog.Warning("Level", $"Cannot load {id}: {exception.Message}", this);
                return false;
            }
            ConfigureSession(id.Mode);
            m_LevelMode = id.Mode;
            m_HasLevelIdentity = true;
            CurrentLevelIndex = id.Number;
            m_AwardedLevelIndex = -1;
            m_LastCompletedLevelScore = 0;
            m_ResultRecordedForCurrentLevel = false;
            LevelXml = xml;
            BuildLevel();
            return m_CurrentLevel != null;
        }

        public void LoadNextLevel()
        {
            TryLoadLevel(new MutinyLevelId(CurrentLevelId.Mode, CurrentLevelIndex + 1));
        }

        public void RestartCurrentLevel()
        {
            MutinyDebugLog.Info("Level",
                $"restart requested level={CurrentLevelIndex} frame={Time.frameCount} currentRoot={(m_CurrentLevel != null ? m_CurrentLevel.name : "none")}",
                this);
            if (ActiveGameMode == MutinyGameMode.SinglePlayer)
                ResetSinglePlayerScore();
            TryLoadLevel(CurrentLevelId);
        }

        /// <summary>
        /// Mirrors Controller.get1PLevelScore: living health is divided by the
        /// original crew size, then the player team's completed-turn penalty is
        /// applied.  A level always supplies its level-number minimum score.
        /// </summary>
        public static int CalculateOriginalSinglePlayerLevelScore(MutinyTeam playerTeam, int levelIndex)
        {
            int safeLevelIndex = Mathf.Max(1, levelIndex);
            if (playerTeam == null || playerTeam.Characters == null || playerTeam.Characters.Count == 0)
                return safeLevelIndex * 10;

            float livingHealth = 0f;
            for (int i = 0; i < playerTeam.Characters.Count; i++)
            {
                MutinyCharacter character = playerTeam.Characters[i];
                if (character != null && character.IsAlive)
                    livingHealth += character.Health;
            }

            int score = Mathf.FloorToInt(
                livingHealth / playerTeam.Characters.Count * 20f - playerTeam.TotalTurnsTaken * 25f);
            return Mathf.Max(score, safeLevelIndex * 10);
        }

        /// <summary>
        /// Applies the one-player completion award once for the currently loaded
        /// level.  The guard prevents repeated GameOver observers from adding
        /// score twice while the popup remains on screen.
        /// </summary>
        public int AwardSinglePlayerLevelWin(MutinyTeam playerTeam)
        {
            if (ActiveGameMode != MutinyGameMode.SinglePlayer)
                return 0;
            if (m_AwardedLevelIndex == CurrentLevelIndex)
                return m_LastCompletedLevelScore;

            m_LastCompletedLevelScore = CalculateOriginalSinglePlayerLevelScore(playerTeam, CurrentLevelIndex);
            m_SinglePlayerScore += m_LastCompletedLevelScore;
            m_AwardedLevelIndex = CurrentLevelIndex;
            if (CurrentLevelIndex == Mutiny.Presentation.MutinyFrontendController.SinglePlayerLevelCount)
                Mutiny.Persistence.MutinySaveSystem.RecordCompletedScore(m_SinglePlayerScore);
            MutinyDebugLog.Info("Level",
                $"END-POP-01 level complete level={CurrentLevelIndex} award={m_LastCompletedLevelScore} total={m_SinglePlayerScore}", this);
            return m_LastCompletedLevelScore;
        }

        public void ResetSinglePlayerScore()
        {
            m_SinglePlayerScore = 0;
            m_LastCompletedLevelScore = 0;
            m_AwardedLevelIndex = -1;
            MutinyDebugLog.Info("Level", "END-POP-06 single-player session score reset", this);
        }

        private void ParseLevelIdentityFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (MutinyLevelId.TryParse(name, out MutinyLevelId id))
            {
                CurrentLevelIndex = id.Number;
                m_LevelMode = id.Mode;
                m_HasLevelIdentity = true;
                if (!m_HasMenuSessionMode) ConfigureSession(id.Mode);
            }
            else if (name.StartsWith("level_", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(name.Substring(6), out int original) && original >= 1 && original <= 33)
            {
                // Compatibility for old serialized/baked assets, never a runtime resource fallback.
                id = MutinyLevelId.FromOriginalNumber(original);
                CurrentLevelIndex = id.Number;
                m_LevelMode = id.Mode;
                m_HasLevelIdentity = true;
                if (!m_HasMenuSessionMode) ConfigureSession(id.Mode);
            }
        }

        [ContextMenu("Build Level")]
        public void BuildLevel()
        {
            // A legacy baked scene owns the controller and level on the same GameObject.
            // Move level ownership to a persistent host before rebuilding so ClearLevel
            // never destroys the component that is executing this method.
            if (m_CurrentLevel != null && m_CurrentLevel.gameObject == gameObject)
            {
                MigrateLegacyLevelAndBuild();
                return;
            }

            ClearLevel();

            if (m_LevelXml == null)
            {
                Debug.LogError("[MutinyLevelController] LevelXml is not assigned.");
                return;
            }

            ParseLevelIdentityFromName(m_LevelXml.name);

            try
            {
                MutinyLevelData levelData = MutinyLevelXmlParser.Parse(m_LevelXml.text, m_LevelXml.name);
                GameObject levelObj = MutinyLevelBuilder.BuildLevel(levelData, transform, OriginalLevelIndex,
                    m_HasMenuSessionMode ? (MutinyGameMode?)SessionMode : null);
                m_CurrentLevel = levelObj.GetComponent<MutinyLevelRoot>();
                BindLevelController(m_CurrentLevel, this);
                MutinyCameraController.ResetCamerasForLevel(m_CurrentLevel);
                Debug.Log($"[MutinyLevelController] Built level '{levelData.Name}': {levelData.Width}x{levelData.Height}, {m_CurrentLevel.Characters.Count} characters.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [ContextMenu("Clear Level")]
        public void ClearLevel()
        {
            MutinyMine.ClearForLevelUnload();
            // BoxWeapon instances are spawned outside the level-root hierarchy.
            // Clear both their visuals and collision state immediately; do not
            // wait for delayed OnDestroy callbacks from the old scene graph.
            MutinyBoxRegistry.ClearForLevelUnload();

            if (m_CurrentLevel != null)
            {
                // A GM switch can rebuild during the current frame. Retire the old
                // input/AI immediately, before Unity's deferred destruction.
                m_CurrentLevel.gameObject.SetActive(false);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(m_CurrentLevel.gameObject);
                else
                    Destroy(m_CurrentLevel.gameObject);
#else
                Destroy(m_CurrentLevel.gameObject);
#endif
                m_CurrentLevel = null;
            }

            // Also remove any stray children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                child.gameObject.SetActive(false);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        private void MigrateLegacyLevelAndBuild()
        {
            TextAsset xml = m_LevelXml;
            int levelIndex = CurrentLevelIndex;

            GameObject host = new GameObject("MutinyGame");
            if (transform.parent != null)
                host.transform.SetParent(transform.parent, false);

            MutinyLevelController replacement = host.AddComponent<MutinyLevelController>();
            replacement.m_LevelXml = xml;
            replacement.m_BuildOnStart = false;
            replacement.CurrentLevelIndex = levelIndex;
            replacement.m_LevelMode = m_LevelMode;
            replacement.m_HasLevelIdentity = m_HasLevelIdentity;
            replacement.SessionMode = SessionMode;
            replacement.m_HasMenuSessionMode = m_HasMenuSessionMode;
            replacement.m_Player1Wins = m_Player1Wins;
            replacement.m_Player2Wins = m_Player2Wins;
            replacement.BuildLevel();

            Destroy(gameObject);
        }

        private static void BindLevelController(MutinyLevelRoot level, MutinyLevelController controller)
        {
            if (level == null)
                return;

            var hud = level.GetComponent<Mutiny.Presentation.MutinyGameHUD>();
            if (hud != null)
                hud.LevelController = controller;
        }
    }
}
