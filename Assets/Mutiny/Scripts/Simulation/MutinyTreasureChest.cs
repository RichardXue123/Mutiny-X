using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MutinyTreasureChest : MonoBehaviour
    {
        private enum VisualSequence
        {
            Static,
            Falling,
            Touchdown,
            Open,
            WeaponOut,
            FadeOut
        }

        private static readonly Dictionary<int, Sprite> FrameCache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<string, Sprite> IconCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<int, Sprite> PopupFrameCache = new Dictionary<int, Sprite>();
        private readonly List<string> m_Contents = new List<string>();
        private MutinyTreasureChestManager m_Manager;
        private SpriteRenderer m_Renderer;
        private MutinyCharacter m_CharacterTouched;
        private float m_FloorY;
        private int m_ToNextWeapon;
        private float m_Visibility = 1f;
        private int m_VisualFrame = 10;
        private VisualSequence m_VisualSequence = VisualSequence.Falling;
        private string m_ReleasedWeapon;
        private SpriteRenderer m_ReleasedWeaponFrameRenderer;
        private SpriteRenderer m_ReleasedWeaponRenderer;
        private MutinyPlayerInput m_PlayerInput;

        public float PixelX { get; private set; }
        public float PixelY { get; private set; }
        public float FloorPixelY => m_FloorY;
        public bool IsFalling { get; private set; } = true;
        public bool IsFinished { get; private set; }
        public int TimeTaken { get; private set; }
        public int CurrentVisualFrame => m_VisualFrame;
        public int RemainingContents => m_Contents.Count;
        public MutinyCharacter CollectingCharacter => m_CharacterTouched;

        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Renderer.sortingOrder = MutinyLevelBuilder.ObjectSortingOrder;
            CreateReleasedWeaponRenderer();
        }

        public void Initialize(MutinyTreasureChestManager manager, float x, float floorY, List<string> contents)
        {
            m_Manager = manager;
            PixelX = x;
            PixelY = -300f;
            m_FloorY = floorY;
            m_Contents.AddRange(contents);

            if (!MutinyTreasureChestManager.SystemEnabled)
            {
                IsFalling = false;
                IsFinished = true;
                if (m_Renderer != null)
                    m_Renderer.enabled = false;
                MutinyDebugLog.Warning("Chest",
                    $"chest initialization suppressed while airdrops are disabled name={name}", this);
                return;
            }

            PlaySequence(VisualSequence.Falling);
            SyncTransform();
            MutinyAudioManager.Instance?.PlaySFX("chest_appear");
            MutinyDebugLog.Info("Chest",
                $"spawned x={PixelX:0} startY={PixelY:0} floorY={m_FloorY:0} itemCount={m_Contents.Count} sound=chest_appear", this);
        }

        public void AdvanceOriginalTick()
        {
            if (!MutinyTreasureChestManager.SystemEnabled)
                return;

            AdvanceVisualTimeline();

            if (IsFinished)
            {
                m_Visibility -= 0.1f;
                ApplyVisibility();
                if (m_VisualFrame >= 90 || m_Visibility < 0f)
                    Destroy(gameObject);
            }
            else if (IsFalling)
            {
                PixelY += 3f;
                if (PixelY >= m_FloorY - 15f)
                {
                    PixelY = m_FloorY - 15f;
                    IsFalling = false;
                    PlaySequence(VisualSequence.Touchdown);
                    MutinyDebugLog.Info("Chest", $"landed name={name} x={PixelX:0} y={PixelY:0}", this);
                }
                SyncTransform();
            }
            else if (m_CharacterTouched != null)
            {
                m_ToNextWeapon--;
                if (m_ToNextWeapon <= 0)
                {
                    if (m_Contents.Count > 0)
                    {
                        m_ToNextWeapon = 40;
                        string releasedWeapon = m_Contents[0];
                        m_Contents.RemoveAt(0);
                        m_ReleasedWeapon = releasedWeapon;
                        m_CharacterTouched.AddWeapon(releasedWeapon);
                        MutinyDebugLog.Info("Chest",
                            $"collected weapon={releasedWeapon} character={m_CharacterTouched.name} remaining={m_Contents.Count}", this);
                        PlaySequence(VisualSequence.WeaponOut);
                        MutinyAudioManager.Instance?.PlaySFX("icon_collect");
                    }
                    else
                    {
                        PlaySequence(VisualSequence.FadeOut);
                        IsFinished = true;
                        MutinyDebugLog.Info("Chest",
                            $"contents exhausted collector={m_CharacterTouched.name}; starting fade_out", this);
                    }
                }

                if (!m_CharacterTouched.IsAlive)
                {
                    m_Visibility -= 0.05f;
                    ApplyVisibility();
                }
            }
            else
            {
                TryTouchCharacter();
            }

            if ((IsFalling || m_CharacterTouched != null) && !IsFinished)
                FindAnyObjectByType<MutinyTurnManager>()?.NotifyAmbientActivity();

            TimeTaken++;
        }

        private void TryTouchCharacter()
        {
            MutinyLevelRoot levelRoot = m_Manager != null ? m_Manager.LevelRoot : null;
            if (levelRoot != null && levelRoot.Team1 != null && TryTouchTeam(levelRoot.Team1))
                return;
            if (levelRoot != null && levelRoot.Team2 != null && TryTouchTeam(levelRoot.Team2))
                return;

            // Verification and legacy scenes may not have a populated LevelRoot.
            MutinyCharacter[] characters = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                if (TryTouchCharacter(characters[i]))
                    return;
            }
        }

        private bool TryTouchTeam(MutinyTeam team)
        {
            for (int i = 0; i < team.Characters.Count; i++)
            {
                if (TryTouchCharacter(team.Characters[i]))
                    return true;
            }
            return false;
        }

        private bool TryTouchCharacter(MutinyCharacter character)
        {
            if (character == null || !character.IsAlive || character.PhysicsBody == null)
                return false;

            if (m_PlayerInput == null)
                m_PlayerInput = FindAnyObjectByType<MutinyPlayerInput>();
            // Controller.dragging rejects only an actively mouse-dragged character.
            // A character moving because of an explosion remains eligible.
            if (m_PlayerInput != null && m_PlayerInput.IsCharacterThrowDragInProgress(character))
                return false;

            PhysicsBodyState state = character.PhysicsBody.State;
            if (state.X + state.RightExtent < PixelX - 20f ||
                state.X - state.LeftExtent > PixelX + 20f ||
                state.Y + state.BottomExtent < PixelY - 16f ||
                state.Y - state.TopExtent > PixelY + 16f)
                return false;

            m_CharacterTouched = character;
            m_ToNextWeapon = 10;
            PlaySequence(VisualSequence.Open);
            MutinyAudioManager.Instance?.PlaySFX("click");
            MutinyDebugLog.Info("Chest",
                $"opened collector={character.name} team={character.TeamIndex} position=({state.X:0.0},{state.Y:0.0}) passiveVelocity=({state.VelocityX:0.0},{state.VelocityY:0.0}) sound=click", this);
            return true;
        }

        public static readonly Vector2 OriginalPivot = new Vector2(29f / 62f, 27.95f / 82f); // Symbol 1725: origin (29, 54.05) of 62x82

        private void SetVisualFrame(int frame)
        {
            m_VisualFrame = frame;
            if (!FrameCache.TryGetValue(frame, out Sprite sprite) || sprite == null)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Objects/TreasureChest/{frame}");
                if (texture == null)
                    return;
                texture.filterMode = FilterMode.Point;
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
                FrameCache[frame] = sprite;
            }
            m_Renderer.sprite = sprite;
            UpdateReleasedWeaponVisual();
        }

        private void PlaySequence(VisualSequence sequence)
        {
            m_VisualSequence = sequence;
            SetVisualFrame(sequence switch
            {
                VisualSequence.Static => 1,
                VisualSequence.Falling => 10,
                VisualSequence.Touchdown => 20,
                VisualSequence.Open => 35,
                VisualSequence.WeaponOut => 44,
                VisualSequence.FadeOut => 81,
                _ => 1
            });
        }

        private void AdvanceVisualTimeline()
        {
            switch (m_VisualSequence)
            {
                case VisualSequence.Falling:
                    SetVisualFrame(m_VisualFrame >= 19 ? 10 : m_VisualFrame + 1);
                    break;
                case VisualSequence.Touchdown:
                    if (m_VisualFrame >= 34)
                        PlaySequence(VisualSequence.Static);
                    else
                        SetVisualFrame(m_VisualFrame + 1);
                    break;
                case VisualSequence.Open:
                    if (m_VisualFrame < 43)
                        SetVisualFrame(m_VisualFrame + 1);
                    break;
                case VisualSequence.WeaponOut:
                    if (m_VisualFrame < 80)
                        SetVisualFrame(m_VisualFrame + 1);
                    break;
                case VisualSequence.FadeOut:
                    if (m_VisualFrame < 90)
                        SetVisualFrame(m_VisualFrame + 1);
                    break;
            }
        }

        private void CreateReleasedWeaponRenderer()
        {
            GameObject frameObject = new GameObject("ReleasedWeaponTeamFrame");
            frameObject.transform.SetParent(transform, false);
            m_ReleasedWeaponFrameRenderer = frameObject.AddComponent<SpriteRenderer>();
            m_ReleasedWeaponFrameRenderer.sortingOrder = MutinyLevelBuilder.ObjectSortingOrder + 1;
            m_ReleasedWeaponFrameRenderer.enabled = false;

            GameObject iconObject = new GameObject("ReleasedWeaponIcon");
            iconObject.transform.SetParent(transform, false);
            m_ReleasedWeaponRenderer = iconObject.AddComponent<SpriteRenderer>();
            m_ReleasedWeaponRenderer.sortingOrder = MutinyLevelBuilder.ObjectSortingOrder + 2;
            m_ReleasedWeaponRenderer.enabled = false;
        }

        private void UpdateReleasedWeaponVisual()
        {
            if (m_ReleasedWeaponRenderer == null)
                return;

            bool visible = m_VisualSequence == VisualSequence.WeaponOut &&
                           m_VisualFrame >= 47 && m_VisualFrame < 80 &&
                           !string.IsNullOrEmpty(m_ReleasedWeapon);
            if (m_ReleasedWeaponFrameRenderer != null)
                m_ReleasedWeaponFrameRenderer.enabled = visible;
            m_ReleasedWeaponRenderer.enabled = visible;
            if (!visible)
                return;

            int team = m_CharacterTouched != null && m_CharacterTouched.TeamIndex == 2 ? 2 : 1;
            if (!PopupFrameCache.TryGetValue(team, out Sprite popupFrame) || popupFrame == null)
            {
                string path = team == 1 ? "UI/weapon_slot_red_up" : "UI/weapon_slot_blue_up";
                Texture2D frameTexture = Resources.Load<Texture2D>(path);
                if (frameTexture != null)
                {
                    frameTexture.filterMode = FilterMode.Point;
                    // The upper 24x24 part of the original weapon slot is the same
                    // team frame used by the chest's nested weapon clip.
                    popupFrame = Sprite.Create(frameTexture,
                        new Rect(0f, frameTexture.height - 24f, 24f, 24f),
                        new Vector2(0.5f, 0.5f), MutinyPhysics.PixelsPerUnit);
                    PopupFrameCache[team] = popupFrame;
                }
            }
            if (m_ReleasedWeaponFrameRenderer != null)
            {
                m_ReleasedWeaponFrameRenderer.sprite = popupFrame;
                m_ReleasedWeaponFrameRenderer.transform.localScale = Vector3.one * (22f / 24f);
            }

            if (!IconCache.TryGetValue(m_ReleasedWeapon, out Sprite icon) || icon == null)
            {
                Texture2D texture = Resources.Load<Texture2D>($"UI/WeaponIcons/{m_ReleasedWeapon}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), MutinyPhysics.PixelsPerUnit);
                    IconCache[m_ReleasedWeapon] = icon;
                }
            }
            m_ReleasedWeaponRenderer.sprite = icon;

            // The nested Flash `weapon` clip rises from y=26 to y=0 (texture-top
            // coordinates) during frames 47..54, then holds until frame 80.
            float top = m_VisualFrame switch
            {
                47 => 26f,
                48 => 20f,
                49 => 15f,
                50 => 10f,
                51 => 7f,
                52 => 4f,
                53 => 2f,
                _ => 0f
            };
            float centerXFromRegistration = 28.5f - 29f;
            float centerYFromRegistration = 54.05f - (top + 11f);
            m_ReleasedWeaponRenderer.transform.localPosition =
                new Vector3(centerXFromRegistration, centerYFromRegistration, 0f) /
                MutinyPhysics.PixelsPerUnit;
            if (m_ReleasedWeaponFrameRenderer != null)
                m_ReleasedWeaponFrameRenderer.transform.localPosition =
                    m_ReleasedWeaponRenderer.transform.localPosition;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            float alpha = Mathf.Clamp01(m_Visibility);
            Color color = m_Renderer.color;
            color.a = alpha;
            m_Renderer.color = color;
            if (m_ReleasedWeaponRenderer != null)
            {
                color = m_ReleasedWeaponRenderer.color;
                color.a = alpha;
                m_ReleasedWeaponRenderer.color = color;
            }
            if (m_ReleasedWeaponFrameRenderer != null)
            {
                color = m_ReleasedWeaponFrameRenderer.color;
                color.a = alpha;
                m_ReleasedWeaponFrameRenderer.color = color;
            }
        }

        private void SyncTransform()
        {
            transform.localPosition = MutinyPhysics.PixelToUnity(PixelX, PixelY);
        }

        private void OnDestroy()
        {
            m_Manager?.Unregister(this);
        }
    }
}
