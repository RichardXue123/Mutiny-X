using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Mutiny.Persistence
{
    public static class MutinySaveSystem
    {
        private const string KeyHighestLevel = "mutiny_highest_unlocked_level";
        private const string KeySfxEnabled = "mutiny_sfx_enabled";
        private const string KeyMusicEnabled = "mutiny_music_enabled";
        private const string KeySfxVolume = "mutiny_sfx_volume";
        private const string KeyMusicVolume = "mutiny_music_volume";
        private const string KeyCompletedScores = "mutiny_completed_scores_v1";

        public const int MaxLevel = 18;
        public const int MaxCompletedScores = 5;

        private static readonly Dictionary<string, object> s_MockPrefs = new Dictionary<string, object>(StringComparer.Ordinal);
        private static readonly MethodInfo s_GetIntMethod;
        private static readonly MethodInfo s_SetIntMethod;
        private static readonly MethodInfo s_GetFloatMethod;
        private static readonly MethodInfo s_SetFloatMethod;
        private static readonly MethodInfo s_GetStringMethod;
        private static readonly MethodInfo s_SetStringMethod;
        private static readonly MethodInfo s_DeleteKeyMethod;
        private static readonly MethodInfo s_SaveMethod;
        private static bool s_ReflectionFailed = false;

        static MutinySaveSystem()
        {
            try
            {
                Type type = typeof(PlayerPrefs);
                s_GetIntMethod = type.GetMethod("GetInt", new[] { typeof(string), typeof(int) });
                s_SetIntMethod = type.GetMethod("SetInt", new[] { typeof(string), typeof(int) });
                s_GetFloatMethod = type.GetMethod("GetFloat", new[] { typeof(string), typeof(float) });
                s_SetFloatMethod = type.GetMethod("SetFloat", new[] { typeof(string), typeof(float) });
                s_GetStringMethod = type.GetMethod("GetString", new[] { typeof(string), typeof(string) });
                s_SetStringMethod = type.GetMethod("SetString", new[] { typeof(string), typeof(string) });
                s_DeleteKeyMethod = type.GetMethod("DeleteKey", new[] { typeof(string) });
                s_SaveMethod = type.GetMethod("Save", Type.EmptyTypes);
            }
            catch
            {
                s_ReflectionFailed = true;
            }
        }

        private static int GetPrefInt(string key, int defaultValue)
        {
            if (!s_ReflectionFailed && s_GetIntMethod != null)
            {
                try
                {
                    return (int)s_GetIntMethod.Invoke(null, new object[] { key, defaultValue });
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            return s_MockPrefs.TryGetValue(key, out var val) ? Convert.ToInt32(val) : defaultValue;
        }

        private static void SetPrefInt(string key, int value)
        {
            if (!s_ReflectionFailed && s_SetIntMethod != null)
            {
                try
                {
                    s_SetIntMethod.Invoke(null, new object[] { key, value });
                    s_SaveMethod?.Invoke(null, null);
                    return;
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            s_MockPrefs[key] = value;
        }

        private static float GetPrefFloat(string key, float defaultValue)
        {
            if (!s_ReflectionFailed && s_GetFloatMethod != null)
            {
                try
                {
                    return (float)s_GetFloatMethod.Invoke(null, new object[] { key, defaultValue });
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            return s_MockPrefs.TryGetValue(key, out var val) ? Convert.ToSingle(val) : defaultValue;
        }

        private static void SetPrefFloat(string key, float value)
        {
            if (!s_ReflectionFailed && s_SetFloatMethod != null)
            {
                try
                {
                    s_SetFloatMethod.Invoke(null, new object[] { key, value });
                    s_SaveMethod?.Invoke(null, null);
                    return;
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            s_MockPrefs[key] = value;
        }

        private static void DeletePrefKey(string key)
        {
            if (!s_ReflectionFailed && s_DeleteKeyMethod != null)
            {
                try
                {
                    s_DeleteKeyMethod.Invoke(null, new object[] { key });
                    s_SaveMethod?.Invoke(null, null);
                    return;
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            s_MockPrefs.Remove(key);
        }

        private static string GetPrefString(string key, string defaultValue)
        {
            if (!s_ReflectionFailed && s_GetStringMethod != null)
            {
                try
                {
                    return (string)s_GetStringMethod.Invoke(null, new object[] { key, defaultValue });
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            return s_MockPrefs.TryGetValue(key, out var value) ? Convert.ToString(value) : defaultValue;
        }

        private static void SetPrefString(string key, string value)
        {
            if (!s_ReflectionFailed && s_SetStringMethod != null)
            {
                try
                {
                    s_SetStringMethod.Invoke(null, new object[] { key, value });
                    s_SaveMethod?.Invoke(null, null);
                    return;
                }
                catch
                {
                    s_ReflectionFailed = true;
                }
            }
            s_MockPrefs[key] = value;
        }

        public readonly struct CompletedScoreEntry
        {
            public readonly int Score;
            public readonly string Timestamp;

            public CompletedScoreEntry(int score, string timestamp)
            {
                Score = score;
                Timestamp = timestamp ?? string.Empty;
            }
        }

        public static CompletedScoreEntry[] GetTopCompletedScoreEntries()
        {
            string saved = GetPrefString(KeyCompletedScores, string.Empty);
            if (string.IsNullOrEmpty(saved))
                return Array.Empty<CompletedScoreEntry>();

            var entries = new List<CompletedScoreEntry>(MaxCompletedScores);
            foreach (string field in saved.Split(','))
            {
                if (string.IsNullOrEmpty(field))
                    continue;

                int atIdx = field.IndexOf('@');
                if (atIdx >= 0)
                {
                    string scorePart = field.Substring(0, atIdx);
                    string timePart = field.Substring(atIdx + 1);
                    if (int.TryParse(scorePart, NumberStyles.None, CultureInfo.InvariantCulture, out int score))
                        entries.Add(new CompletedScoreEntry(score, timePart));
                }
                else
                {
                    if (int.TryParse(field, NumberStyles.None, CultureInfo.InvariantCulture, out int score))
                        entries.Add(new CompletedScoreEntry(score, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }

            entries.Sort((left, right) => right.Score.CompareTo(left.Score));
            if (entries.Count > MaxCompletedScores)
                entries.RemoveRange(MaxCompletedScores, entries.Count - MaxCompletedScores);
            return entries.ToArray();
        }

        public static int[] GetTopCompletedScores()
        {
            CompletedScoreEntry[] entries = GetTopCompletedScoreEntries();
            int[] scores = new int[entries.Length];
            for (int i = 0; i < entries.Length; i++)
                scores[i] = entries[i].Score;
            return scores;
        }

        /// <summary>Records one completed single-player run if it belongs in the local top five.</summary>
        public static bool RecordCompletedScore(int totalScore, string timestamp = null)
        {
            if (totalScore < 0)
                return false;

            if (string.IsNullOrEmpty(timestamp))
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var entries = new List<CompletedScoreEntry>(GetTopCompletedScoreEntries());
            if (entries.Count == MaxCompletedScores && totalScore < entries[entries.Count - 1].Score)
                return false;

            entries.Add(new CompletedScoreEntry(totalScore, timestamp));
            entries.Sort((left, right) => right.Score.CompareTo(left.Score));
            if (entries.Count > MaxCompletedScores)
                entries.RemoveRange(MaxCompletedScores, entries.Count - MaxCompletedScores);

            string[] fields = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                fields[i] = $"{entries[i].Score.ToString(CultureInfo.InvariantCulture)}@{entries[i].Timestamp}";
            SetPrefString(KeyCompletedScores, string.Join(",", fields));
            return true;
        }

        public static int HighestUnlockedLevel
        {
            get => GetPrefInt(KeyHighestLevel, 1);
            set
            {
                int clamped = Mathf.Clamp(value, 1, MaxLevel);
                SetPrefInt(KeyHighestLevel, clamped);
            }
        }

        public static bool SfxEnabled
        {
            get => GetPrefInt(KeySfxEnabled, 1) == 1;
            set => SetPrefInt(KeySfxEnabled, value ? 1 : 0);
        }

        public static bool MusicEnabled
        {
            get => GetPrefInt(KeyMusicEnabled, 1) == 1;
            set => SetPrefInt(KeyMusicEnabled, value ? 1 : 0);
        }

        public static float SfxVolume
        {
            get => GetPrefFloat(KeySfxVolume, 1.0f);
            set => SetPrefFloat(KeySfxVolume, Mathf.Clamp01(value));
        }

        public static float MusicVolume
        {
            get => GetPrefFloat(KeyMusicVolume, 0.8f);
            set => SetPrefFloat(KeyMusicVolume, Mathf.Clamp01(value));
        }

        public static bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex <= 1) return true;
            return levelIndex <= HighestUnlockedLevel;
        }

        public static bool UnlockLevel(int levelIndex)
        {
            if (levelIndex > HighestUnlockedLevel && levelIndex <= MaxLevel)
            {
                HighestUnlockedLevel = levelIndex;
                return true;
            }
            return false;
        }

        public static void ResetProgress()
        {
            DeletePrefKey(KeyHighestLevel);
        }
    }
}
