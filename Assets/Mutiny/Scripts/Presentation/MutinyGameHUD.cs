using System;
using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using Mutiny.Persistence;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    public enum MutinyThrowButtonVisualState
    {
        Disabled,
        RedUp,
        RedOver,
        BlueUp,
        BlueOver
    }

    public enum MutinyCornerToggleVisualState
    {
        OnUp,
        OnOver,
        OffUp,
        OffOver
    }

    public enum MutinyCornerControl
    {
        Quit,
        Music,
        Sfx
    }

    public enum MutinyGameEndPopupKind
    {
        None,
        LevelComplete,
        LevelFailed,
        GameComplete
    }

    [DisallowMultipleComponent]
    public sealed class MutinyGameHUD : MonoBehaviour
    {
        private const float OriginalUiTickSeconds = 1f / 25f;
        private const float OriginalPanelAlphaStep = 0.25f;
        private const float OriginalPopupAlphaStep = 0.25f;
        private const float OriginalCanvasWidth = 550f;
        private const float OriginalCanvasHeight = 400f;
        private const float OriginalMapHolderX = 20f;
        private const float OriginalMapHolderY = 20f;
        private const float OriginalTeam1OriginX = 1439f / 20f;
        private const float OriginalTeam2OriginX = 9557f / 20f;
        private const float OriginalTeamOriginY = 380f;
        // DefineShape_326 bounds are (-3500,-2600)..(3499,2599) twips,
        // under IngamePopup's x=275 / y=200 stage registration point.
        private static readonly Rect OriginalPopupPanelRect = new Rect(100f, 70f, 350f, 260f);
        // DefineShape_328 (the shared popup-button background) spans
        // (-2800,0)..(2800,480) twips.  Its parent button clips are placed at
        // y=900 and y=1600 twips respectively.
        private static readonly Rect OriginalPopupPrimaryButtonRect = new Rect(135f, 245f, 280f, 24f);
        private static readonly Rect OriginalPopupSecondaryButtonRect = new Rect(135f, 280f, 280f, 24f);
        // Root-timeline placements are: quit (9818,359), music (10239,359),
        // sfx (10658,359) twips.  The exported frames retain the negative
        // shape bounds used by the original tooltip bubbles, so these visual
        // rectangles start at the timeline placement plus that frame's min
        // bound rather than at the visible icon itself.
        private static readonly Rect OriginalQuitCornerVisualRect = new Rect(479.9f, 11f, 23f, 34f);
        private static readonly Rect OriginalMusicCornerVisualRect = new Rect(496.9f, 11f, 31f, 34f);
        private static readonly Rect OriginalSfxCornerVisualRect = new Rect(501.9f, 11f, 47f, 34f);
        // The clickable part is the normal-state icon shape.  The tooltip is
        // presentation only and must not become a larger invisible hit target.
        private static readonly Rect OriginalQuitCornerHitRect = new Rect(483.95f, 11f, 13.95f, 13.9f);
        private static readonly Rect OriginalMusicCornerHitRect = new Rect(502f, 11f, 19.95f, 13.9f);
        private static readonly Rect OriginalSfxCornerHitRect = new Rect(525.95f, 11f, 13.95f, 13.9f);

        public MutinyTurnManager TurnManager;
        public MutinyPlayerInput PlayerInput;
        public MutinyLevelController LevelController;
        public MutinySpeechController Speech;
        public MutinyIngameTextArea IngameText;

        private bool m_ShowLevelSelect = false;
        private GUIStyle m_TitleStyle;
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_ButtonStyle;
        private GUIStyle m_ActiveButtonStyle;
        private GUIStyle m_PanelStyle;
        private GUIStyle m_SubheaderStyle;
        private GUIStyle m_OriginalSlotStyle;
        private GUIStyle m_OriginalTitleTextStyle;
        private GUIStyle m_OriginalDescriptionTextStyle;
        private GUIStyle m_OriginalAmmoTextStyle;
        private Texture2D m_RedWeaponPanel;
        private Texture2D m_BlueWeaponPanel;
        private Texture2D m_WeaponSlotDisabledTexture;
        private Texture2D m_WeaponSlotRedUpTexture;
        private Texture2D m_WeaponSlotBlueUpTexture;
        private Texture2D m_WeaponSlotOverTexture;
        private Texture2D m_InfiniteAmmoTexture;
        private Texture2D m_ThrowDisabledTexture;
        private Texture2D m_ThrowRedUpTexture;
        private Texture2D m_ThrowRedOverTexture;
        private Texture2D m_ThrowBlueUpTexture;
        private Texture2D m_ThrowBlueOverTexture;
        private Texture2D m_EndTurnRedUpTexture;
        private Texture2D m_EndTurnRedOverTexture;
        private Texture2D m_EndTurnBlueUpTexture;
        private Texture2D m_EndTurnBlueOverTexture;
        private Texture2D m_RedCancelButton;
        private Texture2D m_BlueCancelButton;
        private Texture2D m_Team1Panel;
        private Texture2D m_Team2Panel;
        private Texture2D m_SpeechBubbleTexture;
        private Texture2D m_Team1Portrait;
        private Texture2D[] m_OpponentPortraits = Array.Empty<Texture2D>();
        private readonly Dictionary<string, Texture2D> m_WeaponIcons = new Dictionary<string, Texture2D>();
        private bool m_StylesInitialized = false;
        private float m_ActionPanelAlpha = 0f;
        private float m_ActionPanelTickAccumulator = 0f;
        private bool m_ActionPanelContentsActive = false;
        // IngamePopup has a distinct show flag and fade alpha. A fading-out popup
        // still blocks CornerQuitButton, so it cannot be represented by alpha alone.
        private bool m_QuitPromptShow;
        private float m_QuitPromptAlpha;
        private bool m_GameEndPopupShow;
        private float m_GameEndPopupAlpha;
        private MutinyGameEndPopupKind m_GameEndPopupKind;
        private int m_GameEndTargetLevelScore;
        private int m_GameEndTargetTotalScore;
        private int m_GameEndDisplayedLevelScore;
        private int m_GameEndDisplayedTotalScore;
        private bool m_QuitHovered;
        private bool m_MusicHovered;
        private bool m_SfxHovered;
        private Texture2D m_CornerButtonTexture;
        private Texture2D m_CornerButtonOverTexture;
        private Texture2D m_CornerBackButtonTexture;
        private Texture2D m_CornerBackButtonOverTexture;
        private Texture2D m_QuitCornerUpTexture;
        private Texture2D m_QuitCornerOverTexture;
        private Texture2D m_MusicCornerOnUpTexture;
        private Texture2D m_MusicCornerOnOverTexture;
        private Texture2D m_MusicCornerOffUpTexture;
        private Texture2D m_MusicCornerOffOverTexture;
        private Texture2D m_SfxCornerOnUpTexture;
        private Texture2D m_SfxCornerOnOverTexture;
        private Texture2D m_SfxCornerOffUpTexture;
        private Texture2D m_SfxCornerOffOverTexture;
        private int m_Team1HealthFrame = 1;
        private int m_Team2HealthFrame = 1;
        private TextAsset m_CachedMapXml;
        private MutinyLevelData m_CachedMapLevel;

        public float ActionPanelAlpha => Mathf.Clamp01(m_ActionPanelAlpha);
        public bool ActionPanelContentsActive => m_ActionPanelContentsActive;
        public bool IsQuitPromptVisible => m_QuitPromptAlpha > 0f;
        public bool IsQuitPromptShowRequested => m_QuitPromptShow;
        public float QuitPromptAlpha => Mathf.Clamp01(m_QuitPromptAlpha);
        public bool IsGameEndPopupShowRequested => m_GameEndPopupShow;
        public float GameEndPopupAlpha => Mathf.Clamp01(m_GameEndPopupAlpha);
        public MutinyGameEndPopupKind GameEndPopupKind => m_GameEndPopupKind;

        public static Rect ResolveOriginalPopupPanelRect() => OriginalPopupPanelRect;
        public static Rect ResolveOriginalPopupPrimaryButtonRect() => OriginalPopupPrimaryButtonRect;
        public static Rect ResolveOriginalPopupSecondaryButtonRect() => OriginalPopupSecondaryButtonRect;

        public static Rect ResolveWeaponSlotAmmoNumberRect(int column, int row)
        {
            return new Rect(112f + column * 31f, 29f + row * 43f + 21f, 24f, 12f);
        }

        public static Rect ResolveWeaponSlotInfiniteAmmoRect(int column, int row)
        {
            return new Rect(112f + column * 31f + 3f, 29f + row * 43f + 23f, 18f, 9f);
        }

        public static Rect ResolveOriginalCornerVisualRect(MutinyCornerControl control)
        {
            switch (control)
            {
                case MutinyCornerControl.Quit: return OriginalQuitCornerVisualRect;
                case MutinyCornerControl.Music: return OriginalMusicCornerVisualRect;
                default: return OriginalSfxCornerVisualRect;
            }
        }

        public static Rect ResolveOriginalCornerHitRect(MutinyCornerControl control)
        {
            switch (control)
            {
                case MutinyCornerControl.Quit: return OriginalQuitCornerHitRect;
                case MutinyCornerControl.Music: return OriginalMusicCornerHitRect;
                default: return OriginalSfxCornerHitRect;
            }
        }

        public static string ResolveOriginalCornerTooltip(MutinyCornerControl control)
        {
            switch (control)
            {
                case MutinyCornerControl.Quit: return "quit";
                case MutinyCornerControl.Music: return "music";
                default: return "sound fx";
            }
        }

        public static Rect ResolveOriginalCornerBubbleRect(MutinyCornerControl control)
        {
            Rect visualRect = ResolveOriginalCornerVisualRect(control);
            return new Rect(visualRect.x, visualRect.y + 17f, visualRect.width, 17f);
        }

        public static bool IsCornerHovered(MutinyCornerControl control, Vector2 canvasMouse, bool currentlyHovered)
        {
            Rect hitRect = ResolveOriginalCornerHitRect(control);
            Rect bubbleRect = ResolveOriginalCornerBubbleRect(control);
            // When already hovered, the tooltip bubble is visible below the button, so hovering over
            // either the icon (hitRect) or the speech bubble below (bubbleRect) maintains the hovered state.
            // Using bubbleRect rather than the full AABB bounding box (visualRect) ensures the empty
            // transparent space above the bubble (which overlays adjacent controls like Music) does not keep hover active.
            return currentlyHovered
                ? (hitRect.Contains(canvasMouse) || bubbleRect.Contains(canvasMouse))
                : hitRect.Contains(canvasMouse);
        }

        public static Vector2 ScreenToCanvasPoint(Vector2 screenPoint, float screenWidth, float screenHeight,
            float canvasWidth = OriginalCanvasWidth, float canvasHeight = OriginalCanvasHeight)
        {
            float scale = Mathf.Min(screenWidth / canvasWidth, screenHeight / canvasHeight);
            if (scale <= 0f)
                return Vector2.zero;
            float left = (screenWidth - canvasWidth * scale) * 0.5f;
            float top = (screenHeight - canvasHeight * scale) * 0.5f;
            return new Vector2((screenPoint.x - left) / scale, (screenPoint.y - top) / scale);
        }

        public static Vector2 GetOriginalCanvasMousePosition()
        {
            // IMGUI transforms Event.current.mousePosition into the active GUI.matrix
            // coordinate space before controls and custom drawing are evaluated.
            return Event.current != null ? Event.current.mousePosition : Vector2.zero;
        }

        public static void DrawCornerTooltipBubble(Rect visualRect, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Authentic Flash speech bubble below button: y=28..45 (17px height)
            Rect bubbleRect = new Rect(visualRect.x, visualRect.y + 17f, visualRect.width, 17f);

            // Draw bubble background & border
            DrawSolidRect(new Rect(bubbleRect.x + 1f, bubbleRect.y + 3f, bubbleRect.width - 2f, 13f), Color.white);
            DrawOutline(new Rect(bubbleRect.x + 1f, bubbleRect.y + 3f, bubbleRect.width - 2f, 13f), Color.black);

            // Pointer arrow pointing up to icon
            float pointerX = Mathf.Round(bubbleRect.x + bubbleRect.width * 0.5f);
            DrawSolidRect(new Rect(pointerX - 2f, bubbleRect.y + 1f, 4f, 2f), Color.white);
            DrawOutline(new Rect(pointerX - 2f, bubbleRect.y, 4f, 3f), Color.black);

            // Render label using MutinyBitmapFont Dangle text
            MutinyBitmapFont.DrawDangleText(
                new Rect(bubbleRect.x, bubbleRect.y + 3f, bubbleRect.width, 12f),
                text, Color.black, TextAnchor.MiddleCenter, -1);
        }

        private static readonly string[] OriginalWeaponOrder =
        {
            "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle",
            "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull",
            "mine", "cannon", "anchor", "voodooDoll", "tidalWave"
        };

        private void Start()
        {
            EnsureReferences();
        }

        private void Update()
        {
            EnsureReferences();
            m_ActionPanelTickAccumulator += Time.unscaledDeltaTime;
            while (m_ActionPanelTickAccumulator >= OriginalUiTickSeconds)
            {
                m_ActionPanelTickAccumulator -= OriginalUiTickSeconds;
                AdvanceActionPanelAnimationTick();
                AdvanceTeamHealthAnimationTick();
                AdvanceQuitPromptAnimationTick();
                AdvanceGameEndPopupAnimationTick();
            }
        }

        private void AdvanceQuitPromptAnimationTick()
        {
            AdvanceQuitPromptState(m_QuitPromptShow, ref m_QuitPromptAlpha);
        }

        internal static void AdvanceQuitPromptState(bool show, ref float alpha)
        {
            alpha = show
                ? Mathf.Min(1f, alpha + OriginalPopupAlphaStep)
                : Mathf.Max(0f, alpha - OriginalPopupAlphaStep);
        }

        internal static void AdvanceGameEndPopupState(bool show, ref float alpha)
        {
            AdvanceQuitPromptState(show, ref alpha);
        }

        public static MutinyGameEndPopupKind ResolveGameEndPopupKind(GameOverResult result, int levelIndex)
        {
            if (result == GameOverResult.Team1Wins)
                return levelIndex == MutinyFrontendController.SinglePlayerLevelCount
                    ? MutinyGameEndPopupKind.GameComplete
                    : MutinyGameEndPopupKind.LevelComplete;
            if (result == GameOverResult.Team2Wins || result == GameOverResult.Draw)
                return MutinyGameEndPopupKind.LevelFailed;
            return MutinyGameEndPopupKind.None;
        }

        internal static int AdvanceDisplayedScore(int displayedScore, int targetScore, int step)
        {
            return displayedScore >= targetScore ? targetScore : Mathf.Min(targetScore, displayedScore + step);
        }

        private void AdvanceGameEndPopupAnimationTick()
        {
            SynchronizeGameEndPopup();
            if (!m_GameEndPopupShow)
                return;

            AdvanceGameEndPopupState(m_GameEndPopupShow, ref m_GameEndPopupAlpha);
            if (m_GameEndPopupKind == MutinyGameEndPopupKind.LevelComplete)
            {
                m_GameEndDisplayedLevelScore = AdvanceDisplayedScore(
                    m_GameEndDisplayedLevelScore, m_GameEndTargetLevelScore, 287);
                m_GameEndDisplayedTotalScore = AdvanceDisplayedScore(
                    m_GameEndDisplayedTotalScore, m_GameEndTargetTotalScore, 347);
            }
            else if (m_GameEndPopupKind == MutinyGameEndPopupKind.LevelFailed ||
                     m_GameEndPopupKind == MutinyGameEndPopupKind.GameComplete)
            {
                m_GameEndDisplayedTotalScore = AdvanceDisplayedScore(
                    m_GameEndDisplayedTotalScore, m_GameEndTargetTotalScore, 347);
            }
        }

        /// <summary>
        /// Production GameOver observer.  It is public so the parity harness can
        /// drive exactly the same result-to-popup boundary without faking HUD
        /// state; Update invokes it before every original-rate popup tick.
        /// </summary>
        public bool SynchronizeGameEndPopup()
        {
            if (TurnManager == null || TurnManager.CurrentPhase != TurnPhase.GameOver || m_GameEndPopupShow)
                return false;

            if (Speech != null && Speech.IsPlayingEndingLine)
                return false;

            OpenGameEndPopupForCurrentResult();
            return m_GameEndPopupShow;
        }

        private void OpenGameEndPopupForCurrentResult()
        {
            int levelIndex = LevelController != null ? LevelController.CurrentLevelIndex : 1;
            m_GameEndPopupKind = ResolveGameEndPopupKind(TurnManager.GameResult, levelIndex);
            if (m_GameEndPopupKind == MutinyGameEndPopupKind.None)
                return;

            m_GameEndTargetLevelScore = LevelController != null ? LevelController.LastCompletedLevelScore : 0;
            m_GameEndTargetTotalScore = LevelController != null ? LevelController.SinglePlayerScore : 0;
            m_GameEndDisplayedLevelScore = 0;
            m_GameEndDisplayedTotalScore = 0;
            m_GameEndPopupShow = true;
            MutinyDebugLog.Info("HUD",
                $"END-POP open kind={m_GameEndPopupKind} level={levelIndex} levelScore={m_GameEndTargetLevelScore} totalScore={m_GameEndTargetTotalScore}", this);
        }

        public static MutinyCornerToggleVisualState ResolveCornerToggleVisualState(bool isEnabled, bool isHovered)
        {
            if (isEnabled)
                return isHovered ? MutinyCornerToggleVisualState.OnOver : MutinyCornerToggleVisualState.OnUp;
            return isHovered ? MutinyCornerToggleVisualState.OffOver : MutinyCornerToggleVisualState.OffUp;
        }

        private void AdvanceTeamHealthAnimationTick()
        {
            m_Team1HealthFrame = SlideFrame(
                m_Team1HealthFrame, ResolveTeamHealthTargetFrame(TurnManager != null ? TurnManager.Team1 : null));
            m_Team2HealthFrame = SlideFrame(
                m_Team2HealthFrame, ResolveTeamHealthTargetFrame(TurnManager != null ? TurnManager.Team2 : null));
        }

        public static int ResolveTeamHealthTargetFrame(MutinyTeam team)
        {
            if (team == null || team.Characters == null || team.Characters.Count == 0)
                return 1;

            float health = 0f;
            float maxHealth = 0f;
            for (int i = 0; i < team.Characters.Count; i++)
            {
                MutinyCharacter character = team.Characters[i];
                if (character == null)
                    continue;
                health += Mathf.Max(0f, character.Health);
                maxHealth += Mathf.Max(0f, character.MaxHealth);
            }

            if (maxHealth <= 0f)
                return 1;
            return Mathf.Clamp(1 + Mathf.FloorToInt(96f * health / maxHealth), 1, 97);
        }

        public static int SlideFrame(int currentFrame, int targetFrame)
        {
            currentFrame = Mathf.Clamp(currentFrame, 1, 97);
            targetFrame = Mathf.Clamp(targetFrame, 1, 97);
            return currentFrame < targetFrame ? currentFrame + 1 :
                currentFrame > targetFrame ? currentFrame - 1 : currentFrame;
        }

        private void AdvanceActionPanelAnimationTick()
        {
            bool shouldOpen = PlayerInput != null && PlayerInput.IsActionMenuOpen &&
                              TurnManager != null && TurnManager.CurrentTeam != null &&
                              !TurnManager.CurrentTeam.IsAiControlled &&
                              TurnManager.CurrentTeam.SelectedCharacter != null &&
                              TurnManager.CurrentTeam.SelectedCharacter.IsAlive;

            AdvanceActionPanelState(shouldOpen, ref m_ActionPanelAlpha, ref m_ActionPanelContentsActive);
        }

        internal static void AdvanceActionPanelState(
            bool shouldOpen, ref float panelAlpha, ref bool contentsActive)
        {
            if (shouldOpen)
            {
                panelAlpha += OriginalPanelAlphaStep;
                if (panelAlpha >= 1f)
                {
                    panelAlpha = 1f;
                    contentsActive = true;
                }
            }
            else
            {
                panelAlpha -= OriginalPanelAlphaStep;
                if (panelAlpha <= 0f)
                    panelAlpha = -OriginalPanelAlphaStep;
                contentsActive = false;
            }
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

            m_OriginalTitleTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                fontSize = 10,
                padding = new RectOffset(0, 0, 0, 0)
            };
            m_OriginalTitleTextStyle.normal.textColor = Color.white;

            m_OriginalDescriptionTextStyle = new GUIStyle(m_OriginalTitleTextStyle)
            {
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                fontSize = 9
            };

            m_OriginalAmmoTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                fontSize = 8,
                padding = new RectOffset(0, 0, 0, 0)
            };
            m_OriginalAmmoTextStyle.normal.textColor = Color.white;

            m_RedWeaponPanel = Resources.Load<Texture2D>("UI/weapon_select_red");
            m_BlueWeaponPanel = Resources.Load<Texture2D>("UI/weapon_select_blue");
            if (m_RedWeaponPanel != null) m_RedWeaponPanel.filterMode = FilterMode.Point;
            if (m_BlueWeaponPanel != null) m_BlueWeaponPanel.filterMode = FilterMode.Point;

            m_WeaponSlotDisabledTexture = LoadPointTexture("UI/weapon_slot_disabled");
            m_WeaponSlotRedUpTexture = LoadPointTexture("UI/weapon_slot_red_up");
            m_WeaponSlotBlueUpTexture = LoadPointTexture("UI/weapon_slot_blue_up");
            m_WeaponSlotOverTexture = LoadPointTexture("UI/weapon_slot_over");
            m_InfiniteAmmoTexture = LoadPointTexture("UI/weapon_ammo_infinite");

            m_ThrowDisabledTexture = Resources.Load<Texture2D>("UI/button_throw_disabled");
            if (m_ThrowDisabledTexture != null) m_ThrowDisabledTexture.filterMode = FilterMode.Point;

            m_ThrowRedUpTexture = LoadPointTexture("UI/button_throw_red_up");
            m_ThrowRedOverTexture = LoadPointTexture("UI/button_throw_red_over");
            m_ThrowBlueUpTexture = LoadPointTexture("UI/button_throw_blue_up");
            m_ThrowBlueOverTexture = LoadPointTexture("UI/button_throw_blue_over");

            m_EndTurnRedUpTexture = LoadPointTexture("UI/button_end_turn_red_up");
            m_EndTurnRedOverTexture = LoadPointTexture("UI/button_end_turn_red_over");
            m_EndTurnBlueUpTexture = LoadPointTexture("UI/button_end_turn_blue_up");
            m_EndTurnBlueOverTexture = LoadPointTexture("UI/button_end_turn_blue_over");

            m_RedCancelButton = Resources.Load<Texture2D>("UI/button_cancel_red");
            if (m_RedCancelButton != null) m_RedCancelButton.filterMode = FilterMode.Point;

            m_BlueCancelButton = Resources.Load<Texture2D>("UI/button_cancel_blue");
            if (m_BlueCancelButton != null) m_BlueCancelButton.filterMode = FilterMode.Point;

            m_Team1Panel = LoadPointTexture("UI/BattleHUD/team1_panel");
            m_Team2Panel = LoadPointTexture("UI/BattleHUD/team2_panel");
            m_SpeechBubbleTexture = LoadPointTexture("UI/BattleHUD/speech_bubble");
            m_Team1Portrait = LoadPointTexture("UI/BattleHUD/team1_portrait");
            // These are exported original button backgrounds also used by the
            // front-end. Text remains an independent bitmap-font child just as in
            // the Flash MovieClip; it is not baked into one state image.
            m_CornerButtonTexture = LoadPointTexture("UI/Frontend/button_small");
            m_CornerButtonOverTexture = LoadPointTexture("UI/Frontend/button_small_over");
            m_CornerBackButtonTexture = LoadPointTexture("UI/Frontend/button_back");
            m_CornerBackButtonOverTexture = LoadPointTexture("UI/Frontend/button_back_over");
            m_QuitCornerUpTexture = LoadPointTexture("UI/CornerControls/quit_up");
            m_QuitCornerOverTexture = LoadPointTexture("UI/CornerControls/quit_over");
            m_MusicCornerOnUpTexture = LoadPointTexture("UI/CornerControls/music_on_up");
            m_MusicCornerOnOverTexture = LoadPointTexture("UI/CornerControls/music_on_over");
            m_MusicCornerOffUpTexture = LoadPointTexture("UI/CornerControls/music_off_up");
            m_MusicCornerOffOverTexture = LoadPointTexture("UI/CornerControls/music_off_over");
            m_SfxCornerOnUpTexture = LoadPointTexture("UI/CornerControls/sfx_on_up");
            m_SfxCornerOnOverTexture = LoadPointTexture("UI/CornerControls/sfx_on_over");
            m_SfxCornerOffUpTexture = LoadPointTexture("UI/CornerControls/sfx_off_up");
            m_SfxCornerOffOverTexture = LoadPointTexture("UI/CornerControls/sfx_off_over");
            m_OpponentPortraits = Resources.LoadAll<Texture2D>("UI/BattleHUD/Opponents");
            Array.Sort(m_OpponentPortraits, (a, b) => ParseNumericTextureName(a).CompareTo(ParseNumericTextureName(b)));
            for (int i = 0; i < OriginalWeaponOrder.Length; i++)
            {
                string weaponType = OriginalWeaponOrder[i];
                m_WeaponIcons[weaponType] = Resources.Load<Texture2D>($"UI/WeaponIcons/{weaponType}");
                if (m_WeaponIcons[weaponType] != null)
                    m_WeaponIcons[weaponType].filterMode = FilterMode.Point;
            }

            m_StylesInitialized = true;
        }

        private static Texture2D LoadPointTexture(string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture != null)
                texture.filterMode = FilterMode.Point;
            return texture;
        }

        private static int ParseNumericTextureName(Texture2D texture)
        {
            return texture != null && int.TryParse(texture.name, out int value) ? value : int.MaxValue;
        }

        private void OnGUI()
        {
            EnsureReferences();
            InitStyles();

            DrawSpeechBubble();
            DrawOriginalBattleHud();
            DrawOriginalCornerControls();
            DrawBottomBar();
            DrawIngameText();

            if (TurnManager != null && TurnManager.CurrentPhase == TurnPhase.GameOver)
            {
                DrawOriginalGameEndPopup();
            }

            if (m_ShowLevelSelect)
            {
                DrawLevelSelectModal();
            }
        }

        private void DrawIngameText()
        {
            if (IngameText == null || !IngameText.IsVisible ||
                string.IsNullOrEmpty(IngameText.VisibleText))
                return;

            float scale = Mathf.Min(Screen.width / OriginalCanvasWidth,
                Screen.height / OriginalCanvasHeight);
            float left = (Screen.width - OriginalCanvasWidth * scale) * 0.5f;
            float top = (Screen.height - OriginalCanvasHeight * scale) * 0.5f;
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f),
                Quaternion.identity, new Vector3(scale, scale, 1f));

            // Stage instance "text" is at (275,400); its textField's DangleFont
            // child is placed 10 px below the clip origin and centered on x=275.
            GUI.BeginGroup(new Rect(0f, 0f, OriginalCanvasWidth, OriginalCanvasHeight));
            MutinyBitmapFont.DrawDangleText(
                new Rect(25f, IngameText.ClipY + 10f, 500f, 13f),
                IngameText.VisibleText, Color.white, TextAnchor.MiddleCenter, 0, 13);
            GUI.EndGroup();
            GUI.matrix = oldMatrix;
        }

        private void DrawSpeechBubble()
        {
            if (Speech == null || !Speech.IsBubbleVisible)
                return;

            Camera gameCamera = Camera.main;
            if (gameCamera == null)
                return;

            Vector3 projected = gameCamera.WorldToScreenPoint(Speech.BubbleWorldPosition);
            if (projected.z <= 0f)
                return;

            float scale = Mathf.Min(Screen.width / OriginalCanvasWidth, Screen.height / OriginalCanvasHeight);
            float left = (Screen.width - OriginalCanvasWidth * scale) * 0.5f;
            float top = (Screen.height - OriginalCanvasHeight * scale) * 0.5f;
            float x = (projected.x - left) / scale;
            float y = (Screen.height - projected.y - top) / scale;
            // SWF DefineShape 463 is 244x132 and centred on the bubble clip.
            Rect bubbleRect = new Rect(x - 122f, y - 66f, 244f, 132f);
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));
            if (m_SpeechBubbleTexture != null)
                GUI.DrawTexture(bubbleRect, m_SpeechBubbleTexture, ScaleMode.StretchToFill, true);

            // DefineSprite 465: textHolder at (-75,-21), DangleFont at (-25,-10)
            // relative to the clip registration point. Keep the same visible field
            // for typing and click-to-complete (the Flash mouseDown used a wrong path).
            MutinyBitmapFont.DrawSpeechText(
                new Rect(x - 100f, y - 31f, 220f, 92f), Speech.VisibleText,
                TextAnchor.UpperLeft, 0, 13);

            Event evt = Event.current;
            if (evt != null && evt.type == EventType.MouseDown && evt.button == 0 &&
                new Rect(0f, 0f, OriginalCanvasWidth, OriginalCanvasHeight).Contains(GetOriginalCanvasMousePosition()))
            {
                Speech.Click();
                evt.Use();
            }
            GUI.matrix = oldMatrix;
        }

        private void DrawOriginalCornerControls()
        {
            if (TurnManager == null)
                return;

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            float scale = Mathf.Min(Screen.width / OriginalCanvasWidth, Screen.height / OriginalCanvasHeight);
            float left = (Screen.width - OriginalCanvasWidth * scale) * 0.5f;
            float top = (Screen.height - OriginalCanvasHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));

            Rect sfxVisualRect = ResolveOriginalCornerVisualRect(MutinyCornerControl.Sfx);
            Rect musicVisualRect = ResolveOriginalCornerVisualRect(MutinyCornerControl.Music);
            Rect quitVisualRect = ResolveOriginalCornerVisualRect(MutinyCornerControl.Quit);
            Rect sfxHitRect = ResolveOriginalCornerHitRect(MutinyCornerControl.Sfx);
            Rect musicHitRect = ResolveOriginalCornerHitRect(MutinyCornerControl.Music);
            Rect quitHitRect = ResolveOriginalCornerHitRect(MutinyCornerControl.Quit);
            MutinyAudioManager audio = MutinyAudioManager.Instance;

            Vector2 mousePosition = GetOriginalCanvasMousePosition();
            bool sfxHovered = IsCornerHovered(MutinyCornerControl.Sfx, mousePosition, m_SfxHovered);
            bool musicHovered = IsCornerHovered(MutinyCornerControl.Music, mousePosition, m_MusicHovered);
            bool quitHovered = IsCornerHovered(MutinyCornerControl.Quit, mousePosition, m_QuitHovered);
            UpdateCornerHover(ref m_SfxHovered, sfxHovered);
            UpdateCornerHover(ref m_MusicHovered, musicHovered);
            UpdateCornerHover(ref m_QuitHovered, quitHovered);

            // Draw unhovered controls first, then hovered control on top so its speech bubble is never overlapped
            if (!quitHovered)
                DrawOriginalCornerSprite(quitVisualRect, m_QuitCornerUpTexture, false, MutinyCornerControl.Quit);
            if (!musicHovered)
                DrawOriginalCornerSprite(musicVisualRect,
                    ResolveCornerToggleTexture(true, audio != null && audio.MusicEnabled, false), false, MutinyCornerControl.Music);
            if (!sfxHovered)
                DrawOriginalCornerSprite(sfxVisualRect,
                    ResolveCornerToggleTexture(false, audio != null && audio.SfxEnabled, false), false, MutinyCornerControl.Sfx);

            if (quitHovered)
                DrawOriginalCornerSprite(quitVisualRect, m_QuitCornerOverTexture, true, MutinyCornerControl.Quit);
            if (musicHovered)
                DrawOriginalCornerSprite(musicVisualRect,
                    ResolveCornerToggleTexture(true, audio != null && audio.MusicEnabled, true), true, MutinyCornerControl.Music);
            if (sfxHovered)
                DrawOriginalCornerSprite(sfxVisualRect,
                    ResolveCornerToggleTexture(false, audio != null && audio.SfxEnabled, true), true, MutinyCornerControl.Sfx);

            // While hovered, clicking either the icon or the speech bubble below activates the button
            Rect sfxBubbleRect = ResolveOriginalCornerBubbleRect(MutinyCornerControl.Sfx);
            Rect musicBubbleRect = ResolveOriginalCornerBubbleRect(MutinyCornerControl.Music);
            Rect quitBubbleRect = ResolveOriginalCornerBubbleRect(MutinyCornerControl.Quit);

            if (!MutinyTransitionManager.IsTransitionActive)
            {
                if (GUI.Button(sfxHitRect, GUIContent.none, GUIStyle.none) ||
                    (sfxHovered && GUI.Button(sfxBubbleRect, GUIContent.none, GUIStyle.none)))
                    ToggleCornerSfx();
                if (GUI.Button(musicHitRect, GUIContent.none, GUIStyle.none) ||
                    (musicHovered && GUI.Button(musicBubbleRect, GUIContent.none, GUIStyle.none)))
                    ToggleCornerMusic();
                if (GUI.Button(quitHitRect, GUIContent.none, GUIStyle.none) ||
                    (quitHovered && GUI.Button(quitBubbleRect, GUIContent.none, GUIStyle.none)))
                    OpenQuitPrompt();
            }

            if (m_QuitPromptAlpha > 0f)
                DrawQuitPrompt();

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }

        private void UpdateCornerHover(ref bool previous, bool current)
        {
            if (current && !previous)
                MutinyAudioManager.Instance?.PlaySFX("rollover");
            previous = current;
        }

        private Texture2D ResolveCornerToggleTexture(bool isMusic, bool enabled, bool hovered)
        {
            MutinyCornerToggleVisualState state = ResolveCornerToggleVisualState(enabled, hovered);
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

        private static void DrawOriginalCornerSprite(Rect rect, Texture2D texture, bool hovered, MutinyCornerControl control)
        {
            if (texture != null)
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            else if (hovered)
                DrawCornerTooltipBubble(rect, ResolveOriginalCornerTooltip(control));
        }

        private void DrawCornerButton(Rect rect, string label, bool hovered)
        {
            Texture2D tex = (hovered && m_CornerButtonOverTexture != null) ? m_CornerButtonOverTexture : m_CornerButtonTexture;
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);
            else
            {
                DrawSolidRect(rect, hovered ? new Color32(112, 60, 37, 255) : new Color32(57, 43, 34, 255));
                DrawOutline(rect, Color.black);
            }

            MutinyBitmapFont.DrawDangleText(rect, label, hovered ? Color.yellow : Color.white,
                TextAnchor.MiddleCenter, -1, 5);
        }

        private void DrawQuitPrompt()
        {
            Color prior = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(m_QuitPromptAlpha));

            // DefineShape_326 is 350x260 px, centered at (275,200).  The old
            // 232x114 approximation forced both labels into half-width buttons.
            Rect panel = ResolveOriginalPopupPanelRect();
            DrawPopupSolid(new Rect(panel.x + 2f, panel.y + 2f, panel.width, panel.height), Color.black);
            DrawPopupSolid(panel, new Color32(51, 51, 51, 255));
            DrawPopupOutline(panel, new Color32(239, 49, 28, 255));
            DrawPopupOutline(new Rect(panel.x + 3f, panel.y + 3f, panel.width - 6f, panel.height - 6f), Color.black);
            MutinyBitmapFont.DrawPirateText(new Rect(panel.x, 84f, panel.width, 28f),
                "quit level", false, true, -3);

            // The original continue_game MovieClip is a 280x24 button placed at
            // popup y=45.  Back to menu is the user-authorized companion action,
            // placed on the matching y=80 row without reducing either hit area.
            Rect continueRect = ResolveOriginalPopupPrimaryButtonRect();
            Rect backRect = ResolveOriginalPopupSecondaryButtonRect();
            bool continueHovered = continueRect.Contains(GetOriginalCanvasMousePosition());
            bool backHovered = backRect.Contains(GetOriginalCanvasMousePosition());
            DrawPopupButton(continueRect, "continue", continueHovered);
            DrawPopupButton(backRect, "back to menu", backHovered);

            if (!MutinyTransitionManager.IsTransitionActive && GUI.Button(continueRect, GUIContent.none, GUIStyle.none))
                ContinueQuitPrompt();
            if (!MutinyTransitionManager.IsTransitionActive && GUI.Button(backRect, GUIContent.none, GUIStyle.none))
            {
                MutinyTransitionManager.RequestTransition(() => BackToSinglePlayerMenu(), showLoading: false);
            }

            GUI.color = prior;
        }

        private void DrawPopupButton(Rect rect, string label, bool hovered)
        {
            Texture2D tex = (hovered && m_CornerBackButtonOverTexture != null) ? m_CornerBackButtonOverTexture : m_CornerBackButtonTexture;
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);
            else
                DrawCornerButton(rect, label, hovered);
            MutinyBitmapFont.DrawPirateText(rect, label, hovered, true, -3);
        }

        private void DrawPopupSolid(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(m_QuitPromptAlpha));
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previous;
        }

        private void DrawPopupOutline(Rect rect, Color color)
        {
            DrawPopupSolid(new Rect(rect.x, rect.y, rect.width, 1f), color);
            DrawPopupSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            DrawPopupSolid(new Rect(rect.x, rect.y, 1f, rect.height), color);
            DrawPopupSolid(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }


        public bool OpenQuitPrompt()
        {
            if (m_QuitPromptShow || m_QuitPromptAlpha > 0f || m_GameEndPopupShow ||
                (TurnManager != null && TurnManager.CurrentPhase == TurnPhase.GameOver))
                return false;

            m_QuitPromptShow = true;
            Debug.Log("[MutinyHUD] HUD-CORNER-01 quit prompt opened", this);
            return true;
        }

        public bool ContinueQuitPrompt()
        {
            if (!m_QuitPromptShow && m_QuitPromptAlpha <= 0f)
                return false;

            m_QuitPromptShow = false;
            Debug.Log("[MutinyHUD] HUD-CORNER-03 quit prompt continued", this);
            return true;
        }

        public void ToggleCornerMusic()
        {
            MutinyAudioManager.Instance?.ToggleMusic();
        }

        public void ToggleCornerSfx()
        {
            MutinyAudioManager.Instance?.ToggleSFX();
        }

        public bool BackToSinglePlayerMenu()
        {
            MutinyFrontendController frontend = FindAnyObjectByType<MutinyFrontendController>();
            if (frontend == null)
            {
                Debug.LogWarning("[MutinyHUD] HUD-CORNER-04 cannot return to level select: front-end controller was not found.", this);
                return false;
            }

            bool returned = frontend.ReturnToSinglePlayerLevelSelect();
            if (returned)
            {
                m_QuitPromptShow = false;
                m_QuitPromptAlpha = 0f;
                m_GameEndPopupShow = false;
                m_GameEndPopupAlpha = 0f;
                Debug.Log("[MutinyHUD] HUD-CORNER-04 back to single-player level select", this);
            }
            return returned;
        }

        private void DrawOriginalBattleHud()
        {
            if (TurnManager == null || TurnManager.Team1 == null || TurnManager.Team2 == null)
                return;

            const float canvasWidth = 550f;
            const float canvasHeight = 400f;
            float scale = Mathf.Min(Screen.width / canvasWidth, Screen.height / canvasHeight);
            float left = (Screen.width - canvasWidth * scale) * 0.5f;
            float top = (Screen.height - canvasHeight * scale) * 0.5f;
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(left, top, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            DrawOriginalMap();
            DrawOriginalTeamHealth();

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawOriginalTeamHealth()
        {
            Rect team1HealthRect = ResolveOriginalTeam1HealthRect();
            DrawSolidRect(team1HealthRect, new Color32(49, 49, 49, 255));
            float redWidth = Mathf.Clamp(m_Team1HealthFrame - 1, 0, 96);
            if (redWidth > 0f)
                DrawSolidRect(new Rect(team1HealthRect.x, team1HealthRect.y, redWidth, team1HealthRect.height),
                    new Color32(255, 56, 41, 255));
            if (m_Team1Panel != null)
                GUI.DrawTexture(ResolveOriginalTeam1PanelRect(), m_Team1Panel, ScaleMode.StretchToFill, true);
            if (m_Team1Portrait != null)
                GUI.DrawTexture(ResolveOriginalTeam1PortraitRect(), m_Team1Portrait, ScaleMode.StretchToFill, true);

            Rect team2HealthRect = ResolveOriginalTeam2HealthRect();
            DrawSolidRect(team2HealthRect, new Color32(49, 49, 49, 255));
            float blueWidth = Mathf.Clamp(m_Team2HealthFrame - 1, 0, 96);
            if (blueWidth > 0f)
                DrawSolidRect(new Rect(team2HealthRect.xMax - blueWidth, team2HealthRect.y,
                    blueWidth, team2HealthRect.height), new Color32(51, 95, 255, 255));
            if (m_Team2Panel != null)
                GUI.DrawTexture(ResolveOriginalTeam2PanelRect(), m_Team2Panel, ScaleMode.StretchToFill, true);

            int level = LevelController != null ? LevelController.CurrentLevelIndex : 1;
            if (m_OpponentPortraits.Length > 0)
            {
                int portraitIndex = Mathf.Clamp(level - 1, 0, m_OpponentPortraits.Length - 1);
                if (m_OpponentPortraits[portraitIndex] != null)
                {
                    GUI.DrawTexture(ResolveOriginalTeam2PortraitRect(),
                        m_OpponentPortraits[portraitIndex], ScaleMode.StretchToFill, true);
                }
            }
        }

        public static Rect ResolveOriginalTeam1PanelRect()
        {
            return new Rect(OriginalTeam1OriginX - 65.95f, OriginalTeamOriginY - 16.95f, 132f, 34f);
        }

        public static Rect ResolveOriginalTeam2PanelRect()
        {
            return new Rect(OriginalTeam2OriginX - 65.95f, OriginalTeamOriginY - 16.95f, 132f, 34f);
        }

        public static Rect ResolveOriginalTeam1HealthRect()
        {
            // team1.item is at +28px; shape 811 spans x=-62..34, y=1..9.
            return new Rect(OriginalTeam1OriginX + 28f - 62f, OriginalTeamOriginY + 1f, 96f, 8f);
        }

        public static Rect ResolveOriginalTeam2HealthRect()
        {
            // team2.item is at 0px and uses the same 96x8 shape.
            return new Rect(OriginalTeam2OriginX - 62f, OriginalTeamOriginY + 1f, 96f, 8f);
        }

        public static Rect ResolveOriginalTeam1PortraitRect()
        {
            // Bitmap 1999 is registered at (-59,-10) inside team1.
            return new Rect(OriginalTeam1OriginX - 59f, OriginalTeamOriginY - 10f, 22f, 21f);
        }

        public static Rect ResolveOriginalTeam2PortraitRect()
        {
            // opponent_image is at (+50,+1); the shared 40x58 frame canvas
            // spans x=-22..18 and y=-44..14 around its registration point.
            return new Rect(OriginalTeam2OriginX + 50f - 22f,
                OriginalTeamOriginY + 1f - 44f, 40f, 58f);
        }

        private void DrawOriginalMap()
        {
            MutinyLevelData level = GetMapLevelData();
            if (level == null || level.Width <= 0 || level.Height <= 0)
                return;

            const float holderX = OriginalMapHolderX;
            const float holderY = OriginalMapHolderY;
            const float dotSize = 3f;
            for (int x = -2; x < level.Width + 2; x++)
            {
                for (int y = -2; y < level.Height + 2; y++)
                {
                    bool occupied = x >= 0 && y >= 0 && x < level.Width && y < level.Height &&
                                    level.Terrain != null && level.Terrain[y, x] != null;
                    byte alpha = occupied ? (byte)128 : (byte)51;
                    DrawSolidRect(new Rect(holderX + x * dotSize, holderY + y * dotSize, dotSize, dotSize),
                        new Color32(0, 0, 0, alpha));
                }
            }

            MutinyTreasureChest[] chests = FindObjectsByType<MutinyTreasureChest>();
            for (int i = 0; i < chests.Length; i++)
            {
                MutinyTreasureChest chest = chests[i];
                if (chest == null || chest.IsFinished || chest.PixelY <= -64f)
                    continue;
                int x = ProjectMapCoordinate(chest.PixelX, 0);
                int y = ProjectMapCoordinate(chest.PixelY, 0);
                if (y > -6)
                    DrawSolidRect(new Rect(holderX + x - 1f, holderY + y - 1f, dotSize, dotSize),
                        new Color32(255, 255, 0, 128));
            }

            DrawMapTeam(TurnManager.Team1, new Color32(255, 56, 41, 255), holderX, holderY, level, dotSize);
            DrawMapTeam(TurnManager.Team2, new Color32(51, 95, 255, 255), holderX, holderY, level, dotSize);

            Rect borderRect = ResolveOriginalMapBorderRect(level.Width, level.Height);
            DrawOutline(new Rect(borderRect.x + 1f, borderRect.y + 1f, borderRect.width, borderRect.height),
                new Color32(0, 0, 0, 52));
            DrawOutline(borderRect, Color.white);
        }

        public static Rect ResolveOriginalMapBorderRect(int levelWidth, int levelHeight)
        {
            // Map.reset keeps the content origin at mapHolder (20,20). Its four
            // corner symbols make the visible frame extend exactly 10px beyond
            // the level's 3px-per-tile area on every side.
            return new Rect(OriginalMapHolderX - 10f, OriginalMapHolderY - 10f,
                Mathf.Max(0, levelWidth) * 3f + 20f,
                Mathf.Max(0, levelHeight) * 3f + 20f);
        }

        private static void DrawMapTeam(
            MutinyTeam team, Color color, float holderX, float holderY, MutinyLevelData level, float dotSize)
        {
            if (team == null || team.Characters == null)
                return;

            for (int i = 0; i < team.Characters.Count; i++)
            {
                MutinyCharacter character = team.Characters[i];
                if (character == null || !character.IsAlive || character.PhysicsBody == null)
                    continue;
                PhysicsBodyState state = character.PhysicsBody.State;
                int x = ProjectMapCoordinate(state.X, 0);
                int y = ProjectMapCoordinate(state.Y, 0);
                if (x <= -6 || x >= (level.Width + 2) * dotSize ||
                    y <= -6 || y >= (level.Height + 2) * dotSize)
                    continue;
                DrawSolidRect(new Rect(holderX + x - 2f, holderY + y - 2f, dotSize, dotSize), color);
            }
        }

        private MutinyLevelData GetMapLevelData()
        {
            TextAsset xml = LevelController != null ? LevelController.LevelXml : null;
            if (xml != null && (m_CachedMapLevel == null || m_CachedMapXml != xml))
            {
                try
                {
                    m_CachedMapLevel = MutinyLevelXmlParser.Parse(xml.text, xml.name);
                    m_CachedMapXml = xml;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                    m_CachedMapLevel = null;
                    m_CachedMapXml = xml;
                }
            }

            if (m_CachedMapLevel != null)
                return m_CachedMapLevel;

            MutinyLevelRoot root = LevelController != null ? LevelController.CurrentLevel : null;
            if (root == null)
                root = FindAnyObjectByType<MutinyLevelRoot>();
            return root == null ? null : new MutinyLevelData { Width = root.Width, Height = root.Height };
        }

        public static int ProjectMapCoordinate(float pixelCoordinate, int pixelOffset)
        {
            return ((int)(pixelCoordinate * 3f) >> 5) + pixelOffset;
        }

        public static void DrawOutline(Rect rect, Color color)
        {
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
            DrawSolidRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            DrawSolidRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
            DrawSolidRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }

        public static void DrawSolidRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previous;
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

            if (PlayerInput == null || m_ActionPanelAlpha <= 0f)
                return;

            MutinyCharacter selectedChar = TurnManager.CurrentTeam.SelectedCharacter;
            if (selectedChar == null || !selectedChar.IsAlive)
                return;

            float scale = Mathf.Min(Screen.width / 550f, Screen.height / 400f);
            m_OriginalSlotStyle.fontSize = Mathf.Max(9, Mathf.RoundToInt(9f * scale));
            // Panel-space matrix for DangleFont bitmap text rendering.
            // Origin (0,0) is at the top-left of the weapon panel texture (left, top),
            // matching pixel coordinates directly against the 271x247 panel artwork.
            float panelWidth = 271f * scale;
            float panelHeight = 247f * scale;
            float canvasTop = (Screen.height - 400f * scale) * 0.5f;
            float left = (Screen.width - panelWidth) * 0.5f;
            float top = canvasTop + 23f * scale;
            Rect panelRect = new Rect(left, top, panelWidth, panelHeight);
            Matrix4x4 panelMatrix = Matrix4x4.TRS(
                new Vector3(left, top, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));
            Color previousColor = GUI.color;
            bool previousEnabled = GUI.enabled;
            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b,
                previousColor.a * Mathf.Clamp01(m_ActionPanelAlpha));
            bool contentsActive = m_ActionPanelContentsActive && PlayerInput.IsActionMenuOpen;
            string hoveredAction = null;

            Texture2D panelTexture = TurnManager.CurrentTeam.TeamNumber == 2
                ? m_BlueWeaponPanel
                : m_RedWeaponPanel;
            if (panelTexture != null)
                GUI.DrawTexture(panelRect, panelTexture, ScaleMode.StretchToFill, true);
            else
                GUI.Box(panelRect, GUIContent.none, m_PanelStyle);

            Rect throwRect = ScaledRect(left, top, scale, 11f, 29f, 86f, 57f);
            bool throwHovered = throwRect.Contains(Event.current.mousePosition);
            MutinyThrowButtonVisualState throwState = ResolveThrowButtonVisualState(
                TurnManager.CurrentTeam.TeamNumber, selectedChar.CanThrow, throwHovered);
            Texture2D throwTexture = GetThrowButtonTexture(throwState);
            if (throwTexture != null)
                GUI.DrawTexture(throwRect, throwTexture, ScaleMode.StretchToFill, true);

            if (selectedChar.CanThrow)
            {
                if (throwHovered)
                    hoveredAction = "throw character";
                GUI.enabled = previousEnabled && contentsActive;
                if (GUI.Button(throwRect, new GUIContent(string.Empty, "throw character"), m_OriginalSlotStyle))
                    PlayerInput.SelectCharacterThrow();
            }
            else
            {
                if (throwTexture == null)
                {
                    Color origColor = GUI.color;
                    GUI.color = new Color(0.25f, 0.25f, 0.25f, 0.7f);
                    GUI.DrawTexture(throwRect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
                    GUI.color = origColor;
                }

                GUI.enabled = false;
                GUI.Button(throwRect, new GUIContent(string.Empty, "throw character used"), m_OriginalSlotStyle);
            }

            Rect endTurnRect = ScaledRect(left, top, scale, 11f, 93.95f, 86f, 57f);
            bool endTurnHovered = endTurnRect.Contains(Event.current.mousePosition);
            if (endTurnHovered)
                hoveredAction = "end turn";
            Texture2D endTurnTexture = GetEndTurnButtonTexture(
                TurnManager.CurrentTeam.TeamNumber, endTurnHovered);
            if (endTurnTexture != null)
                GUI.DrawTexture(endTurnRect, endTurnTexture, ScaleMode.StretchToFill, true);
            GUI.enabled = previousEnabled && contentsActive;
            if (GUI.Button(endTurnRect,
                    new GUIContent(string.Empty, "end turn"), m_OriginalSlotStyle))
                PlayerInput.EndTurn();

            if (selectedChar.CanThrow)
            {
                Texture2D cancelTex = TurnManager.CurrentTeam.TeamNumber == 2
                    ? m_BlueCancelButton
                    : m_RedCancelButton;
                Rect cancelRect = ScaledRect(left, top, scale, 256f, 0f, 20f, 20f);
                if (cancelTex != null)
                {
                    GUI.DrawTexture(cancelRect, cancelTex, ScaleMode.StretchToFill, true);
                }

                if (cancelRect.Contains(Event.current.mousePosition))
                    hoveredAction = "cancel character";
                GUI.enabled = previousEnabled && contentsActive;
                if (GUI.Button(cancelRect, new GUIContent(string.Empty, "cancel character"), m_OriginalSlotStyle))
                    PlayerInput.ReturnToCharacterSelection();
            }

            for (int i = 0; i < OriginalWeaponOrder.Length; i++)
            {
                GUI.enabled = previousEnabled;
                string weaponType = OriginalWeaponOrder[i];
                int column = i % 5;
                int row = i / 5;
                Rect slot = ScaledRect(left, top, scale, 112f + column * 31f, 29f + row * 43f, 24f, 36f);
                bool available = selectedChar.CanShoot && selectedChar.HasWeapon(weaponType);
                int ammo = selectedChar.GetAmmunition(weaponType);
                m_WeaponIcons.TryGetValue(weaponType, out Texture2D icon);
                bool slotHovered = available && slot.Contains(Event.current.mousePosition);

                Texture2D slotTexture = available
                    ? (TurnManager.CurrentTeam.TeamNumber == 2
                        ? m_WeaponSlotBlueUpTexture
                        : m_WeaponSlotRedUpTexture)
                    : m_WeaponSlotDisabledTexture;
                if (slotTexture != null)
                    GUI.DrawTexture(slot, slotTexture, ScaleMode.StretchToFill, true);

                if (icon != null && available)
                {
                    Rect iconRect = ScaledRect(slot.x, slot.y, scale, 3f, 3f, 18f, 17f);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.StretchToFill, true);
                }

                if (available && ammo < 0 && m_InfiniteAmmoTexture != null)
                {
                    Matrix4x4 ammoSavedMatrix = GUI.matrix;
                    GUI.matrix = panelMatrix;
                    GUI.DrawTexture(
                        ResolveWeaponSlotInfiniteAmmoRect(column, row),
                        m_InfiniteAmmoTexture,
                        ScaleMode.StretchToFill,
                        true);
                    GUI.matrix = ammoSavedMatrix;
                }
                else if (available)
                {
                    Matrix4x4 ammoSavedMatrix = GUI.matrix;
                    GUI.matrix = panelMatrix;
                    MutinyBitmapFont.DrawDangleText(
                        ResolveWeaponSlotAmmoNumberRect(column, row),
                        ammo.ToString("D2"),
                        new Color(1f, 1f, 1f, Mathf.Clamp01(m_ActionPanelAlpha)),
                        TextAnchor.MiddleCenter);
                    GUI.matrix = ammoSavedMatrix;
                }

                if (slotHovered && m_WeaponSlotOverTexture != null)
                    GUI.DrawTexture(slot, m_WeaponSlotOverTexture, ScaleMode.StretchToFill, true);

                if (slotHovered)
                    hoveredAction = weaponType;
                GUI.enabled = previousEnabled && contentsActive && available;
                if (GUI.Button(slot, new GUIContent(string.Empty, weaponType), m_OriginalSlotStyle))
                    PlayerInput.SelectWeapon(weaponType);
            }

            GetOriginalActionCopy(hoveredAction, out string title, out string description);
            GUI.enabled = previousEnabled;
            Matrix4x4 textSavedMatrix = GUI.matrix;
            GUI.matrix = panelMatrix;
            Color textColor = new Color(1f, 1f, 1f, Mathf.Clamp01(m_ActionPanelAlpha));
            MutinyBitmapFont.DrawDangleText(
                new Rect(10.35f, 1f, 200f, 16f),
                title, textColor, TextAnchor.MiddleLeft);
            MutinyBitmapFont.DrawDangleText(
                new Rect(20f, 166f, 238f, 66f),
                description, textColor, TextAnchor.UpperLeft, 0, 12);
            GUI.matrix = textSavedMatrix;
            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        public static MutinyThrowButtonVisualState ResolveThrowButtonVisualState(
            int teamNumber, bool canThrow, bool hovered)
        {
            if (!canThrow)
                return MutinyThrowButtonVisualState.Disabled;
            if (teamNumber == 2)
                return hovered ? MutinyThrowButtonVisualState.BlueOver : MutinyThrowButtonVisualState.BlueUp;
            return hovered ? MutinyThrowButtonVisualState.RedOver : MutinyThrowButtonVisualState.RedUp;
        }

        private Texture2D GetThrowButtonTexture(MutinyThrowButtonVisualState state)
        {
            switch (state)
            {
                case MutinyThrowButtonVisualState.RedUp: return m_ThrowRedUpTexture;
                case MutinyThrowButtonVisualState.RedOver: return m_ThrowRedOverTexture;
                case MutinyThrowButtonVisualState.BlueUp: return m_ThrowBlueUpTexture;
                case MutinyThrowButtonVisualState.BlueOver: return m_ThrowBlueOverTexture;
                default: return m_ThrowDisabledTexture;
            }
        }

        private Texture2D GetEndTurnButtonTexture(int teamNumber, bool hovered)
        {
            if (teamNumber == 2)
                return hovered ? m_EndTurnBlueOverTexture : m_EndTurnBlueUpTexture;
            return hovered ? m_EndTurnRedOverTexture : m_EndTurnRedUpTexture;
        }

        private static Rect ScaledRect(float left, float top, float scale, float x, float y, float width, float height)
        {
            return new Rect(left + x * scale, top + y * scale, width * scale, height * scale);
        }

        public static void GetOriginalActionCopy(string action, out string title, out string description)
        {
            title = "weapons";
            description = "Click one of the options above\nto select it.";
            switch (action)
            {
                case "throw character":
                    title = "throw character";
                    description = "Click your character and drag\nwith the mouse to aim and set\nthe power.\nyou get to use this once per\nturn before you use a weapon.";
                    break;
                case "end turn":
                    title = "end go";
                    description = "click here if you want to finish\nyour turn without using a\nweapon.";
                    break;
                case "cancel character":
                    title = "close";
                    description = "click here to cancel and select\nanother player.";
                    break;
                case "cherryBomb":
                    title = "cherry bomb";
                    description = "Basic weak weapon which\nexplodes on impact.\nClick the cherry bomb and drag\nwith your mouse to aim and set\nthe power.";
                    break;
                case "dynamite":
                    title = "dynamite";
                    description = "This weapon explodes when it\ncomes to rest.\nClick the dynamite and drag\nwith your mouse to aim and set\nthe power.";
                    break;
                case "boulder":
                    title = "boulder";
                    description = "Large boulder which can bash\nother players out of the way.\nClick the boulder and drag\nwith your mouse to aim and set\nthe power.";
                    break;
                case "piecesOfEight":
                    title = "pieces of eight";
                    description = "Eight coins which inflict a small\namount of damage. You get\neight turns with this weapon.\nClick each one and drag to aim\nand set the power.";
                    break;
                case "rumBottle":
                    title = "rum bottle";
                    description = "This weapon explodes on impact\nand sets the nearby area on fire.\nClick it and drag\nwith your mouse to aim and set\nthe power.";
                    break;
                case "banana":
                    title = "banana";
                    description = "This weapon is very bouncy.\nClick and drag with the mouse\nto aim and set the power.\nThen click your mouse again to\nmake it explode.";
                    break;
                case "parachuteBomb":
                    title = "parachute bomb";
                    description = "Click and drag with the mouse\nto aim and set the power.\nAs it drifts down use\nyour cursor as a fan to push it\nleft or right.";
                    break;
                case "woodenCrate":
                    title = "crates";
                    description = "Click anywhere on the stage to\nplace down three crates.\nuse these to form a wall to\nprotect your characters.";
                    break;
                case "gunpowderBarrel":
                    title = "gunpowder barrels";
                    description = "Click anywhere on the stage to\nplace down two barrels.\nthese will explode when hit by\na weapon.";
                    break;
                case "seagull":
                    title = "seagull";
                    description = "Click on the screen to choose a\npath for the seagull to fly.\nThen click repeatedly to poop\non the enemy!";
                    break;
                case "mine":
                    title = "mine";
                    description = "Click the mine and drag\nwith the mouse to aim and\nset the power.\nIt will detonate when\nanother player moves nearby.";
                    break;
                case "cannon":
                    title = "cannon";
                    description = "Drag the cannon into position\nwithin the circle.\nclick and drag the pin at the\nback to turn the cannon.\nrelease it to fire!";
                    break;
                case "anchor":
                    title = "anchor";
                    description = "Click anywhere in the stage to\ndrop a huge anchor down\nonto enemies!";
                    break;
                case "voodooDoll":
                    title = "voodoo doll";
                    description = "choose an enemy player by\nclicking on them. then click\nand drag to throw the doll and\nwatch the enemy helplessly\nfly off in the same direction!";
                    break;
                case "tidalWave":
                    title = "tidal wave";
                    description = "click to send a huge tidal wave\nacross the bottom of the stage.\nIt will affect all players it\nhits.";
                    break;
            }
        }

        private void DrawOriginalGameEndPopup()
        {
            if (!m_GameEndPopupShow && m_GameEndPopupAlpha <= 0f)
                SynchronizeGameEndPopup();
            if (m_GameEndPopupKind == MutinyGameEndPopupKind.None)
                return;

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = Mathf.Min(Screen.width / OriginalCanvasWidth, Screen.height / OriginalCanvasHeight);
            float left = (Screen.width - OriginalCanvasWidth * scale) * 0.5f;
            float top = (Screen.height - OriginalCanvasHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));

            float alpha = Mathf.Clamp01(m_GameEndPopupAlpha);
            GUI.color = new Color(1f, 1f, 1f, alpha);

            // DefineSprite_342_popup places its common dark panel below text fields
            // and independent button clips.  Keep those children separate so score
            // values and hover states are never baked into a static popup texture.
            Rect panel = ResolveOriginalPopupPanelRect();
            DrawGameEndSolid(new Rect(panel.x + 2f, panel.y + 2f, panel.width, panel.height), Color.black, alpha);
            DrawGameEndSolid(panel, new Color32(51, 51, 51, 255), alpha);
            DrawGameEndOutline(panel, new Color32(239, 49, 28, 255), alpha);
            DrawGameEndOutline(new Rect(panel.x + 3f, panel.y + 3f, panel.width - 6f, panel.height - 6f), Color.black, alpha);

            bool complete = m_GameEndPopupKind == MutinyGameEndPopupKind.LevelComplete;
            bool finalComplete = m_GameEndPopupKind == MutinyGameEndPopupKind.GameComplete;
            string title = complete ? "level complete" : finalComplete ? "game complete" : "level failed";
            // Title is at popup y=-105 (= -2100 twips); score labels are at
            // y=-40/-10 for complete and y=-44 for failed.  These are stage
            // coordinates after the popup's (275,200) registration point.
            MutinyBitmapFont.DrawPirateText(new Rect(panel.x, 84f, panel.width, 28f), title, false, true, -3);

            if (complete)
            {
                DrawGameEndScoreRow(175f, 160f, "level score", m_GameEndDisplayedLevelScore, alpha);
                DrawGameEndScoreRow(175f, 190f, "total score", m_GameEndDisplayedTotalScore, alpha);

                Rect nextRect = ResolveOriginalPopupPrimaryButtonRect();
                Rect backRect = ResolveOriginalPopupSecondaryButtonRect();
                DrawGameEndButton(nextRect, "next level", m_CornerButtonTexture, m_CornerButtonOverTexture, alpha);
                DrawGameEndButton(backRect, "back to title", m_CornerBackButtonTexture, m_CornerBackButtonOverTexture, alpha);
                if (!MutinyTransitionManager.IsTransitionActive && alpha > 0f && GUI.Button(nextRect, GUIContent.none, GUIStyle.none))
                {
                    MutinyTransitionManager.RequestTransition(() => AdvanceToNextLevel(), showLoading: true);
                }
                if (!MutinyTransitionManager.IsTransitionActive && alpha > 0f && GUI.Button(backRect, GUIContent.none, GUIStyle.none))
                {
                    MutinyTransitionManager.RequestTransition(() => BackToSinglePlayerMenu(), showLoading: false);
                }
            }
            else if (finalComplete)
            {
                DrawGameEndScoreRow(175f, 155f, "final score", m_GameEndDisplayedTotalScore, alpha);
                Rect congratsRect = ResolveOriginalPopupPrimaryButtonRect();
                DrawGameEndButton(congratsRect, "congratulations", m_CornerButtonTexture, m_CornerButtonOverTexture, alpha);
                if (!MutinyTransitionManager.IsTransitionActive && alpha > 0f && GUI.Button(congratsRect, GUIContent.none, GUIStyle.none))
                {
                    MutinyTransitionManager.RequestTransition(() => CompleteCampaignAndReturnToLevelSelect(), showLoading: false);
                }
            }
            else
            {
                DrawGameEndScoreRow(175f, 156f, "final score", m_GameEndDisplayedTotalScore, alpha);
                Rect restartRect = ResolveOriginalPopupPrimaryButtonRect();
                Rect backRect = ResolveOriginalPopupSecondaryButtonRect();
                DrawGameEndButton(restartRect, "restart level", m_CornerButtonTexture, m_CornerButtonOverTexture, alpha);
                DrawGameEndButton(backRect, "back to title", m_CornerBackButtonTexture, m_CornerBackButtonOverTexture, alpha);
                if (!MutinyTransitionManager.IsTransitionActive && alpha > 0f && GUI.Button(restartRect, GUIContent.none, GUIStyle.none))
                {
                    MutinyTransitionManager.RequestTransition(() => RestartFromGameEndPopup(), showLoading: true);
                }
                if (!MutinyTransitionManager.IsTransitionActive && alpha > 0f && GUI.Button(backRect, GUIContent.none, GUIStyle.none))
                {
                    MutinyTransitionManager.RequestTransition(() => BackToSinglePlayerMenu(), showLoading: false);
                }
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private static void DrawGameEndScoreRow(float x, float y, string label, int score, float alpha)
        {
            Color textColor = new Color(1f, 1f, 1f, alpha);
            // Both source DynamicText instances use align="left".  Keeping
            // separate fields at x=-100 and x=20 avoids a long score colliding
            // with its label.
            MutinyBitmapFont.DrawDangleText(new Rect(x, y, 112f, 13f), label, textColor, TextAnchor.MiddleLeft, 0, 13);
            MutinyBitmapFont.DrawDangleText(new Rect(295f, y, 90f, 13f), score.ToString(), textColor, TextAnchor.MiddleLeft, 0, 13);
        }

        private void DrawGameEndButton(Rect rect, string label, Texture2D texture, Texture2D hoverTexture, float alpha)
        {
            bool hovered = rect.Contains(GetOriginalCanvasMousePosition());
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            Texture2D tex = (hovered && hoverTexture != null) ? hoverTexture : texture;
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);
            else
                DrawGameEndSolid(rect, hovered ? new Color32(112, 60, 37, 255) : new Color32(57, 43, 34, 255), alpha);
            MutinyBitmapFont.DrawPirateText(rect, label, hovered, true, -3);
            GUI.color = previous;
        }

        private static void DrawGameEndSolid(Rect rect, Color color, float alpha)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * alpha);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previous;
        }

        private static void DrawGameEndOutline(Rect rect, Color color, float alpha)
        {
            DrawGameEndSolid(new Rect(rect.x, rect.y, rect.width, 1f), color, alpha);
            DrawGameEndSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color, alpha);
            DrawGameEndSolid(new Rect(rect.x, rect.y, 1f, rect.height), color, alpha);
            DrawGameEndSolid(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color, alpha);
        }

        public bool AdvanceToNextLevel()
        {
            if (!m_GameEndPopupShow || m_GameEndPopupKind != MutinyGameEndPopupKind.LevelComplete || LevelController == null)
                return false;

            int nextLevel = LevelController.CurrentLevelIndex + 1;
            m_GameEndPopupShow = false;
            m_GameEndPopupAlpha = 0f;
            MutinyDebugLog.Info("HUD", $"END-POP-05 next level={nextLevel}", this);
            LevelController.LoadLevel(nextLevel);
            return true;
        }

        public bool RestartFromGameEndPopup()
        {
            if (!m_GameEndPopupShow || m_GameEndPopupKind != MutinyGameEndPopupKind.LevelFailed || LevelController == null)
                return false;

            m_GameEndPopupShow = false;
            m_GameEndPopupAlpha = 0f;
            MutinyDebugLog.Info("HUD", $"END-POP-06 restart level={LevelController.CurrentLevelIndex}", this);
            LevelController.RestartCurrentLevel();
            return true;
        }

        private bool CompleteCampaignAndReturnToLevelSelect()
        {
            if (!m_GameEndPopupShow || m_GameEndPopupKind != MutinyGameEndPopupKind.GameComplete)
                return false;
            return BackToSinglePlayerMenu();
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
                        int targetLvl = lvl;
                        MutinyTransitionManager.RequestTransition(() => LevelController?.LoadLevel(targetLvl), showLoading: true);
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
