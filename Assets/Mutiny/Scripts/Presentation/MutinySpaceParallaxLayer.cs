using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Presentation
{
    // Transparent planes repeat in world units, never in viewport-sized quads.
    // Camera-relative displacement = -camera displacement * Parallax.
    [DisallowMultipleComponent]
    public sealed class MutinySpaceParallaxLayer : MonoBehaviour
    {
        [SerializeField] private string m_Resource;
        [SerializeField] private Vector2 m_Parallax;
        [SerializeField] private Vector2 m_Anchor;
        [SerializeField] private Vector2 m_Size;
        [SerializeField] private Vector2 m_Period;
        [SerializeField] private int m_Order;
        [SerializeField] private bool m_RepeatY;
        [SerializeField] private bool m_Mirror;
        [SerializeField] private bool m_Animated;
        [SerializeField] private List<SpriteRenderer> m_Renderers = new List<SpriteRenderer>();
        private Sprite m_Sprite;
        private Material m_Material;
        private MaterialPropertyBlock m_Properties;
        private static readonly int PhaseId = Shader.PropertyToID("_GalaxyPhase");
        private static readonly int BandId = Shader.PropertyToID("_BandCenter");
        private static readonly int OpaqueId = Shader.PropertyToID("_OpaqueBelow");

        public Vector2 Parallax => m_Parallax;
        public Vector2 TileSize => m_Size;

        public void Configure(string resource, Vector2 parallax, Vector2 anchor, Vector2 size,
            Vector2 period, int order, bool repeatY = false, bool mirror = false, bool animated = false)
        {
            m_Resource = resource;
            m_Parallax = parallax;
            m_Anchor = anchor;
            m_Size = size;
            m_Period = period;
            m_Order = order;
            m_RepeatY = repeatY;
            m_Mirror = mirror;
            m_Animated = animated;
        }

        public void Refresh(Camera camera, Vector3 levelOrigin, float phase)
        {
            if (m_Sprite == null) m_Sprite = Resources.Load<Sprite>(MutinySpaceVisuals.ResourcePath + m_Resource);
            if (m_Sprite == null) return;
            if (m_Animated && m_Material == null)
                m_Material = Resources.Load<Material>(MutinySpaceVisuals.ResourcePath + "GalaxyAnimated");
            float h = camera.orthographicSize * 2;
            float w = h * camera.aspect;
            Vector3 c = camera.transform.position;
            Vector2 origin = (Vector2)levelOrigin + m_Anchor + Vector2.Scale((Vector2)(c - levelOrigin), Vector2.one - m_Parallax);
            int firstX = Mathf.FloorToInt((c.x - w * 0.5f - origin.x - m_Size.x) / m_Period.x);
            int columns = Mathf.CeilToInt((w + m_Size.x) / m_Period.x) + 2;
            int firstY = m_RepeatY ? Mathf.FloorToInt((c.y - h * 0.5f - origin.y) / m_Period.y) : 0;
            int rows = m_RepeatY ? Mathf.CeilToInt((h + m_Size.y) / m_Period.y) + 2 : 1;
            int used = 0;
            for (int row = firstY; row < firstY + rows; row++)
                for (int column = firstX; column < firstX + columns; column++)
                {
                    SpriteRenderer renderer = GetRenderer(used++);
                    bool mirror = m_Mirror && (column & 1) != 0;
                    renderer.gameObject.name = $"Slice_{column}_{row}";
                    renderer.gameObject.SetActive(true);
                    renderer.sprite = m_Sprite;
                    renderer.sortingOrder = m_Order;
                    renderer.transform.position = new Vector3(origin.x + column * m_Period.x + (mirror ? m_Size.x : 0),
                        origin.y + row * m_Period.y, levelOrigin.z);
                    Vector2 spriteSize = m_Sprite.bounds.size;
                    renderer.transform.localScale = new Vector3(m_Size.x / spriteSize.x * (mirror ? -1 : 1), m_Size.y / spriteSize.y, 1);
                    if (m_Animated && m_Material != null)
                    {
                        renderer.sharedMaterial = m_Material;
                        if (m_Properties == null) m_Properties = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(m_Properties);
                        m_Properties.SetFloat(PhaseId, phase);
                        m_Properties.SetFloat(BandId, 1f - 32f / 216f);
                        m_Properties.SetFloat(OpaqueId, 1f - 36f / 216f);
                        renderer.SetPropertyBlock(m_Properties);
                    }
                }
            for (int i = used; i < m_Renderers.Count; i++)
                if (m_Renderers[i] != null) m_Renderers[i].gameObject.SetActive(false);
        }

        private SpriteRenderer GetRenderer(int index)
        {
            while (m_Renderers.Count <= index) m_Renderers.Add(null);
            if (m_Renderers[index] == null)
            {
                var child = new GameObject("Slice");
                child.transform.SetParent(transform, false);
                m_Renderers[index] = child.AddComponent<SpriteRenderer>();
            }
            return m_Renderers[index];
        }
    }
}
