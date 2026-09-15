using System;
using System.Collections.Generic;
using Mutiny.Levels;
using Mutiny.Persistence;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MutinyGameHUD : MonoBehaviour
    {
        public MutinyTurnManager TurnManager;
        public MutinyPlayerInput PlayerInput;
        public MutinyLevelController LevelController;

        private bool m_ShowLevelSelect = false;
        private GUIStyle m_TitleStyle;
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_ButtonStyle;
        private GUIStyle m_ActiveButtonStyle;
        private GUIStyle m_PanelStyle;
        private GUIStyle m_SubheaderStyle;
        private GUIStyle m_OriginalSlotStyle;
        private GUIStyle m_OriginalPanelTextStyle;
        private Texture2D m_RedWeaponPanel;
        private Texture2D m_BlueWeaponPanel;
        private Texture2D m_ThrowDisabledTexture;
        private Texture2D m_RedCancelButton;
        private Texture2D m_BlueCancelButton;
        private readonly Dictionary<string, Texture2D> m_WeaponIcons = new Dictionary<string, Texture2D>();
        private bool m_StylesInitialized = false;

        private static readonly string[] OriginalWeaponOrder =
        {
            "cherryBomb", "dynamite", "boulder", "piecesOfEight", "rumBottle",
            "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull",
            "mine", "cannon", "anchor", "voodooDoll", "tidalWave"
        };

        private void Start()
        {
            EnsureReferences();
        }

        public void EnsureReferences()
        {
            if (TurnManager == null)
                TurnManager = FindAnyObjectByType<MutinyTurnManager>();

            if (PlayerInput == null)
                PlayerInput = FindAnyObjectByType<MutinyPlayerInput>();

            if (LevelController == null)
                LevelController = FindAnyObjectByType<MutinyLevelController>();
        }

        private void InitStyles()
        {
            if (m_StylesInitialized) return;

            m_TitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            m_TitleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            m_HeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            m_HeaderStyle.normal.textColor = Color.white;

            m_SubheaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            m_SubheaderStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            m_ButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            m_ActiveButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            m_ActiveButtonStyle.normal.textColor = new Color(1f, 0.9f, 0.2f);

            m_PanelStyle = new GUIStyle(GUI.skin.box);

            m_OriginalSlotStyle = new GUIStyle(GUIStyle.none)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 9
            };
            m_OriginalSlotStyle.normal.textColor = Color.white;
            m_OriginalSlotStyle.hover.textColor = Color.yellow;

            m_OriginalPanelTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                fontSize = 10
            };
            m_OriginalPanelTextStyle.normal.textColor = Color.white;

            m_RedWeaponPanel = Resources.Load<Texture2D>("UI/weapon_select_red");
            m_BlueWeaponPanel = Resources.Load<Texture2D>("UI/weapon_select_blue");
            if (m_RedWeaponPanel != null) m_RedWeaponPanel.filterMode = FilterMode.Point;
            if (m_BlueWeaponPanel != null) m_BlueWeaponPanel.filterMode = FilterMode.Point;

            m_ThrowDisabledTexture = Resources.Load<Texture2D>("UI/button_throw_disabled");
            if (m_ThrowDisabledTexture != null) m_ThrowDisabledTexture.filterMode = FilterMode.Point;

            m_RedCancelButton = Resources.Load<Texture2D>("UI/button_cancel_red");
            if (m_RedCancelButton != null) m_RedCancelButton.filterMode = FilterMode.Point;

            m_BlueCancelButton = Resources.Load<Texture2D>("UI/button_cancel_blue");
            if (m_BlueCancelButton != null) m_BlueCancelButton.filterMode = FilterMode.Point;
            for (int i = 0; i < OriginalWeaponOrder.Length; i++)
            {
                string weaponType = OriginalWeaponOrder[i];
                m_WeaponIcons[weaponType] = Resources.Load<Texture2D>($"UI/WeaponIcons/{weaponType}");
                if (m_WeaponIcons[weaponType] != null)
                    m_WeaponIcons[weaponType].filterMode = FilterMode.Point;
            }

            m_StylesInitialized = true;
        }

        private void OnGUI()
        {
            EnsureReferences();
            InitStyles();

            DrawTopBar();
            DrawBottomBar();

            if (TurnManager != null && TurnManager.CurrentPhase == TurnPhase.GameOver)
            {
                DrawGameOverModal();
            }

            if (m_ShowLevelSelect)
            {
                DrawLevelSelectModal();
            }
        }

        private void DrawTopBar()
        {
            float barWidth = Screen.width - 20;
            float barHeight = 44;
            Rect barRect = new Rect(10, 10, barWidth, barHeight);
            GUI.Box(barRect, GUIContent.none, m_PanelStyle);

            GUILayout.BeginArea(new Rect(15, 12, barWidth - 10, barHeight - 4));
            GUILayout.BeginHorizontal();

            // 1. Current turn status
            string turnText = "MUTINY";
            Color turnColor = Color.white;
            if (TurnManager != null)
            {
                if (TurnManager.CurrentPhase == TurnPhase.TurnActive)
                {
                    if (TurnManager.CurrentTeam == TurnManager.Team1)
                    {
                        turnText = "PLAYER TURN (RED)";
                        turnColor = new Color(1f, 0.4f, 0.4f);
                    }
                    else
                    {
                        turnText = "ENEMY TURN (BLUE)";
                        turnColor = new Color(0.4f, 0.6f, 1f);
                    }
                }
                else if (TurnManager.CurrentPhase == TurnPhase.ActionExecuting || TurnManager.CurrentPhase == TurnPhase.Settling)
                {
                    turnText = "SIMULATING ACTION...";
                    turnColor = new Color(1f, 0.9f, 0.3f);
                }
                else if (TurnManager.CurrentPhase == TurnPhase.GameOver)
                {
                    turnText = "GAME OVER";
                    turnColor = Color.green;
                }
            }

            var origColor = GUI.color;
            GUI.color = turnColor;
            GUILayout.Label(turnText, m_HeaderStyle, GUILayout.Width(230));
            GUI.color = origColor;

            // 2. Team stats
            int team1Alive = TurnManager != null && TurnManager.Team1 != null ? TurnManager.Team1.AliveCount : 0;
            int team2Alive = TurnManager != null && TurnManager.Team2 != null ? TurnManager.Team2.AliveCount : 0;
            int turnNum = TurnManager != null ? TurnManager.TurnCount + 1 : 1;
            GUILayout.Label($"Turn {turnNum} | Red Crew: {team1Alive} | Blue Crew: {team2Alive}", m_SubheaderStyle, GUILayout.Width(270));

            GUILayout.FlexibleSpace();

            // 3. Audio & level controls
            string sfxText = MutinyAudioManager.Instance != null && MutinyAudioManager.Instance.SfxEnabled ? "SFX: ON" : "SFX: OFF";
            if (GUILayout.Button(sfxText, m_ButtonStyle, GUILayout.Width(75), GUILayout.Height(28)))
            {
                MutinyAudioManager.Instance?.ToggleSFX();
            }

            string musicText = MutinyAudioManager.Instance != null && MutinyAudioManager.Instance.MusicEnabled ? "Music: ON" : "Music: OFF";
            if (GUILayout.Button(musicText, m_ButtonStyle, GUILayout.Width(85), GUILayout.Height(28)))
            {
                MutinyAudioManager.Instance?.ToggleMusic();
            }

            if (GUILayout.Button("Restart", m_ButtonStyle, GUILayout.Width(70), GUILayout.Height(28)))
            {
                LevelController?.RestartCurrentLevel();
            }

            if (GUILayout.Button("Levels", m_ButtonStyle, GUILayout.Width(65), GUILayout.Height(28)))
            {
                m_ShowLevelSelect = !m_ShowLevelSelect;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawBottomBar()
        {
            if (TurnManager == null || TurnManager.CurrentTeam == null || TurnManager.CurrentTeam.IsAiControlled)
                return;

            if (PlayerInput == null || !PlayerInput.IsActionMenuOpen)
                return;

            MutinyCharacter selectedChar = TurnManager.CurrentTeam.SelectedCharacter;
            if (selectedChar == null || !selectedChar.IsAlive)
                return;

            float scale = Mathf.Min(Screen.width / 550f, Screen.height / 400f);
            m_OriginalSlotStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            m_OriginalPanelTextStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(10f * scale));
            float panelWidth = 270f * scale;
            float panelHeight = 246f * scale;
            float left = (Screen.width - panelWidth) * 0.5f;
            float top = 23f * scale;
            Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

            Texture2D panelTexture = TurnManager.CurrentTeam.TeamNumber == 2
                ? m_BlueWeaponPanel
                : m_RedWeaponPanel;
            if (panelTexture != null)
                GUI.DrawTexture(panelRect, panelTexture, ScaleMode.StretchToFill, true);
            else
                GUI.Box(panelRect, GUIContent.none, m_PanelStyle);

            Rect throwRect = ScaledRect(left, top, scale, 10f, 24f, 86f, 57f);
            if (selectedChar.CanThrow)
            {
                if (GUI.Button(throwRect, new GUIContent(string.Empty, "throw character"), m_OriginalSlotStyle))
                    PlayerInput.SelectCharacterThrow();
            }
            else
            {
                if (m_ThrowDisabledTexture != null)
                {
                    GUI.DrawTexture(throwRect, m_ThrowDisabledTexture, ScaleMode.StretchToFill, true);
                }
                else
                {
                    var origColor = GUI.color;
                    GUI.color = new Color(0.25f, 0.25f, 0.25f, 0.7f);
                    GUI.DrawTexture(throwRect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
                    GUI.color = origColor;
                }

                GUI.enabled = false;
                GUI.Button(throwRect, new GUIContent(string.Empty, "throw character used"), m_OriginalSlotStyle);
                GUI.enabled = true;
            }

            if (GUI.Button(ScaledRect(left, top, scale, 10f, 89f, 86f, 57f),
                    new GUIContent(string.Empty, "end turn"), m_OriginalSlotStyle))
                PlayerInput.EndTurn();

            if (selectedChar.CanThrow)
            {
                Texture2D cancelTex = TurnManager.CurrentTeam.TeamNumber == 2
                    ? m_BlueCancelButton
                    : m_RedCancelButton;
                Rect cancelRect = ScaledRect(left, top, scale, 250f, 0f, 20f, 20f);
                if (cancelTex != null)
                {
                    GUI.DrawTexture(cancelRect, cancelTex, ScaleMode.StretchToFill, true);
                }

                if (GUI.Button(cancelRect, new GUIContent(string.Empty, "cancel character"), m_OriginalSlotStyle))
                    PlayerInput.ReturnToCharacterSelection();
            }

            for (int i = 0; i < OriginalWeaponOrder.Length; i++)
            {
                string weaponType = OriginalWeaponOrder[i];
                int column = i % 5;
                int row = i / 5;
                Rect slot = ScaledRect(left, top, scale, 111f + column * 31f, 25f + row * 42f, 24f, 35f);
                bool available = selectedChar.CanShoot && selectedChar.HasWeapon(weaponType);
                int ammo = selectedChar.GetAmmunition(weaponType);
                string ammoText = ammo < 0 ? "∞" : ammo.ToString();
                string shortName = GetWeaponAbbreviation(weaponType);
                m_WeaponIcons.TryGetValue(weaponType, out Texture2D icon);

                if (icon != null && available)
                {
                    Rect iconRect = new Rect(
                        slot.x + 3f * scale,
                        slot.y + 3f * scale,
                        slot.width - 6f * scale,
                        slot.height - 9f * scale);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                }

                GUI.enabled = available;
                string slotText = icon != null ? $"\n{ammoText}" : $"{shortName}\n{ammoText}";
                if (GUI.Button(slot, new GUIContent(slotText, weaponType), m_OriginalSlotStyle))
                    PlayerInput.SelectWeapon(weaponType);
                GUI.enabled = true;
            }

            string description = GetOriginalActionDescription(GUI.tooltip, selectedChar);
            Rect descriptionRect = ScaledRect(left, top, scale, 13f, 164f, 244f, 69f);
            GUI.Label(descriptionRect, description, m_OriginalPanelTextStyle);
        }

        private static Rect ScaledRect(float left, float top, float scale, float x, float y, float width, float height)
        {
            return new Rect(left + x * scale, top + y * scale, width * scale, height * scale);
        }

        private static string GetWeaponAbbreviation(string weaponType)
        {
            if (weaponType == "piecesOfEight") return "8";
            if (weaponType == "gunpowderBarrel") return "GB";
            if (weaponType == "parachuteBomb") return "PB";
            if (weaponType == "woodenCrate") return "CR";
            if (weaponType == "voodooDoll") return "VD";
            if (weaponType == "tidalWave") return "TW";
            return weaponType.Substring(0, Mathf.Min(2, weaponType.Length)).ToUpperInvariant();
        }

        private static string GetOriginalActionDescription(string action, MutinyCharacter character)
        {
            if (string.IsNullOrEmpty(action))
                return $"{character.CharacterType}\nHP {character.Health:F0}/{character.MaxHealth:F0}";
            if (action == "throw character")
                return "THROW CHARACTER\nClick your character and drag with the mouse to aim and set the power.\nYou get to use this once per turn before you use a weapon.";
            if (action == "throw character used")
                return "THROW CHARACTER (USED)\nJump already used this turn. Select a weapon to attack, or click End Go.";
            if (action == "end turn")
                return "END GO\nClick here if you want to finish your turn without using a weapon.";
            if (action == "cancel character")
                return "CLOSE\nClick here to cancel and select another player.";
            return $"{action.ToUpperInvariant()}\nSelect this weapon, then use the mouse on the stage.";
        }

        private void DrawGameOverModal()
        {
            float modalWidth = 380;
            float modalHeight = 220;
            float left = (Screen.width - modalWidth) * 0.5f;
            float top = (Screen.height - modalHeight) * 0.5f;

            Rect rect = new Rect(left, top, modalWidth, modalHeight);
            GUI.Box(rect, GUIContent.none, m_PanelStyle);

            GUILayout.BeginArea(new Rect(left + 20, top + 15, modalWidth - 40, modalHeight - 30));
            GUILayout.BeginVertical();

            string title = "GAME OVER";
            Color titleColor = Color.white;

            if (TurnManager.GameResult == GameOverResult.Team1Wins)
            {
                title = "VICTORY!";
                titleColor = new Color(1f, 0.85f, 0.2f);
            }
            else if (TurnManager.GameResult == GameOverResult.Team2Wins)
            {
                title = "DEFEAT";
                titleColor = new Color(1f, 0.35f, 0.35f);
            }
            else
            {
                title = "DRAW!";
                titleColor = Color.yellow;
            }

            var origColor = GUI.color;
            GUI.color = titleColor;
            GUILayout.Label(title, m_TitleStyle);
            GUI.color = origColor;

            GUILayout.Space(10);

            string desc = TurnManager.GameResult == GameOverResult.Team1Wins
                ? "Enemy crew has been sent to Davy Jones' locker!"
                : "Your crew has been defeated!";
            GUILayout.Label(desc, m_SubheaderStyle);

            GUILayout.Space(20);

            if (TurnManager.GameResult == GameOverResult.Team1Wins)
            {
                if (GUILayout.Button("Next Level", m_ButtonStyle, GUILayout.Height(36)))
                {
                    LevelController?.LoadNextLevel();
                }
                GUILayout.Space(6);
            }

            if (GUILayout.Button("Restart Level", m_ButtonStyle, GUILayout.Height(32)))
            {
                LevelController?.RestartCurrentLevel();
            }

            GUILayout.Space(6);

            if (GUILayout.Button("Level Select", m_ButtonStyle, GUILayout.Height(28)))
            {
                m_ShowLevelSelect = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawLevelSelectModal()
        {
            float modalWidth = 460;
            float modalHeight = 320;
            float left = (Screen.width - modalWidth) * 0.5f;
            float top = (Screen.height - modalHeight) * 0.5f;

            Rect rect = new Rect(left, top, modalWidth, modalHeight);
            GUI.Box(rect, GUIContent.none, m_PanelStyle);

            GUILayout.BeginArea(new Rect(left + 15, top + 12, modalWidth - 30, modalHeight - 24));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("SELECT LEVEL", m_HeaderStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", m_ButtonStyle, GUILayout.Width(28), GUILayout.Height(24)))
            {
                m_ShowLevelSelect = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 18 levels displayed in a 6x3 grid
            for (int row = 0; row < 3; row++)
            {
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 6; col++)
                {
                    int lvl = row * 6 + col + 1;
                    bool unlocked = MutinySaveSystem.IsLevelUnlocked(lvl);

                    GUI.enabled = unlocked;
                    string label = unlocked ? $"Lvl {lvl}" : $"Lvl {lvl} LOCKED";
                    if (GUILayout.Button(label, m_ButtonStyle, GUILayout.Height(40)))
                    {
                        m_ShowLevelSelect = false;
                        LevelController?.LoadLevel(lvl);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Progress", m_ButtonStyle, GUILayout.Width(120), GUILayout.Height(26)))
            {
                MutinySaveSystem.ResetProgress();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private string FormatWeaponName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            if (raw.Equals("cherryBomb", StringComparison.OrdinalIgnoreCase)) return "Cherry Bomb";
            if (raw.Equals("piecesOfEight", StringComparison.OrdinalIgnoreCase)) return "Pieces of 8";
            if (raw.Equals("gunpowderBarrel", StringComparison.OrdinalIgnoreCase)) return "Barrel";
            if (raw.Equals("parachuteBomb", StringComparison.OrdinalIgnoreCase)) return "Parachute";
            if (raw.Equals("voodooDoll", StringComparison.OrdinalIgnoreCase)) return "Voodoo";
            if (raw.Equals("tidalWave", StringComparison.OrdinalIgnoreCase)) return "Tidal Wave";
            if (raw.Equals("woodenCrate", StringComparison.OrdinalIgnoreCase)) return "Crate";
            if (raw.Equals("rumBottle", StringComparison.OrdinalIgnoreCase)) return "Rum Bottle";

            // Capitalize first letter
            return char.ToUpper(raw[0]) + raw.Substring(1);
        }
    }
}

