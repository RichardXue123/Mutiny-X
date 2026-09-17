using System;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>
    /// Presents the two non-drag placeable-weapon cursors from the Flash root
    /// <c>cursor</c> MovieClip. The embedded PNG payloads are the unmodified
    /// exports of DefineSprite_1813 and DefineSprite_1898, so player builds do
    /// not depend on the reverse-engineering evidence directory.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class MutinySpecialWeaponCursor : MonoBehaviour
    {
        internal enum Mode
        {
            None,
            Seagull,
            TidalWave
        }

        // DefineSprite_1813_cursor frame 21, labelled "seagull".
        private const string SeagullCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAAAnElEQVR4Xu2RTQqEMAxGC3OAqQtBXEmZ+x+id3HjbNt1NWC7+FItcRzc5EGw5OdFiDGKojyNc+69fVIlEKzXesSkEAILygt7xBRRjDFlQM6W3vEDyXvPZNIgB7lQ3oKJrga5UN6iDH+XJQvmsetmlMMSdoo9J+Js+GhxtadSa1JuTm8smnP5K9ev3pxoDf5a/x+TtfYzDD3mFeUxVkloenvnshtAAAAAAElFTkSuQmCC";
        // DefineSprite_1813_cursor frame 32, labelled "tidalWave".
        private const string TidalWaveCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAABLUlEQVR4Xu2Uv2qEQBCHbdKkyBXprEQsfFVbH8FH8T1EBEELwT+NiOiG3+Z2b27OHMk6hBT5YLhjb2a+GeXW82RQB/ErqDzPHwLnPFEaLRqGQR2B3695L7RIAivetu1Ouu+76rqODyCGFnMhjXEcVV3XcgPEcfx+/arldGMIkyTRATHCgHzax4koit68AzGAtCgKva0ZAOATNbyXKw+PHECI82VZVN/39hz5vIErWrCuK9F+kmXZ3QBN01hxGIYX1seJw60BzoMgsIFcXnwGLZimiXv1pgA5vEgCu7ERUfAacC59s5mbycrneWbqG7hY6AC+77/eWrnx5Xvm4L9elqXYE/i2mNK27ekBnMQGXC6uA6g0TVVVVbznj0Ef9OOCZ5zamuKyvSmQjH/+Bh+cYikQ7j95nQAAAABJRU5ErkJggg==";
        // DefineSprite_1898_dottedLine frame 1 (636 px by 2 px).
        private const string DottedLinePng =
            "iVBORw0KGgoAAAANSUhEUgAAAnwAAAACCAYAAADCWgHOAAAAPklEQVR4Xu3MMQ4AIAgEQZ7uz7E/bGyMxZBsAxOqu9episk7x3Fcxk3DcRz3hcvF9QNuGI7jOI7LuGm4d24Db2F3lxo/n7EAAAAASUVORK5CYII=";

        private Texture2D m_SeagullCursor;
        private Texture2D m_TidalWaveCursor;
        private Texture2D m_DottedLine;
        private Mode m_Mode;
        private Vector2 m_MousePosition;

        internal Mode CurrentMode => m_Mode;
        internal bool ShowsDottedGuide => m_Mode == Mode.Seagull;

        internal void SetMode(Mode mode, Vector2 mousePosition)
        {
            m_Mode = mode;
            m_MousePosition = mousePosition;
            Cursor.visible = mode == Mode.None;
        }

        internal void Clear()
        {
            SetMode(Mode.None, Vector2.zero);
        }

        private void OnDisable()
        {
            Cursor.visible = true;
        }

        private void OnGUI()
        {
            if (m_Mode == Mode.None || Event.current.type != EventType.Repaint)
                return;

            EnsureTextures();
            float guiY = Screen.height - m_MousePosition.y;
            if (m_Mode == Mode.Seagull && m_DottedLine != null)
            {
                // Seagull.as sets dottedLine.x to the left edge of visible content
                // and dottedLine.y to content._ymouse every pre-flight tick.
                GUI.DrawTextureWithTexCoords(
                    new Rect(0f, guiY - 1f, Screen.width, m_DottedLine.height),
                    m_DottedLine,
                    new Rect(0f, 0f, Screen.width / (float)m_DottedLine.width, 1f),
                    true);
            }

            Texture2D cursor = m_Mode == Mode.Seagull ? m_SeagullCursor : m_TidalWaveCursor;
            if (cursor != null)
                GUI.DrawTexture(new Rect(m_MousePosition.x - 15f, guiY - 11f, cursor.width, cursor.height), cursor, ScaleMode.StretchToFill, true);
        }

        private void EnsureTextures()
        {
            if (m_SeagullCursor == null)
                m_SeagullCursor = DecodeOriginalTexture(SeagullCursorPng, "OriginalCursor_Seagull", TextureWrapMode.Clamp);
            if (m_TidalWaveCursor == null)
                m_TidalWaveCursor = DecodeOriginalTexture(TidalWaveCursorPng, "OriginalCursor_TidalWave", TextureWrapMode.Clamp);
            if (m_DottedLine == null)
                m_DottedLine = DecodeOriginalTexture(DottedLinePng, "OriginalCursor_SeagullDottedLine", TextureWrapMode.Repeat);
        }

        private static Texture2D DecodeOriginalTexture(string encodedPng, string textureName, TextureWrapMode wrapMode)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = wrapMode
            };
            if (!ImageConversion.LoadImage(texture, Convert.FromBase64String(encodedPng), true))
            {
                Destroy(texture);
                return null;
            }
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = wrapMode;
            return texture;
        }
    }
}
