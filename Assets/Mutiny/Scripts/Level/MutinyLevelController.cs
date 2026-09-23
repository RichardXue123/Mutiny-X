using System;
using Mutiny.Diagnostics;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Levels
{
    [DisallowMultipleComponent]
    public sealed class MutinyLevelController : MonoBehaviour
    {
        [SerializeField]
        private TextAsset m_LevelXml;

        [SerializeField]
        private bool m_BuildOnStart = true;

        [SerializeField]
        private MutinyLevelRoot m_CurrentLevel;

        // `_root.score` is a one-player session value in the Flash game.  It must
        // outlive a level rebuild, but it is not a saved unlock setting.
        private int m_SinglePlayerScore;
        private int m_LastCompletedLevelScore;
        private int m_AwardedLevelIndex = -1;

        public int CurrentLevelIndex { get; set; } = 1;
        public int SinglePlayerScore => m_SinglePlayerScore;
        public int LastCompletedLevelScore => m_LastCompletedLevelScore;

        public TextAsset LevelXml
        {
            get => m_LevelXml;
            set => m_LevelXml = value;
        }

        public MutinyLevelRoot CurrentLevel => m_CurrentLevel;

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
                ParseLevelIndexFromName(m_LevelXml.name);
            }
        }

        private void Start()
        {
            if (m_BuildOnStart && m_CurrentLevel == null && m_LevelXml != null)
            {
                BuildLevel();
            }
        }

        public void LoadLevel(int levelNumber)
        {
            levelNumber = Mathf.Clamp(levelNumber, 1, 18);
            CurrentLevelIndex = levelNumber;
            m_AwardedLevelIndex = -1;
            m_LastCompletedLevelScore = 0;
            string padded = levelNumber.ToString("D2");
            TextAsset xml = Resources.Load<TextAsset>($"Data/Levels/level_{padded}");
            if (xml != null)
            {
                LevelXml = xml;
                BuildLevel();
            }
            else
            {
                Debug.LogError($"[MutinyLevelController] Runtime level resource not found: level_{padded}.xml", this);
            }
        }

        public void LoadNextLevel()
        {
            LoadLevel(CurrentLevelIndex + 1);
        }

        public void RestartCurrentLevel()
        {
            MutinyDebugLog.Info("Level",
                $"restart requested level={CurrentLevelIndex} frame={Time.frameCount} currentRoot={(m_CurrentLevel != null ? m_CurrentLevel.name : "none")}",
                this);
            ResetSinglePlayerScore();
            LoadLevel(CurrentLevelIndex);
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
            if (m_AwardedLevelIndex == CurrentLevelIndex)
                return m_LastCompletedLevelScore;

            m_LastCompletedLevelScore = CalculateOriginalSinglePlayerLevelScore(playerTeam, CurrentLevelIndex);
            m_SinglePlayerScore += m_LastCompletedLevelScore;
            m_AwardedLevelIndex = CurrentLevelIndex;
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

        private void ParseLevelIndexFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            string digits = "";
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsDigit(name[i])) digits += name[i];
            }
            if (int.TryParse(digits, out int idx))
            {
                CurrentLevelIndex = idx;
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

            try
            {
                MutinyLevelData levelData = MutinyLevelXmlParser.Parse(m_LevelXml.text, m_LevelXml.name);
                GameObject levelObj = MutinyLevelBuilder.BuildLevel(levelData, transform, CurrentLevelIndex);
                m_CurrentLevel = levelObj.GetComponent<MutinyLevelRoot>();
                BindLevelController(m_CurrentLevel, this);
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
            // BoxWeapon instances are spawned outside the level-root hierarchy.
            // Clear both their visuals and collision state immediately; do not
            // wait for delayed OnDestroy callbacks from the old scene graph.
            MutinyBoxRegistry.ClearForLevelEnd();

            if (m_CurrentLevel != null)
            {
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
