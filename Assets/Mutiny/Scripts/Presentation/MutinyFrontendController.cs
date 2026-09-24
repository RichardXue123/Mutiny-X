using Mutiny.Levels;
using Mutiny.Persistence;
using Mutiny.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mutiny.Presentation
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class MutinyFrontendController : MonoBehaviour
    {
        public const int SinglePlayerLevelCount = 15;
        private const float CanvasWidth = 550f;
        private const float CanvasHeight = 400f;

        private readonly MutinyFrontendFlow m_Flow = new MutinyFrontendFlow();
        private readonly Texture2D[] m_LevelPreviews = new Texture2D[SinglePlayerLevelCount];
        // Flash menu_background_anim: bg children in display-depth order, then water.
        private readonly Texture2D[] m_BackgroundLayers = new Texture2D[7];
        private readonly float[] m_BackgroundOffsets = new float[5];
        private float m_BackgroundTickAccumulator;
        private bool m_HasAnimatedBackground;

        private MutinyLevelController m_LevelController;
        private Texture2D m_Background;
        private Texture2D m_TitleLogo;
        private Texture2D m_GameSelectPanel;
        private Texture2D m_LevelSelectPanel;
        private Texture2D m_GameTypePirates;
        private Texture2D m_ButtonSmall;
        private Texture2D m_ButtonSmallOver;
        private Texture2D m_ButtonWide;
        private Texture2D m_ButtonWideOver;
        private Texture2D m_ButtonBack;
        private Texture2D m_ButtonBackOver;
        private Texture2D m_LevelSlot;
        private Texture2D m_LevelSlotOver;
        private Texture2D m_MusicCornerOnUpTexture;
        private Texture2D m_MusicCornerOnOverTexture;
        private Texture2D m_MusicCornerOffUpTexture;
        private Texture2D m_MusicCornerOffOverTexture;
        private Texture2D m_SfxCornerOnUpTexture;
        private Texture2D m_SfxCornerOnOverTexture;
        private Texture2D m_SfxCornerOffUpTexture;
        private Texture2D m_SfxCornerOffOverTexture;
        private bool m_MusicHovered;
        private bool m_SfxHovered;
        private GUIStyle m_ButtonStyle;
        private GUIStyle m_HeadingStyle;
        private GUIStyle m_LevelNumberStyle;
        private GUIStyle m_QuestionStyle;

        public MutinyFrontendPage CurrentPage => m_Flow.CurrentPage;
        public bool AreCornerAudioControlsVisible => m_Flow.CurrentPage != MutinyFrontendPage.Gameplay;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name != "Main" ||
                FindAnyObjectByType<MutinyFrontendController>() != null)
            {
                return;
            }

            MutinyLevelController levelController = FindAnyObjectByType<MutinyLevelController>();
            if (levelController == null)
                return;

            GameObject host = new GameObject("MutinyFrontend");
            MutinyFrontendController frontend = host.AddComponent<MutinyFrontendController>();
            frontend.Initialize(levelController);
        }

        public void Initialize(MutinyLevelController levelController)
        {
            m_LevelController = levelController;
            LoadResources();

            if (m_LevelController != null && m_LevelController.CurrentLevel != null)
                m_LevelController.CurrentLevel.gameObject.SetActive(false);

            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<MutinyCameraController>() == null)
            {
                mainCam.gameObject.AddComponent<MutinyCameraController>();
            }

            MutinyAudioManager.Instance?.PlayMusic("menu_music");
            Debug.Log($"[MutinyFrontend] FRONT-01 title shown; highestUnlocked={MutinySaveSystem.HighestUnlockedLevel}", this);
        }

        private void LoadResources()
        {
            m_Background = Resources.Load<Texture2D>("UI/Frontend/background");
            string[] layerNames = { "sky", "backClouds1", "hills1", "frontClouds1",
                "cloudBase1", "waterBackground1", "water" };
            m_HasAnimatedBackground = true;
            for (int i = 0; i < layerNames.Length; i++)
            {
                m_BackgroundLayers[i] = LoadPointTexture(MutinyOriginalBackground.ResourcePath + layerNames[i]);
                if (m_BackgroundLayers[i] == null)
                    m_HasAnimatedBackground = false;
            }
            m_TitleLogo = Resources.Load<Texture2D>("UI/Frontend/title_logo");
            m_GameSelectPanel = Resources.Load<Texture2D>("UI/Frontend/game_select_panel");
            m_LevelSelectPanel = Resources.Load<Texture2D>("UI/Frontend/level_select_panel");
            m_GameTypePirates = Resources.Load<Texture2D>("UI/Frontend/game_type_pirates");
            m_ButtonSmall = Resources.Load<Texture2D>("UI/Frontend/button_small");
            m_ButtonSmallOver = Resources.Load<Texture2D>("UI/Frontend/button_small_over");
            m_ButtonWide = Resources.Load<Texture2D>("UI/Frontend/button_wide");
            m_ButtonWideOver = Resources.Load<Texture2D>("UI/Frontend/button_wide_over");
            m_ButtonBack = Resources.Load<Texture2D>("UI/Frontend/button_back");
            m_ButtonBackOver = Resources.Load<Texture2D>("UI/Frontend/button_back_over");
            m_LevelSlot = Resources.Load<Texture2D>("UI/Frontend/level_slot");
            m_LevelSlotOver = Resources.Load<Texture2D>("UI/Frontend/level_slot_over");
            for (int i = 0; i < SinglePlayerLevelCount; i++)
                m_LevelPreviews[i] = Resources.Load<Texture2D>($"UI/Frontend/LevelPreviews/{i + 1:D2}");

            m_MusicCornerOnUpTexture = LoadPointTexture("UI/CornerControls/music_on_up");
            m_MusicCornerOnOverTexture = LoadPointTexture("UI/CornerControls/music_on_over");
            m_MusicCornerOffUpTexture = LoadPointTexture("UI/CornerControls/music_off_up");
            m_MusicCornerOffOverTexture = LoadPointTexture("UI/CornerControls/music_off_over");
            m_SfxCornerOnUpTexture = LoadPointTexture("UI/CornerControls/sfx_on_up");
            m_SfxCornerOnOverTexture = LoadPointTexture("UI/CornerControls/sfx_on_over");
            m_SfxCornerOffUpTexture = LoadPointTexture("UI/CornerControls/sfx_off_up");
            m_SfxCornerOffOverTexture = LoadPointTexture("UI/CornerControls/sfx_off_over");
        }

        private static Texture2D LoadPointTexture(string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture != null)
                texture.filterMode = FilterMode.Point;
            return texture;
        }

        private void Update()
        {
            if (m_Flow.CurrentPage == MutinyFrontendPage.Gameplay)
                return;

            m_BackgroundTickAccumulator += Time.unscaledDeltaTime;
            while (m_BackgroundTickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_BackgroundTickAccumulator -= MutinyPhysics.TimeStep;
                // MenuBackgroundAnim.onEnterFrame: water, back clouds, hills,
                // front clouds and cloud base have independent Flash wrap widths.
                AdvanceBackgroundOffset(0, 8f, 64f);
                AdvanceBackgroundOffset(1, 1f, 900f);
                AdvanceBackgroundOffset(2, 2f, 840f);
                AdvanceBackgroundOffset(3, 4f, 1000f);
                AdvanceBackgroundOffset(4, 2f, 550f);
            }
        }

        private void AdvanceBackgroundOffset(int index, float speed, float width)
        {
            m_BackgroundOffsets[index] -= speed;
            if (m_BackgroundOffsets[index] <= -width)
                m_BackgroundOffsets[index] = 0f;
        }

        private void DrawAnimatedBackground()
        {
            if (!m_HasAnimatedBackground)
            {
                DrawTexture(new Rect(0f, 0f, CanvasWidth, CanvasHeight), m_Background);
                return;
            }

            GUI.BeginGroup(new Rect(0f, 0f, CanvasWidth, CanvasHeight));
            DrawBackgroundLayer(0, 0f, 0f);
            // Original sprite registration offsets, from sprite-origins.csv.
            DrawBackgroundLayer(1, m_BackgroundOffsets[1] + 197f, 12f);
            DrawBackgroundLayer(2, m_BackgroundOffsets[2], 53f);
            DrawBackgroundLayer(3, m_BackgroundOffsets[3] + 71f, 6f);
            DrawBackgroundLayer(4, m_BackgroundOffsets[4], 275.95f);
            DrawBackgroundLayer(5, 0f, 360f);
            DrawBackgroundLayer(6, m_BackgroundOffsets[0], 332f);
            GUI.EndGroup();
        }

        private void DrawBackgroundLayer(int index, float x, float y)
        {
            Texture2D texture = m_BackgroundLayers[index];
            DrawTexture(new Rect(x, y, texture.width, texture.height), texture);
        }

        private void OnGUI()
        {
            if (m_Flow.CurrentPage == MutinyFrontendPage.Gameplay)
                return;

            GUI.depth = -10000;
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;

            float scale = Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight);
            float left = (Screen.width - CanvasWidth * scale) * 0.5f;
            float top = (Screen.height - CanvasHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(-left / scale, -top / scale, Screen.width / scale, Screen.height / scale), Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawAnimatedBackground();

            switch (m_Flow.CurrentPage)
            {
                case MutinyFrontendPage.Title:
                    DrawTitle();
                    break;
                case MutinyFrontendPage.GameSelect:
                    DrawGameSelect();
                    break;
                case MutinyFrontendPage.LevelSelect:
                    DrawLevelSelect();
                    break;
            }

            DrawCornerAudioControls();

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }

        private void DrawTitle()
        {
            DrawTexture(new Rect(54f, 36f, 452f, 154f), m_TitleLogo);
            if (DrawOriginalButton(new Rect(193f, 187f, 163f, 24f), "play", m_ButtonSmall, m_ButtonSmallOver))
            {
                MutinyTransitionManager.RequestTransition(() =>
                {
                    m_Flow.PressPlay();
                    LogPage("FRONT-01 play", m_Flow.CurrentPage);
                }, showLoading: false);
            }

            DrawOriginalButton(new Rect(193f, 216f, 163f, 24f), "scores", m_ButtonSmall, m_ButtonSmallOver);
            DrawOriginalButton(new Rect(193f, 245f, 163f, 24f), "help", m_ButtonSmall, m_ButtonSmallOver);
            DrawOriginalButton(new Rect(193f, 274f, 163f, 24f), "credits", m_ButtonSmall, m_ButtonSmallOver);
        }

        private void DrawGameSelect()
        {
            DrawTexture(new Rect(44f, 24f, 460f, 350f), m_GameSelectPanel);
            MutinyBitmapFont.DrawPirateText(new Rect(44f, 38f, 460f, 26f), "select game", false, true, -3);
            MutinyBitmapFont.DrawDangleText(new Rect(161f, 76f, 300f, 60f), "click one of the buttons below.||play against the computer or|against a friend!", Color.black, TextAnchor.UpperLeft, 0, 13);
            DrawTexture(new Rect(127f, 169.5f, 345f, 80f), m_GameTypePirates);

            if (DrawOriginalButton(new Rect(63f, 263f, 200f, 24f), "1 player", m_ButtonWide, m_ButtonWideOver))
            {
                MutinyTransitionManager.RequestTransition(() =>
                {
                    m_Flow.PressOnePlayer();
                    LogPage("FRONT-02 one player", m_Flow.CurrentPage);
                }, showLoading: false);
            }

            DrawOriginalButton(new Rect(287f, 263f, 200f, 24f), "2 player", m_ButtonWide, m_ButtonWideOver);
            if (DrawOriginalButton(new Rect(205f, 334f, 140f, 24f), "back", m_ButtonBack, m_ButtonBackOver))
            {
                MutinyTransitionManager.RequestTransition(() =>
                {
                    m_Flow.PressGameSelectBack();
                    LogPage("FRONT-02 back", m_Flow.CurrentPage);
                }, showLoading: false);
            }
        }

        private void DrawLevelSelect()
        {
            DrawTexture(new Rect(44f, 24f, 462f, 352f), m_LevelSelectPanel);
            MutinyBitmapFont.DrawPirateText(new Rect(44f, 34f, 462f, 26f), "select level", false, true, -3);

            for (int level = 1; level <= SinglePlayerLevelCount; level++)
            {
                int column = (level - 1) % 5;
                int row = (level - 1) / 5;
                DrawLevelButton(level, new Rect(110f + column * 70f, 68f + row * 90f, 51f, 77f));
            }

            if (DrawOriginalButton(new Rect(205f, 334f, 140f, 24f), "back", m_ButtonBack, m_ButtonBackOver))
            {
                MutinyTransitionManager.RequestTransition(() =>
                {
                    m_Flow.PressLevelSelectBack();
                    LogPage("FRONT-03 back", m_Flow.CurrentPage);
                }, showLoading: false);
            }
        }

        private void DrawLevelButton(int level, Rect rect)
        {
            bool isTransitioning = MutinyTransitionManager.IsTransitionActive;
            bool unlocked = MutinySaveSystem.IsLevelUnlocked(level);
            bool hovered = !isTransitioning && unlocked && rect.Contains(GetCanvasMousePosition());
            bool pressed = !isTransitioning && unlocked && IsPointerDown(rect);

            GUI.color = Color.white;
            DrawTexture(rect, m_LevelSlot);

            Texture2D preview = m_LevelPreviews[level - 1];
            GUI.color = unlocked ? Color.white : Color.black;
            DrawTexture(new Rect(rect.x + 5.25f, rect.y + 3f, 39f, 56f), preview);
            GUI.color = Color.white;

            if (unlocked)
            {
                MutinyBitmapFont.DrawDangleText(new Rect(rect.x, rect.y + 60f, rect.width, 14f), level.ToString("D2"), Color.white, TextAnchor.MiddleCenter);
                if (hovered && m_LevelSlotOver != null)
                {
                    Color prev = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 102f / 255f);
                    DrawTexture(rect, m_LevelSlotOver);
                    GUI.color = prev;
                }

                bool clicked = !isTransitioning && GUI.Button(rect, GUIContent.none, GUIStyle.none);
                if ((clicked || pressed) && !isTransitioning)
                {
                    int targetLevel = level;
                    MutinyTransitionManager.RequestTransition(() =>
                    {
                        if (m_Flow.TrySelectLevel(targetLevel, MutinySaveSystem.IsLevelUnlocked))
                        {
                            StartLevel(targetLevel);
                        }
                    }, showLoading: true);
                }
            }
            else
            {
                MutinyBitmapFont.DrawDangleText(new Rect(rect.x, rect.y + 20f, rect.width, 18f), "?", Color.white, TextAnchor.MiddleCenter);
            }
        }

        private bool DrawOriginalButton(Rect rect, string text, Texture2D texture, Texture2D hoverTexture = null, bool activateOnPress = false)
        {
            bool isTransitioning = MutinyTransitionManager.IsTransitionActive;
            bool hovered = !isTransitioning && rect.Contains(GetCanvasMousePosition());
            bool pressed = !isTransitioning && activateOnPress && IsPointerDown(rect);

            Texture2D texToDraw = (hovered && hoverTexture != null) ? hoverTexture : texture;
            DrawTexture(rect, texToDraw);
            MutinyBitmapFont.DrawPirateText(rect, text, hovered, true, -3);

            bool clicked = !isTransitioning && GUI.Button(rect, GUIContent.none, GUIStyle.none);
            return activateOnPress ? (pressed || clicked) : clicked;
        }

        private static bool IsPointerDown(Rect rect)
        {
            Event guiEvent = Event.current;
            return guiEvent != null && guiEvent.type == EventType.MouseDown && guiEvent.button == 0 &&
                   rect.Contains(GetCanvasMousePosition());
        }

        private static Vector2 GetCanvasMousePosition()
        {
            // IMGUI transforms Event.current.mousePosition into the active GUI.matrix
            // coordinate space before controls and custom drawing are evaluated. Applying
            // ScreenToCanvasPoint or GUI.matrix.inverse here transforms the pointer a second time,
            // so the original Flash-style up/over hit tests fail when scaled or letterboxed.
            return Event.current != null ? Event.current.mousePosition : Vector2.zero;
        }

        private void StartLevel(int level)
        {
            Debug.Log($"[MutinyFrontend] FRONT-05 select level={level:D2}; loading production level", this);
            if (m_LevelController == null)
                m_LevelController = FindAnyObjectByType<MutinyLevelController>();

            if (m_LevelController == null)
            {
                Debug.LogError("[MutinyFrontend] Cannot enter gameplay: MutinyLevelController was not found.", this);
                return;
            }

            // LevelSelectButton.doPress clears `_root.score` before entering a
            // new one-player game.  Advancing inside an active game does not.
            m_LevelController.ResetSinglePlayerScore();
            m_LevelController.LoadLevel(level);
            if (m_LevelController.CurrentLevel != null)
                m_LevelController.CurrentLevel.gameObject.SetActive(true);
            MutinyAudioManager.Instance?.PlayMusic("game_music");
        }

        /// <summary>
        /// Production counterpart of QuitGameButton's level_select_1p callback.
        /// The level root is hidden before this front-end draws the level selector.
        /// </summary>
        public bool ReturnToSinglePlayerLevelSelect()
        {
            if (!m_Flow.ReturnToSinglePlayerLevelSelect())
                return false;

            if (m_LevelController == null)
                m_LevelController = FindAnyObjectByType<MutinyLevelController>();
            if (m_LevelController != null && m_LevelController.CurrentLevel != null)
                m_LevelController.CurrentLevel.gameObject.SetActive(false);

            MutinyAudioManager.Instance?.PlayMusic("menu_music");
            Debug.Log("[MutinyFrontend] HUD-CORNER-04 back to menu -> level select 1p", this);
            return true;
        }

        private void DrawCornerAudioControls()
        {
            Rect sfxVisualRect = MutinyGameHUD.ResolveOriginalCornerVisualRect(MutinyCornerControl.Sfx);
            Rect musicVisualRect = MutinyGameHUD.ResolveOriginalCornerVisualRect(MutinyCornerControl.Music);
            Rect sfxHitRect = MutinyGameHUD.ResolveOriginalCornerHitRect(MutinyCornerControl.Sfx);
            Rect musicHitRect = MutinyGameHUD.ResolveOriginalCornerHitRect(MutinyCornerControl.Music);
            MutinyAudioManager audio = MutinyAudioManager.Instance;

            Vector2 mousePosition = GetCanvasMousePosition();
            bool sfxHovered = MutinyGameHUD.IsCornerHovered(MutinyCornerControl.Sfx, mousePosition, m_SfxHovered);
            bool musicHovered = MutinyGameHUD.IsCornerHovered(MutinyCornerControl.Music, mousePosition, m_MusicHovered);
            UpdateCornerHover(ref m_SfxHovered, sfxHovered);
            UpdateCornerHover(ref m_MusicHovered, musicHovered);

            // Draw unhovered first, hovered last so active tooltip bubble is always on top
            if (!musicHovered)
                DrawCornerSprite(musicVisualRect,
                    ResolveCornerToggleTexture(true, audio != null && audio.MusicEnabled, false), false, MutinyCornerControl.Music);
            if (!sfxHovered)
                DrawCornerSprite(sfxVisualRect,
                    ResolveCornerToggleTexture(false, audio != null && audio.SfxEnabled, false), false, MutinyCornerControl.Sfx);

            if (musicHovered)
                DrawCornerSprite(musicVisualRect,
                    ResolveCornerToggleTexture(true, audio != null && audio.MusicEnabled, true), true, MutinyCornerControl.Music);
            if (sfxHovered)
                DrawCornerSprite(sfxVisualRect,
                    ResolveCornerToggleTexture(false, audio != null && audio.SfxEnabled, true), true, MutinyCornerControl.Sfx);

            Rect sfxBubbleRect = MutinyGameHUD.ResolveOriginalCornerBubbleRect(MutinyCornerControl.Sfx);
            Rect musicBubbleRect = MutinyGameHUD.ResolveOriginalCornerBubbleRect(MutinyCornerControl.Music);

            if (!MutinyTransitionManager.IsTransitionActive)
            {
                if (GUI.Button(sfxHitRect, GUIContent.none, GUIStyle.none) ||
                    (sfxHovered && GUI.Button(sfxBubbleRect, GUIContent.none, GUIStyle.none)))
                    audio?.ToggleSFX();
                if (GUI.Button(musicHitRect, GUIContent.none, GUIStyle.none) ||
                    (musicHovered && GUI.Button(musicBubbleRect, GUIContent.none, GUIStyle.none)))
                    audio?.ToggleMusic();
            }
        }

        private static void DrawCornerSprite(Rect rect, Texture2D texture, bool hovered, MutinyCornerControl control)
        {
            if (texture != null)
            {
                DrawTexture(rect, texture);
            }
            else if (hovered)
            {
                string label = MutinyGameHUD.ResolveOriginalCornerTooltip(control);
                MutinyGameHUD.DrawCornerTooltipBubble(rect, label);
            }
        }

        private void UpdateCornerHover(ref bool previous, bool current)
        {
            if (current && !previous)
                MutinyAudioManager.Instance?.PlaySFX("rollover");
            previous = current;
        }

        private Texture2D ResolveCornerToggleTexture(bool isMusic, bool enabled, bool hovered)
        {
            MutinyCornerToggleVisualState state = MutinyGameHUD.ResolveCornerToggleVisualState(enabled, hovered);
            if (isMusic)
            {
                switch (state)
                {
                    case MutinyCornerToggleVisualState.OnUp: return m_MusicCornerOnUpTexture;
                    case MutinyCornerToggleVisualState.OnOver: return m_MusicCornerOnOverTexture;
                    case MutinyCornerToggleVisualState.OffUp: return m_MusicCornerOffUpTexture;
                    default: return m_MusicCornerOffOverTexture;
                }
            }

            switch (state)
            {
                case MutinyCornerToggleVisualState.OnUp: return m_SfxCornerOnUpTexture;
                case MutinyCornerToggleVisualState.OnOver: return m_SfxCornerOnOverTexture;
                case MutinyCornerToggleVisualState.OffUp: return m_SfxCornerOffUpTexture;
                default: return m_SfxCornerOffOverTexture;
            }
        }

        private static void DrawTexture(Rect rect, Texture2D texture)
        {
            if (texture != null)
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }

        private static void DrawOutlinedLabel(Rect rect, string text, GUIStyle style)
        {
            Color textColor = style.normal.textColor;
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = textColor;
            GUI.Label(rect, text, style);
        }

        private void LogPage(string action, MutinyFrontendPage page)
        {
            Debug.Log($"[MutinyFrontend] {action} -> {page}", this);
        }
    }
}
