using Mutiny.Levels;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    [ExecuteAlways, DefaultExecutionOrder(1000), DisallowMultipleComponent]
    public sealed class MutinySpaceBackground : MonoBehaviour
    {
        // Serialized references survive the editor Load -> Save -> Play lifecycle.
        [SerializeField] private MutinyLevelRoot m_Level;
        [SerializeField] private SpriteRenderer m_Sky;
        [SerializeField] private SpriteRenderer m_Galaxy;
        [SerializeField] private MutinySpaceParallaxLayer[] m_ParallaxLayers;
        private bool m_ParallaxReady;
        public const int AnimationFrameCount = 100;
        public int CurrentAnimationFrame { get; private set; }
        private float m_AnimationAccumulator;
#if UNITY_EDITOR
        private double m_EditorTime;
#endif

        private void OnEnable()
        {
            CurrentAnimationFrame = 0;
            m_AnimationAccumulator = 0;
            m_ParallaxReady = false;
#if UNITY_EDITOR
            m_EditorTime = UnityEditor.EditorApplication.timeSinceStartup;
            UnityEditor.EditorApplication.update += UpdateEditorAnimation;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= UpdateEditorAnimation;
#endif
        }

        public void Initialize(MutinyLevelRoot level)
        {
            m_Level = level;
            m_Sky = CreateLayer("Parallax/deep_space", -100);
            m_Sky.gameObject.name = "Space_sky";
            EnsureParallaxLayers();
            RefreshForCamera(Camera.main);
        }

        private SpriteRenderer CreateLayer(string name, int order)
        {
            var child = new GameObject("Space_" + name);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>(MutinySpaceVisuals.ResourcePath + name);
            renderer.sortingOrder = order;
            if (renderer.sprite == null) Debug.LogError("[Mutiny:Space] Missing background: " + name, this);
            return renderer;
        }

        private void LateUpdate() => RefreshForCamera(Camera.main);

        private void Update()
        {
            if (Application.isPlaying) AdvanceAnimation(Time.deltaTime);
        }

        private bool AdvanceAnimation(float delta)
        {
            m_AnimationAccumulator += delta;
            int steps = Mathf.FloorToInt(m_AnimationAccumulator / MutinyPhysics.TimeStep);
            if (steps <= 0) return false;
            m_AnimationAccumulator -= steps * MutinyPhysics.TimeStep;
            CurrentAnimationFrame = (CurrentAnimationFrame + steps) % AnimationFrameCount;
            return true;
        }

        private void EnsureParallaxLayers()
        {
            if (m_ParallaxReady || m_Level == null) return;
            // Migrate scenes baked with the former flat sky/galaxy without duplicate layers.
            if (m_Sky != null)
            {
                m_Sky.sprite = Resources.Load<Sprite>(MutinySpaceVisuals.ResourcePath + "Parallax/deep_space");
                m_Sky.sortingOrder = -100;
            }
            if (m_Galaxy != null) m_Galaxy.enabled = false;
            m_ParallaxLayers = new[]
            {
                Layer("FarStars", "Parallax/starfield", new Vector2(.08f,.08f), Vector2.zero,
                    new Vector2(24,16), new Vector2(24,16), -90, true),
                Layer("Sun", "Parallax/sun", new Vector2(.12f,.10f), new Vector2(9,3),
                    new Vector2(2.5f,2.5f), new Vector2(32,0), -80),
                Layer("Moon", "Parallax/moon", new Vector2(.20f,.18f), new Vector2(15,-1),
                    new Vector2(2.5f,2.5f), new Vector2(34,0), -75),
                Layer("RingPlanet", "Parallax/ring_planet", new Vector2(.35f,.28f), new Vector2(3,1),
                    new Vector2(7.5f,6), new Vector2(32,0), -70),
                Layer("GalaxyFar", "galaxy", new Vector2(.3f,1), new Vector2(0,m_Level.WaterLevelY+2.4f),
                    new Vector2(24,8), new Vector2(24,0), -20, false, true),
                Layer("GalaxySurface", "Parallax/galaxy_surface", Vector2.one, new Vector2(0,m_Level.WaterLevelY+1),
                    new Vector2(12,6.75f), new Vector2(12,0), MutinyLevelBuilder.WaterSortingOrder, false, true, true)
            };
            m_ParallaxReady = true;
        }

        private MutinySpaceParallaxLayer Layer(string name, string resource, Vector2 parallax, Vector2 anchor,
            Vector2 size, Vector2 period, int order, bool repeatY = false, bool mirror = false, bool animated = false)
        {
            Transform child = transform.Find("Space_" + name);
            if (child == null)
            {
                child = new GameObject("Space_" + name).transform;
                child.SetParent(transform, false);
            }
            var layer = child.GetComponent<MutinySpaceParallaxLayer>();
            if (layer == null) layer = child.gameObject.AddComponent<MutinySpaceParallaxLayer>();
            layer.Configure(resource, parallax, anchor, size, period, order, repeatY, mirror, animated);
            return layer;
        }

#if UNITY_EDITOR
        private void UpdateEditorAnimation()
        {
            double now = UnityEditor.EditorApplication.timeSinceStartup;
            float delta = (float)(now - m_EditorTime);
            m_EditorTime = now;
            if (Application.isPlaying || !isActiveAndEnabled || m_Level == null) return;
            if (!AdvanceAnimation(delta)) return;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif

        public void RefreshForCamera(Camera camera)
        {
            if (camera == null || m_Level == null || !camera.orthographic) return;
            EnsureParallaxLayers();
            float height = camera.orthographicSize * 2f;
            float width = height * camera.aspect;
            float left = camera.transform.position.x - width * 0.5f;
            float top = camera.transform.position.y + height * 0.5f;
            if (m_Sky != null && m_Sky.sprite != null)
            {
                Vector2 skySize = m_Sky.sprite.bounds.size;
                float scale = Mathf.Max(width / skySize.x, height / skySize.y);
                Place(m_Sky, left - (skySize.x * scale - width) * 0.5f, top,
                    skySize.x * scale, skySize.y * scale);
            }
            foreach (var layer in m_ParallaxLayers)
                layer.Refresh(camera, m_Level.transform.position, (float)CurrentAnimationFrame / AnimationFrameCount);
        }

        private static void Place(SpriteRenderer renderer, float x, float y, float width, float height)
        {
            if (renderer == null || renderer.sprite == null) return;
            renderer.transform.position = new Vector3(x, y, 0f);
            Vector2 size = renderer.sprite.bounds.size;
            renderer.transform.localScale = new Vector3(width / size.x, height / size.y, 1f);
        }
    }
}
