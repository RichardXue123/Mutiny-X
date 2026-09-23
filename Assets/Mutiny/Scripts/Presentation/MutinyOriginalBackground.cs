using Mutiny.Levels;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    // The source sprites keep the Flash registration point outside several PNGs.
    // These offsets are from the original sprite-origins.csv, not their PNG centres.
    public static class MutinyOriginalBackground
    {
        public const string ResourcePath = "Art/Background/";
        public const float Width = 550f;
        public const float Height = 400f;

        public static int SkyColourForLevel(int levelIndex)
        {
            if (levelIndex >= 6 && levelIndex <= 10 || levelIndex >= 22 && levelIndex <= 27)
                return 2;
            if (levelIndex >= 11 && levelIndex <= 15 || levelIndex >= 28 && levelIndex <= 33)
                return 3;
            return 1;
        }

        // Global.negativeModulo: a non-negative exact multiple maps to -period.
        public static float NegativeModulo(float value, float period)
        {
            return value >= 0f ? value % period - period : value % period;
        }

        public static Vector2 BattleLayerPosition(int layer, float contentX, float waterScreenY)
        {
            float relativeY = waterScreenY - 350f;
            switch (layer)
            {
                case 0: // backClouds, PNG origin (-197, -12)
                    return new Vector2(Mathf.Floor(NegativeModulo(contentX * 0.2f, 900f)) - 450f + 197f,
                        Mathf.Floor(relativeY * 0.2f - 30f) + 12f);
                case 1: // hills, PNG origin (0, 0)
                    return new Vector2(Mathf.Floor(NegativeModulo(contentX * 0.25f, 840f)),
                        Mathf.Floor(relativeY * 0.3f + 45f));
                case 2: // frontClouds, PNG origin (-71, -6)
                    return new Vector2(Mathf.Floor(NegativeModulo(contentX * 0.3f, 1000f)) + 71f,
                        Mathf.Floor(relativeY * 0.4f - 30f) + 6f);
                case 3: // cloudBase, PNG origin (0, -246)
                    return new Vector2(Mathf.Floor(NegativeModulo(contentX * 0.3f, 550f)),
                        Mathf.Floor(relativeY * 0.5f) + 246f);
                case 4: // waterBackground follows Water.y + Controller.content._y.
                    return new Vector2(0f, waterScreenY);
                default:
                    return Vector2.zero;
            }
        }
    }

    [DefaultExecutionOrder(1000)] // After MutinyCameraController.LateUpdate.
    [DisallowMultipleComponent]
    public sealed class MutinyBattleBackground : MonoBehaviour
    {
        private static readonly string[] LayerNames =
            { "backClouds", "hills", "frontClouds", "cloudBase", "waterBackground" };

        private readonly SpriteRenderer[] m_Layers = new SpriteRenderer[5];
        private SpriteRenderer m_Sky;
        private MutinyLevelRoot m_Level;
        private Camera m_Camera;

        public void Initialize(MutinyLevelRoot level, int skyColour)
        {
            m_Level = level;
            m_Sky = CreateLayer("sky", -70);
            int colour = Mathf.Clamp(skyColour, 1, 3);
            for (int i = 0; i < m_Layers.Length; i++)
                m_Layers[i] = CreateLayer(LayerNames[i] + colour, -60 + i * 10);
            UpdatePositions();
        }

        private SpriteRenderer CreateLayer(string assetName, int order)
        {
            Texture2D texture = Resources.Load<Texture2D>(MutinyOriginalBackground.ResourcePath + assetName);
            if (texture == null)
            {
                Debug.LogError("[Mutiny:Background] Missing original background layer: " + assetName, this);
                return null;
            }

            texture.filterMode = FilterMode.Point;
            GameObject child = new GameObject(assetName);
            child.transform.SetParent(transform, false);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0f, 1f), MutinyPhysics.PixelsPerUnit);
            renderer.sortingOrder = order;
            return renderer;
        }

        private void LateUpdate()
        {
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            if (m_Level == null)
                return;
            if (m_Camera == null)
            {
                MutinyCameraController controller = FindAnyObjectByType<MutinyCameraController>();
                m_Camera = controller != null ? controller.GetComponent<Camera>() : Camera.main;
            }
            if (m_Camera == null)
                return;

            float ppu = MutinyPhysics.PixelsPerUnit;
            Vector3 topLeft = m_Camera.transform.position +
                new Vector3(-MutinyOriginalBackground.Width / (2f * ppu),
                    MutinyOriginalBackground.Height / (2f * ppu), 0f);
            float cameraX = (topLeft.x - m_Level.transform.position.x) * ppu;
            float cameraY = (m_Level.transform.position.y - topLeft.y) * ppu;
            float waterScreenY = -m_Level.WaterLevelY * ppu - cameraY;

            Position(m_Sky, topLeft, Vector2.zero, ppu);
            for (int i = 0; i < m_Layers.Length; i++)
                Position(m_Layers[i], topLeft,
                    MutinyOriginalBackground.BattleLayerPosition(i, -cameraX, waterScreenY), ppu);
        }

        private static void Position(SpriteRenderer renderer, Vector3 topLeft, Vector2 screen, float ppu)
        {
            if (renderer != null)
                renderer.transform.position = new Vector3(topLeft.x + screen.x / ppu,
                    topLeft.y - screen.y / ppu, 0f);
        }
    }
}
