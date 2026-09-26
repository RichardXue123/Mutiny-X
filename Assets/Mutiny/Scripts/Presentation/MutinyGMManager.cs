using System;
using System.Collections.Generic;
using Mutiny.Persistence;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MutinyGMManager : MonoBehaviour
    {
        public static MutinyGMManager Instance { get; private set; }
        public const float CanvasReferenceWidth = 550f;
        public const float CanvasReferenceHeight = 400f;
        public const float BaseButtonSize = 30f;
        public const float BaseButtonMargin = 4f;
        public const int MaxRecentSuccessfulCommands = 5;

        // Kept for backward compatibility
        public const float ButtonSize = BaseButtonSize;

        public static float CalculateScale(float screenWidth, float screenHeight)
        {
            return Mathf.Min(screenWidth / CanvasReferenceWidth, screenHeight / CanvasReferenceHeight);
        }

        public static Rect ResolveButtonRect(float screenHeight)
        {
            return ResolveButtonRect(screenHeight * (CanvasReferenceWidth / CanvasReferenceHeight), screenHeight);
        }

        public static Rect ResolveButtonRect(float screenWidth, float screenHeight)
        {
            float scale = CalculateScale(screenWidth, screenHeight);
            float size = BaseButtonSize * scale;
            float canvasLeft = (screenWidth - CanvasReferenceWidth * scale) * 0.5f;
            float canvasTop = (screenHeight - CanvasReferenceHeight * scale) * 0.5f;
            float x = canvasLeft + BaseButtonMargin * scale;
            float y = canvasTop + (CanvasReferenceHeight * scale - size) * 0.5f;
            return new Rect(x, y, size, size);
        }

        private bool m_IsOpen = false;
        public bool IsOpen => m_IsOpen;
        private string m_InputText = "";
        private string m_StatusMessage = "Mutiny GM Console ready. Type 'help' for commands.";
        private Color m_StatusColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
        private readonly List<string> m_RecentSuccessfulCommands = new List<string>();
        public IReadOnlyList<string> RecentSuccessfulCommands => m_RecentSuccessfulCommands;

        // GUI Styles and Textures
        private Texture2D m_CircleNormalTex;
        private Texture2D m_CircleHoverTex;
        private Texture2D m_PanelBackgroundTex;
        private Texture2D m_InputBackgroundTex;
        private Texture2D m_ButtonBackgroundTex;

        private GUIStyle m_CircleButtonStyle;
        private GUIStyle m_PanelHeaderStyle;
        private GUIStyle m_InputFieldStyle;
        private GUIStyle m_ActionButtonStyle;
        private GUIStyle m_StatusLabelStyle;
        private GUIStyle m_RecentButtonStyle;
        private GUIStyle m_RecentHeaderStyle;

        private const string FocusControlName = "GM_Command_Input";
        private bool m_NeedsFocus = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null)
                return;

            GameObject host = new GameObject("MutinyGMManager");
            DontDestroyOnLoad(host);
            host.AddComponent<MutinyGMManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void EnsureResources(float scale)
        {
            if (m_CircleNormalTex == null)
            {
                const int circleTexResolution = 128;
                m_CircleNormalTex = CreateCircleTexture(circleTexResolution, new Color(0.2f, 0.2f, 0.2f, 0.70f), new Color(0.6f, 0.6f, 0.6f, 0.85f), 6f);
                m_CircleHoverTex = CreateCircleTexture(circleTexResolution, new Color(0.35f, 0.35f, 0.35f, 0.90f), new Color(1.0f, 0.85f, 0.3f, 1.0f), 6f);

                m_PanelBackgroundTex = CreateSolidTexture(new Color(0.08f, 0.09f, 0.12f, 0.90f));
                m_InputBackgroundTex = CreateSolidTexture(new Color(0.15f, 0.16f, 0.20f, 0.95f));
                m_ButtonBackgroundTex = CreateSolidTexture(new Color(0.24f, 0.26f, 0.34f, 0.95f));

                m_CircleButtonStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                m_CircleButtonStyle.normal.textColor = Color.white;
                m_CircleButtonStyle.hover.textColor = new Color(1f, 0.92f, 0.45f);

                m_PanelHeaderStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold
                };
                m_PanelHeaderStyle.normal.textColor = new Color(0.95f, 0.85f, 0.45f);

                m_InputFieldStyle = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };
                m_InputFieldStyle.normal.background = m_InputBackgroundTex;
                m_InputFieldStyle.normal.textColor = Color.white;
                m_InputFieldStyle.focused.background = m_InputBackgroundTex;
                m_InputFieldStyle.focused.textColor = Color.white;

                m_ActionButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                m_ActionButtonStyle.normal.background = m_ButtonBackgroundTex;
                m_ActionButtonStyle.normal.textColor = Color.white;

                m_StatusLabelStyle = new GUIStyle
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true
                };
                m_StatusLabelStyle.normal.textColor = m_StatusColor;

                m_RecentButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 4, 2, 2)
                };
                m_RecentButtonStyle.normal.background = m_ButtonBackgroundTex;
                m_RecentButtonStyle.normal.textColor = Color.white;
                m_RecentButtonStyle.hover.textColor = new Color(1f, 0.92f, 0.45f);

                m_RecentHeaderStyle = new GUIStyle(m_StatusLabelStyle)
                {
                    fontStyle = FontStyle.Bold
                };
                m_RecentHeaderStyle.normal.textColor = new Color(0.75f, 0.81f, 0.91f);
            }

            // Dynamically scale fonts and paddings relative to canvas scale
            m_CircleButtonStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(10f * scale));
            m_PanelHeaderStyle.fontSize = Mathf.Max(11, Mathf.RoundToInt(12f * scale));

            m_InputFieldStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(11f * scale));
            int padX = Mathf.Max(4, Mathf.RoundToInt(6f * scale));
            int padY = Mathf.Max(3, Mathf.RoundToInt(4f * scale));
            m_InputFieldStyle.padding = new RectOffset(padX, padX, padY, padY);

            m_ActionButtonStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(11f * scale));
            m_StatusLabelStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9.5f * scale));
            m_RecentButtonStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            m_RecentHeaderStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            m_RecentButtonStyle.padding = new RectOffset(
                Mathf.Max(4, Mathf.RoundToInt(6f * scale)), 4, 2, 2);
        }

        private void OnGUI()
        {
            float scale = CalculateScale(Screen.width, Screen.height);
            EnsureResources(scale);

            // Save GUI state
            Matrix4x4 prevMatrix = GUI.matrix;
            Color prevColor = GUI.color;

            // Render in screen-space, topmost depth
            GUI.depth = -20000;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;

            // 1. Draw circular floating GM button on the left vertical center, relative to canvas scale
            Rect buttonRect = ResolveButtonRect(Screen.width, Screen.height);
            float btnSize = buttonRect.width;
            float btnX = buttonRect.x;
            float btnY = buttonRect.y;

            bool isHovered = buttonRect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(buttonRect, isHovered ? m_CircleHoverTex : m_CircleNormalTex);
            if (GUI.Button(buttonRect, "GM", m_CircleButtonStyle))
            {
                m_IsOpen = !m_IsOpen;
                if (m_IsOpen)
                {
                    m_NeedsFocus = true;
                }
            }

            // 2. Draw command window if open
            if (m_IsOpen)
            {
                DrawCommandPanel(btnX + btnSize + 8f * scale, btnY + btnSize * 0.5f, scale);
            }

            // Restore GUI state
            GUI.color = prevColor;
            GUI.matrix = prevMatrix;
        }

        private void DrawCommandPanel(float originX, float centerY, float scale)
        {
            float panelWidth = Mathf.Min(360f * scale, Screen.width - (originX + 10f * scale));
            float panelHeight = Mathf.Min(320f * scale, Screen.height - 20f * scale);
            float panelY = Mathf.Clamp(centerY - panelHeight * 0.5f, 10f * scale, Screen.height - panelHeight - 10f * scale);
            Rect panelRect = new Rect(originX, panelY, panelWidth, panelHeight);

            // Semi-transparent background
            GUI.DrawTexture(panelRect, m_PanelBackgroundTex);

            // Border
            DrawOutline(panelRect, new Color(0.40f, 0.45f, 0.60f, 0.85f), Mathf.Max(1f, 2f * scale));

            // Inner content layout
            float padding = 10f * scale;
            float contentWidth = panelWidth - padding * 2;

            // Header: Title & Close [X]
            float headerHeight = 20f * scale;
            float closeWidth = 22f * scale;
            Rect headerRect = new Rect(panelRect.x + padding, panelRect.y + padding, contentWidth - closeWidth - 6f * scale, headerHeight);
            GUI.Label(headerRect, "MUTINY GM CONSOLE", m_PanelHeaderStyle);

            Rect closeRect = new Rect(panelRect.xMax - padding - closeWidth, panelRect.y + padding, closeWidth, headerHeight);
            if (GUI.Button(closeRect, "X", m_ActionButtonStyle))
            {
                m_IsOpen = false;
            }

            // Bottom section: Hint, Input row
            float inputHeight = 28f * scale;
            float buttonWidth = 56f * scale;
            float hintHeight = 16f * scale;

            float hintY = panelRect.yMax - padding - hintHeight;
            float inputY = hintY - inputHeight - 6f * scale;

            // Status / Log Display Area (fills space between header and input)
            float statusY = headerRect.yMax + 6f * scale;
            float statusHeight = inputY - statusY - 6f * scale;
            float recentHeight = m_RecentSuccessfulCommands.Count > 0 ? 106f * scale : 0f;
            float statusTextHeight = Mathf.Max(0f, statusHeight - recentHeight - (recentHeight > 0f ? 6f * scale : 0f));
            Rect statusRect = new Rect(panelRect.x + padding, statusY, contentWidth, statusTextHeight);
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
            GUI.Label(statusRect, m_StatusMessage, m_StatusLabelStyle);

            if (recentHeight > 0f)
            {
                Rect recentRect = new Rect(panelRect.x + padding, statusRect.yMax + 6f * scale,
                    contentWidth, recentHeight);
                DrawRecentCommands(recentRect, scale);
            }

            // Input field & Submit button
            Rect inputRect = new Rect(panelRect.x + padding, inputY, contentWidth - buttonWidth - 6f * scale, inputHeight);
            Rect runRect = new Rect(inputRect.xMax + 6f * scale, inputY, buttonWidth, inputHeight);

            GUI.SetNextControlName(FocusControlName);
            m_InputText = GUI.TextField(inputRect, m_InputText, m_InputFieldStyle);

            if (m_NeedsFocus)
            {
                GUI.FocusControl(FocusControlName);
                m_NeedsFocus = false;
            }

            // Keyboard shortcut: Press Enter to submit
            Event evt = Event.current;
            bool enterPressed = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);

            if (GUI.Button(runRect, "Run", m_ActionButtonStyle) || (enterPressed && GUI.GetNameOfFocusedControl() == FocusControlName))
            {
                if (enterPressed)
                {
                    evt.Use();
                }
                SubmitCommand();
            }

            // Quick command shortcut hint
            Rect hintRect = new Rect(panelRect.x + padding, hintY, contentWidth, hintHeight);
            var hintStyle = new GUIStyle(m_StatusLabelStyle)
            {
                fontSize = Mathf.Max(8, Mathf.RoundToInt(8f * scale)),
                normal = { textColor = new Color(0.65f, 0.70f, 0.80f, 0.9f) }
            };
            GUI.Label(hintRect, "Tip: 'setlanguage cn' / 'setlanguage en'. Type 'help' for more.", hintStyle);
        }

        private void DrawRecentCommands(Rect area, float scale)
        {
            GUI.Label(new Rect(area.x, area.y, area.width, 16f * scale),
                "RECENT SUCCESSFUL COMMANDS", m_RecentHeaderStyle);
            float buttonHeight = 26f * scale;
            float gap = 3f * scale;
            float buttonWidth = (area.width - gap) * 0.5f;
            int count = Mathf.Min(m_RecentSuccessfulCommands.Count, MaxRecentSuccessfulCommands);
            for (int i = 0; i < count; i++)
            {
                int row = i / 2;
                int column = i % 2;
                Rect buttonRect = new Rect(area.x + column * (buttonWidth + gap),
                    area.y + 20f * scale + row * (buttonHeight + gap), buttonWidth, buttonHeight);
                string command = m_RecentSuccessfulCommands[i];
                if (GUI.Button(buttonRect, command, m_RecentButtonStyle))
                {
                    RunRecentCommand(i);
                    break;
                }
            }
        }

        private void SubmitCommand()
        {
            string command = m_InputText?.Trim();
            m_InputText = "";
            m_NeedsFocus = true;

            if (string.IsNullOrEmpty(command))
                return;

            ExecuteCommand(command);
        }

        public bool RunRecentCommand(int index)
        {
            if (index < 0 || index >= m_RecentSuccessfulCommands.Count)
                return false;
            return ExecuteCommand(m_RecentSuccessfulCommands[index]);
        }

        public bool ExecuteCommand(string rawCommand)
        {
            string cmd = rawCommand.Trim();
            string lower = cmd.ToLowerInvariant();
            bool succeeded = false;

            Debug.Log($"[MutinyGM] Executing command: '{cmd}'", this);

            if (lower.StartsWith("setlanguage", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string code = parts.Length == 2 && string.Equals(parts[0], "setlanguage", StringComparison.OrdinalIgnoreCase)
                    ? string.Equals(parts[1], "cn", StringComparison.OrdinalIgnoreCase) ? MutinyLocalization.SimplifiedChinese
                    : string.Equals(parts[1], "en", StringComparison.OrdinalIgnoreCase) ? MutinyLocalization.English : null
                    : null;
                if (code == null)
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: setlanguage cn | en";
                }
                else
                {
                    MutinyLocalization.Initialize(this);
                    MutinyLocalization.Select(code);
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = "[SUCCESS] Language set to " +
                        (code == MutinyLocalization.SimplifiedChinese ? "Simplified Chinese (zh-Hans)." : "English (en).") +
                        (MutinyLocalization.IsReady ? string.Empty : "\nLoading language tables...");
                    succeeded = true;
                }
            }
            else if (lower == "unlockalllevels" || lower == "unlockall" || lower == "unlock all")
            {
                MutinySaveSystem.HighestUnlockedLevel = MutinySaveSystem.MaxLevel;
                m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                m_StatusMessage = $"[SUCCESS] All levels unlocked! (1..{MutinySaveSystem.MaxLevel})\nHighestUnlockedLevel is now {MutinySaveSystem.HighestUnlockedLevel}.";
                succeeded = true;
            }
            else if (lower == "resetlevels" || lower == "resetprogress" || lower == "lockall")
            {
                MutinySaveSystem.ResetProgress();
                m_StatusColor = new Color(1.0f, 0.85f, 0.35f);
                m_StatusMessage = $"[RESET] Level progress reset to default.\nHighestUnlockedLevel is now {MutinySaveSystem.HighestUnlockedLevel}.";
                succeeded = true;
            }
            else if (lower.StartsWith("aitakeover", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string error = "Usage: aitakeover 1 (current human turn only).";
                MutinyTurnManager manager = FindAnyObjectByType<MutinyTurnManager>();
                if (parts.Length == 2 &&
                    string.Equals(parts[0], "aitakeover", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[1], out int turns) && turns == 1 && manager != null)
                    succeeded = manager.TryTakeOverCurrentPlayerTurn(out error);
                m_StatusColor = succeeded ? new Color(0.35f, 1.0f, 0.45f) : new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = succeeded
                    ? "[SUCCESS] AI controls the rest of this turn, including the weapon stage after a jump. Player control returns next turn."
                    : $"[ERROR] {error}";
            }
            else if (lower.StartsWith("aiforceusewaepon", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !string.Equals(parts[0], "aiforceusewaepon", StringComparison.OrdinalIgnoreCase) ||
                    !int.TryParse(parts[1], out int weaponId) ||
                    !MutinyAIController.TrySetForcedWeaponId(weaponId))
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: aiforceusewaepon {weaponid} (0..15).";
                }
                else
                {
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = weaponId == 0
                        ? "[SUCCESS] AI weapon override disabled; actual inventory restored."
                        : $"[SUCCESS] All AI teams now consider only infinite weapon {weaponId} ({MutinyAIController.ForcedWeaponType}). Jump and pass remain available.";
                    succeeded = true;
                }
            }
            else if (lower == "unlockweapons" || lower == "unlockallweapons" || lower == "infiniteweapons" ||
                     lower == "allweapons" || lower == "weapons" || lower == "解锁武器" || lower == "无限武器" ||
                     lower.StartsWith("unlockweapon") || lower.StartsWith("infiniteweapon"))
            {
                bool targetAll = lower.Contains("all") && (lower.Contains("team") || lower.Contains("player"));
                if (targetAll)
                {
                    MutinyCharacter[] allChars = FindObjectsByType<MutinyCharacter>();
                    int unlockedCount = 0;
                    for (int i = 0; i < allChars.Length; i++)
                    {
                        MutinyCharacter c = allChars[i];
                        if (c.IsAlive && c.TeamIndex == 1)
                        {
                            c.UnlockAllWeapons(infinite: true);
                            unlockedCount++;
                        }
                    }
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = $"[SUCCESS] Unlocked all 15 weapons (infinite ammo) for {unlockedCount} characters on Team 1!";
                    succeeded = true;
                }
                else
                {
                    MutinyCharacter target = FindTargetCharacter(out string detail);
                    if (target != null)
                    {
                        target.UnlockAllWeapons(infinite: true);

                        MutinyTurnManager turnManager = FindAnyObjectByType<MutinyTurnManager>();
                        if (turnManager != null && turnManager.CurrentTeam != null && turnManager.CurrentTeam.Characters.Contains(target))
                        {
                            if (turnManager.CurrentTeam.SelectedCharacter != target)
                                turnManager.CurrentTeam.SelectCharacter(target);
                        }

                        m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                        m_StatusMessage = $"[SUCCESS] All 15 weapons unlocked (infinite ammo) for {detail}!";
                        succeeded = true;
                    }
                    else
                    {
                        m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                        m_StatusMessage = "[ERROR] No alive character found in the scene.\nPlease enter a game level first.";
                    }
                }
            }
            else if (lower == "help" || lower == "?")
            {
                m_StatusColor = new Color(0.5f, 0.85f, 1.0f);
                m_StatusMessage = "Available GM Commands:\n" +
                                  "• UnlockWeapons   - Unlocks all 15 weapons (infinite ammo) for current character\n" +
                                  $"• unlockalllevels - Unlocks all 1..{MutinySaveSystem.MaxLevel} levels\n" +
                                  "• ResetLevels     - Resets progress to level 1\n" +
                                  "• aiforceusewaepon 1..15 - Forces one infinite AI weapon; 0 disables\n" +
                                  "• aitakeover 1    - AI plays the current human turn (also after a jump)\n" +
                                  "• setlanguage cn / en - Selects Simplified Chinese / English\n" +
                                  "• Help            - Shows this help message";
                succeeded = true;
            }
            else
            {
                m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = $"[ERROR] Unknown command: '{cmd}'\nType 'help' to view available commands.";
            }

            if (succeeded)
            {
                m_RecentSuccessfulCommands.Insert(0, cmd);
                if (m_RecentSuccessfulCommands.Count > MaxRecentSuccessfulCommands)
                    m_RecentSuccessfulCommands.RemoveAt(MaxRecentSuccessfulCommands);
            }
            return succeeded;
        }

        private MutinyCharacter FindTargetCharacter(out string detail)
        {
            MutinyTurnManager turnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (turnManager != null)
            {
                if (turnManager.CurrentTeam != null && turnManager.CurrentTeam.SelectedCharacter != null && turnManager.CurrentTeam.SelectedCharacter.IsAlive)
                {
                    detail = $"active selected character '{turnManager.CurrentTeam.SelectedCharacter.name}' (Team {turnManager.CurrentTeam.TeamNumber})";
                    return turnManager.CurrentTeam.SelectedCharacter;
                }

                if (turnManager.Team1 != null && turnManager.Team1.SelectedCharacter != null && turnManager.Team1.SelectedCharacter.IsAlive)
                {
                    detail = $"Team 1 selected character '{turnManager.Team1.SelectedCharacter.name}'";
                    return turnManager.Team1.SelectedCharacter;
                }

                if (turnManager.CurrentTeam != null)
                {
                    MutinyCharacter firstAlive = turnManager.CurrentTeam.Characters.Find(c => c != null && c.IsAlive);
                    if (firstAlive != null)
                    {
                        detail = $"current team's character '{firstAlive.name}' (Team {turnManager.CurrentTeam.TeamNumber})";
                        return firstAlive;
                    }
                }

                if (turnManager.Team1 != null)
                {
                    MutinyCharacter firstAlive = turnManager.Team1.Characters.Find(c => c != null && c.IsAlive);
                    if (firstAlive != null)
                    {
                        detail = $"player character '{firstAlive.name}' (Team 1)";
                        return firstAlive;
                    }
                }
            }

            MutinyCharacter[] allChars = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < allChars.Length; i++)
            {
                MutinyCharacter c = allChars[i];
                if (c.IsSelected && c.IsAlive)
                {
                    detail = $"selected character '{c.name}' (Team {c.TeamIndex})";
                    return c;
                }
            }

            for (int i = 0; i < allChars.Length; i++)
            {
                MutinyCharacter c = allChars[i];
                if (c.TeamIndex == 1 && c.IsAlive)
                {
                    detail = $"player character '{c.name}' (Team 1)";
                    return c;
                }
            }

            for (int i = 0; i < allChars.Length; i++)
            {
                MutinyCharacter c = allChars[i];
                if (c.IsAlive)
                {
                    detail = $"character '{c.name}' (Team {c.TeamIndex})";
                    return c;
                }
            }

            detail = null;
            return null;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleTexture(int size, Color fillColor, Color borderColor, float borderThickness = 6f)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 2.5f;
            float radiusSqr = radius * radius;
            float innerRadius = Mathf.Max(0f, radius - borderThickness);
            float innerRadiusSqr = innerRadius * innerRadius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float distSqr = dx * dx + dy * dy;

                    if (distSqr > radiusSqr + 2f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (distSqr > radiusSqr)
                    {
                        // Anti-aliased outer edge
                        float t = Mathf.InverseLerp(radiusSqr + 2f, radiusSqr, distSqr);
                        tex.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * t));
                    }
                    else if (distSqr > innerRadiusSqr)
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            Texture2D tex = Texture2D.whiteTexture;
            Color prev = GUI.color;
            GUI.color = color;
            // Top, Bottom, Left, Right
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), tex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), tex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), tex);
            GUI.color = prev;
        }
    }
}
