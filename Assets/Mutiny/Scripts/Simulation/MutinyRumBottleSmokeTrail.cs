using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyRumBottleSmokeTrail : MonoBehaviour
    {
        public const int OriginalFrameCount = 19;
        public const int OriginalDestroyFrame = 19;
        public const int SmokeSortingOrder = 9;
        public static readonly Vector2 OriginalPivot = new Vector2(8f / 17f, 9f / 17f);

        private static bool? s_HasOriginalFrames;
        private static Sprite[] s_Frames;

        private SpriteRenderer m_Renderer;
        private int m_CurrentFrame;
        private float m_Accumulator;
        private bool m_IsComplete;

        public int CurrentFrame => m_CurrentFrame + 1;
        public int FrameCount => s_Frames == null ? 0 : s_Frames.Length;
        public bool IsComplete => m_IsComplete;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_HasOriginalFrames = null;
            s_Frames = null;
        }

        public static MutinyRumBottleSmokeTrail Spawn(Vector2 pixelPosition)
        {
            if (!HasOriginalFrames())
                return null;

            GameObject trailObject = new GameObject("CannonSmokeTrail");
            trailObject.transform.position = MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y);
            return trailObject.AddComponent<MutinyRumBottleSmokeTrail>();
        }

        private static bool HasOriginalFrames()
        {
            if (!s_HasOriginalFrames.HasValue)
            {
                s_HasOriginalFrames =
                    Resources.Load<Texture2D>("Art/Effects/CannonSmokeTrail/1") != null ||
                    Resources.Load<Sprite>("Art/Effects/CannonSmokeTrail/1") != null;
            }
            return s_HasOriginalFrames.Value;
        }

        private void Awake()
        {
            EnsureFramesLoaded();
            m_Renderer = gameObject.AddComponent<SpriteRenderer>();
            // Controller creates effectsLayer before tileLayer and characterLayer.
            // Keep smoke behind terrain/characters instead of attaching it to the
            // projectile's high dynamic sorting order.
            m_Renderer.sortingOrder = SmokeSortingOrder;
            if (s_Frames != null && s_Frames.Length > 0)
                m_Renderer.sprite = s_Frames[0];
        }

        private static void EnsureFramesLoaded()
        {
            if (s_Frames != null && s_Frames.Length == OriginalFrameCount && s_Frames[0] != null)
                return;

            var frames = new List<Sprite>(OriginalFrameCount);
            for (int frame = 1; frame <= OriginalFrameCount; frame++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Effects/CannonSmokeTrail/{frame}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot,
                        MutinyPhysics.PixelsPerUnit);
                    sprite.name = $"cannonSmokeTrail_{frame:D2}";
                    frames.Add(sprite);
                    continue;
                }

                Sprite importedSprite = Resources.Load<Sprite>($"Art/Effects/CannonSmokeTrail/{frame}");
                if (importedSprite != null)
                    frames.Add(importedSprite);
            }
            s_Frames = frames.ToArray();
        }

        private void Update()
        {
            m_Accumulator += Time.deltaTime;
            while (m_Accumulator >= MutinyPhysics.TimeStep)
            {
                m_Accumulator -= MutinyPhysics.TimeStep;
                AdvanceOriginalTick(true);
                if (m_IsComplete)
                    return;
            }
        }

        public void AdvanceOriginalTickForVerification()
        {
            AdvanceOriginalTick(false);
        }

        private void AdvanceOriginalTick(bool destroyObject)
        {
            if (m_IsComplete || s_Frames == null || s_Frames.Length == 0)
                return;

            int nextFrame = CurrentFrame + 1;
            if (nextFrame >= OriginalDestroyFrame)
            {
                // Frame 19 contains cl.destroy(). It is an action frame, not a
                // frame that remains visible for one extra original tick.
                m_IsComplete = true;
                if (m_Renderer != null)
                    m_Renderer.enabled = false;
                if (destroyObject)
                    Destroy(gameObject);
                return;
            }

            m_CurrentFrame++;
            if (m_Renderer != null && m_CurrentFrame < s_Frames.Length)
                m_Renderer.sprite = s_Frames[m_CurrentFrame];
        }
    }
}
