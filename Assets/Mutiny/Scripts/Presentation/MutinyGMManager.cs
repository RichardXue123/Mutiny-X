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
        public const float ButtonSize = 60f;

        public static Rect ResolveButtonRect(float screenHeight)
        {
            return new Rect(8f, (screenHeight - ButtonSize) * 0.5f, ButtonSize, ButtonSize);
        }

        public static Rect ResolveButtonRect(float screenWidth, float screenHeight)
        {
            float scale = Mathf.Min(screenWidth / 550f, screenHeight / 400f);
            float canvasLeft = (screenWidth - 550f * scale) * 0.5f;
            float x = canvasLeft + 8f;
            float y = (screenHeight - ButtonSize) * 0.5f;
            return new Rect(x, y, ButtonSize, ButtonSize);
        }

        private bool m_IsOpen = false;
        private string m_InputText = "";
        private string m_StatusMessage = "Mutiny GM Console ready. Type 'help' for commands.";
        private Color m_StatusColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

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

        private void EnsureResources()
        {
            if (m_CircleNormalTex != null)
                return;

            const int circleSize = (int)ButtonSize;
            m_CircleNormalTex = CreateCircleTexture(circleSize, new Color(0.2f, 0.2f, 0.2f, 0.70f), new Color(0.6f, 0.6f, 0.6f, 0.85f), 3f);
            m_CircleHoverTex = CreateCircleTexture(circleSize, new Color(0.35f, 0.35f, 0.35f, 0.90f), new Color(1.0f, 0.85f, 0.3f, 1.0f), 3f);

            m_PanelBackgroundTex = CreateSolidTexture(new Color(0.08f, 0.09f, 0.12f, 0.90f));
            m_InputBackgroundTex = CreateSolidTexture(new Color(0.15f, 0.16f, 0.20f, 0.95f));
            m_ButtonBackgroundTex = CreateSolidTexture(new Color(0.24f, 0.26f, 0.34f, 0.95f));

            m_CircleButtonStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            m_CircleButtonStyle.normal.textColor = Color.white;
            m_CircleButtonStyle.hover.textColor = new Color(1f, 0.92f, 0.45f);

            m_PanelHeaderStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            m_PanelHeaderStyle.normal.textColor = new Color(0.95f, 0.85f, 0.45f);

            m_InputFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 20,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(12, 12, 8, 8)
            };
            m_InputFieldStyle.normal.background = m_InputBackgroundTex;
            m_InputFieldStyle.normal.textColor = Color.white;
            m_InputFieldStyle.focused.background = m_InputBackgroundTex;
            m_InputFieldStyle.focused.textColor = Color.white;

            m_ActionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            m_ActionButtonStyle.normal.background = m_ButtonBackgroundTex;
            m_ActionButtonStyle.normal.textColor = Color.white;

            m_StatusLabelStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 17,
                wordWrap = true
            };
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
        }

        private void OnGUI()
        {
            EnsureResources();

            // Save GUI state
            Matrix4x4 prevMatrix = GUI.matrix;
            Color prevColor = GUI.color;

            // Render in screen-space, topmost depth
            GUI.depth = -20000;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;

            // 1. Draw circular floating GM button on the left vertical center (reduced to 1/3: 60px)
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
                DrawCommandPanel(btnX + btnSize + 14f, btnY + btnSize * 0.5f);
            }

            // Restore GUI state
            GUI.color = prevColor;
            GUI.matrix = prevMatrix;
        }

        private void DrawCommandPanel(float originX, float centerY)
        {
            float panelWidth = Mathf.Min(680f, Screen.width - (originX + 15f));
            float panelHeight = Mathf.Min(360f, Screen.height - 30f);
            float panelY = Mathf.Clamp(centerY - panelHeight * 0.5f, 15f, Screen.height - panelHeight - 15f);
            Rect panelRect = new Rect(originX, panelY, panelWidth, panelHeight);

            // Semi-transparent background
            GUI.DrawTexture(panelRect, m_PanelBackgroundTex);

            // Border
            DrawOutline(panelRect, new Color(0.40f, 0.45f, 0.60f, 0.85f), 2f);

            // Inner content layout
            float padding = 16f;
            float contentWidth = panelWidth - padding * 2;

            // Header: Title & Close [X]
            float headerHeight = 32f;
            Rect headerRect = new Rect(panelRect.x + padding, panelRect.y + padding, contentWidth - 44f, headerHeight);
            GUI.Label(headerRect, "MUTINY GM CONSOLE", m_PanelHeaderStyle);

            Rect closeRect = new Rect(panelRect.xMax - padding - 36f, panelRect.y + padding, 36f, 32f);
            if (GUI.Button(closeRect, "X", m_ActionButtonStyle))
            {
                m_IsOpen = false;
            }

            // Bottom section: Hint, Input row
            float inputHeight = 46f;
            float buttonWidth = 92f;
            float hintHeight = 24f;

            float hintY = panelRect.yMax - padding - hintHeight;
            float inputY = hintY - inputHeight - 8f;

            // Status / Log Display Area (fills space between header and input)
            float statusY = headerRect.yMax + 12f;
            float statusHeight = inputY - statusY - 12f;
            Rect statusRect = new Rect(panelRect.x + padding, statusY, contentWidth, statusHeight);
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
            GUI.Label(statusRect, m_StatusMessage, m_StatusLabelStyle);

            // Input field & Submit button
            Rect inputRect = new Rect(panelRect.x + padding, inputY, contentWidth - buttonWidth - 10f, inputHeight);
            Rect runRect = new Rect(inputRect.xMax + 10f, inputY, buttonWidth, inputHeight);

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
                fontSize = 14,
                normal = { textColor = new Color(0.65f, 0.70f, 0.80f, 0.9f) }
            };
            GUI.Label(hintRect, "Tip: Type 'UnlockWeapons' or 'UnlockAllLevels' and press Enter.", hintStyle);
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

        public void ExecuteCommand(string rawCommand)
        {
            string cmd = rawCommand.Trim();
            string lower = cmd.ToLowerInvariant();

            Debug.Log($"[MutinyGM] Executing command: '{cmd}'", this);

            if (lower == "unlockalllevels" || lower == "unlockall" || lower == "unlock all")
            {
                MutinySaveSystem.HighestUnlockedLevel = MutinySaveSystem.MaxLevel;
                m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                m_StatusMessage = $"[SUCCESS] All levels unlocked! (1..{MutinySaveSystem.MaxLevel})\nHighestUnlockedLevel is now {MutinySaveSystem.HighestUnlockedLevel}.";
            }
            else if (lower == "resetlevels" || lower == "resetprogress" || lower == "lockall")
            {
                MutinySaveSystem.ResetProgress();
                m_StatusColor = new Color(1.0f, 0.85f, 0.35f);
                m_StatusMessage = $"[RESET] Level progress reset to default.\nHighestUnlockedLevel is now {MutinySaveSystem.HighestUnlockedLevel}.";
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
                                  "• UnlockAllLevels - Unlocks all 1..18 levels\n" +
                                  "• ResetLevels     - Resets progress to level 1\n" +
                                  "• aiforceusewaepon 1..15 - Forces one infinite AI weapon; 0 disables\n" +
                                  "• Help            - Shows this help message";
            }
            else
            {
                m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = $"[ERROR] Unknown command: '{cmd}'\nType 'help' to view available commands.";
            }
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
