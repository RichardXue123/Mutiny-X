using System;
using System.Globalization;
using System.Text;
using Mutiny.Levels;
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

        public enum CommandCategory { Levels, AI, Weapons, Camera, System }

        private sealed class CommandDefinition
        {
            public readonly string Name;
            public readonly CommandCategory Category;
            public readonly string Label;
            public readonly string Template;
            public readonly string Description;
            public readonly string ShortAlias;

            public CommandDefinition(string name, CommandCategory category, string label, bool requiresParameter,
                string description, string shortAlias = null)
            {
                Name = name;
                Category = category;
                Label = label;
                Template = name + (requiresParameter ? " " : string.Empty);
                Description = description;
                ShortAlias = shortAlias;
            }
        }

        private static readonly CommandDefinition[] Commands =
        {
            new CommandDefinition("enterlevel", CommandCategory.Levels, "enterlevel {level_id}", true,
                "进入指定关卡。纯数字为单人；1_XX、2_XX 可指定模式；关卡资源必须存在。", "level"),
            new CommandDefinition("unlockalllevels", CommandCategory.Levels, "unlockalllevels", false,
                "解锁全部关卡并保存最高解锁进度。", "unlocklevels"),
            new CommandDefinition("resetlevels", CommandCategory.Levels, "resetlevels", false,
                "将关卡解锁进度重置为第 1 关；不清除分数和音频设置。"),
            new CommandDefinition("aienhance", CommandCategory.AI, "aienhance {flag}", true,
                "AI 效果模拟开关：1 启用增强策略，0 恢复原兼容策略。"),
            new CommandDefinition("aisetluck", CommandCategory.AI, "aisetluck {luck}", true,
                "设置当前单人关卡敌方 AI 的 Luck，范围 0–99999；0 也是有效值。", "ailuck"),
            new CommandDefinition("airesetluck", CommandCategory.AI, "airesetluck", false,
                "清除当前单人关卡的 AI Luck 覆盖，恢复每名敌人的默认值。"),
            new CommandDefinition("aiweapon", CommandCategory.AI, "aiweapon {weapon_id}", true,
                "强制 AI 只考虑指定编号的无限武器：1–15；输入 0 关闭覆盖。"),
            new CommandDefinition("aitakeover", CommandCategory.AI, "aitakeover {luck}", true,
                "以指定 Luck 接管当前人类队伍本回合剩余行动，范围 0–99999。"),
            new CommandDefinition("ailog", CommandCategory.AI, "ailog {flag}", true,
                "AI 行动详细日志：1 开启，0 关闭；日志写入 Player.log。"),
            new CommandDefinition("unlockweapons", CommandCategory.Weapons, "unlockweapons", false,
                "按照 GM 目标优先级，为一名存活角色解锁 15 种无限弹药武器。"),
            new CommandDefinition("unlockweaponsallteam", CommandCategory.Weapons, "unlockweaponsallteam", false,
                "为所有存活的 Team 1 角色解锁 15 种无限弹药武器。", "unlockteamweapons"),
            new CommandDefinition("excamera", CommandCategory.Camera, "excamera {flag}", true,
                "爆炸击退运镜：1 开启；0 或不带参数关闭。", "blastcam"),
            new CommandDefinition("lang", CommandCategory.System, "lang {code}", true,
                "切换界面语言：en 英文、zh-cn 简中、zh-hk 香港繁中。"),
            new CommandDefinition("help", CommandCategory.System, "help", false,
                "显示全部 GM 命令、简写和说明。")
        };

        private static readonly string[] CategoryLabels = { "关卡", "AI", "武器", "镜头", "系统" };

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
        private bool m_LockOpen = true;
        public bool LockOpen { get => m_LockOpen; set => m_LockOpen = value; }
        public bool IsPinned => m_LockOpen;
        private string m_InputText = "";
        private string m_StatusMessage = "Mutiny GM Console ready. Type 'help' for commands.";
        private Color m_StatusColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
        private CommandCategory m_SelectedCategory = CommandCategory.Levels;
        private string m_SelectedDescription = "";
        private Vector2 m_CommandScroll;
        private Vector2 m_OutputScroll;
        private bool m_FocusCaretAtEnd;
        public string InputText => m_InputText;
        public CommandCategory SelectedCategory => m_SelectedCategory;

        // GUI Styles and Textures
        private Texture2D m_CircleNormalTex;
        private Texture2D m_CircleHoverTex;
        private Texture2D m_PanelBackgroundTex;
        private Texture2D m_InputBackgroundTex;
        private Texture2D m_ButtonBackgroundTex;
        private Texture2D m_LockActiveBackgroundTex;
        private Texture2D m_LockLockedTex;
        private Texture2D m_LockUnlockedTex;

        private GUIStyle m_CircleButtonStyle;
        private GUIStyle m_PanelHeaderStyle;
        private GUIStyle m_InputFieldStyle;
        private GUIStyle m_ActionButtonStyle;
        private GUIStyle m_LockActionButtonStyle;
        private GUIStyle m_LockActiveButtonStyle;
        private GUIStyle m_StatusLabelStyle;
        private GUIStyle m_CommandButtonStyle;
        private GUIStyle m_TabStyle;
        private GUIStyle m_ActiveTabStyle;
        private GUIStyle m_TooltipStyle;
        private GUIStyle m_DescriptionStyle;
        private Font m_ChineseFont;

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
                m_LockActiveBackgroundTex = CreateSolidTexture(new Color(0.20f, 0.38f, 0.30f, 0.95f));

                m_LockLockedTex = CreateLockTexture(64, true, new Color(1.0f, 0.86f, 0.35f, 1.0f));
                m_LockUnlockedTex = CreateLockTexture(64, false, new Color(0.72f, 0.76f, 0.84f, 0.90f));

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

                m_LockActionButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageOnly
                };
                m_LockActionButtonStyle.normal.background = m_ButtonBackgroundTex;

                m_LockActiveButtonStyle = new GUIStyle(m_LockActionButtonStyle);
                m_LockActiveButtonStyle.normal.background = m_LockActiveBackgroundTex;

                m_StatusLabelStyle = new GUIStyle
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true
                };
                m_StatusLabelStyle.normal.textColor = m_StatusColor;

                m_CommandButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 4, 2, 2)
                };
                m_CommandButtonStyle.normal.background = m_ButtonBackgroundTex;
                m_CommandButtonStyle.normal.textColor = Color.white;
                m_CommandButtonStyle.hover.textColor = new Color(1f, 0.92f, 0.45f);

                m_ChineseFont = Resources.Load<Font>(MutinyLocalizedText.CjkFontResource(MutinyLocalization.SimplifiedChinese));
                m_StatusLabelStyle.font = m_ChineseFont;
                m_DescriptionStyle = new GUIStyle(m_StatusLabelStyle);
                m_DescriptionStyle.normal.textColor = new Color(0.82f, 0.88f, 0.95f);
                m_TabStyle = new GUIStyle(m_CommandButtonStyle)
                {
                    alignment = TextAnchor.MiddleCenter,
                    font = m_ChineseFont,
                    fontStyle = FontStyle.Bold
                };
                m_ActiveTabStyle = new GUIStyle(m_TabStyle);
                m_ActiveTabStyle.normal.background = m_LockActiveBackgroundTex;
                m_TooltipStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    padding = new RectOffset(6, 6, 5, 5)
                };
                m_TooltipStyle.normal.background = m_PanelBackgroundTex;
                m_TooltipStyle.normal.textColor = Color.white;
                m_TooltipStyle.font = m_ChineseFont;
            }

            // Dynamically scale fonts and paddings relative to canvas scale
            m_CircleButtonStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(10f * scale));
            m_PanelHeaderStyle.fontSize = Mathf.Max(11, Mathf.RoundToInt(12f * scale));

            m_InputFieldStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(11f * scale));
            int padX = Mathf.Max(4, Mathf.RoundToInt(6f * scale));
            int padY = Mathf.Max(3, Mathf.RoundToInt(4f * scale));
            m_InputFieldStyle.padding = new RectOffset(padX, padX, padY, padY);

            m_ActionButtonStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(11f * scale));
            int lockPad = Mathf.Max(2, Mathf.RoundToInt(2.5f * scale));
            m_LockActionButtonStyle.padding = new RectOffset(lockPad, lockPad, lockPad, lockPad);
            m_LockActiveButtonStyle.padding = new RectOffset(lockPad, lockPad, lockPad, lockPad);

            m_StatusLabelStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9.5f * scale));
            m_DescriptionStyle.fontSize = m_StatusLabelStyle.fontSize;
            m_CommandButtonStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            m_TabStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(10f * scale));
            m_ActiveTabStyle.fontSize = m_TabStyle.fontSize;
            m_TooltipStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            m_CommandButtonStyle.padding = new RectOffset(
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
            float panelWidth = Mathf.Min(450f * scale, Screen.width - (originX + 10f * scale));
            float panelHeight = Mathf.Min(370f * scale, Screen.height - 20f * scale);
            float panelY = Mathf.Clamp(centerY - panelHeight * 0.5f, 10f * scale, Screen.height - panelHeight - 10f * scale);
            Rect panelRect = new Rect(originX, panelY, panelWidth, panelHeight);

            // Semi-transparent background
            GUI.DrawTexture(panelRect, m_PanelBackgroundTex);

            // Border
            DrawOutline(panelRect, new Color(0.40f, 0.45f, 0.60f, 0.85f), Mathf.Max(1f, 2f * scale));

            // Inner content layout
            float padding = 10f * scale;
            float contentWidth = panelWidth - padding * 2;

            // Header: Title, Lock [Logo], Close [X]
            float headerHeight = 20f * scale;
            float closeWidth = 22f * scale;
            float lockWidth = 22f * scale;
            float btnGap = 4f * scale;

            // Close button is at the far right; Lock button is placed to the left of Close button
            Rect closeRect = new Rect(panelRect.xMax - padding - closeWidth, panelRect.y + padding, closeWidth, headerHeight);
            Rect lockRect = new Rect(closeRect.x - btnGap - lockWidth, panelRect.y + padding, lockWidth, headerHeight);
            Rect headerRect = new Rect(panelRect.x + padding, panelRect.y + padding, contentWidth - closeWidth - lockWidth - btnGap - 6f * scale, headerHeight);

            GUI.Label(headerRect, "MUTINY GM CONSOLE", m_PanelHeaderStyle);

            Texture2D lockTex = m_LockOpen ? m_LockLockedTex : m_LockUnlockedTex;
            GUIStyle lockStyle = m_LockOpen ? m_LockActiveButtonStyle : m_LockActionButtonStyle;
            string lockTooltip = m_LockOpen ? "已固定：执行后保持面板打开" : "未固定：执行后自动关闭面板";
            if (GUI.Button(lockRect, new GUIContent(lockTex, lockTooltip), lockStyle))
            {
                m_LockOpen = !m_LockOpen;
            }

            if (GUI.Button(closeRect, "X", m_ActionButtonStyle))
            {
                m_IsOpen = false;
            }

            float inputHeight = 28f * scale;
            float buttonWidth = 56f * scale;
            float outputHeight = 98f * scale;
            float outputTitleHeight = 14f * scale;
            float outputY = panelRect.yMax - padding - outputHeight;
            float inputY = outputY - outputTitleHeight - inputHeight - 8f * scale;
            Rect commandArea = new Rect(panelRect.x + padding, headerRect.yMax + 8f * scale,
                contentWidth, inputY - headerRect.yMax - 16f * scale);

            // Input field & Submit button
            Rect inputRect = new Rect(panelRect.x + padding, inputY, contentWidth - buttonWidth - 6f * scale, inputHeight);
            Rect runRect = new Rect(inputRect.xMax + 6f * scale, inputY, buttonWidth, inputHeight);

            GUI.SetNextControlName(FocusControlName);
            m_InputText = GUI.TextField(inputRect, m_InputText, m_InputFieldStyle);

            if (m_NeedsFocus)
            {
                GUI.FocusControl(FocusControlName);
                if (m_FocusCaretAtEnd)
                {
                    TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    editor.cursorIndex = m_InputText.Length;
                    editor.selectIndex = m_InputText.Length;
                    m_FocusCaretAtEnd = false;
                }
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

            // Process template clicks after the text field so its old editor state cannot overwrite the trailing space.
            DrawCommandCategories(commandArea, scale);

            GUI.Label(new Rect(panelRect.x + padding, inputY + inputHeight + 3f * scale,
                contentWidth, outputTitleHeight), "OUTPUT", m_PanelHeaderStyle);
            Rect outputRect = new Rect(panelRect.x + padding, outputY, contentWidth, outputHeight);
            GUI.DrawTexture(outputRect, m_InputBackgroundTex);
            DrawOutline(outputRect, new Color(0.32f, 0.36f, 0.45f, 0.8f), Mathf.Max(1f, scale));
            float outputPadding = 6f * scale;
            float outputTextWidth = Mathf.Max(20f, outputRect.width - 24f * scale);
            float outputTextHeight = Mathf.Max(outputRect.height - outputPadding * 2f,
                m_StatusLabelStyle.CalcHeight(new GUIContent(m_StatusMessage), outputTextWidth) + outputPadding);
            m_OutputScroll = GUI.BeginScrollView(outputRect, m_OutputScroll,
                new Rect(0f, 0f, outputTextWidth + outputPadding, outputTextHeight));
            m_StatusLabelStyle.normal.textColor = m_StatusColor;
            GUI.Label(new Rect(outputPadding, outputPadding, outputTextWidth,
                outputTextHeight - outputPadding), m_StatusMessage, m_StatusLabelStyle);
            GUI.EndScrollView();

            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                float tooltipWidth = Mathf.Min(260f * scale, panelRect.width - padding * 2f);
                float tooltipHeight = m_TooltipStyle.CalcHeight(new GUIContent(GUI.tooltip), tooltipWidth);
                Vector2 mouse = Event.current.mousePosition;
                Rect tooltipRect = new Rect(
                    Mathf.Clamp(mouse.x + 12f * scale, panelRect.x + padding, panelRect.xMax - padding - tooltipWidth),
                    Mathf.Clamp(mouse.y + 12f * scale, panelRect.y + padding, panelRect.yMax - padding - tooltipHeight),
                    tooltipWidth, tooltipHeight);
                GUI.Label(tooltipRect, GUI.tooltip, m_TooltipStyle);
            }
        }

        private void DrawCommandCategories(Rect area, float scale)
        {
            float tabWidth = 76f * scale;
            float tabHeight = 27f * scale;
            float gap = 5f * scale;
            for (int i = 0; i < CategoryLabels.Length; i++)
            {
                CommandCategory category = (CommandCategory)i;
                Rect tabRect = new Rect(area.x, area.y + i * (tabHeight + 3f * scale), tabWidth, tabHeight);
                if (GUI.Button(tabRect, CategoryLabels[i], m_SelectedCategory == category ? m_ActiveTabStyle : m_TabStyle))
                    SelectCategory(category);
            }

            Rect rightArea = new Rect(area.x + tabWidth + gap, area.y,
                area.width - tabWidth - gap, area.height);
            float descriptionHeight = 40f * scale;
            Rect listRect = new Rect(rightArea.x, rightArea.y, rightArea.width,
                Mathf.Max(20f * scale, rightArea.height - descriptionHeight - gap));
            float buttonHeight = 26f * scale;
            int count = 0;
            foreach (CommandDefinition definition in Commands)
                if (definition.Category == m_SelectedCategory) count++;
            float listHeight = Mathf.Max(listRect.height, count * (buttonHeight + 3f * scale));
            float buttonWidth = listRect.width - (listHeight > listRect.height ? 18f * scale : 2f * scale);
            m_CommandScroll = GUI.BeginScrollView(listRect, m_CommandScroll,
                new Rect(0f, 0f, listRect.width - 2f * scale, listHeight));
            int row = 0;
            foreach (CommandDefinition definition in Commands)
            {
                if (definition.Category != m_SelectedCategory) continue;
                Rect buttonRect = new Rect(0f, row * (buttonHeight + 3f * scale), buttonWidth, buttonHeight);
                if (GUI.Button(buttonRect, new GUIContent(definition.Label, definition.Description), m_CommandButtonStyle))
                    ChooseCommandTemplate(definition.Name);
                row++;
            }
            GUI.EndScrollView();
            GUI.Label(new Rect(rightArea.x, listRect.yMax + gap, rightArea.width, descriptionHeight),
                m_SelectedDescription, m_DescriptionStyle);
        }

        public void SelectCategory(CommandCategory category)
        {
            if (!Enum.IsDefined(typeof(CommandCategory), category) || m_SelectedCategory == category) return;
            m_SelectedCategory = category;
            m_CommandScroll = Vector2.zero;
            m_SelectedDescription = "";
        }

        public bool ChooseCommandTemplate(string commandName)
        {
            foreach (CommandDefinition definition in Commands)
            {
                if (!string.Equals(definition.Name, commandName, StringComparison.OrdinalIgnoreCase)) continue;
                m_InputText = definition.Template;
                m_SelectedDescription = definition.Description;
                m_NeedsFocus = true;
                m_FocusCaretAtEnd = true;
                return true;
            }
            return false;
        }

        public string GetCommandDescription(string commandName)
        {
            foreach (CommandDefinition definition in Commands)
                if (string.Equals(definition.Name, commandName, StringComparison.OrdinalIgnoreCase))
                    return definition.Description;
            return null;
        }

        private static string BuildHelpMessage()
        {
            var help = new StringBuilder("可用 GM 命令：");
            foreach (CommandDefinition definition in Commands)
            {
                help.Append('\n').Append("• ").Append(definition.Label);
                if (!string.IsNullOrEmpty(definition.ShortAlias))
                    help.Append("（简写：").Append(definition.ShortAlias).Append('）');
                help.Append(" — ").Append(definition.Description);
            }
            return help.ToString();
        }

        private void SubmitCommand()
        {
            string command = m_InputText?.Trim();
            m_InputText = "";
            m_NeedsFocus = true;

            if (string.IsNullOrEmpty(command))
                return;

            ExecuteCommand(command);
            if (!m_LockOpen)
            {
                m_IsOpen = false;
            }
        }

        public bool ExecuteCommand(string rawCommand)
        {
            string cmd = rawCommand.Trim();
            int separator = cmd.IndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            string commandWord = separator < 0 ? cmd : cmd.Substring(0, separator);
            foreach (CommandDefinition definition in Commands)
            {
                if (!string.Equals(definition.ShortAlias, commandWord, StringComparison.OrdinalIgnoreCase)) continue;
                cmd = definition.Name + cmd.Substring(commandWord.Length);
                break;
            }
            string lower = cmd.ToLowerInvariant();
            bool succeeded = false;

            Debug.Log($"[MutinyGM] Executing command: '{cmd}'", this);

            if (lower.StartsWith("enterlevel", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string error = "Usage: enterlevel <number | 1_XX | 2_XX> (bare number = single player).";
                if (parts.Length == 2 && string.Equals(parts[0], "enterlevel", StringComparison.OrdinalIgnoreCase) &&
                    MutinyLevelId.TryParse(parts[1], out MutinyLevelId id))
                {
                    MutinyFrontendController frontend = FindAnyObjectByType<MutinyFrontendController>();
                    if (frontend != null)
                        succeeded = frontend.TryEnterLevelFromGM(id, out error);
                    else
                        error = "The gameplay front-end is not available.";
                }
                m_StatusColor = succeeded ? new Color(0.35f, 1.0f, 0.45f) : new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = succeeded ? $"[SUCCESS] Entered {parts[1]}." : $"[ERROR] {error}";
            }
            else if (lower.StartsWith("lang", StringComparison.Ordinal) || lower.StartsWith("setlanguage", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string code = parts.Length == 2 &&
                    (string.Equals(parts[0], "lang", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(parts[0], "setlanguage", StringComparison.OrdinalIgnoreCase))
                    ? string.Equals(parts[1], "zh-cn", StringComparison.OrdinalIgnoreCase) ? MutinyLocalization.SimplifiedChinese
                    : string.Equals(parts[1], "zh-hk", StringComparison.OrdinalIgnoreCase) ? MutinyLocalization.TraditionalChineseHongKong
                    : string.Equals(parts[1], "en", StringComparison.OrdinalIgnoreCase) ? MutinyLocalization.English : null
                    : null;
                if (code == null)
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: lang en | zh-cn | zh-hk";
                }
                else
                {
                    MutinyLocalization.Initialize(this);
                    MutinyLocalization.Select(code);
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = "[SUCCESS] Language set to " +
                        (code == MutinyLocalization.SimplifiedChinese ? "Simplified Chinese (zh-cn)." :
                         code == MutinyLocalization.TraditionalChineseHongKong ? "Traditional Chinese - Hong Kong (zh-hk)." : "English (en).") +
                        (MutinyLocalization.IsReady ? string.Empty : "\nBundled translations unavailable; using English fallback.");
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
            else if (lower.StartsWith("aitakeover", StringComparison.Ordinal) || lower.StartsWith("aitakeoverwithluck", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string error = "Usage: aitakeover {luck} (0..99999, current human turn only).";
                float luck = 0f;
                MutinyTurnManager manager = FindAnyObjectByType<MutinyTurnManager>();
                if (parts.Length == 2 &&
                    (string.Equals(parts[0], "aitakeover", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(parts[0], "aitakeoverwithluck", StringComparison.OrdinalIgnoreCase)) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out luck) &&
                    manager != null)
                    succeeded = manager.TryTakeOverCurrentPlayerTurn(luck, out error);
                m_StatusColor = succeeded ? new Color(0.35f, 1.0f, 0.45f) : new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = succeeded
                    ? "[SUCCESS] AI controls the rest of this turn with Luck " +
                      luck.ToString("0.###", CultureInfo.InvariantCulture) + ". Player control returns next turn."
                    : $"[ERROR] {error}";
            }
            else if (lower.StartsWith("aisetluck", StringComparison.Ordinal) ||
                     lower.StartsWith("airesetluck", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                bool reset = parts.Length == 1 && string.Equals(parts[0], "airesetluck", StringComparison.OrdinalIgnoreCase);
                float luck = 0f;
                bool set = parts.Length == 2 && string.Equals(parts[0], "aisetluck", StringComparison.OrdinalIgnoreCase) &&
                           float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out luck);
                string error = "Usage: aisetluck {luck} (0..99999) | airesetluck (single-player level only).";
                MutinyLevelController controller = FindAnyObjectByType<MutinyLevelController>();
                if ((reset || set) && controller != null)
                    succeeded = controller.TrySetCurrentAiLuck(reset ? (float?)null : luck, out error);
                m_StatusColor = succeeded ? new Color(0.35f, 1.0f, 0.45f) : new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = succeeded
                    ? reset ? "[SUCCESS] Current level AI Luck restored to each character's default."
                            : "[SUCCESS] Current level AI Luck set to " + luck.ToString("0.###", CultureInfo.InvariantCulture) + "."
                    : $"[ERROR] {error}";
            }
            else if (lower.StartsWith("excamera", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1 || parts.Length > 2 ||
                    !string.Equals(parts[0], "excamera", StringComparison.OrdinalIgnoreCase) ||
                    (parts.Length == 2 && parts[1] != "0" && parts[1] != "1"))
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: excamera 1 (on) | excamera (off).";
                }
                else
                {
                    bool enabled = parts.Length == 2 && parts[1] == "1";
                    MutinyCameraController.SetExplosionCameraEnabled(enabled);
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = enabled
                        ? "[SUCCESS] Explosion knockback camera enabled."
                        : "[SUCCESS] Explosion knockback camera disabled.";
                    succeeded = true;
                }
            }
            else if (lower.StartsWith("aienhance", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !string.Equals(parts[0], "aienhance", StringComparison.OrdinalIgnoreCase) ||
                    (parts[1] != "0" && parts[1] != "1"))
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: aienhance 1 | 0.";
                }
                else
                {
                    MutinyAIController.SetEnhancementEnabled(parts[1] == "1");
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = parts[1] == "1"
                        ? "[SUCCESS] Enhanced AI effects-v1 enabled (bounded full-effect simulation; see ailog for model limits)."
                        : "[SUCCESS] Legacy AI strategy selected.";
                    m_StatusMessage += " Uncommitted decisions restart; committed weapon sequences finish unchanged.";
                    succeeded = true;
                }
            }
            else if (lower.StartsWith("ailog", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !string.Equals(parts[0], "ailog", StringComparison.OrdinalIgnoreCase) ||
                    (parts[1] != "0" && parts[1] != "1"))
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: ailog 1 | 0.";
                }
                else
                {
                    MutinyAIController.SetActionLogEnabled(parts[1] == "1");
                    m_StatusColor = new Color(0.35f, 1.0f, 0.45f);
                    m_StatusMessage = parts[1] == "1"
                        ? "[SUCCESS] AI action decision log enabled. Read [Mutiny:AI-Action] in Player.log."
                        : "[SUCCESS] AI action decision log disabled.";
                    succeeded = true;
                }
            }
            else if (lower.StartsWith("aiweapon", StringComparison.Ordinal) ||
                     lower.StartsWith("aiforceusewaepon", StringComparison.Ordinal) ||
                     lower.StartsWith("aiforceuseweapon", StringComparison.Ordinal))
            {
                string[] parts = cmd.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    (!string.Equals(parts[0], "aiweapon", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(parts[0], "aiforceusewaepon", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(parts[0], "aiforceuseweapon", StringComparison.OrdinalIgnoreCase)) ||
                    !int.TryParse(parts[1], out int weaponId) ||
                    !MutinyAIController.TrySetForcedWeaponId(weaponId))
                {
                    m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                    m_StatusMessage = "[ERROR] Usage: aiweapon {weaponid} (0..15).";
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
                m_StatusMessage = BuildHelpMessage();
                succeeded = true;
            }
            else
            {
                m_StatusColor = new Color(1.0f, 0.45f, 0.45f);
                m_StatusMessage = $"[ERROR] Unknown command: '{cmd}'\nType 'help' to view available commands.";
            }

            m_OutputScroll = Vector2.zero;
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

        private static Texture2D CreateLockTexture(int size, bool isLocked, Color lockColor)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float scale = size / 64f;

            float shackleOuterR = 12f * scale;
            float shackleInnerR = 7.5f * scale;
            float shackleThick = (shackleOuterR - shackleInnerR) * 0.5f;
            float shackleMidR = (shackleOuterR + shackleInnerR) * 0.5f;

            float archCenterY = isLocked ? 41f * scale : 47f * scale;
            float archCenterX = 32f * scale;
            float bodyTopY = 29f * scale;
            float bodyBottomY = 11f * scale;
            float bodyCenterY = (bodyTopY + bodyBottomY) * 0.5f;
            float bodyHalfW = 16f * scale;
            float bodyHalfH = (bodyTopY - bodyBottomY) * 0.5f;
            float bodyCornerR = 4f * scale;

            Color keyholeColor = new Color(0.10f, 0.12f, 0.16f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // 1. Check Body
                    float bdx = Mathf.Max(0f, Mathf.Abs(px - archCenterX) - (bodyHalfW - bodyCornerR));
                    float bdy = Mathf.Max(0f, Mathf.Abs(py - bodyCenterY) - (bodyHalfH - bodyCornerR));
                    float bDist = Mathf.Sqrt(bdx * bdx + bdy * bdy);
                    float bodyAlpha = Mathf.Clamp01(bodyCornerR + 0.5f - bDist);

                    // 2. Check Shackle
                    float shackleAlpha = 0f;
                    if (py >= archCenterY)
                    {
                        // Arch top
                        float sdx = px - archCenterX;
                        float sdy = py - archCenterY;
                        float sDist = Mathf.Sqrt(sdx * sdx + sdy * sdy);
                        float distFromMid = Mathf.Abs(sDist - shackleMidR);
                        shackleAlpha = Mathf.Clamp01(shackleThick + 0.5f - distFromMid);
                    }
                    else
                    {
                        // Legs
                        float leftMidX = archCenterX - shackleMidR;
                        float rightMidX = archCenterX + shackleMidR;
                        float leftDist = Mathf.Abs(px - leftMidX);
                        float rightDist = Mathf.Abs(px - rightMidX);

                        float minYLeft = bodyTopY - 2f * scale;
                        float maxYLeft = archCenterY;
                        if (py >= minYLeft && py <= maxYLeft)
                        {
                            shackleAlpha = Mathf.Max(shackleAlpha, Mathf.Clamp01(shackleThick + 0.5f - leftDist));
                        }

                        float minYRight = isLocked ? (bodyTopY - 2f * scale) : (archCenterY - 6f * scale);
                        float maxYRight = archCenterY;
                        if (py >= minYRight && py <= maxYRight)
                        {
                            shackleAlpha = Mathf.Max(shackleAlpha, Mathf.Clamp01(shackleThick + 0.5f - rightDist));
                        }
                    }

                    // Combine Shackle and Body
                    float combinedAlpha = Mathf.Clamp01(Mathf.Max(bodyAlpha, shackleAlpha));
                    if (combinedAlpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color pixelColor = new Color(lockColor.r, lockColor.g, lockColor.b, lockColor.a * combinedAlpha);

                    // 3. Keyhole cutout inside body
                    if (bodyAlpha > 0.5f)
                    {
                        float kCircleX = archCenterX;
                        float kCircleY = bodyCenterY + 2f * scale;
                        float kDist = Mathf.Sqrt((px - kCircleX) * (px - kCircleX) + (py - kCircleY) * (py - kCircleY));
                        bool inKeyholeCircle = kDist <= 2.8f * scale;
                        bool inKeyholeSlot = Mathf.Abs(px - archCenterX) <= 1.4f * scale && py >= (bodyCenterY - 4.5f * scale) && py <= kCircleY;

                        if (inKeyholeCircle || inKeyholeSlot)
                        {
                            pixelColor = Color.Lerp(pixelColor, keyholeColor, 0.9f);
                        }
                    }

                    tex.SetPixel(x, y, pixelColor);
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
