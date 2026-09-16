using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyRumBottleSmokeTrail : MonoBehaviour
    {
        private const int OriginalFrameCount = 19;
        private static bool? s_HasOriginalFrames;

        private readonly List<Sprite> m_Frames = new List<Sprite>(OriginalFrameCount);
        private SpriteRenderer m_Renderer;
        private int m_CurrentFrame;
        private float m_Accumulator;

        public static void Spawn(Vector2 pixelPosition)
        {
            if (!HasOriginalFrames())
                return;

            GameObject trailObject = new GameObject("RumBottleSmokeTrail");
            trailObject.transform.position = MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y);
            trailObject.AddComponent<MutinyRumBottleSmokeTrail>();
        }

        private static bool HasOriginalFrames()
        {
            if (!s_HasOriginalFrames.HasValue)
                s_HasOriginalFrames = Resources.Load<Sprite>("Art/Effects/CannonSmokeTrail/1") != null;
            return s_HasOriginalFrames.Value;
        }

        private void Awake()
        {
            m_Renderer = gameObject.AddComponent<SpriteRenderer>();
            m_Renderer.sortingOrder = MutinyWeapon.WeaponSortingOrder + 1;
            for (int frame = 1; frame <= OriginalFrameCount; frame++)
            {
                Sprite sprite = Resources.Load<Sprite>($"Art/Effects/CannonSmokeTrail/{frame}");
                if (sprite != null)
                    m_Frames.Add(sprite);
            }
            if (m_Frames.Count > 0)
                m_Renderer.sprite = m_Frames[0];
        }

        private void Update()
        {
            m_Accumulator += Time.deltaTime;
            while (m_Accumulator >= MutinyPhysics.TimeStep)
            {
                m_Accumulator -= MutinyPhysics.TimeStep;
                m_CurrentFrame++;
                if (m_CurrentFrame >= OriginalFrameCount)
                {
                    Destroy(gameObject);
                    return;
                }
                if (m_CurrentFrame < m_Frames.Count)
                    m_Renderer.sprite = m_Frames[m_CurrentFrame];
            }
        }
    }
}
