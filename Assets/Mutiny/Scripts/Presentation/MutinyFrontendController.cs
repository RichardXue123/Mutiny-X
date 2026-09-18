using Mutiny.Levels;
using Mutiny.Persistence;
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

        private MutinyLevelController m_LevelController;
        private Texture2D m_Background;
        private Texture2D m_TitleLogo;
        private Texture2D m_GameSelectPanel;
        private Texture2D m_LevelSelectPanel;
        private Texture2D m_GameTypePirates;
        private Texture2D m_ButtonSmall;
        private Texture2D m_ButtonWide;
        private Texture2D m_ButtonBack;
        private Texture2D m_LevelSlot;
        private GUIStyle m_ButtonStyle;
        private GUIStyle m_HeadingStyle;
        private GUIStyle m_LevelNumberStyle;
        private GUIStyle m_QuestionStyle;

        public MutinyFrontendPage CurrentPage => m_Flow.CurrentPage;

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

            MutinyAudioManager.Instance?.StartMenuMusic();
            Debug.Log($"[MutinyFrontend] FRONT-01 title shown; highestUnlocked={MutinySaveSystem.HighestUnlockedLevel}", this);
        }

        private void LoadResources()
        {
            m_Background = Resources.Load<Texture2D>("UI/Frontend/background");
            m_TitleLogo = Resources.Load<Texture2D>("UI/Frontend/title_logo");
            m_GameSelectPanel = Resources.Load<Texture2D>("UI/Frontend/game_select_panel");
            m_LevelSelectPanel = Resources.Load<Texture2D>("UI/Frontend/level_select_panel");
            m_GameTypePirates = Resources.Load<Texture2D>("UI/Frontend/game_type_pirates");
            m_ButtonSmall = Resources.Load<Texture2D>("UI/Frontend/button_small");
            m_ButtonWide = Resources.Load<Texture2D>("UI/Frontend/button_wide");
            m_ButtonBack = Resources.Load<Texture2D>("UI/Frontend/button_back");
            m_LevelSlot = Resources.Load<Texture2D>("UI/Frontend/level_slot");
            for (int i = 0; i < SinglePlayerLevelCount; i++)
                m_LevelPreviews[i] = Resources.Load<Texture2D>($"UI/Frontend/LevelPreviews/{i + 1:D2}");
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
            DrawTexture(new Rect(0f, 0f, CanvasWidth, CanvasHeight), m_Background);

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

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }

        private void DrawTitle()
        {
            DrawTexture(new Rect(54f, 36f, 452f, 154f), m_TitleLogo);
            if (DrawOriginalButton(new Rect(193f, 187f, 163f, 24f), "play", m_ButtonSmall))
            {
                m_Flow.PressPlay();
                LogPage("FRONT-01 play", m_Flow.CurrentPage);
            }

            DrawOriginalButton(new Rect(193f, 216f, 163f, 24f), "scores", m_ButtonSmall);
            DrawOriginalButton(new Rect(193f, 245f, 163f, 24f), "help", m_ButtonSmall);
            DrawOriginalButton(new Rect(193f, 274f, 163f, 24f), "credits", m_ButtonSmall);
        }

        private void DrawGameSelect()
        {
            DrawTexture(new Rect(44f, 24f, 460f, 350f), m_GameSelectPanel);
            MutinyBitmapFont.DrawPirateText(new Rect(44f, 38f, 460f, 26f), "select game", false, true, -3);
            MutinyBitmapFont.DrawDangleText(new Rect(161f, 76f, 300f, 60f), "click one of the buttons below.||play against the computer or|against a friend!", Color.black, TextAnchor.UpperLeft, 0, 13);
            DrawTexture(new Rect(127f, 169.5f, 345f, 80f), m_GameTypePirates);

            if (DrawOriginalButton(new Rect(63f, 263f, 200f, 24f), "1 player", m_ButtonWide))
            {
                m_Flow.PressOnePlayer();
                LogPage("FRONT-02 one player", m_Flow.CurrentPage);
            }

            DrawOriginalButton(new Rect(287f, 263f, 200f, 24f), "2 player", m_ButtonWide);
            if (DrawOriginalButton(new Rect(205f, 334f, 140f, 24f), "back", m_ButtonBack))
            {
                m_Flow.PressGameSelectBack();
                LogPage("FRONT-02 back", m_Flow.CurrentPage);
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

            if (DrawOriginalButton(new Rect(205f, 334f, 140f, 24f), "back", m_ButtonBack))
            {
                m_Flow.PressLevelSelectBack();
                LogPage("FRONT-03 back", m_Flow.CurrentPage);
            }
        }

        private void DrawLevelButton(int level, Rect rect)
        {
            bool unlocked = MutinySaveSystem.IsLevelUnlocked(level);
            bool hovered = unlocked && rect.Contains(GetCanvasMousePosition());
            bool pressed = unlocked && IsPointerDown(rect);

            GUI.color = hovered ? new Color(1f, 0.82f, 0.52f, 1f) : Color.white;
            DrawTexture(rect, m_LevelSlot);

            Texture2D preview = m_LevelPreviews[level - 1];
            GUI.color = unlocked ? Color.white : Color.black;
            DrawTexture(new Rect(rect.x + 5.25f, rect.y + 3f, 39f, 56f), preview);
            GUI.color = Color.white;

            if (unlocked)
            {
                MutinyBitmapFont.DrawDangleText(new Rect(rect.x, rect.y + 60f, rect.width, 14f), level.ToString("D2"), Color.white, TextAnchor.MiddleCenter);
                bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
                if ((clicked || pressed) &&
                    m_Flow.TrySelectLevel(level, MutinySaveSystem.IsLevelUnlocked))
                {
                    StartLevel(level);
                }
            }
            else
            {
                MutinyBitmapFont.DrawDangleText(new Rect(rect.x, rect.y + 20f, rect.width, 18f), "?", Color.white, TextAnchor.MiddleCenter);
            }
        }

        private bool DrawOriginalButton(Rect rect, string text, Texture2D texture, bool activateOnPress = false)
        {
            bool hovered = rect.Contains(GetCanvasMousePosition());
            bool pressed = activateOnPress && IsPointerDown(rect);

            DrawTexture(rect, texture);
            MutinyBitmapFont.DrawPirateText(rect, text, hovered, true, -3);

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
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
            // GUI.matrix.inverse here transformed the pointer a second time, so the
            // original Flash-style up/over hit tests almost never matched the drawn
            // button when the 550x400 canvas was scaled or letterboxed.
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
            MutinyAudioManager.Instance?.StartGameMusic();
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

            MutinyAudioManager.Instance?.StartMenuMusic();
            Debug.Log("[MutinyFrontend] HUD-CORNER-04 back to menu -> level select 1p", this);
            return true;
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
