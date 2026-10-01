using System;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Mutiny.Presentation.Editor
{
    public static class MutinyLocalizationTableBuilder
    {
        private const string DirectoryPath = "Assets/Mutiny/Localization";
        private const string SourcePath = DirectoryPath + "/Mutiny.tsv";
        private const string TablePath = DirectoryPath + "/Tables";
        private const string RuntimeTextPath = "Assets/Mutiny/Resources/Localization/MutinyRuntime.txt";

        [MenuItem("Mutiny/Localization/Rebuild String Tables")]
        public static void Build()
        {
            var translations = MutinyLocalization.ParseTranslations(File.ReadAllText(SourcePath));
            Directory.CreateDirectory(TablePath);
            EnsureSettings();
            var collection = LocalizationEditorSettings.GetStringTableCollection("Mutiny") ??
                LocalizationEditorSettings.CreateStringTableCollection("Mutiny", TablePath);
            foreach (string code in MutinyLocalization.SupportedCodes)
            {
                string displayName = code == MutinyLocalization.English ? "English" :
                    code == MutinyLocalization.SimplifiedChinese ? "简体中文" : "繁體中文（香港）";
                Locale locale = EnsureLocale(code, displayName);
                var table = collection.GetTable(locale.Identifier) as StringTable ??
                    collection.AddNewTable(locale.Identifier) as StringTable;
                if (table == null) throw new InvalidOperationException("Failed to create string table: " + code);
                foreach (var entry in translations[code])
                    Set(table, entry.Key, entry.Value);
                LocalizationEditorSettings.SetPreloadTableFlag(table, true);
                EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(collection.SharedData);
            File.Copy(SourcePath, RuntimeTextPath, true);
            AssetDatabase.ImportAsset(RuntimeTextPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Localization] Built {translations[MutinyLocalization.English].Count} keys in {translations.Count} languages.");
        }

        private static void EnsureSettings()
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null)
                return;
            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            settings.name = "Mutiny Localization Settings";
            AssetDatabase.CreateAsset(settings, DirectoryPath + "/Localization Settings.asset");
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
        }

        private static Locale EnsureLocale(string code, string displayName)
        {
            Locale locale = LocalizationEditorSettings.GetLocale(code);
            if (locale != null)
                return locale;
            locale = Locale.CreateLocale(code);
            locale.LocaleName = displayName;
            AssetDatabase.CreateAsset(locale, DirectoryPath + "/Locale_" + code + ".asset");
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static void Set(StringTable table, string key, string value)
        {
            var entry = table.GetEntry(key) ?? table.AddEntry(key, value);
            entry.Value = value;
        }

    }
}
