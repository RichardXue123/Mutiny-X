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
        private readonly List<string> m_Contents = new List<string>();
        private MutinyTreasureChestManager m_Manager;
        private SpriteRenderer m_Renderer;
        private MutinyCharacter m_CharacterTouched;
        private float m_FloorY;
        private int m_ToNextWeapon;
        private float m_Visibility = 1f;
        private int m_VisualFrame = 10;

        public float PixelX { get; private set; }
        public float PixelY { get; private set; }
        public float FloorPixelY => m_FloorY;
        public bool IsFalling { get; private set; } = true;
        public bool IsFinished { get; private set; }
        public int TimeTaken { get; private set; }

        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Renderer.sortingOrder = MutinyLevelBuilder.ObjectSortingOrder;
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

            SetVisualFrame(10);
            SyncTransform();
            MutinyAudioManager.Instance?.PlaySFX("chest_appear");
        }

        public void AdvanceOriginalTick()
        {
            if (!MutinyTreasureChestManager.SystemEnabled)
                return;

            if (IsFinished)
            {
                m_Visibility -= 0.1f;
                Color color = m_Renderer.color;
                color.a = Mathf.Clamp01(m_Visibility);
                m_Renderer.color = color;
                if (m_Visibility < 0f)
                    Destroy(gameObject);
            }
            else if (IsFalling)
            {
                PixelY += 3f;
                if (PixelY >= m_FloorY - 15f)
                {
                    PixelY = m_FloorY - 15f;
                    IsFalling = false;
                    SetVisualFrame(20);
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
                        m_CharacterTouched.AddWeapon(releasedWeapon);
                        MutinyDebugLog.Info("Chest",
                            $"collected weapon={releasedWeapon} character={m_CharacterTouched.name} remaining={m_Contents.Count}", this);
                        SetVisualFrame(44);
                        MutinyAudioManager.Instance?.PlaySFX("icon_collect");
                    }
                    else
                    {
                        SetVisualFrame(81);
                        IsFinished = true;
                    }
                }

                if (!m_CharacterTouched.IsAlive)
                    m_Visibility -= 0.05f;
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
            MutinyCharacter[] characters = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                MutinyCharacter character = characters[i];
                if (character == null || !character.IsAlive || character.PhysicsBody == null)
                    continue;

                PhysicsBodyState state = character.PhysicsBody.State;
                if (state.X + state.RightExtent >= PixelX - 20f &&
                    state.X - state.LeftExtent <= PixelX + 20f &&
                    state.Y + state.BottomExtent >= PixelY - 16f &&
                    state.Y - state.TopExtent <= PixelY + 16f)
                {
                    m_CharacterTouched = character;
                    m_ToNextWeapon = 10;
                    SetVisualFrame(35);
                    MutinyAudioManager.Instance?.PlaySFX("click");
                    return;
                }
            }
        }

        public static readonly Vector2 OriginalPivot = new Vector2(29f / 62f, 27.95f / 82f); // Symbol 1725: origin (29, 54.05) of 62x82

        private void SetVisualFrame(int frame)
        {
            m_VisualFrame = frame;
            Texture2D texture = Resources.Load<Texture2D>($"Art/Objects/TreasureChest/{m_VisualFrame}");
            if (texture == null)
                return;

            texture.filterMode = FilterMode.Point;
            m_Renderer.sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                OriginalPivot,
                MutinyPhysics.PixelsPerUnit);
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
