using System;
using Mutiny.Diagnostics;
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

        public int CurrentLevelIndex { get; set; } = 1;

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
            LoadLevel(CurrentLevelIndex);
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
                GameObject levelObj = MutinyLevelBuilder.BuildLevel(levelData, transform);
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
