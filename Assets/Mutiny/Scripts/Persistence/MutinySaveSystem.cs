using System;
using System.Collections.Generic;
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

        public const int MaxLevel = 18;

        private static readonly Dictionary<string, object> s_MockPrefs = new Dictionary<string, object>(StringComparer.Ordinal);
        private static readonly MethodInfo s_GetIntMethod;
        private static readonly MethodInfo s_SetIntMethod;
        private static readonly MethodInfo s_GetFloatMethod;
        private static readonly MethodInfo s_SetFloatMethod;
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

