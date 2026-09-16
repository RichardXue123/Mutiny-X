using System;
using System.Collections.Generic;
using Mutiny.Persistence;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MutinyGMManager : MonoBehaviour
    {
        public static MutinyGMManager Instance { get; private set; }

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

            m_CircleNormalTex = CreateCircleTexture(36, new Color(0.2f, 0.2f, 0.2f, 0.65f), new Color(0.6f, 0.6f, 0.6f, 0.8f));
            m_CircleHoverTex = CreateCircleTexture(36, new Color(0.35f, 0.35f, 0.35f, 0.85f), new Color(1.0f, 0.85f, 0.3f, 1.0f));

            m_PanelBackgroundTex = CreateSolidTexture(new Color(0.08f, 0.09f, 0.12f, 0.90f));
            m_InputBackgroundTex = CreateSolidTexture(new Color(0.15f, 0.16f, 0.20f, 0.95f));
            m_ButtonBackgroundTex = CreateSolidTexture(new Color(0.24f, 0.26f, 0.34f, 0.95f));

            m_CircleButtonStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            m_CircleButtonStyle.normal.textColor = Color.white;
            m_CircleButtonStyle.hover.textColor = new Color(1f, 0.92f, 0.45f);

            m_PanelHeaderStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            m_PanelHeaderStyle.normal.textColor = new Color(0.95f, 0.85f, 0.45f);

            m_InputFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(6, 6, 4, 4)
            };
            m_InputFieldStyle.normal.background = m_InputBackgroundTex;
            m_InputFieldStyle.normal.textColor = Color.white;
            m_InputFieldStyle.focused.background = m_InputBackgroundTex;
            m_InputFieldStyle.focused.textColor = Color.white;

            m_ActionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            m_ActionButtonStyle.normal.background = m_ButtonBackgroundTex;
            m_ActionButtonStyle.normal.textColor = Color.white;

            m_StatusLabelStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11,
                wordWrap = true
            };
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
        }

        private void OnGUI()
        {
            EnsureResources();

            // Save GUI state
            int prevDepth = GUI.depth;
            Matrix4x4 prevMatrix = GUI.matrix;
            Color prevColor = GUI.color;

            // Render in screen-space, topmost depth
            GUI.depth = -20000;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;

            // 1. Draw circular floating GM button on the left vertical center
            float btnSize = 36f;
            float btnX = 8f;
            float btnY = (Screen.height - btnSize) * 0.5f;
            Rect buttonRect = new Rect(btnX, btnY, btnSize, btnSize);

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
                DrawCommandPanel(btnX + btnSize + 10f, btnY);
            }

            // Restore GUI state
            GUI.color = prevColor;
            GUI.matrix = prevMatrix;
            GUI.depth = prevDepth;
        }

        private void DrawCommandPanel(float originX, float centerY)
        {
            float panelWidth = 360f;
            float panelHeight = 175f;
            float panelY = Mathf.Clamp(centerY - panelHeight * 0.4f, 15f, Screen.height - panelHeight - 15f);
            Rect panelRect = new Rect(originX, panelY, panelWidth, panelHeight);

            // Semi-transparent background
            GUI.DrawTexture(panelRect, m_PanelBackgroundTex);

            // Border
            DrawOutline(panelRect, new Color(0.35f, 0.38f, 0.50f, 0.75f), 1f);

            // Inner content layout
            float padding = 10f;
            float contentWidth = panelWidth - padding * 2;

            // Header: Title & Close [X]
            Rect headerRect = new Rect(panelRect.x + padding, panelRect.y + padding, contentWidth - 28f, 20f);
            GUI.Label(headerRect, "MUTINY GM CONSOLE", m_PanelHeaderStyle);

            Rect closeRect = new Rect(panelRect.xMax - padding - 22f, panelRect.y + padding - 2f, 22f, 22f);
            if (GUI.Button(closeRect, "X", m_ActionButtonStyle))
            {
                m_IsOpen = false;
            }

            // Status / Log Display Area
            Rect statusRect = new Rect(panelRect.x + padding, headerRect.yMax + 8f, contentWidth, 55f);
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
            GUI.Label(statusRect, m_StatusMessage, m_StatusLabelStyle);

            // Input field & Submit button
            float inputHeight = 28f;
            float buttonWidth = 56f;
            float inputY = statusRect.yMax + 10f;

            Rect inputRect = new Rect(panelRect.x + padding, inputY, contentWidth - buttonWidth - 6f, inputHeight);
            Rect runRect = new Rect(inputRect.xMax + 6f, inputY, buttonWidth, inputHeight);

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
            Rect hintRect = new Rect(panelRect.x + padding, inputY + inputHeight + 6f, contentWidth, 18f);
            var hintStyle = new GUIStyle(m_StatusLabelStyle)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.6f, 0.65f, 0.75f, 0.8f) }
            };
            GUI.Label(hintRect, "Tip: Type 'UnlockAllLevels' and press Enter.", hintStyle);
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
            else if (lower == "help" || lower == "?")
            {
                m_StatusColor = new Color(0.5f, 0.85f, 1.0f);
                m_StatusMessage = "Available GM Commands:\n" +
                                  "• UnlockAllLevels  - Unlocks all 1..18 levels\n" +
                                  "• ResetLevels      - Resets progress to level 1\n" +
                                  "• Help             - Shows this help message";
            }
            else
            {
                m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = $"[ERROR] Unknown command: '{cmd}'\nType 'help' to view available commands.";
            }
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleTexture(int size, Color fillColor, Color borderColor)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1.5f;
            float radiusSqr = radius * radius;
            float innerRadiusSqr = (radius - 2.0f) * (radius - 2.0f);

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
