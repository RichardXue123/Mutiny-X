using System;
using System.Collections.Generic;
using Mutiny.Persistence;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>Bundled translations used by the IMGUI presentation layer.</summary>
    public static class MutinyLocalization
    {
        public const string English = "en";
        public const string SimplifiedChinese = "zh-Hans";
        public const string TraditionalChineseHongKong = "zh-HK";
        // The order matches the language columns in Mutiny.tsv.
        public static IReadOnlyList<string> SupportedCodes { get; } =
            Array.AsReadOnly(new[] { English, SimplifiedChinese, TraditionalChineseHongKong });
        private const string RuntimeTextResource = "Localization/MutinyRuntime";

        private static readonly Dictionary<string, Dictionary<string, string>> RuntimeText =
            new Dictionary<string, Dictionary<string, string>>();
        private static readonly HashSet<string> MissingKeys = new HashSet<string>();
        private static bool s_Started;
        private static bool s_RuntimeTextLoaded;
        private static string s_Code = English;

        public static event Action Changed;
        public static string Code => s_Code;
        public static bool IsReady
        {
            get
            {
                foreach (string code in SupportedCodes)
                    if (!HasRuntimeText(code)) return false;
                return true;
            }
        }
        public static bool UseOriginalFont => s_Code == English ||
            !HasRuntimeText(s_Code);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            RuntimeText.Clear();
            MissingKeys.Clear();
            s_Started = false;
            s_RuntimeTextLoaded = false;
            s_Code = English;
            Changed = null;
        }

        public static void Initialize(MonoBehaviour host)
        {
            if (s_Started || host == null)
                return;
            s_Started = true;
            EnsureRuntimeText();
            string saved = MutinySaveSystem.LanguageCode;
            s_Code = IsSupported(saved) ? saved : English;
        }

        public static void Select(string code)
        {
            if (!IsSupported(code))
                return;
            if (code == s_Code)
            {
                MutinySaveSystem.LanguageCode = code;
                return;
            }
            s_Code = code;
            MutinySaveSystem.LanguageCode = code;
            Changed?.Invoke();
        }

        private static bool IsSupported(string code)
        {
            foreach (string supported in SupportedCodes)
                if (code == supported) return true;
            return false;
        }

        private static bool HasRuntimeText(string code)
        {
            EnsureRuntimeText();
            return RuntimeText.TryGetValue(code, out Dictionary<string, string> entries) &&
                   entries.ContainsKey("frontend.play");
        }

        private static bool TryRuntimeText(string code, string key, out string value)
        {
            EnsureRuntimeText();
            if (RuntimeText.TryGetValue(code, out Dictionary<string, string> entries) &&
                entries.TryGetValue(key, out value) && !string.IsNullOrEmpty(value))
                return true;
            value = null;
            return false;
        }

        private static void EnsureRuntimeText()
        {
            if (s_RuntimeTextLoaded)
                return;
            s_RuntimeTextLoaded = true;
            TextAsset asset = Resources.Load<TextAsset>(RuntimeTextResource);
            if (asset == null)
            {
                Debug.LogError("[Localization] Bundled runtime text is missing.");
                return;
            }

            try
            {
                foreach (var locale in ParseTranslations(asset.text))
                    RuntimeText.Add(locale.Key, locale.Value);
            }
            catch (FormatException exception)
            {
                Debug.LogError("[Localization] Invalid bundled text; using English fallback. " + exception.Message);
            }
        }

        /// <summary>Shared by runtime loading and editor table generation.</summary>
        public static Dictionary<string, Dictionary<string, string>> ParseTranslations(string text)
        {
            var translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (string code in SupportedCodes)
                translations.Add(code, new Dictionary<string, string>(StringComparer.Ordinal));
            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r').TrimStart('\uFEFF');
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
                string[] fields = line.Split('\t');
                if (fields.Length != SupportedCodes.Count + 1 || string.IsNullOrWhiteSpace(fields[0]) ||
                    translations[English].ContainsKey(fields[0]))
                    throw new FormatException("Invalid or duplicate translation row: " + line);
                for (int index = 0; index < SupportedCodes.Count; index++)
                {
                    if (string.IsNullOrWhiteSpace(fields[index + 1]))
                        throw new FormatException("Empty translation: " + fields[0] + "/" + SupportedCodes[index]);
                    translations[SupportedCodes[index]].Add(fields[0], fields[index + 1].Replace("\\n", "\n"));
                }
            }
            return translations;
        }

        public static bool TryGetEnglishText(string key, out string value)
        {
            EnsureRuntimeText();
            value = null;
            return key != null && RuntimeText.TryGetValue(English, out var entries) &&
                entries.TryGetValue(key, out value);
        }

        public static string Text(string key, string englishFallback, params object[] arguments)
        {
            if (string.IsNullOrEmpty(key))
                return Format(englishFallback, arguments);
            if (TryRuntimeText(s_Code, key, out string runtimeValue))
                return Format(runtimeValue, arguments);
            if (TryRuntimeText(English, key, out string runtimeEnglish))
            {
                if (s_Code != English)
                    ReportMissing(key);
                return Format(runtimeEnglish, arguments);
            }
            ReportMissing(key);
            return Format(englishFallback ?? key, arguments);
        }

        private static string Format(string value, object[] arguments)
        {
            if (arguments == null || arguments.Length == 0 || value == null)
                return value;
            return string.Format(value, arguments);
        }

        private static void ReportMissing(string key)
        {
            string id = s_Code + ":" + key;
            if (MissingKeys.Add(id))
                Debug.LogWarning($"[Localization] Missing translation: {id}");
        }
    }
}
