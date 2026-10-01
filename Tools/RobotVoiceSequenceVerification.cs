// Copy into an isolated project's Assets/Editor and execute
// Mutiny.Verification.Editor.RobotVoiceSequenceVerification.RunBatch.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class RobotVoiceSequenceVerification
    {
        const string Pending = "Mutiny.RobotVoiceVerify";
        static int checks, frames;
        static readonly StringBuilder Report = new StringBuilder();
        static RobotVoiceSequenceVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                {
                    frames = 0;
                    EditorApplication.update += Tick;
                }
            };
        }
        public static void RunBatch()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error))
                throw new Exception(error);
            const string folder = "Assets/RobotVoiceVerification";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "RobotVoiceVerification");
            string path = folder + "/RobotPreview.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.OpenScene(path);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition, string description)
        {
            if (!condition) throw new Exception(description);
            checks++;
            Report.AppendLine("PASS " + description);
        }
        static void Tick()
        {
            if (++frames < 20) return;
            EditorApplication.update -= Tick;
            SessionState.EraseBool(Pending);
            Report.AppendLine("Unity " + Application.unityVersion + " isolated batchmode/nographics, serialized Scene preview and natural Play startup; 2026-10-01");
            bool success = false;
            var audio = MutinyAudioManager.Instance;
            bool originalSfx = audio.SfxEnabled;
            var heard = new List<string>();
            Action<string> observe = name => heard.Add(name);
            audio.SfxPlayed += observe;
            try
            {
                if (!audio.SfxEnabled) audio.ToggleSFX();
                for (int index = 1; index <= 8; index++)
                {
                    var clip = Resources.Load<AudioClip>("Audio/SFX/daftpunk_" + index.ToString("D2"));
                    Check(clip != null && clip.samples > 0 && clip.frequency > 0 && clip.channels > 0,
                        "EXT-SPACE16-VOICE-04 imported clip " + index + " has valid samples/frequency/channels");
                    Report.AppendLine("CLIP " + clip.name + " seconds=" + clip.length + " Hz=" + clip.frequency + " channels=" + clip.channels);
                }
                var controller = Object.FindObjectsByType<MutinyLevelController>().Single(c => c.StartInGameplay);
                var frontend = Object.FindAnyObjectByType<MutinyFrontendController>();
                Check(frontend.CurrentPage == MutinyFrontendPage.Gameplay && controller.CurrentLevel.NextRobotVoiceNumber == 1,
                    "EXT-SPACE16-VOICE-02 serialized Scene Play starts new counter at 01");
                var gm = MutinyGMManager.Instance;
                if (gm == null) gm = new GameObject("RobotVoice_GM").AddComponent<MutinyGMManager>();
                Check(gm.ExecuteCommand("enterlevel 16"), "Production GM enters single16");
                var root = controller.CurrentLevel;
                var captain = root.Team2.Characters.Single(c => c.CharacterType == "RobotCaptain");
                var robot = root.Team2.Characters.First(c => c.CharacterType == "Robot");
                heard.Clear();
                for (int index = 0; index < 18; index++)
                {
                    root.Team2.SelectCharacter(index % 2 == 0 ? captain : robot);
                    Check(heard.Count == index + 1 && heard[index] == "daftpunk_" + (index % 8 + 1).ToString("D2"),
                        "EXT-SPACE16-VOICE-01 real alternating Captain/Robot selection " + (index + 1) + " -> " + heard.Last());
                }
                Report.AppendLine("SEQUENCE " + string.Join(", ", heard));
                Check(root.NextRobotVoiceNumber == 3, "Shared counter after 18 plays points to 03");
                root.Team1.SelectCharacter(root.Team1.Characters[0]);
                audio.PlaySFX("click");
                Check(heard[18] == "redPirate" && heard[19] == "click" && root.NextRobotVoiceNumber == 3,
                    "EXT-SPACE16-VOICE-03 ordinary pirate/click play without advancing robot sequence");
                int priorEvents = heard.Count;
                audio.ToggleSFX();
                root.Team2.SelectCharacter(robot);
                Check(heard.Count == priorEvents && root.NextRobotVoiceNumber == 3, "SFX off produces no event or sequence advance");
                audio.ToggleSFX();
                root.Team2.SelectCharacter(captain);
                Check(heard.Last() == "daftpunk_03" && root.NextRobotVoiceNumber == 4, "SFX on resumes next clip03");
                var source = audio.SfxSource;
                audio.SfxSource = null;
                try { audio.PlayCharacterVoice("Robot"); }
                finally { audio.SfxSource = source; }
                Check(root.NextRobotVoiceNumber == 4, "Missing AudioSource does not advance");
                bool sourceEnabled = source.enabled;
                source.enabled = false;
                try { audio.PlayCharacterVoice("Robot"); }
                finally { source.enabled = sourceEnabled; }
                Check(root.NextRobotVoiceNumber == 4, "Disabled AudioSource does not advance");
                audio.PlaySFX("RobotCaptain");
                Check(heard.Last() == "daftpunk_04", "Direct Captain SFX uses same sequence");
                audio.PlaySFX("Robot");
                Check(heard.Last() == "daftpunk_05", "Direct base-type speech SFX uses same sequence");
                var clips = (Dictionary<string, AudioClip>)typeof(MutinyAudioManager).GetField("m_SfxClips",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(audio);
                var missing = clips["daftpunk_06"];
                clips.Remove("daftpunk_06");
                priorEvents = heard.Count;
                try { root.Team2.SelectCharacter(robot); }
                finally { clips["daftpunk_06"] = missing; }
                Check(root.NextRobotVoiceNumber == 6 && heard.Count == priorEvents,
                    "Missing next clip does not advance or report a false playback");
                root.Team2.SelectCharacter(captain);
                Check(heard.Last() == "daftpunk_06", "Restored clip resumes06");
                controller.RestartCurrentLevel();
                root = controller.CurrentLevel;
                Check(root.NextRobotVoiceNumber == 1, "EXT-SPACE16-VOICE-02 production restart resets01");
                root.Team2.SelectCharacter(root.Team2.Characters[0]);
                Check(heard.Last() == "daftpunk_01" && root.NextRobotVoiceNumber == 2, "Restart first selection plays01");
                Check(gm.ExecuteCommand("enterlevel 1"), "GM leaves robot level");
                root = controller.CurrentLevel;
                priorEvents = heard.Count;
                audio.PlayCharacterVoice("Robot");
                Check(heard.Count == priorEvents && root.NextRobotVoiceNumber == 1, "Other single levels do not route robot cycle");
                Check(gm.ExecuteCommand("enterlevel 2_16"), "GM enters dual16");
                root = controller.CurrentLevel;
                priorEvents = heard.Count;
                audio.PlayCharacterVoice("RobotCaptain");
                Check(heard.Count == priorEvents && root.NextRobotVoiceNumber == 1, "Dual16 identity does not activate single16 voice cycle");
                Check(gm.ExecuteCommand("enterlevel 16"), "GM re-enters single16");
                root = controller.CurrentLevel;
                root.Team2.SelectCharacter(root.Team2.Characters[0]);
                Check(heard.Last() == "daftpunk_01" && root.NextRobotVoiceNumber == 2,
                    "EXT-SPACE16-VOICE-02 re-entry resets01 despite persistent AudioManager");
                Check(root.GravityScale == 0.5f && root.Team2.Characters.All(c => c.Luck == 50f),
                    "Voice integration preserves half gravity and all robot Luck50");
                Check(frontend.ReturnToSinglePlayerLevelSelect(), "Production quit unloads robot root");
                priorEvents = heard.Count;
                audio.PlayCharacterVoice("Robot");
                Check(heard.Count == priorEvents, "Unloaded level cannot consume old robot sequence");
                success = true;
            }
            catch (Exception ex) { Report.AppendLine("FAIL " + ex); Debug.LogException(ex); }
            finally
            {
                audio.SfxPlayed -= observe;
                if (audio.SfxEnabled != originalSfx) audio.ToggleSFX();
            }
            Report.AppendLine((success ? "PASS " : "FAIL ") + checks + " assertions.");
            Report.AppendLine("Scope: imported audio and actual production playback submissions / final clip events. No claim of audibility, listening quality or complete enemy turns in nographics.");
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../robot-voice-sequence-verification.txt")), Report.ToString());
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
