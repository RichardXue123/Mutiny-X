using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Mutiny.Levels
{
    public static class MutinyLevelXmlParser
    {
        public static MutinyLevelData Parse(string xml, string sourceName = null)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            XDocument document;
            try
            {
                document = XDocument.Parse(xml, LoadOptions.SetLineInfo);
            }
            catch (XmlException exception)
            {
                throw new FormatException(
                    $"{sourceName ?? "Level XML"}: invalid XML at line {exception.LineNumber}, " +
                    $"column {exception.LinePosition}: {exception.Message}", exception);
            }

            XElement root = document.Root;
            if (root == null || root.Name != "level")
                throw Error(root, sourceName, "Expected <level> root without a namespace.");

            var level = new MutinyLevelData
            {
                Width = GetRequiredInt(root, "width", sourceName),
                Height = GetRequiredInt(root, "height", sourceName),
                Players = GetRequiredInt(root, "players", sourceName),
                Name = (string)root.Attribute("name") ?? string.Empty,
                GravityScale = GetGravityScale(root, sourceName),
                VisualTheme = ((string)root.Attribute("visualTheme") ?? string.Empty).Trim(),
                SpaceThemeMinX = root.Attribute("spaceThemeMinX") == null
                    ? 0 : GetRequiredInt(root, "spaceThemeMinX", sourceName)
            };

            if (level.Width <= 0 || level.Height <= 0 || level.Players <= 0)
                throw Error(root, sourceName, "width, height and players must be positive integers.");
            if (level.SpaceThemeMinX < 0 || level.SpaceThemeMinX >= level.Width)
                throw Error(root, sourceName, "spaceThemeMinX must be within the map width.");

            XElement[] rows = root.Elements("row").ToArray();
            XElement[] backgroundRows = root.Elements("bgRow").ToArray();
            ValidateRowCount(root, rows.Length, level.Height, "Terrain", sourceName);
            ValidateRowCount(root, backgroundRows.Length, level.Height, "Background", sourceName);

            level.Terrain = DecodeLayer(rows, level.Width, level.Height, sourceName);
            level.Background = DecodeLayer(backgroundRows, level.Width, level.Height, sourceName);

            foreach (XElement element in root.Elements("obj"))
            {
                string type = (string)element.Attribute("type");
                if (string.IsNullOrWhiteSpace(type))
                    throw Error(element, sourceName, "obj missing non-empty 'type'.");

                var obj = new MutinyLevelObject
                {
                    Type = type,
                    X = GetRequiredInt(element, "x", sourceName),
                    Y = GetRequiredInt(element, "y", sourceName)
                };

                if (obj.X < 0 || obj.X >= level.Width || obj.Y < 0 || obj.Y >= level.Height)
                    throw Error(element, sourceName,
                        $"Object {obj} is outside level bounds {level.Width} x {level.Height}.");

                foreach (XAttribute attribute in element.Attributes())
                    obj.Properties.Add(attribute.Name.ToString(), attribute.Value);

                level.Objects.Add(obj);
            }

            foreach (XElement element in root.Elements("speechAudio"))
            {
                int line = GetRequiredInt(element, "line", sourceName);
                if (line < 0 || line > 3 || level.SpeechAudio.Any(audio => audio.Line == line))
                    throw Error(element, sourceName, "speechAudio line must be unique and between 0 and 3.");
                string[] clips = ((string)element.Attribute("clips") ?? string.Empty)
                    .Split(',').Select(clip => clip.Trim()).ToArray();
                if (clips.Any(string.IsNullOrEmpty))
                    throw Error(element, sourceName, "speechAudio clips must be a non-empty comma-separated list.");
                float gap = .5f;
                if (element.Attribute("gapSeconds") != null &&
                    (!float.TryParse((string)element.Attribute("gapSeconds"), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out gap) || float.IsNaN(gap) || float.IsInfinity(gap) || gap < 0))
                    throw Error(element, sourceName, "speechAudio gapSeconds must be finite and non-negative.");
                level.SpeechAudio.Add(new MutinySpeechAudio { Line = line, Clips = clips, GapSeconds = gap });
            }
            return level;
        }

        public static string[] DecodeRow(string encoded, int expectedWidth)
        {
            if (encoded == null)
                throw new ArgumentNullException(nameof(encoded));
            if (expectedWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(expectedWidth));

            var result = new string[expectedWidth];
            int x = 0;
            string[] tokens = encoded.Split(',');
            for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {
                string token = tokens[tokenIndex].Trim();
                int colon = token.IndexOf(':');
                string name = colon < 0 ? token : token.Substring(0, colon).Trim();
                int count = 1;

                if (name.Length == 0)
                    throw new FormatException($"RLE token {tokenIndex + 1} has an empty tile name.");

                if (colon >= 0 &&
                    (!int.TryParse(token.Substring(colon + 1), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out count) || count <= 0))
                    throw new FormatException($"Invalid RLE token {tokenIndex + 1}: '{token}'. " +
                        "Repeat count must be a positive integer.");

                // Check before expanding, avoiding overflow and unbounded allocations.
                if (count > expectedWidth - x)
                    throw new FormatException($"Decoded row exceeds expected width {expectedWidth} " +
                        $"at token {tokenIndex + 1}: '{token}'.");

                string value = name == "-" ? null : name;
                for (int i = 0; i < count; i++)
                    result[x++] = value;
            }

            if (x != expectedWidth)
                throw new FormatException($"Decoded row width mismatch: expected {expectedWidth}, got {x}.");

            return result;
        }

        private static string[,] DecodeLayer(XElement[] rows, int width, int height, string sourceName)
        {
            var layer = new string[height, width];
            for (int y = 0; y < height; y++)
            {
                if (rows[y].HasElements)
                    throw Error(rows[y], sourceName, $"<{rows[y].Name}> at y={y} must contain only RLE text.");

                string[] decoded;
                try
                {
                    decoded = DecodeRow(rows[y].Value, width);
                }
                catch (FormatException exception)
                {
                    throw Error(rows[y], sourceName,
                        $"<{rows[y].Name}> at y={y}: {exception.Message}", exception);
                }

                for (int x = 0; x < width; x++)
                    layer[y, x] = decoded[x];
            }
            return layer;
        }

        private static void ValidateRowCount(XElement root, int actual, int expected,
            string layerName, string sourceName)
        {
            if (actual != expected)
                throw Error(root, sourceName,
                    $"{layerName} row count mismatch: expected {expected}, got {actual}.");
        }

        private static float GetGravityScale(XElement root, string sourceName)
        {
            XAttribute attribute = root.Attribute("gravityScale");
            if (attribute == null) return 1f;
            if (!float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
                float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw Error(attribute, sourceName, "gravityScale must be a finite positive number.");
            return value;
        }

        private static int GetRequiredInt(XElement element, string name, string sourceName)
        {
            XAttribute attribute = element.Attribute(name);
            if (attribute == null)
                throw Error(element, sourceName, $"<{element.Name}> missing '{name}'.");
            if (!int.TryParse(attribute.Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int value))
                throw Error(attribute, sourceName, $"Invalid integer '{name}': '{attribute.Value}'.");
            return value;
        }

        private static FormatException Error(XObject location, string sourceName, string message,
            Exception innerException = null)
        {
            string prefix = sourceName ?? "Level XML";
            if (location is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
                prefix += $" (line {lineInfo.LineNumber}, column {lineInfo.LinePosition})";
            return new FormatException($"{prefix}: {message}", innerException);
        }
    }
}
