using System.Collections.Generic;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Levels
{
    // Tile.show synchronizes a multi-frame MovieClip to animationCounter.
    // A one-frame cave_torch instead contains a freely playing 24-frame child.
    [DefaultExecutionOrder(1100)] // Read the camera after MutinyCameraController.LateUpdate.
    [DisallowMultipleComponent]
    public sealed class MutinyAnimatedTiles : MonoBehaviour
    {
        private sealed class TileAnimation
        {
            public SpriteRenderer Renderer;
            public Sprite[] Frames;
            public int GridX;
            public int GridY;
            public int LocalFrame;
            public bool IsTorch;
            public bool WasVisible;
        }

        private readonly Dictionary<string, Sprite[]> m_SharedFrames = new Dictionary<string, Sprite[]>();
        private readonly List<TileAnimation> m_Tiles = new List<TileAnimation>();
        private Camera m_Camera;
        private float m_Accumulator;
        private int m_AnimationCounter;
        private bool m_VisibilityInitialized;

        public void Register(SpriteRenderer renderer, string tileName, int gridX, int gridY)
        {
            if (renderer == null || string.IsNullOrEmpty(tileName))
                return;

            if (tileName == "cave_torch")
            {
                RegisterTorch(renderer, gridX, gridY);
                return;
            }

            if (!MutinyTileCatalog.TryGetDefinition(tileName, out MutinyTileDefinition definition) ||
                definition.IsLogic || definition.FrameCount <= 1)
                return;

            string path = $"Art/Tiles/Animated/{tileName}/{tileName}_";
            Sprite[] frames = LoadFrames(path, definition.FrameCount, true);
            if (frames == null)
                return;

            renderer.sprite = frames[0];
            m_Tiles.Add(new TileAnimation
            {
                Renderer = renderer,
                Frames = frames,
                GridX = gridX,
                GridY = gridY
            });
        }

        private void RegisterTorch(SpriteRenderer parent, int gridX, int gridY)
        {
            Sprite baseSprite = Resources.Load<Sprite>("Art/Tiles/Single/cave_torch_base");
            Sprite[] flames = LoadFrames("Art/Tiles/Animated/cave_torch_flame/", 24, false);
            if (baseSprite == null || flames == null)
            {
                Debug.LogError("[Mutiny:Tiles] Original cave_torch base or flame frames are missing.", this);
                return; // Keep the existing composite frame if extraction is incomplete.
            }

            parent.sprite = baseSprite;
            GameObject flame = new GameObject("cave_torch_flame");
            flame.transform.SetParent(parent.transform, false);
            // Symbol 1486 places its child 1485 at (280, 260) twips = (14, 13) px.
            flame.transform.localPosition = new Vector3(14f / MutinyPhysics.PixelsPerUnit,
                -13f / MutinyPhysics.PixelsPerUnit, 0f);
            SpriteRenderer flameRenderer = flame.AddComponent<SpriteRenderer>();
            flameRenderer.sprite = flames[0];
            flameRenderer.sortingLayerID = parent.sortingLayerID;
            flameRenderer.sortingOrder = parent.sortingOrder + 1;
            m_Tiles.Add(new TileAnimation
            {
                Renderer = flameRenderer,
                Frames = flames,
                GridX = gridX,
                GridY = gridY,
                IsTorch = true
            });
        }

        private Sprite[] LoadFrames(string resourcePrefix, int count, bool twoDigitSuffix)
        {
            if (m_SharedFrames.TryGetValue(resourcePrefix, out Sprite[] cached))
                return cached;

            Sprite[] frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                string frameName = twoDigitSuffix ? (i + 1).ToString("D2") : (i + 1).ToString();
                frames[i] = Resources.Load<Sprite>(resourcePrefix + frameName);
                if (frames[i] != null)
                    continue;

                Debug.LogError($"[Mutiny:Tiles] Missing animation frame {resourcePrefix}{frameName}", this);
                return null;
            }

            m_SharedFrames.Add(resourcePrefix, frames);
            return frames;
        }

        private void LateUpdate()
        {
            if (m_Tiles.Count == 0)
                return;

            if (!m_VisibilityInitialized)
            {
                for (int i = 0; i < m_Tiles.Count; i++)
                {
                    TileAnimation tile = m_Tiles[i];
                    if (tile.IsTorch)
                        tile.WasVisible = IsInOriginalTileViewport(tile.GridX, tile.GridY);
                }
                m_VisibilityInitialized = true;
            }

            m_Accumulator += Time.deltaTime;
            while (m_Accumulator >= MutinyPhysics.TimeStep)
            {
                m_Accumulator -= MutinyPhysics.TimeStep;
                AdvanceOriginalTick();
            }
        }

        // Called once per original 25 Hz tick. Frames 1..16 are shared across
        // all ripple tiles; nested torch flames start when their tile is shown.
        internal void AdvanceOriginalTick()
        {
            m_AnimationCounter++;
            for (int i = 0; i < m_Tiles.Count; i++)
            {
                TileAnimation tile = m_Tiles[i];
                if (tile.Renderer == null)
                    continue;

                if (!tile.IsTorch)
                {
                    tile.Renderer.sprite = tile.Frames[m_AnimationCounter % tile.Frames.Length];
                    continue;
                }

                bool visible = IsInOriginalTileViewport(tile.GridX, tile.GridY);
                if (!visible)
                {
                    tile.WasVisible = false;
                    continue;
                }

                tile.LocalFrame = tile.WasVisible ? (tile.LocalFrame + 1) % tile.Frames.Length : 0;
                tile.WasVisible = true;
                tile.Renderer.sprite = tile.Frames[tile.LocalFrame];
            }
        }

        private bool IsInOriginalTileViewport(int gridX, int gridY)
        {
            if (m_Camera == null)
            {
                MutinyCameraController cameraController = FindAnyObjectByType<MutinyCameraController>();
                m_Camera = cameraController != null ? cameraController.GetComponent<Camera>() : Camera.main;
            }
            if (m_Camera == null)
                return true;

            float ppu = MutinyPhysics.PixelsPerUnit;
            Vector3 topLeft = m_Camera.transform.position + new Vector3(
                -MutinyOriginalBackground.Width / (2f * ppu),
                MutinyOriginalBackground.Height / (2f * ppu), 0f);
            int cameraGridX = Mathf.FloorToInt((topLeft.x - transform.position.x) * ppu / 32f);
            int cameraGridY = Mathf.FloorToInt((transform.position.y - topLeft.y) * ppu / 32f);
            // TileSystem.panCamera: (cameraX >> 5)-1..(cameraX >> 5)+18,
            // and (cameraY >> 5)-1..(cameraY >> 5)+13.
            return gridX >= cameraGridX - 1 && gridX <= cameraGridX + 18 &&
                   gridY >= cameraGridY - 1 && gridY <= cameraGridY + 13;
        }
    }
}
