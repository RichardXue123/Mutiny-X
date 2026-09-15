using System;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MutinyCharacter))]
    public sealed class MutinyCharacterOverlay : MonoBehaviour
    {
        private const int OverlaySortingOrder = 25;

        private MutinyCharacter m_Character;
        private GameObject m_OverlayRoot;
        private Transform m_HealthBarFill;
        private SpriteRenderer m_HealthBarFillRenderer;
        private GameObject m_IndicatorArrow;
        private GameObject m_SelectionCorners;

        private static Sprite s_WhiteSprite;

        private static Sprite WhiteSprite
        {
            get
            {
                if (s_WhiteSprite == null)
                {
                    s_WhiteSprite = Sprite.Create(
                        Texture2D.whiteTexture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        1f
                    );
                }
                return s_WhiteSprite;
            }
        }

        private void Awake()
        {
            m_Character = GetComponent<MutinyCharacter>();
            CreateOverlayUI();
        }

        private void OnEnable()
        {
            if (m_Character != null)
            {
                m_Character.OnHealthChanged += UpdateHealthBar;
                m_Character.OnDeath += OnCharacterDeath;
            }
        }

        private void OnDisable()
        {
            if (m_Character != null)
            {
                m_Character.OnHealthChanged -= UpdateHealthBar;
                m_Character.OnDeath -= OnCharacterDeath;
            }
        }

        private void LateUpdate()
        {
            if (m_Character == null || !m_Character.IsAlive)
            {
                if (m_OverlayRoot != null && m_OverlayRoot.activeSelf)
                    m_OverlayRoot.SetActive(false);
                return;
            }

            if (m_IndicatorArrow != null)
            {
                bool showIndicator = m_Character.IsSelected;
                if (m_IndicatorArrow.activeSelf != showIndicator)
                    m_IndicatorArrow.SetActive(showIndicator);
            }

            if (m_SelectionCorners != null)
            {
                bool showCorners = m_Character.IsSelected || m_Character.IsHovered;
                if (m_SelectionCorners.activeSelf != showCorners)
                    m_SelectionCorners.SetActive(showCorners);
            }
        }

        private void CreateOverlayUI()
        {
            m_OverlayRoot = new GameObject("OverlayUI");
            m_OverlayRoot.transform.SetParent(transform, false);
            m_OverlayRoot.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            // 1. Health bar background (black/dark border)
            GameObject barBg = new GameObject("HealthBar_Bg");
            barBg.transform.SetParent(m_OverlayRoot.transform, false);
            barBg.transform.localScale = new Vector3(0.6f, 0.08f, 1f);
            var srBg = barBg.AddComponent<SpriteRenderer>();
            srBg.sprite = WhiteSprite;
            srBg.color = new Color(0f, 0f, 0f, 0.8f);
            srBg.sortingOrder = OverlaySortingOrder;

            // 2. Health bar fill
            GameObject barFill = new GameObject("HealthBar_Fill");
            barFill.transform.SetParent(m_OverlayRoot.transform, false);
            barFill.transform.localPosition = new Vector3(-0.28f, 0f, 0f);
            barFill.transform.localScale = new Vector3(0.56f, 0.06f, 1f);
            m_HealthBarFill = barFill.transform;
            m_HealthBarFillRenderer = barFill.AddComponent<SpriteRenderer>();
            m_HealthBarFillRenderer.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0f, 0.5f), // Pivot at left edge so scaling scales to right
                1f
            );
            m_HealthBarFillRenderer.color = Color.green;
            m_HealthBarFillRenderer.sortingOrder = OverlaySortingOrder + 1;

            // 3. Selection indicator arrow (downwards pointing triangle)
            m_IndicatorArrow = new GameObject("SelectionIndicator");
            m_IndicatorArrow.transform.SetParent(m_OverlayRoot.transform, false);
            m_IndicatorArrow.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            m_IndicatorArrow.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            var srArrow = m_IndicatorArrow.AddComponent<SpriteRenderer>();
            srArrow.sprite = WhiteSprite;
            srArrow.color = m_Character.TeamIndex == 1 ? Color.yellow : new Color(0.2f, 0.8f, 1f);
            srArrow.sortingOrder = OverlaySortingOrder + 2;
            m_IndicatorArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 45f); // diamond / arrow style
            m_IndicatorArrow.SetActive(false);

            // Original Character.overlay.corners: four white corner brackets shown
            // for the character under the mouse and for the selected character.
            m_SelectionCorners = new GameObject("SelectionCorners");
            m_SelectionCorners.transform.SetParent(m_OverlayRoot.transform, false);
            m_SelectionCorners.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            CreateCorner("TopLeft", new Vector2(-0.45f, 0.45f), new Vector2(1f, -1f));
            CreateCorner("TopRight", new Vector2(0.45f, 0.45f), new Vector2(-1f, -1f));
            CreateCorner("BottomLeft", new Vector2(-0.45f, -0.45f), new Vector2(1f, 1f));
            CreateCorner("BottomRight", new Vector2(0.45f, -0.45f), new Vector2(-1f, 1f));
            m_SelectionCorners.SetActive(false);

            UpdateHealthBar();
        }

        private void CreateCorner(string cornerName, Vector2 corner, Vector2 direction)
        {
            GameObject cornerObject = new GameObject(cornerName);
            cornerObject.transform.SetParent(m_SelectionCorners.transform, false);

            LineRenderer line = cornerObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = false;
            line.positionCount = 3;
            line.startWidth = 2f / MutinyPhysics.PixelsPerUnit;
            line.endWidth = 2f / MutinyPhysics.PixelsPerUnit;
            line.numCapVertices = 0;
            line.sortingOrder = OverlaySortingOrder + 3;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.white;
            line.endColor = Color.white;

            const float length = 0.18f;
            line.SetPosition(0, new Vector3(corner.x, corner.y + direction.y * length, 0f));
            line.SetPosition(1, new Vector3(corner.x, corner.y, 0f));
            line.SetPosition(2, new Vector3(corner.x + direction.x * length, corner.y, 0f));
        }

        private void UpdateHealthBar()
        {
            if (m_Character == null || m_HealthBarFill == null)
                return;

            float ratio = Mathf.Clamp01(m_Character.Health / m_Character.MaxHealth);
            m_HealthBarFill.localScale = new Vector3(0.56f * ratio, 0.06f, 1f);

            if (m_HealthBarFillRenderer != null)
            {
                if (ratio > 0.5f)
                    m_HealthBarFillRenderer.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
                else
                    m_HealthBarFillRenderer.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
            }
        }

        private void OnCharacterDeath()
        {
            if (m_OverlayRoot != null)
            {
                m_OverlayRoot.SetActive(false);
            }
        }
    }
}
