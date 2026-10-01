using System;
using System.Collections;
using System.Collections.Generic;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MutinyAudioManager : MonoBehaviour
    {
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

        // Production diagnostics and parity tests observe actual resolved SFX here.
        // The event reports the final clip name after a valid playback submission.
        public event Action<string> SfxPlayed;

        private readonly Queue<(MutinyLevelRoot Level, float Volume)> m_RobotVoiceRequests = new();
        private Coroutine m_RobotVoiceRoutine;
        private Coroutine m_SpeechRoutine;
        private MutinySpeechController m_SpeechOwner;
        private AudioSource m_SpeechSource;
        private AudioSource m_RobotVoiceSource;

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
            m_SpeechSource = gameObject.AddComponent<AudioSource>();
            m_SpeechSource.playOnAwake = false;
            m_RobotVoiceSource = gameObject.AddComponent<AudioSource>();
            m_RobotVoiceSource.playOnAwake = false;
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
            if (!SfxEnabled || SfxSource == null || !SfxSource.isActiveAndEnabled || string.IsNullOrEmpty(soundName))
                return;

            MutinyLevelRoot robotLevel = null;
            if (string.Equals(soundName, "Robot", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(soundName, "RobotCaptain", StringComparison.OrdinalIgnoreCase))
            {
                robotLevel = FindRobotVoiceLevel();
                if (robotLevel != null)
                {
                    if (!m_SfxClips.ContainsKey($"daftpunk_{robotLevel.NextRobotVoiceNumber:D2}")) return;
                    m_RobotVoiceRequests.Enqueue((robotLevel, volumeScale));
                    if (m_RobotVoiceRoutine == null)
                        m_RobotVoiceRoutine = StartCoroutine(PlayRobotVoiceRequests());
                    return;
                }
            }

            if (m_SfxClips.TryGetValue(soundName, out AudioClip clip))
            {
                SfxSource.PlayOneShot(clip, SfxVolume * volumeScale);
                SfxPlayed?.Invoke(soundName);
            }
        }

        private bool CanPlayRobotVoice(MutinyLevelRoot level) =>
            level != null && level == FindRobotVoiceLevel() && SfxEnabled &&
            SfxSource != null && SfxSource.isActiveAndEnabled;

        private IEnumerator PlayRobotVoiceRequests()
        {
            // Yield once so the coroutine handle is assigned even if a request
            // becomes invalid before its first submission.
            yield return null;
            while (m_RobotVoiceRequests.Count > 0)
            {
                while (m_SpeechRoutine != null) yield return null;
                var request = m_RobotVoiceRequests.Dequeue();
                for (int part = 0; part < 2; part++)
                {
                    if (!CanPlayRobotVoice(request.Level)) break;
                    string name = $"daftpunk_{request.Level.NextRobotVoiceNumber:D2}";
                    if (!m_SfxClips.TryGetValue(name, out AudioClip clip) || clip == null) break;
                    m_RobotVoiceSource.pitch = SfxSource.pitch;
                    m_RobotVoiceSource.PlayOneShot(clip, SfxVolume * request.Volume);
                    request.Level.AdvanceRobotVoiceSequence();
                    double duration = clip.length / Math.Max(.01f, Mathf.Abs(SfxSource.pitch));
                    double deadline = Time.realtimeSinceStartupAsDouble + duration + (part == 0 ? .5 : 0);
                    SfxPlayed?.Invoke(name);
                    while (Time.realtimeSinceStartupAsDouble < deadline && CanPlayRobotVoice(request.Level))
                        yield return null;
                }
            }
            m_RobotVoiceRoutine = null;
        }

        public void PlaySpeechAudio(MutinySpeechController owner, MutinySpeechAudio sequence)
        {
            CancelSpeechAudio(m_SpeechOwner);
            // A scripted speech takes priority over pending selection voices.
            if (m_RobotVoiceRoutine != null) StopCoroutine(m_RobotVoiceRoutine);
            m_RobotVoiceRoutine = null;
            m_RobotVoiceRequests.Clear();
            m_RobotVoiceSource?.Stop();
            if (owner == null || sequence == null || !SfxEnabled || m_SpeechSource == null) return;
            m_SpeechOwner = owner;
            m_SpeechRoutine = StartCoroutine(PlaySpeechSequence(owner, sequence));
        }

        public void CancelSpeechAudio(MutinySpeechController owner)
        {
            if (m_SpeechOwner != owner) return;
            if (m_SpeechRoutine != null) StopCoroutine(m_SpeechRoutine);
            m_SpeechRoutine = null;
            m_SpeechOwner = null;
            if (m_SpeechSource != null) m_SpeechSource.Stop();
        }

        private bool CanPlaySpeech(MutinySpeechController owner) =>
            owner != null && owner.isActiveAndEnabled && owner.HasActiveBubble &&
            SfxEnabled && m_SpeechSource != null && m_SpeechSource.isActiveAndEnabled;

        private IEnumerator PlaySpeechSequence(MutinySpeechController owner, MutinySpeechAudio sequence)
        {
            yield return null;
            for (int index = 0; index < sequence.Clips.Length; index++)
            {
                if (!CanPlaySpeech(owner) || !m_SfxClips.TryGetValue(sequence.Clips[index], out AudioClip clip) || clip == null) break;
                m_SpeechSource.PlayOneShot(clip, SfxVolume);
                double deadline = Time.realtimeSinceStartupAsDouble + clip.length /
                    Math.Max(.01f, Mathf.Abs(m_SpeechSource.pitch)) +
                    (index < sequence.Clips.Length - 1 ? sequence.GapSeconds : 0);
                SfxPlayed?.Invoke(clip.name);
                while (Time.realtimeSinceStartupAsDouble < deadline && CanPlaySpeech(owner)) yield return null;
            }
            m_SpeechSource.Stop();
            m_SpeechRoutine = null;
            m_SpeechOwner = null;
        }

        private static MutinyLevelRoot FindRobotVoiceLevel()
        {
            foreach (MutinyLevelController controller in FindObjectsByType<MutinyLevelController>())
            {
                MutinyLevelRoot root = controller.CurrentLevel;
                if (root != null && root.gameObject.activeInHierarchy &&
                    controller.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16)))
                    return root;
            }
            return null;
        }

        public void PlayMusic(string musicName, bool loop = true)
        {
            if (MusicSource == null || string.IsNullOrEmpty(musicName))
                return;

            if (m_MusicClips.TryGetValue(musicName, out AudioClip clip))
            {
                bool alreadyPlaying = MusicSource.clip == clip && MusicSource.isPlaying;

                // The requested menu/game track is logical state, so remember it even
                // while music is disabled. Turning music back on must start the track
                // belonging to the current screen instead of waiting for another request.
                MusicSource.clip = clip;
                MusicSource.loop = loop;
                MusicSource.volume = MusicVolume;

                if (!MusicEnabled || alreadyPlaying)
                    return;

                MusicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (MusicSource != null)
            {
                MusicSource.Stop();
            }
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
            if (!SfxEnabled)
            {
                CancelSpeechAudio(m_SpeechOwner);
                if (m_RobotVoiceRoutine != null) StopCoroutine(m_RobotVoiceRoutine);
                m_RobotVoiceRoutine = null;
                m_RobotVoiceRequests.Clear();
                m_RobotVoiceSource?.Stop();
            }
            Mutiny.Persistence.MutinySaveSystem.SfxEnabled = SfxEnabled;
            Debug.Log($"[MutinyAudio] HUD-CORNER-06 SFX={(SfxEnabled ? "on" : "off")}", this);
        }

        public void ToggleMusic()
        {
            MusicEnabled = !MusicEnabled;
            Mutiny.Persistence.MutinySaveSystem.MusicEnabled = MusicEnabled;
            if (!MusicEnabled && MusicSource != null)
            {
                // MusicController.turnOffMusic stops both Flash Sound instances. Do
                // not pause: turning it back on restarts the current menu/game track.
                MusicSource.Stop();
            }
            else if (MusicEnabled && MusicSource != null)
            {
                if (MusicSource.clip != null)
                {
                    MusicSource.volume = MusicVolume;
                    MusicSource.Play();
                }
            }

            Debug.Log($"[MutinyAudio] HUD-CORNER-06 music={(MusicEnabled ? "on" : "off")} clip={(MusicSource != null && MusicSource.clip != null ? MusicSource.clip.name : "none")}", this);
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
