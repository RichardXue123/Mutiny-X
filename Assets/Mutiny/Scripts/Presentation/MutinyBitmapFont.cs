using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Presentation
{
    public static class MutinyBitmapFont
    {
        public readonly struct Glyph
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Width;
            public readonly int Height;
            public readonly int OriginYFromTop;

            public Glyph(int x, int y, int width, int height, int originYFromTop = 0)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                OriginYFromTop = originYFromTop;
            }
        }

        private static Texture2D s_PirateFontTexture;
        private static Texture2D s_DangleFontTexture;

        private static readonly Dictionary<char, Glyph> s_PirateNormal = new Dictionary<char, Glyph>
        {
            ['a'] = new Glyph(2, 2, 26, 23),
            ['b'] = new Glyph(32, 2, 23, 23),
            ['c'] = new Glyph(59, 2, 24, 23),
            ['d'] = new Glyph(87, 2, 24, 23),
            ['e'] = new Glyph(115, 2, 26, 23),
            ['f'] = new Glyph(145, 2, 22, 23),
            ['g'] = new Glyph(171, 2, 24, 26),
            ['h'] = new Glyph(199, 2, 25, 23),
            ['i'] = new Glyph(228, 2, 15, 23),
            ['j'] = new Glyph(247, 2, 22, 26),
            ['k'] = new Glyph(273, 2, 38, 31),
            ['l'] = new Glyph(315, 2, 23, 23),
            ['m'] = new Glyph(342, 2, 28, 23),
            ['n'] = new Glyph(374, 2, 40, 33, 2),
            ['o'] = new Glyph(418, 2, 24, 23),
            ['p'] = new Glyph(446, 2, 23, 24, 1),
            ['q'] = new Glyph(473, 2, 26, 27),
            ['r'] = new Glyph(2, 39, 39, 31, 1),
            ['s'] = new Glyph(45, 39, 22, 23),
            ['t'] = new Glyph(71, 39, 23, 23),
            ['u'] = new Glyph(98, 39, 24, 23),
            ['v'] = new Glyph(126, 39, 26, 23),
            ['w'] = new Glyph(156, 39, 35, 23),
            ['x'] = new Glyph(195, 39, 26, 23),
            ['y'] = new Glyph(225, 39, 23, 23),
            ['z'] = new Glyph(252, 39, 23, 23),
            ['0'] = new Glyph(279, 39, 16, 18),
            ['1'] = new Glyph(299, 39, 15, 23),
            ['2'] = new Glyph(318, 39, 26, 23),
            ['3'] = new Glyph(348, 39, 16, 18),
            ['4'] = new Glyph(368, 39, 16, 18),
            ['5'] = new Glyph(388, 39, 16, 18),
            ['6'] = new Glyph(408, 39, 16, 18),
            ['7'] = new Glyph(428, 39, 16, 18),
            ['8'] = new Glyph(448, 39, 16, 18),
            ['9'] = new Glyph(468, 39, 16, 18),
            [' '] = new Glyph(488, 39, 16, 18),
            ['.'] = new Glyph(2, 76, 10, 18),
            [','] = new Glyph(16, 76, 10, 20),
            ['?'] = new Glyph(30, 76, 16, 18),
            ['!'] = new Glyph(50, 76, 10, 18),
            ['-'] = new Glyph(64, 76, 16, 18),
        };

        private static readonly Dictionary<char, Glyph> s_PirateHover = new Dictionary<char, Glyph>
        {
            ['a'] = new Glyph(2, 117, 26, 23),
            ['b'] = new Glyph(32, 117, 23, 23),
            ['c'] = new Glyph(59, 117, 24, 23),
            ['d'] = new Glyph(87, 117, 24, 23),
            ['e'] = new Glyph(115, 117, 26, 23),
            ['f'] = new Glyph(145, 117, 22, 23),
            ['g'] = new Glyph(171, 117, 24, 26),
            ['h'] = new Glyph(199, 117, 25, 23),
            ['i'] = new Glyph(228, 117, 15, 23),
            ['j'] = new Glyph(247, 117, 22, 26),
            ['k'] = new Glyph(273, 117, 38, 31),
            ['l'] = new Glyph(315, 117, 23, 23),
            ['m'] = new Glyph(342, 117, 28, 23),
            ['n'] = new Glyph(374, 117, 40, 33, 2),
            ['o'] = new Glyph(418, 117, 24, 23),
            ['p'] = new Glyph(446, 117, 23, 24, 1),
            ['q'] = new Glyph(473, 117, 26, 27),
            ['r'] = new Glyph(2, 154, 39, 31, 1),
            ['s'] = new Glyph(45, 154, 22, 23),
            ['t'] = new Glyph(71, 154, 23, 23),
            ['u'] = new Glyph(98, 154, 24, 23),
            ['v'] = new Glyph(126, 154, 26, 23),
            ['w'] = new Glyph(156, 154, 35, 23),
            ['x'] = new Glyph(195, 154, 26, 23),
            ['y'] = new Glyph(225, 154, 23, 23),
            ['z'] = new Glyph(252, 154, 23, 23),
            ['0'] = new Glyph(279, 154, 16, 18),
            ['1'] = new Glyph(299, 154, 15, 23),
            ['2'] = new Glyph(318, 154, 26, 23),
            ['3'] = new Glyph(348, 154, 16, 18),
            ['4'] = new Glyph(368, 154, 16, 18),
            ['5'] = new Glyph(388, 154, 16, 18),
            ['6'] = new Glyph(408, 154, 16, 18),
            ['7'] = new Glyph(428, 154, 16, 18),
            ['8'] = new Glyph(448, 154, 16, 18),
            ['9'] = new Glyph(468, 154, 16, 18),
            [' '] = new Glyph(488, 154, 16, 18),
            ['.'] = new Glyph(2, 191, 10, 18),
            [','] = new Glyph(16, 191, 10, 20),
            ['?'] = new Glyph(30, 191, 16, 18),
            ['!'] = new Glyph(50, 191, 10, 18),
            ['-'] = new Glyph(64, 191, 16, 18),
        };

        private static readonly Dictionary<char, Glyph> s_DangleGlyphs = new Dictionary<char, Glyph>
        {
            ['a'] = new Glyph(2, 2, 8, 11),
            ['b'] = new Glyph(14, 2, 8, 11),
            ['c'] = new Glyph(26, 2, 8, 11),
            ['d'] = new Glyph(38, 2, 8, 11),
            ['e'] = new Glyph(50, 2, 8, 11),
            ['f'] = new Glyph(62, 2, 8, 11),
            ['g'] = new Glyph(74, 2, 8, 11),
            ['h'] = new Glyph(86, 2, 8, 11),
            ['i'] = new Glyph(98, 2, 4, 11),
            ['j'] = new Glyph(106, 2, 8, 11),
            ['k'] = new Glyph(118, 2, 8, 11),
            ['l'] = new Glyph(130, 2, 8, 11),
            ['m'] = new Glyph(142, 2, 12, 11),
            ['n'] = new Glyph(158, 2, 8, 11),
            ['o'] = new Glyph(170, 2, 8, 11),
            ['p'] = new Glyph(182, 2, 8, 11),
            ['q'] = new Glyph(194, 2, 9, 11),
            ['r'] = new Glyph(207, 2, 8, 11),
            ['s'] = new Glyph(219, 2, 8, 11),
            ['t'] = new Glyph(231, 2, 8, 11),
            ['u'] = new Glyph(243, 2, 8, 11),
            ['v'] = new Glyph(2, 17, 8, 11),
            ['w'] = new Glyph(14, 17, 12, 11),
            ['x'] = new Glyph(30, 17, 8, 11),
            ['y'] = new Glyph(42, 17, 8, 11),
            ['z'] = new Glyph(54, 17, 8, 11),
            ['0'] = new Glyph(66, 17, 8, 11),
            ['1'] = new Glyph(78, 17, 6, 11),
            ['2'] = new Glyph(88, 17, 8, 11),
            ['3'] = new Glyph(100, 17, 8, 11),
            ['4'] = new Glyph(112, 17, 8, 11),
            ['5'] = new Glyph(124, 17, 8, 11),
            ['6'] = new Glyph(136, 17, 8, 11),
            ['7'] = new Glyph(148, 17, 8, 11),
            ['8'] = new Glyph(160, 17, 8, 11),
            ['9'] = new Glyph(172, 17, 8, 11),
            [' '] = new Glyph(184, 17, 4, 11),
            ['.'] = new Glyph(192, 17, 4, 11),
            [','] = new Glyph(200, 17, 4, 12),
            ['?'] = new Glyph(208, 17, 8, 11),
            ['!'] = new Glyph(220, 17, 4, 11),
            ['-'] = new Glyph(228, 17, 8, 11),
            ['\''] = new Glyph(240, 17, 4, 11),
        };

        private static void EnsureTextures()
        {
            if (s_PirateFontTexture == null)
                s_PirateFontTexture = Resources.Load<Texture2D>("UI/Fonts/pirate_font");
            if (s_DangleFontTexture == null)
                s_DangleFontTexture = Resources.Load<Texture2D>("UI/Fonts/dangle_font");
        }

        // --- PirateFont API ---

        public static float MeasurePirateText(string text, int tracking = -3)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            text = text.ToLowerInvariant();
            float totalWidth = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (s_PirateNormal.TryGetValue(c, out Glyph g))
                {
                    float charW = GetPirateAdvance(c, g);
                    totalWidth += charW;
                    if (i < text.Length - 1)
                        totalWidth += tracking;
                }
            }
            return Mathf.Max(0f, totalWidth);
        }

        private static float GetPirateAdvance(char c, in Glyph g)
        {
            // Flash ActionScript kerning overrides
            switch (c)
            {
                case 'k': return 27f;
                case 'n': return 28f;
                case 'r': return 29f;
                default: return g.Width;
            }
        }

        /// <summary>
        /// Returns the original Flash symbol's top edge relative to the common
        /// PirateFont holder origin. PirateFont.as never changes a letter's Y;
        /// the exported symbol registration point supplies this offset.
        /// </summary>
        public static float GetPirateGlyphTopOffset(char character)
        {
            char normalized = char.ToLowerInvariant(character);
            return s_PirateNormal.TryGetValue(normalized, out Glyph glyph)
                ? -glyph.OriginYFromTop
                : 0f;
        }

        public static void DrawPirateText(Rect container, string text, bool isHovered, bool centered = true, int tracking = -3)
        {
            if (string.IsNullOrEmpty(text))
                return;

            EnsureTextures();
            if (s_PirateFontTexture == null)
                return;

            text = text.ToLowerInvariant();
            Dictionary<char, Glyph> glyphDict = isHovered ? s_PirateHover : s_PirateNormal;
            float measuredWidth = MeasurePirateText(text, tracking);

            float startX = centered ? (container.x + (container.width - measuredWidth) * 0.5f) : container.x;
            float currentX = Mathf.Round(startX);
            float baseY = container.y + (container.height - 23f) * 0.5f;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (glyphDict.TryGetValue(c, out Glyph g))
                {
                    // All letters are attached at the same holder Y in PirateFont.as.
                    // Preserve each exported symbol's registration point instead of
                    // centering its trimmed bitmap by height.
                    float glyphY = baseY + GetPirateGlyphTopOffset(c);

                    Rect screenRect = new Rect(currentX, Mathf.Round(glyphY), g.Width, g.Height);
                    Rect texCoords = new Rect(
                        (float)g.X / s_PirateFontTexture.width,
                        1f - (float)(g.Y + g.Height) / s_PirateFontTexture.height,
                        (float)g.Width / s_PirateFontTexture.width,
                        (float)g.Height / s_PirateFontTexture.height
                    );

                    GUI.DrawTextureWithTexCoords(screenRect, s_PirateFontTexture, texCoords, true);
                    currentX += GetPirateAdvance(c, g) + tracking;
                }
            }
        }

        // --- DangleFont API ---

        public static float MeasureDangleText(string text, int tracking = 0)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            text = text.ToLowerInvariant();
            float totalWidth = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (s_DangleGlyphs.TryGetValue(c, out Glyph g))
                {
                    float charW = (c == 'm') ? 8f : g.Width;
                    totalWidth += charW;
                    if (i < text.Length - 1)
                        totalWidth += tracking;
                }
            }
            return Mathf.Max(0f, totalWidth);
        }

        public static void DrawDangleText(Rect container, string text, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, int tracking = 0, int lineSpacing = 13)
        {
            if (string.IsNullOrEmpty(text))
                return;

            EnsureTextures();
            if (s_DangleFontTexture == null)
                return;

            Color oldColor = GUI.color;
            GUI.color = color;

            string normalized = text.Replace("||", "\n\n").Replace("|", "\n");
            string[] lines = normalized.Split('\n');
            float totalBlockHeight = (lines.Length - 1) * lineSpacing + 11f;

            float startY = container.y;
            if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleRight)
                startY = container.y + (container.height - totalBlockHeight) * 0.5f;
            else if (anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerRight)
                startY = container.y + container.height - totalBlockHeight;

            for (int l = 0; l < lines.Length; l++)
            {
                string line = lines[l].ToLowerInvariant();
                float lineWidth = MeasureDangleText(line, tracking);
                float lineY = startY + l * lineSpacing;

                float startX = container.x;
                if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.LowerCenter)
                    startX = container.x + (container.width - lineWidth) * 0.5f;
                else if (anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight || anchor == TextAnchor.LowerRight)
                    startX = container.x + container.width - lineWidth;

                float curX = Mathf.Round(startX);
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (s_DangleGlyphs.TryGetValue(c, out Glyph g))
                    {
                        Rect screenRect = new Rect(curX, Mathf.Round(lineY), g.Width, g.Height);
                        Rect texCoords = new Rect(
                            (float)g.X / s_DangleFontTexture.width,
                            1f - (float)(g.Y + g.Height) / s_DangleFontTexture.height,
                            (float)g.Width / s_DangleFontTexture.width,
                            (float)g.Height / s_DangleFontTexture.height
                        );

                        GUI.DrawTextureWithTexCoords(screenRect, s_DangleFontTexture, texCoords, true);
                        float adv = (c == 'm') ? 8f : g.Width;
                        curX += adv + tracking;
                    }
                }
            }

            GUI.color = oldColor;
        }
    }
}
