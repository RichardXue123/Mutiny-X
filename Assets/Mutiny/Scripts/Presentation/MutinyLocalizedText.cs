using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>Retains the original English atlases and renders CJK with a bundled dynamic font.</summary>
    public static class MutinyLocalizedText
    {
        private static Font s_CjkFont;
        private static readonly HashSet<string> MissingGlyphs = new HashSet<string>();

        private static Font CjkFont => s_CjkFont != null ? s_CjkFont :
            s_CjkFont = Resources.Load<Font>("Localization/Fonts/NotoSansCJKsc-Regular");

        public static void Pirate(Rect rect, string key, string english, bool hovered = false, bool centered = true,
            int tracking = -3)
        {
            string value = MutinyLocalization.Text(key, english);
            if (MutinyLocalization.UseOriginalFont)
            {
                MutinyBitmapFont.DrawPirateText(rect, value, hovered, centered, tracking);
                return;
            }
            DrawCjk(rect, value, hovered ? Color.yellow : Color.white,
                centered ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft, 18, true);
        }

        public static void Dangle(Rect rect, string key, string english, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, int tracking = 0, int lineSpacing = 13)
        {
            string value = MutinyLocalization.Text(key, english);
            if (MutinyLocalization.UseOriginalFont)
            {
                MutinyBitmapFont.DrawDangleText(rect, value, color, anchor, tracking, lineSpacing);
                return;
            }
            DrawCjk(rect, value, color, anchor, Mathf.Max(12, lineSpacing - 1), false);
        }

        public static void Speech(Rect rect, string key, string english)
        {
            string value = MutinyLocalization.Text(key, english);
            if (MutinyLocalization.UseOriginalFont)
            {
                MutinyBitmapFont.DrawSpeechText(rect, value, TextAnchor.UpperLeft, 0, 13);
                return;
            }
            DrawCjk(rect, value, new Color32(102, 102, 102, 255), TextAnchor.UpperLeft, 13, false);
        }

        public static void LanguageChoice(Rect rect, bool chinese, bool selected)
        {
            Color color = selected ? Color.yellow : Color.white;
            if (chinese)
                DrawCjk(rect, "简体中文", color, TextAnchor.MiddleCenter, 14, false);
            else
                MutinyBitmapFont.DrawDangleText(rect, "english", color, TextAnchor.MiddleCenter);
        }

        private static void DrawCjk(Rect rect, string text, Color color, TextAnchor anchor, int fontSize, bool bold)
        {
            Font font = CjkFont;
            if (font == null)
            {
                Debug.LogError("[Localization] Bundled CJK font is missing.");
                return;
            }
            string normalized = (text ?? string.Empty).Replace("||", "\n\n").Replace('|', '\n');
            foreach (char character in normalized)
            {
                if (char.IsWhiteSpace(character) || font.HasCharacter(character))
                    continue;
                string id = MutinyLocalization.Code + ":U+" + ((int)character).ToString("X4");
                if (MissingGlyphs.Add(id))
                    Debug.LogWarning("[Localization] Missing glyph " + id);
            }
            var style = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = anchor,
                wordWrap = true,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0)
            };
            style.normal.textColor = color;
            GUI.Label(rect, normalized, style);
        }
    }
}
