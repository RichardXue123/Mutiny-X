using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MutinyCharacterAnimator : MonoBehaviour
    {
        private const int IdleFirstFrame = 1;
        private const int IdleLastFrame = 14;
        private const int HitFirstFrame = 15;
        private const int HitLastFrame = 35;

        private static readonly Dictionary<string, Sprite[]> s_FrameCache =
            new Dictionary<string, Sprite[]>(StringComparer.Ordinal);

        private SpriteRenderer m_Renderer;
        private MutinyCharacter m_Character;
        private Sprite[] m_Frames;
        private int m_Frame = IdleFirstFrame;
        private bool m_HitRequested;
        private bool m_PlayingHitRecovery;
        private float m_TickAccumulator;

        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Character = GetComponent<MutinyCharacter>();
        }

        public void Initialize(string characterType)
        {
            m_Frames = LoadFrames(characterType);
            m_Frame = IdleFirstFrame;
            ApplyFrame();
        }

        public void PlayHit()
        {
            m_HitRequested = true;
            m_PlayingHitRecovery = false;
            m_Frame = HitFirstFrame;
            ApplyFrame();
        }

        private void Update()
        {
            if (m_Frames == null || m_Frames.Length == 0 || m_Character == null || !m_Character.IsAlive)
                return;

            m_TickAccumulator += Time.deltaTime;
            while (m_TickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_TickAccumulator -= MutinyPhysics.TimeStep;
                AdvanceOriginalTick();
            }
        }

        private void AdvanceOriginalTick()
        {
            if (m_HitRequested)
            {
                bool moving = m_Character.PhysicsBody != null && !m_Character.PhysicsBody.IsAtRest;
                if (moving)
                {
                    // Character.advance repeatedly jumps to the "hit" label while the
                    // body is moving, holding the first hit pose.
                    m_Frame = HitFirstFrame;
                }
                else
                {
                    m_HitRequested = false;
                    m_PlayingHitRecovery = true;
                }
            }

            if (m_PlayingHitRecovery)
            {
                m_Frame++;
                if (m_Frame > HitLastFrame)
                {
                    m_PlayingHitRecovery = false;
                    m_Frame = IdleFirstFrame;
                }
            }
            else if (!m_HitRequested)
            {
                m_Frame++;
                if (m_Frame > IdleLastFrame)
                    m_Frame = IdleFirstFrame;
            }

            ApplyFrame();
        }

        private void ApplyFrame()
        {
            int index = m_Frame - 1;
            if (m_Renderer != null && m_Frames != null && index >= 0 && index < m_Frames.Length && m_Frames[index] != null)
                m_Renderer.sprite = m_Frames[index];
        }

        private static Sprite[] LoadFrames(string characterType)
        {
            if (string.IsNullOrEmpty(characterType))
                return Array.Empty<Sprite>();
            if (s_FrameCache.TryGetValue(characterType, out Sprite[] cached))
                return cached;

            Texture2D[] textures = Resources.LoadAll<Texture2D>($"Art/Characters/Animations/{characterType}");
            Array.Sort(textures, (a, b) => ParseFrame(a.name).CompareTo(ParseFrame(b.name)));
            var frames = new Sprite[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                frames[i] = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    MutinyPhysics.PixelsPerUnit);
                frames[i].name = $"{characterType}_{i + 1:D2}";
            }
            s_FrameCache[characterType] = frames;
            return frames;
        }

        private static int ParseFrame(string name)
        {
            return int.TryParse(name, out int frame) ? frame : int.MaxValue;
        }
    }
}

