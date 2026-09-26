using System;
using System.Collections;
using System.Collections.Generic;
using Mutiny.Persistence;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Mutiny.Presentation
{
    /// <summary>Cached Unity Localization tables for the IMGUI presentation layer.</summary>
    public static class MutinyLocalization
    {
        public const string English = "en";
        public const string SimplifiedChinese = "zh-Hans";
        private const string TableName = "Mutiny";
        private const string SharedDataAddress = "Assets/Mutiny/Localization/Tables/Mutiny Shared Data.asset";

        private static readonly Dictionary<string, StringTable> Tables = new Dictionary<string, StringTable>();
        private static readonly HashSet<string> MissingKeys = new HashSet<string>();
        private static bool s_Started;
        private static string s_Code = English;
        private static AsyncOperationHandle<SharedTableData> s_SharedDataHandle;

        public static event Action Changed;
        public static string Code => s_Code;
        public static bool IsReady => HasUsableTable(English) && HasUsableTable(SimplifiedChinese);
        public static bool UseOriginalFont => s_Code == English || !HasUsableTable(s_Code);

        private static bool HasUsableTable(string code) =>
            Tables.TryGetValue(code, out StringTable table) && table != null && table.SharedData != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Tables.Clear();
            MissingKeys.Clear();
            s_Started = false;
            s_Code = English;
            s_SharedDataHandle = default;
            Changed = null;
        }

        public static void Initialize(MonoBehaviour host)
        {
            if (s_Started || host == null)
                return;
            s_Started = true;
            string saved = MutinySaveSystem.LanguageCode;
            s_Code = IsSupported(saved) ? saved : English;
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

            // StringTable bundles in standalone builds can load without their
            // cross-bundle SharedTableData reference. Pin the shared asset before
            // loading either table and repair that reference when necessary.
            s_SharedDataHandle = Addressables.LoadAssetAsync<SharedTableData>(SharedDataAddress);
            yield return s_SharedDataHandle;
            SharedTableData sharedData = s_SharedDataHandle.Status == AsyncOperationStatus.Succeeded
                ? s_SharedDataHandle.Result : null;
            if (sharedData == null)
                Debug.LogError("[Localization] Shared table data did not load; English call-site text will be used.");

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
                if (table.Status == AsyncOperationStatus.Succeeded && table.Result != null)
                {
                    StringTable stringTable = table.Result;
                    if (stringTable.SharedData == null && sharedData != null)
                        stringTable.SharedData = sharedData;
                    if (stringTable.SharedData != null)
                        Tables[code] = stringTable;
                    else
                        Debug.LogError($"[Localization] String Table {TableName}/{code} has no shared key data.");
                }
                else
                    Debug.LogError($"[Localization] Missing String Table {TableName}/{code}.");
            }
            ApplySelectedLocale();
            Changed?.Invoke();
        }

        private static void ApplySelectedLocale()
        {
            if (!HasUsableTable(s_Code))
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
            if (HasUsableTable(s_Code) && Tables.TryGetValue(s_Code, out StringTable table))
            {
                StringTableEntry entry = table.GetEntry(key);
                if (entry != null && !string.IsNullOrEmpty(entry.LocalizedValue))
                    return entry.GetLocalizedString(arguments);
            }
            if (HasUsableTable(English) && Tables.TryGetValue(English, out StringTable english))
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
