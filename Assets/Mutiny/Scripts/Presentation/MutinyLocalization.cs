using System;
using System.Collections;
using System.Collections.Generic;
using Mutiny.Persistence;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Mutiny.Presentation
{
    /// <summary>Cached Unity Localization tables for the IMGUI presentation layer.</summary>
    public static class MutinyLocalization
    {
        public const string English = "en";
        public const string SimplifiedChinese = "zh-Hans";
        private const string TableName = "Mutiny";

        private static readonly Dictionary<string, StringTable> Tables = new Dictionary<string, StringTable>();
        private static readonly HashSet<string> MissingKeys = new HashSet<string>();
        private static bool s_Started;
        private static string s_Code = English;

        public static event Action Changed;
        public static string Code => s_Code;
        public static bool IsReady => Tables.ContainsKey(English) && Tables.ContainsKey(SimplifiedChinese);
        public static bool UseOriginalFont => s_Code == English || !Tables.ContainsKey(s_Code);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Tables.Clear();
            MissingKeys.Clear();
            s_Started = false;
            s_Code = English;
            Changed = null;
        }

        public static void Initialize(MonoBehaviour host)
        {
            if (s_Started || host == null)
                return;
            s_Started = true;
            string saved = MutinySaveSystem.LanguageCode;
            s_Code = IsSupported(saved) ? saved :
                Application.systemLanguage == SystemLanguage.ChineseSimplified ? SimplifiedChinese : English;
            host.StartCoroutine(LoadTables());
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
            ApplySelectedLocale();
            Changed?.Invoke();
        }

        private static bool IsSupported(string code) => code == English || code == SimplifiedChinese;

        private static IEnumerator LoadTables()
        {
            var initialization = LocalizationSettings.InitializationOperation;
            yield return initialization;
            if (initialization.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[Localization] Unity Localization initialization failed.");
                yield break;
            }

            foreach (string code in new[] { English, SimplifiedChinese })
            {
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
                if (locale == null)
                {
                    Debug.LogError($"[Localization] Missing Locale {code}.");
                    continue;
                }
                var table = LocalizationSettings.StringDatabase.GetTableAsync(TableName, locale);
                yield return table;
                if (table.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && table.Result != null)
                    Tables[code] = table.Result;
                else
                    Debug.LogError($"[Localization] Missing String Table {TableName}/{code}.");
            }
            ApplySelectedLocale();
            Changed?.Invoke();
        }

        private static void ApplySelectedLocale()
        {
            if (!Tables.ContainsKey(s_Code))
                return;
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(s_Code);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        public static string Text(string key, string englishFallback, params object[] arguments)
        {
            if (string.IsNullOrEmpty(key))
                return Format(englishFallback, arguments);
            if (Tables.Count == 0)
                return Format(englishFallback ?? key, arguments);
            if (Tables.TryGetValue(s_Code, out StringTable table))
            {
                StringTableEntry entry = table.GetEntry(key);
                if (entry != null && !string.IsNullOrEmpty(entry.LocalizedValue))
                    return entry.GetLocalizedString(arguments);
            }
            if (Tables.TryGetValue(English, out StringTable english))
            {
                StringTableEntry entry = english.GetEntry(key);
                if (entry != null && !string.IsNullOrEmpty(entry.LocalizedValue))
                {
                    ReportMissing(key);
                    return entry.GetLocalizedString(arguments);
                }
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
