using System;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>
    /// Manages the system cursor state, seamlessly switching between the standard
    /// OS arrow pointer and the native pointing hand cursor when hovering over
    /// interactable buttons, characters, weapon slots, and links.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    [DisallowMultipleComponent]
    public sealed class MutinyCursorManager : MonoBehaviour
    {
        private static MutinyCursorManager s_Instance;
        private static Texture2D s_HandCursorTexture;
        private static int s_LastHoverFrame = -1;
        private static bool s_IsHandCursorActive;
        private bool m_WasControllerActive;

        // Windows standard 32x32 pointing hand cursor bitmap
        private const string SystemHandCursorPngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAABr0lEQVR4nGNgwAKERNRCgHgvEG8EYg9samgKQJYfPHj8/7Zte/6DHDEQDvgDBP9//vz1X1xKG+QINno74D8MgNh0tXxQOgCIlwLxfCA2A2JjIDagqwNWrd70f+mytSD2fAfnwP92Dv4gdgXIITRxDLoDkNnfv3////jxMxD7IcghUMdU08UB8krGKOJfvnz9/+zZCxD7Cd0dgO4YmjkAVA7AgKmFO+0cgFzYIDsAFMQw8PHjJzh73/4j1HEAUHMgEO+ClnjtIIcgO4AYQKkDdoF8A/JdRVXLfxFxzf/0dsCBw0dOwQ0DVUJGpi50dUC4lq7t/5cvX8MNRGbT3AFQR9R5ekeCaz9SAUgPUP9PihwAdcSE4pJ6kh0AKxUpdgDUEXMWLV5FkgNA9QSosqKWA4wlZfT+37hxm2gHuHmEgRxgQxUHQB2RA6pgkAsdXGDOvKUgy6dRzXIkR7QbGDn9v3DhClaLQc21to6JsDJDl+oOgDoiGRQdoMLp+Ikz/69euwkuI6ZMnfsf1CYAyk8GYlGaWI7kCE4gzgfiw0B8CdpU7wWlFZpaPApGwSgYUQAAmTw5vMtJ8rQAAAAASUVORK5CYII=";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            EnsureInstance();
        }

        public static void EnsureInstance()
        {
            if (s_Instance != null)
                return;

            GameObject host = new GameObject("MutinyCursorManager");
            DontDestroyOnLoad(host);
            s_Instance = host.AddComponent<MutinyCursorManager>();
        }

        private static void EnsureHandCursorTexture()
        {
            if (s_HandCursorTexture != null)
                return;

            s_HandCursorTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            byte[] pngData = Convert.FromBase64String(SystemHandCursorPngBase64);
            s_HandCursorTexture.LoadImage(pngData, false);
            s_HandCursorTexture.filterMode = FilterMode.Point;
        }

        /// <summary>
        /// Call during Update() or OnGUI() whenever the mouse is hovering over an interactable element.
        /// </summary>
        public static void NotifyHoverInteractable()
        {
            s_LastHoverFrame = Time.frameCount;
            if (!s_IsHandCursorActive)
            {
                s_IsHandCursorActive = true;
                EnsureHandCursorTexture();
                Cursor.SetCursor(s_HandCursorTexture, new Vector2(6f, 0f), CursorMode.Auto);
            }
        }

        public static bool IsHandCursorActive => s_IsHandCursorActive;

        private void LateUpdate()
        {
            UpdateCursor();
            bool controller = MutinyInputHub.Instance != null && MutinyInputHub.Instance.IsControllerActive;
            if (controller)
                Cursor.visible = false;
            else if (m_WasControllerActive)
            {
                MutinyCameraController camera = FindAnyObjectByType<MutinyCameraController>();
                MutinyPlayerInput player = FindAnyObjectByType<MutinyPlayerInput>();
                Cursor.visible = (camera == null || !camera.IsDesktopScrollArrowVisible) &&
                                 (player == null || !player.HasVisibleSpecialCursor);
            }
            m_WasControllerActive = controller;
        }

        public static void UpdateCursor()
        {
            if (s_IsHandCursorActive && (Time.frameCount - s_LastHoverFrame) > 1)
            {
                s_IsHandCursorActive = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        public static void ResetCursor()
        {
            s_LastHoverFrame = -1;
            if (s_IsHandCursorActive)
            {
                s_IsHandCursorActive = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        private void OnDisable()
        {
            ResetCursor();
        }
    }
}
