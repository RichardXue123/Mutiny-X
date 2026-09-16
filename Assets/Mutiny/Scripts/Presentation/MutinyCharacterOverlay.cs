using System;
using Mutiny.Diagnostics;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MutinyCharacter))]
    public sealed class MutinyCharacterOverlay : MonoBehaviour
    {
        private const int OverlaySortingOrder = 25;
        private const float PixelsPerUnit = MutinyPhysics.PixelsPerUnit;
        private const int HealthSegments = 27;

        private MutinyCharacter m_Character;
        private MutinyTeam m_Team;
        private GameObject m_OverlayRoot;
        private GameObject m_Indicator;
        private SpriteRenderer m_IndicatorRenderer;
        private Transform m_HealthFill;
        private SpriteRenderer m_HealthFillRenderer;
        private GameObject m_HealthBar;
        private GameObject m_SelectionCorners;
        private bool m_LastIndicatorVisible;
        private bool m_LastHealthVisible;
        private int m_LastHealthSegments = -1;
        private string m_IndicatorResource;

        private static Sprite s_WhitePixel;

        public bool IsTurnIndicatorVisible => m_Indicator != null && m_Indicator.activeInHierarchy;
        public bool IsHealthBarVisible => m_HealthBar != null && m_HealthBar.activeInHierarchy;
        public int CurrentHealthFrame => 1 + Mathf.Max(0, m_LastHealthSegments);

        private static Sprite WhitePixel
        {
            get
            {
                if (s_WhitePixel == null)
                {
                    s_WhitePixel = Sprite.Create(Texture2D.whiteTexture,
                        new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 0.5f), PixelsPerUnit);
                }
                return s_WhitePixel;
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
                SetOverlayVisible(false);
                return;
            }

            ResolveTeam();
            bool isCurrentTeam = m_Team != null &&
                                 FindAnyObjectByType<MutinyTurnManager>()?.CurrentTeam == m_Team;
            bool isDragged = IsCharacterThrowDragged();
            bool isSelfThrown = m_Character.IsSelfThrown;

            // Character.updateOverlay: triangle = current team && !dragging/self-throw
            // && alive && speechBubble.target != character. The Unity dialogue
            // target layer is not implemented yet. Physical velocity is deliberately
            // absent here: a blast or collision does not set Character.thrown in
            // Flash, so it must keep this overlay visible.
            bool hideForCharacterAction = isDragged || isSelfThrown;
            bool showIndicator = isCurrentTeam && !hideForCharacterAction;
            bool showHealth = !hideForCharacterAction;
            SetActive(m_Indicator, showIndicator, ref m_LastIndicatorVisible, "indicator");
            SetActive(m_HealthBar, showHealth, ref m_LastHealthVisible, "health");

            if (m_SelectionCorners != null)
            {
                bool showCorners = (m_Character.IsSelected || m_Character.IsHovered) && !hideForCharacterAction;
                if (m_SelectionCorners.activeSelf != showCorners)
                    m_SelectionCorners.SetActive(showCorners);
            }

            UpdateIndicatorSprite();
            UpdateHealthBar();
        }

        public void RefreshVisualStateForVerification()
        {
            LateUpdate();
        }

        public static int CalculateOriginalHealthFrame(float shownHealth, float maxHealth)
        {
            return 1 + Mathf.Clamp(Mathf.CeilToInt(HealthSegments * shownHealth / Mathf.Max(1f, maxHealth)),
                0, HealthSegments);
        }

        private void ResolveTeam()
        {
            if (m_Team != null && m_Team.Characters.Contains(m_Character))
                return;

            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].Characters.Contains(m_Character))
                {
                    m_Team = teams[i];
                    return;
                }
            }
        }

        private bool IsCharacterThrowDragged()
        {
            MutinyPlayerInput playerInput = FindAnyObjectByType<MutinyPlayerInput>();
            return playerInput != null && playerInput.IsCharacterThrowDragInProgress(m_Character);
        }

        private void CreateOverlayUI()
        {
            m_OverlayRoot = new GameObject("OriginalCharacterOverlay");
            m_OverlayRoot.transform.SetParent(transform, false);
            // characterOverlay's exported placement is x=0.5px, y=-2.4px.
            m_OverlayRoot.transform.localPosition = new Vector3(0.5f / PixelsPerUnit, 2.4f / PixelsPerUnit, 0f);

            m_Indicator = new GameObject("TurnIndicator");
            m_Indicator.transform.SetParent(m_OverlayRoot.transform, false);
            // characterOverlay.triangle is placed at source y=-761 twips (-38.05px).
            m_Indicator.transform.localPosition = new Vector3(0f, 38.05f / PixelsPerUnit, 0f);
            m_IndicatorRenderer = m_Indicator.AddComponent<SpriteRenderer>();
            m_IndicatorRenderer.sortingOrder = OverlaySortingOrder + 2;
            m_Indicator.SetActive(false);

            m_HealthBar = new GameObject("HealthBar");
            m_HealthBar.transform.SetParent(m_OverlayRoot.transform, false);
            // characterOverlay.health is placed at source y=360 twips (+18px).
            m_HealthBar.transform.localPosition = new Vector3(0f, -18f / PixelsPerUnit, 0f);
            var barBackground = new GameObject("OriginalFrame");
            barBackground.transform.SetParent(m_HealthBar.transform, false);
            SpriteRenderer backgroundRenderer = barBackground.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = LoadSprite("UI/CharacterOverlay/health_background", new Vector2(0.5f, 0.5f));
            backgroundRenderer.sortingOrder = OverlaySortingOrder;

            var fill = new GameObject("DiscreteFill");
            fill.transform.SetParent(m_HealthBar.transform, false);
            fill.transform.localPosition = new Vector3(-13f / PixelsPerUnit, 0f, 0f);
            m_HealthFill = fill.transform;
            m_HealthFillRenderer = fill.AddComponent<SpriteRenderer>();
            m_HealthFillRenderer.sprite = WhitePixel;
            m_HealthFillRenderer.sortingOrder = OverlaySortingOrder + 1;

            CreateSelectionCorners();
            UpdateIndicatorSprite();
            UpdateHealthBar();
        }

        private void UpdateIndicatorSprite()
        {
            if (m_IndicatorRenderer == null)
                return;

            string resource = m_Team != null && m_Team.IsAiControlled
                ? "UI/CharacterOverlay/cpu_indicator"
                : m_Character.TeamIndex == 2
                    ? "UI/CharacterOverlay/p2_indicator"
                    : "UI/CharacterOverlay/p1_indicator";
            if (string.Equals(m_IndicatorResource, resource, StringComparison.Ordinal))
                return;

            Sprite sprite = LoadSprite(resource, new Vector2(0.5f, 0.5f));
            if (sprite != null)
            {
                m_IndicatorRenderer.sprite = sprite;
                m_IndicatorResource = resource;
            }
        }

        private void UpdateHealthBar()
        {
            if (m_Character == null || m_HealthFill == null || m_HealthFillRenderer == null)
                return;

            float maximum = Mathf.Max(1f, m_Character.MaxHealth);
            int segments = CalculateOriginalHealthFrame(m_Character.ShownHealth, maximum) - 1;
            if (segments != m_LastHealthSegments)
            {
                // The Flash health movie has one initial empty state plus 27
                // discrete morph positions. Scaling in whole source pixels keeps
                // its change cadence tied to shownHealth's 25 Hz progression.
                m_HealthFill.localScale = new Vector3(segments, 4f, 1f);
                m_LastHealthSegments = segments;
                MutinyDebugLog.Info("Overlay",
                    $"health frame character={m_Character.name} frame={1 + segments} shown={m_Character.ShownHealth:0} health={m_Character.Health:0}", this);
            }

            // health (1864) is red; the p2 overlay substitutes the blue health movie (1877).
            m_HealthFillRenderer.color = m_Character.TeamIndex == 2
                ? new Color32(51, 95, 255, 255)
                : new Color32(230, 49, 19, 255);
        }

        private static Sprite LoadSprite(string resourcePath, Vector2 pivot)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
                return null;
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), pivot, PixelsPerUnit);
        }

        private void CreateSelectionCorners()
        {
            m_SelectionCorners = new GameObject("SelectionCorners");
            m_SelectionCorners.transform.SetParent(m_OverlayRoot.transform, false);
            m_SelectionCorners.transform.localPosition = Vector3.zero;
            CreateCorner("TopLeft", new Vector2(-14f, 14f), new Vector2(1f, -1f));
            CreateCorner("TopRight", new Vector2(14f, 14f), new Vector2(-1f, -1f));
            CreateCorner("BottomLeft", new Vector2(-14f, -14f), new Vector2(1f, 1f));
            CreateCorner("BottomRight", new Vector2(14f, -14f), new Vector2(-1f, 1f));
            m_SelectionCorners.SetActive(false);
        }

        private void CreateCorner(string cornerName, Vector2 pixelCorner, Vector2 direction)
        {
            GameObject cornerObject = new GameObject(cornerName);
            cornerObject.transform.SetParent(m_SelectionCorners.transform, false);
            LineRenderer line = cornerObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 3;
            line.startWidth = 2f / PixelsPerUnit;
            line.endWidth = 2f / PixelsPerUnit;
            line.sortingOrder = OverlaySortingOrder + 3;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.white;
            line.endColor = Color.white;
            const float length = 6f;
            line.SetPosition(0, ToUnityPixel(pixelCorner + new Vector2(0f, direction.y * length)));
            line.SetPosition(1, ToUnityPixel(pixelCorner));
            line.SetPosition(2, ToUnityPixel(pixelCorner + new Vector2(direction.x * length, 0f)));
        }

        private static Vector3 ToUnityPixel(Vector2 sourcePixel)
        {
            return new Vector3(sourcePixel.x / PixelsPerUnit, -sourcePixel.y / PixelsPerUnit, 0f);
        }

        private void SetActive(GameObject target, bool active, ref bool previous, string label)
        {
            if (target == null || previous == active)
                return;
            target.SetActive(active);
            previous = active;
            MutinyDebugLog.Info("Overlay", $"{label} visible={active} character={m_Character.name}", this);
        }

        private void SetOverlayVisible(bool visible)
        {
            if (m_OverlayRoot != null && m_OverlayRoot.activeSelf != visible)
                m_OverlayRoot.SetActive(visible);
        }

        private void OnCharacterDeath()
        {
            SetOverlayVisible(false);
            MutinyDebugLog.Info("Overlay", $"hidden on death character={m_Character.name}", this);
        }
    }
}
