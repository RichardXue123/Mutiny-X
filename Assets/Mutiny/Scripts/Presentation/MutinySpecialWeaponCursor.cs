using System;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>
    /// Presents special weapon cursors from the Flash root
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
            TidalWave,
            ParachuteFan,
            WoodenCrate,
            GunpowderBarrel,
            Cross
        }

        // DefineSprite_1813_cursor frame 21, labelled "seagull".
        private const string SeagullCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAAAnElEQVR4Xu2RTQqEMAxGC3OAqQtBXEmZ+x+id3HjbNt1NWC7+FItcRzc5EGw5OdFiDGKojyNc+69fVIlEKzXesSkEAILygt7xBRRjDFlQM6W3vEDyXvPZNIgB7lQ3oKJrga5UN6iDH+XJQvmsetmlMMSdoo9J+Js+GhxtadSa1JuTm8smnP5K9ev3pxoDf5a/x+TtfYzDD3mFeUxVkloenvnshtAAAAAAElFTkSuQmCC";
        // DefineSprite_1813_cursor frame 32, labelled "tidalWave".
        private const string TidalWaveCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAABLUlEQVR4Xu2Uv2qEQBCHbdKkyBXprEQsfFVbH8FH8T1EBEELwT+NiOiG3+Z2b27OHMk6hBT5YLhjb2a+GeXW82RQB/ErqDzPHwLnPFEaLRqGQR2B3695L7RIAivetu1Ouu+76rqODyCGFnMhjXEcVV3XcgPEcfx+/arldGMIkyTRATHCgHzax4koit68AzGAtCgKva0ZAOATNbyXKw+PHECI82VZVN/39hz5vIErWrCuK9F+kmXZ3QBN01hxGIYX1seJw60BzoMgsIFcXnwGLZimiXv1pgA5vEgCu7ERUfAacC59s5mbycrneWbqG7hY6AC+77/eWrnx5Xvm4L9elqXYE/i2mNK27ekBnMQGXC6uA6g0TVVVVbznj0Ef9OOCZ5zamuKyvSmQjH/+Bh+cYikQ7j95nQAAAABJRU5ErkJggg==";
        // DefineSprite_1813_cursor labels: gunpowderBarrel frame 11,
        // woodenCrate frame 50, and invalid-placement cross frame 70.
        private const string GunpowderBarrelCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAABYElEQVR4XuWV206DQBCGufTOu3qrvCzhXTSt+gIeWl+h3nMGsW26HBaBcWcipG7VsqwmGr9m03Rg/m+nhWIYP4BpmsdyTRdQWN8KqIDnywFjkbMHgX1ykCpg2/aoZVmW9gbgKUkgiCO4md/Dlm2hbVvIiwKKsoS7hwVNWdc11V3foxqvODwul1pyuLicAReSJE37UJI1zTt5R5TEVGvFKwxD6sccOXgI1FxyDp7v0+QsYyRB8a68EZtBvMCnWp7n8Lxa6ctxSsd1leSMMTr+P+V4UZ7Ppn9Y3ohbaIwcj0+vr/TkvKrA9bxejp8PybM8g/Vmoz853jZhHMPtYi6+ypwk3Qbk+zyIQqpVLxX4QaAvL4Q8Xa/6UAR/io/k3Z+M4zi0dOQITY3Ioq9quEmUY78cqMLeA0NlYb8cOJizyeREvMnDfUr2dk0g2HdqGEdS5Ch2FIfB8+UAXbrQIev38woSrSr9RzEmMAAAAABJRU5ErkJggg==";
        private const string WoodenCrateCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAABIklEQVR4Xs2W3YqDMBCF+/rC9hWW3RtfTC+q+AOipirurEeYIU5bmpoIfnBQp3bOIYnGy+XEUADtgr5/fyjP84+VZRn9EVF0/dodgG5LI0YbvBPMIfTRjV1YTeu6Xo9RFDkL5l3X0d3c/czZeBiGVTjHsLZtS9M0SZ1ljJEAuAd9dGMXNuZMURQSAHrGYeZgHMdNAK3g5joA1+M4flBwc/vIVFX1dAQOMdfnoO97CYD1wPcENcdKnudZrgFfcw0B+IkIag4jW3oEyrKUAMHN9aLihWXTNM1aC27+Cv4NLxy7dri5PQVJkkidzfGKRR/d2IVNs3fwfNtz7rWxtEtykKapzKWL7J0NfXRjF2Rb3ENnej9zfAxwgD3y+ZgA/GcfnZN/MIuNFOJXnfgAAAAASUVORK5CYII=";
        private const string CrossCursorPng =
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAWCAYAAAA4oUfxAAAA50lEQVR4Xu2U0QqDMAxF/f/f9EkRhTIVBe1IWEp7W2e6Fp92INgmN7lBwab58x2LiRty9ZdYgp5YuMD2fZ+jv4SNBbqjAGBjCbqjQIsznaZJs0BgXGUB35jYti01kPMS67reLaomWmAYBn9wUBO8ejHRAvu+s0HXdUHUNhaiBQT5vgTpsLEWYGvtOI7uTHVsqE30BpZlecYYOY7DnamODbVwZhIpSIeNpaAHk8obY6ougPMZyivrP4MzGcqndG3b3unUBIMEyqPwA9f9n03xv/01zxpjwWlLjQV76owF7aJqcgfl6p/nDYW/RkdHdfZMAAAAAElFTkSuQmCC";
        // DefineSprite_1813_cursor frame 60 contains the four-frame child fan
        // DefineSprite 1806. CustomCursor rotates it toward the active bomb.
        private static readonly string[] FanCursorPngs =
        {
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAVCAYAAAC+NTVfAAAA3ElEQVR4Xr2QUQ7EIAhEe/+DeEL/d4MJDT4ZrbttJyEKwjzicezrM4nH1AC11jNKKV3uPZj7S82QoAzuNZ+Bz7aWoFnN5ml4VdJ0p2Y+NF5pMM2MV/kvCwzDynyVxzBfgjJJE9ZXeaybL0HUJaPVXfWbP4FRcjCrqbuqmT+BrmE4M1DAWV8M4xBsGhqVkec8+Z6FcQg2tQc2Z4aEZm9ZOAPcTnIJwtTJcE9wpkqXiCAPBXYP+G5pWCKC4wJ3QqluiQzsPZi7VQ0Qv9tr6HtM2Te/og789gLxmxlb+gJ6eHFMSf1YpgAAAABJRU5ErkJggg==",
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAVCAYAAAC+NTVfAAAA1ElEQVR4XtWQYQrDMAhGe/+D5CCD7UT5v2HAoi+atksz2AdSM/S9LNv2Z3kPalmaoNa6VynFnXUGe1PZpZRlZ90B53JC+Nmz7BN4Ng4UCbJ+9gIdJJJkPUt4FIySwjLhaF54FGQZAjPh0axwKYqSAnjO+ugsXIoYtxCBrva2hE+hTbcQAbXnlz1L+BTatAEuEXz0jUrZ8IUJL0FJ9v1WynSXsCItOT9ez9ukjAOLyMr5j2WegNm4V4jEOoO9W9ME9vn1N8wty/JnzuLEv76AfWbWpXwAkhfiDnjQfJsAAAAASUVORK5CYII=",
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAVCAYAAAC+NTVfAAAA3ElEQVR4Xr2QUQ7EIAhEe/+DeEL/d4MJDT4ZrbttJyEKwjzicezrM4nH1AC11jNKKV3uPZj7S82QoAzuNZ+Bz7aWoFnN5ml4VdJ0p2Y+NF5pMM2MV/kvCwzDynyVxzBfgjJJE9ZXeaybL0HUJaPVXfWbP4FRcjCrqbuqmT+BrmE4M1DAWV8M4xBsGhqVkec8+Z6FcQg2tQc2Z4aEZm9ZOAPcTnIJwtTJcE9wpkqXiCAPBXYP+G5pWCKC4wJ3QqluiQzsPZi7VQ0Qv9tr6HtM2Te/og789gLxmxlb+gJ6eHFMSf1YpgAAAABJRU5ErkJggg==",
            "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAVCAYAAAC+NTVfAAAA3UlEQVR4XrWQYQ6FMAiDvf9BPKH/feElLOwDBjPapHEr0OKOYw93g69iGF/XNfE8T6fZ/sllE3+DKGAVbmvqAd8S7QBqUU38GJBhafRUF18GEW4oM3uiiz8DFa65MuxqlpKzFZyZdjWSC7gGMjLtaqQL7wyxp7pHlB4bPhVWBqxVd9ZsXRcIG6lFenVf6Wm4DkRDVsvOlYdwGZ4ZdM9ZqHKEVwuoIc/88pxRMyVcMUQ2W+OKnAkCp9AIUzONOoH0MN7bcItET277pukX4V5DNfR9AvfEZoHPwed9/Oc/BQ3Xn9Dlh7AAAAAASUVORK5CYII="
        };
        // DefineSprite_1898_dottedLine frame 1 (636 px by 2 px).
        private const string DottedLinePng =
            "iVBORw0KGgoAAAANSUhEUgAAAnwAAAACCAYAAADCWgHOAAAAPklEQVR4Xu3MMQ4AIAgEQZ7uz7E/bGyMxZBsAxOqu9episk7x3Fcxk3DcRz3hcvF9QNuGI7jOI7LuGm4d24Db2F3lxo/n7EAAAAASUVORK5CYII=";

        private Texture2D m_SeagullCursor;
        private Texture2D m_TidalWaveCursor;
        private Texture2D m_WoodenCrateCursor;
        private Texture2D m_GunpowderBarrelCursor;
        private Texture2D m_CrossCursor;
        private readonly Texture2D[] m_FanCursors = new Texture2D[4];
        private Texture2D m_DottedLine;
        private Mode m_Mode;
        private Vector2 m_MousePosition;
        private float m_RotationDegrees;
        private bool m_AnimateFan;
        private float m_FanTickAccumulator;
        private int m_FanFrame;

        internal Mode CurrentMode => m_Mode;
        internal bool ShowsDottedGuide => m_Mode == Mode.Seagull;
        internal float RotationDegrees => m_RotationDegrees;
        internal int CurrentFanFrame => m_FanFrame + 1;
        internal bool IsFanAnimating => m_Mode == Mode.ParachuteFan && m_AnimateFan;

        internal void SetMode(Mode mode, Vector2 mousePosition, float rotationDegrees = 0f, bool animateFan = false)
        {
            if (m_Mode != mode)
            {
                m_FanFrame = 0;
                m_FanTickAccumulator = 0f;
            }
            m_Mode = mode;
            m_MousePosition = mousePosition;
            m_RotationDegrees = rotationDegrees;
            m_AnimateFan = mode == Mode.ParachuteFan && animateFan;
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

        private void Update()
        {
            if (!IsFanAnimating)
                return;

            AdvanceFanAnimation(Time.deltaTime);
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

            Texture2D cursor = ResolveCursorTexture();
            if (cursor == null)
                return;

            Matrix4x4 previousMatrix = GUI.matrix;
            if (m_Mode == Mode.ParachuteFan)
                GUIUtility.RotateAroundPivot(m_RotationDegrees, new Vector2(m_MousePosition.x, guiY));
            float pivotY = m_Mode == Mode.ParachuteFan ? 10f : 11f;
            GUI.DrawTexture(new Rect(m_MousePosition.x - 15f, guiY - pivotY, cursor.width, cursor.height), cursor, ScaleMode.StretchToFill, true);
            GUI.matrix = previousMatrix;
        }

        private void EnsureTextures()
        {
            if (m_SeagullCursor == null)
                m_SeagullCursor = DecodeOriginalTexture(SeagullCursorPng, "OriginalCursor_Seagull", TextureWrapMode.Clamp);
            if (m_TidalWaveCursor == null)
                m_TidalWaveCursor = DecodeOriginalTexture(TidalWaveCursorPng, "OriginalCursor_TidalWave", TextureWrapMode.Clamp);
            if (m_WoodenCrateCursor == null)
                m_WoodenCrateCursor = DecodeOriginalTexture(WoodenCrateCursorPng, "OriginalCursor_WoodenCrate", TextureWrapMode.Clamp);
            if (m_GunpowderBarrelCursor == null)
                m_GunpowderBarrelCursor = DecodeOriginalTexture(GunpowderBarrelCursorPng, "OriginalCursor_GunpowderBarrel", TextureWrapMode.Clamp);
            if (m_CrossCursor == null)
                m_CrossCursor = DecodeOriginalTexture(CrossCursorPng, "OriginalCursor_Cross", TextureWrapMode.Clamp);
            for (int i = 0; i < m_FanCursors.Length; i++)
            {
                if (m_FanCursors[i] == null)
                    m_FanCursors[i] = DecodeOriginalTexture(
                        FanCursorPngs[i], $"OriginalCursor_Fan_{i + 1}", TextureWrapMode.Clamp);
            }
            if (m_DottedLine == null)
                m_DottedLine = DecodeOriginalTexture(DottedLinePng, "OriginalCursor_SeagullDottedLine", TextureWrapMode.Repeat);
        }

        private Texture2D ResolveCursorTexture()
        {
            switch (m_Mode)
            {
                case Mode.Seagull:
                    return m_SeagullCursor;
                case Mode.TidalWave:
                    return m_TidalWaveCursor;
                case Mode.ParachuteFan:
                    return m_FanCursors[m_FanFrame];
                case Mode.WoodenCrate:
                    return m_WoodenCrateCursor;
                case Mode.GunpowderBarrel:
                    return m_GunpowderBarrelCursor;
                case Mode.Cross:
                    return m_CrossCursor;
                default:
                    return null;
            }
        }

        private void AdvanceFanAnimation(float deltaTime)
        {
            m_FanTickAccumulator += deltaTime;
            while (m_FanTickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_FanTickAccumulator -= MutinyPhysics.TimeStep;
                m_FanFrame = (m_FanFrame + 1) % m_FanCursors.Length;
            }
        }

        internal void AdvanceFanFrameForVerification()
        {
            if (IsFanAnimating)
                m_FanFrame = (m_FanFrame + 1) % m_FanCursors.Length;
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
