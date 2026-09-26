using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mutiny.Persistence;
using Mutiny.Verification;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Mutiny.Presentation.Editor
{
    [InitializeOnLoad]
    public static class MutinyLocalizationVerification
    {
        private const string SourcePath = "Assets/Mutiny/Localization/Mutiny.tsv";
        private const string OriginalWeaponPath = "Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/game/WeaponSelectButton.as";
        private const string FontPath = "Assets/Mutiny/Resources/Localization/Fonts/NotoSansCJKsc-Regular.otf";
        private const string PlayModeKey = "Mutiny.LocalizationPlayModeVerification";
        private const string LanguagePrefKey = "mutiny_language_v1";
        private static double s_PlayModeStart;

        static MutinyLocalizationVerification()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (SessionState.GetBool(PlayModeKey, false))
                ArmRunningGameCheck();
        }

        [MenuItem("Mutiny/Localization/Validate Play Mode")]
        public static void ValidatePlayMode()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Start the localization verification outside Play Mode.");
            EditorApplication.delayCall += BeginPlayMode;
        }

        private static void BeginPlayMode()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            SessionState.SetBool(PlayModeKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(PlayModeKey, false))
                return;
            Debug.Log("[Localization] Play Mode state: " + state);
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;
            ArmRunningGameCheck();
        }

        private static void ArmRunningGameCheck()
        {
            s_PlayModeStart = EditorApplication.timeSinceStartup;
            EditorApplication.update -= VerifyRunningGame;
            EditorApplication.update += VerifyRunningGame;
        }

        private static void VerifyRunningGame()
        {
            if (!EditorApplication.isPlaying)
                return;
            MutinyGMManager gm = MutinyGMManager.Instance ?? UnityEngine.Object.FindAnyObjectByType<MutinyGMManager>();
            MutinyLocalization.Initialize(gm);
            if (!MutinyLocalization.IsReady && EditorApplication.timeSinceStartup - s_PlayModeStart < 20d)
                return;
            EditorApplication.update -= VerifyRunningGame;
            SessionState.EraseBool(PlayModeKey);
            bool passed = false;
            bool hadLanguage = PlayerPrefs.HasKey(LanguagePrefKey);
            string previousLanguage = PlayerPrefs.GetString(LanguagePrefKey, string.Empty);
            string previousCode = MutinyLocalization.Code;
            try
            {
                if (!MutinyLocalization.IsReady ||
                    UnityEngine.Object.FindAnyObjectByType<MutinyFrontendController>() == null ||
                    Resources.Load<Font>("Localization/Fonts/NotoSansCJKsc-Regular") == null)
                    throw new InvalidOperationException("Production frontend did not load localization tables and font.");
                MutinyLevel1VerificationResult result = MutinyTurnActionUiVerificationTest.RunGMLanguage(gm);
                if (!result.Passed)
                    throw new InvalidOperationException(string.Join("\n", result.Failures));
                foreach (string line in result.Logs)
                    Debug.Log(line);
                passed = true;
                Debug.Log($"[Localization] GM-09 Play Mode verification passed: {result.PassedAssertions}/{result.TotalAssertions}; production frontend loaded tables and font.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                MutinyLocalization.Select(previousCode);
                if (hadLanguage)
                    PlayerPrefs.SetString(LanguagePrefKey, previousLanguage);
                else
                    PlayerPrefs.DeleteKey(LanguagePrefKey);
                PlayerPrefs.Save();
                if (Application.isBatchMode)
                    EditorApplication.Exit(passed ? 0 : 1);
                else
                    EditorApplication.ExitPlaymode();
            }
        }

        [MenuItem("Mutiny/Localization/Validate Initial Tables")]
        public static void Validate()
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
                throw new InvalidOperationException("Active Localization Settings are missing from the build.");
            var collection = LocalizationEditorSettings.GetStringTableCollection("Mutiny");
            if (collection == null)
                throw new InvalidOperationException("Mutiny String Table Collection is missing.");
            var englishLocale = LocalizationEditorSettings.GetLocale("en");
            var chineseLocale = LocalizationEditorSettings.GetLocale("zh-Hans");
            StringTable english = englishLocale == null ? null : collection.GetTable(englishLocale.Identifier) as StringTable;
            StringTable chinese = chineseLocale == null ? null : collection.GetTable(chineseLocale.Identifier) as StringTable;
            if (english == null || chinese == null)
                throw new InvalidOperationException("English or Simplified Chinese table is missing.");

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null || !font.dynamic)
                throw new InvalidOperationException("Bundled CJK dynamic font is missing.");

            var seen = new HashSet<string>(StringComparer.Ordinal);
            int count = 0;
            foreach (string line in File.ReadAllLines(SourcePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                    continue;
                string[] fields = line.Split('\t');
                if (fields.Length != 3 || !seen.Add(fields[0]))
                    throw new InvalidOperationException("Invalid or duplicate localization key: " + line);

                string en = fields[1].Replace("\\n", "\n");
                string zh = fields[2].Replace("\\n", "\n");
                if (string.IsNullOrWhiteSpace(en) || string.IsNullOrWhiteSpace(zh) ||
                    english.GetEntry(fields[0])?.LocalizedValue != en ||
                    chinese.GetEntry(fields[0])?.LocalizedValue != zh)
                    throw new InvalidOperationException("Missing or stale translation: " + fields[0]);

                string enSlots = string.Join(",", ExtractSlots(en));
                string zhSlots = string.Join(",", ExtractSlots(zh));
                if (enSlots != zhSlots)
                    throw new InvalidOperationException("Placeholder mismatch: " + fields[0]);
                foreach (char character in zh)
                {
                    if (!char.IsWhiteSpace(character) && !font.HasCharacter(character))
                        throw new InvalidOperationException($"Missing bundled glyph U+{(int)character:X4}: {fields[0]}");
                }
                count++;
            }

            var originalWeapons = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (Match match in Regex.Matches(File.ReadAllText(OriginalWeaponPath),
                         @"hover_(\w+):\[""([^""]*)"",""([^""]*)""\]"))
                originalWeapons[match.Groups[1].Value] = new[] { match.Groups[2].Value, match.Groups[3].Value.Replace('|', '\n') };
            foreach (string weapon in new[] { "cherryBomb", "dynamite", "boulder", "piecesOfEight", "rumBottle",
                         "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull", "mine",
                         "cannon", "anchor", "voodooDoll", "tidalWave" })
            {
                MutinyGameHUD.GetOriginalActionCopy(weapon, out string title, out string description);
                if (english.GetEntry("weapon." + weapon + ".name")?.LocalizedValue != title ||
                    english.GetEntry("weapon." + weapon + ".desc")?.LocalizedValue != description ||
                    !originalWeapons.TryGetValue(weapon, out string[] original) ||
                    original[0] != title || original[1] != description)
                    throw new InvalidOperationException("Original weapon copy changed in table: " + weapon);
            }
            Debug.Log($"[Localization] Validation passed: {count} paired keys, bundled glyphs and 15 SWF weapon copies.");
        }

        private static IEnumerable<string> ExtractSlots(string value)
        {
            foreach (Match match in Regex.Matches(value, @"\{\d+\}"))
                yield return match.Value;
        }
    }
}
