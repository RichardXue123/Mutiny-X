using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Presentation
{
    public enum MutinyMusicState
    {
        None,
        Menu,
        Game
    }

    [DisallowMultipleComponent]
    public sealed class MutinyAudioManager : MonoBehaviour
    {
        public const string MenuMusicName = "menu_music";
        public const string GameMusicName = "game_music";
        private static MutinyAudioManager s_Instance;

        public static MutinyAudioManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindAnyObjectByType<MutinyAudioManager>();
                    if (s_Instance == null)
                    {
                        var go = new GameObject("MutinyAudioManager");
                        s_Instance = go.AddComponent<MutinyAudioManager>();
                    }
                }
                return s_Instance;
            }
        }

        [Header("Audio Sources")]
        public AudioSource SfxSource;
        public AudioSource MusicSource;

        [Header("Settings")]
        public bool SfxEnabled = true;
        public bool MusicEnabled = true;
        public float SfxVolume = 1f;
        public float MusicVolume = 0.8f;

        private Dictionary<string, AudioClip> m_SfxClips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, AudioClip> m_MusicClips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private MutinyMusicState m_MusicState = MutinyMusicState.None;

        /// <summary>
        /// Flash MusicController.music_type. This is the requested page/game state,
        /// not merely the clip currently audible: it keeps changing while music is muted.
        /// </summary>
        public MutinyMusicState CurrentMusicState => m_MusicState;

        // Production diagnostics and parity tests observe actual resolved SFX here.
        // The event is raised only when a loaded clip is about to be played.
        public event Action<string> SfxPlayed;

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved settings
            SfxEnabled = Mutiny.Persistence.MutinySaveSystem.SfxEnabled;
            MusicEnabled = Mutiny.Persistence.MutinySaveSystem.MusicEnabled;
            SfxVolume = Mutiny.Persistence.MutinySaveSystem.SfxVolume;
            MusicVolume = Mutiny.Persistence.MutinySaveSystem.MusicVolume;

            if (SfxSource == null)
            {
                SfxSource = gameObject.AddComponent<AudioSource>();
                SfxSource.playOnAwake = false;
            }

            if (MusicSource == null)
            {
                MusicSource = gameObject.AddComponent<AudioSource>();
                MusicSource.loop = true;
                MusicSource.playOnAwake = false;
            }

            LoadAllAudioClips();
        }

        private void LoadAllAudioClips()
        {
            m_SfxClips.Clear();
            AudioClip[] sfxClips = Resources.LoadAll<AudioClip>("Audio/SFX");
            for (int i = 0; i < sfxClips.Length; i++)
            {
                AudioClip clip = sfxClips[i];
                if (clip != null)
                    m_SfxClips[clip.name] = clip;
            }

            m_MusicClips.Clear();
            AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
            for (int i = 0; i < musicClips.Length; i++)
            {
                AudioClip clip = musicClips[i];
                if (clip != null)
                    m_MusicClips[clip.name] = clip;
            }
        }

        public void PlaySFX(string soundName, float volumeScale = 1f)
        {
            if (!SfxEnabled || SfxSource == null || string.IsNullOrEmpty(soundName))
                return;

            if (m_SfxClips.TryGetValue(soundName, out AudioClip clip))
            {
                SfxPlayed?.Invoke(soundName);
                SfxSource.PlayOneShot(clip, SfxVolume * volumeScale);
            }
        }

        /// <summary>
        /// Compatibility entry point. The original game has only two looping BGM
        /// states, menu_music and game_music; use the explicit Start* methods for them.
        /// </summary>
        public void PlayMusic(string musicName, bool loop = true)
        {
            if (string.Equals(musicName, MenuMusicName, StringComparison.OrdinalIgnoreCase))
            {
                StartMenuMusic();
                return;
            }

            if (string.Equals(musicName, GameMusicName, StringComparison.OrdinalIgnoreCase))
            {
                StartGameMusic();
                return;
            }

            if (!MusicEnabled || MusicSource == null || string.IsNullOrEmpty(musicName))
                return;

            PlayResolvedMusic(musicName, loop);
        }

        /// <summary>
        /// Mirrors MusicController.startMenuMusic(from_toggle). Re-entering another
        /// menu frame does not restart the track; turning music back on does.
        /// </summary>
        public void StartMenuMusic(bool fromToggle = false)
        {
            SetMusicState(MutinyMusicState.Menu, fromToggle);
        }

        /// <summary>
        /// Mirrors MusicController.startGameMusic(from_toggle). Loading/restarting
        /// another level while already in game state leaves the looping track alone.
        /// </summary>
        public void StartGameMusic(bool fromToggle = false)
        {
            SetMusicState(MutinyMusicState.Game, fromToggle);
        }

        private void SetMusicState(MutinyMusicState state, bool fromToggle)
        {
            bool changed = m_MusicState != state;
            m_MusicState = state;

            // Original startMenuMusic/startGameMusic always update music_type even
            // while music_on is false. This is what makes a later toggle resume the
            // correct menu/game track.
            if (!changed && !fromToggle)
                return;

            if (!MusicEnabled || MusicSource == null)
                return;

            string musicName = state == MutinyMusicState.Menu
                ? MenuMusicName
                : state == MutinyMusicState.Game
                    ? GameMusicName
                    : null;

            if (!string.IsNullOrEmpty(musicName))
                PlayResolvedMusic(musicName, loop: true);
        }

        private void PlayResolvedMusic(string musicName, bool loop)
        {
            if (MusicSource == null || string.IsNullOrEmpty(musicName))
                return;

            if (!m_MusicClips.TryGetValue(musicName, out AudioClip clip))
            {
                Debug.LogWarning($"[MutinyAudio] Music clip not found: {musicName}", this);
                return;
            }

            MusicSource.Stop();
            MusicSource.clip = clip;
            MusicSource.loop = loop;
            MusicSource.volume = MusicVolume;
            MusicSource.Play();
        }

        private void RestartCurrentMusic()
        {
            if (m_MusicState == MutinyMusicState.Menu)
                StartMenuMusic(fromToggle: true);
            else if (m_MusicState == MutinyMusicState.Game)
                StartGameMusic(fromToggle: true);
        }

        /// <summary>
        /// Stops the audible source without forgetting the original music_type.
        /// clearState is only for callers that intentionally leave both original states.
        /// </summary>
        public void StopMusic(bool clearState = false)
        {
            if (MusicSource != null)
                MusicSource.Stop();

            if (clearState)
                m_MusicState = MutinyMusicState.None;
        }

        public void PlayCharacterVoice(string characterType)
        {
            if (string.IsNullOrEmpty(characterType)) return;
            string voiceName = characterType
                .Replace("Captain", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Chief", "", StringComparison.OrdinalIgnoreCase);
            PlaySFX(voiceName);
        }

        public void ToggleSFX()
        {
            SfxEnabled = !SfxEnabled;
            Mutiny.Persistence.MutinySaveSystem.SfxEnabled = SfxEnabled;
            Debug.Log($"[MutinyAudio] HUD-CORNER-06 SFX={(SfxEnabled ? "on" : "off")}", this);
        }

        public void ToggleMusic()
        {
            if (MusicEnabled)
                TurnOffMusic();
            else
                TurnOnMusic();
        }

        public void TurnOffMusic()
        {
            MusicEnabled = false;
            Mutiny.Persistence.MutinySaveSystem.MusicEnabled = false;
            StopMusic();
            Debug.Log($"[MutinyAudio] HUD-CORNER-06 music=off state={m_MusicState}", this);
        }

        public void TurnOnMusic()
        {
            MusicEnabled = true;
            Mutiny.Persistence.MutinySaveSystem.MusicEnabled = true;

            // Original turnOnMusic restarts the current music_type from its beginning.
            RestartCurrentMusic();
            Debug.Log($"[MutinyAudio] HUD-CORNER-06 music=on state={m_MusicState} clip={(MusicSource != null && MusicSource.clip != null ? MusicSource.clip.name : "none")}", this);
        }

        public void SetSfxVolume(float vol)
        {
            SfxVolume = Mathf.Clamp01(vol);
            Mutiny.Persistence.MutinySaveSystem.SfxVolume = SfxVolume;
        }

        public void SetMusicVolume(float vol)
        {
            MusicVolume = Mathf.Clamp01(vol);
            Mutiny.Persistence.MutinySaveSystem.MusicVolume = MusicVolume;
            if (MusicSource != null)
            {
                MusicSource.volume = MusicVolume;
            }
        }
    }
}
