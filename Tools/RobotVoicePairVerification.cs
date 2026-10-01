using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class RobotVoicePairVerification
    {
        const string Key = "Mutiny.RobotVoicePairVerify";
        static string Report => Path.GetFullPath(Path.Combine(Application.dataPath, "../robot-voice-pair-verification.txt"));
        static IEnumerator test;
        static MutinyAudioManager audio;
        static bool savedSfx;
        static int checks, frames;
        static readonly List<string> heard = new();
        static readonly List<double> times = new();
        static RobotVoicePairVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                { frames = 0; EditorApplication.update += Tick; }
            };
        }
        public static void RunBatch()
        {
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; actual production SelectCharacter / PlaySFX / coroutine / AudioSource submissions, unscaled realtime; 2026-10-01\n");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer,16), out string error)) throw new Exception(error);
            if (!AssetDatabase.IsValidFolder("Assets/RobotVoicePairVerification")) AssetDatabase.CreateFolder("Assets","RobotVoicePairVerification");
            EditorSceneManager.SaveScene(scene,"Assets/RobotVoicePairVerification/Main.unity");
            EditorSceneManager.OpenScene(scene.path);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool ok, string message)
        {
            if (!ok) throw new Exception(message);
            checks++; File.AppendAllText(Report,"PASS " + message + "\n");
        }
        static void Observe(string name)
        {
            heard.Add(name); times.Add(Time.realtimeSinceStartupAsDouble);
            File.AppendAllText(Report,"PLAY " + name + " time=" + times.Last().ToString("F6") + "\n");
        }
        static IEnumerator Wait(double seconds)
        {
            double end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end) yield return null;
        }
        static IEnumerator Count(int count)
        {
            double end = Time.realtimeSinceStartupAsDouble + 20;
            while (heard.Count < count && Time.realtimeSinceStartupAsDouble < end) yield return null;
            Check(heard.Count == count, "Expected exactly " + count + " playback events");
        }
        static double Length(int number) => Resources.Load<AudioClip>("Audio/SFX/daftpunk_" + number.ToString("D2")).length;
        static void ResetEvents() { heard.Clear(); times.Clear(); }
        static IEnumerator Run()
        {
            audio = MutinyAudioManager.Instance; savedSfx = audio.SfxEnabled;
            if (!audio.SfxEnabled) audio.ToggleSFX();
            audio.SfxPlayed += Observe;
            // Freeze gameplay only. Robot voice timing must still run in real time.
            Time.timeScale = 0;
            var controller = Object.FindObjectsByType<MutinyLevelController>().Single(c => c.StartInGameplay);
            var gm = MutinyGMManager.Instance ?? new GameObject("VoicePairGM").AddComponent<MutinyGMManager>();
            Check(controller.CurrentLevel.NextRobotVoiceNumber == 1, "Saved Scene Play starts voice counter at 1");
            Check(gm.ExecuteCommand("enterlevel 16"), "Production GM loads single16");
            var root = controller.CurrentLevel;
            ResetEvents();
            for (int i=0;i<5;i++) root.Team2.SelectCharacter(root.Team2.Characters[i%2]);
            var wait = Count(10); while(wait.MoveNext()) yield return null;
            for (int i=0;i<10;i++) Check(heard[i] == "daftpunk_" + (i%8+1).ToString("D2"), "Five queued selections retain 1,2 / 3,4 / 5,6 / 7,8 / 1,2: clip " + i);
            for (int i=0;i<10;i+=2)
            {
                double gap = times[i+1]-times[i]-Length(i%8+1);
                Check(gap >= .499 && gap < .75, "Pair " + (i/2+1) + " measured post-clip gap=" + gap.ToString("F4") + " seconds");
            }
            Check(root.NextRobotVoiceNumber == 3, "Counter advances per actual clip, wraps at 8");
            wait=Wait(Length(2)+.1); while(wait.MoveNext()) yield return null;
            ResetEvents(); root.Team1.SelectCharacter(root.Team1.Characters[0]); audio.PlaySFX("click");
            Check(heard.SequenceEqual(new[]{"redPirate","click"}) && root.NextRobotVoiceNumber == 3, "Ordinary pirate and click remain immediate, do not consume robot counter");
            ResetEvents(); audio.PlaySFX("RobotCaptain");
            wait=Count(1); while(wait.MoveNext()) yield return null;
            Check(heard[0]=="daftpunk_03", "Direct Captain SFX starts next pair");
            audio.ToggleSFX(); audio.PlayCharacterVoice("Robot");
            wait=Wait(Length(3)+.65); while(wait.MoveNext()) yield return null;
            Check(heard.Count==1 && root.NextRobotVoiceNumber==4, "Mute during pair cancels second part; muted requests do not advance");
            audio.ToggleSFX(); ResetEvents(); audio.PlaySFX("Robot");
            wait=Count(2); while(wait.MoveNext()) yield return null;
            Check(heard.SequenceEqual(new[]{"daftpunk_04","daftpunk_05"}) && root.NextRobotVoiceNumber==6, "Unmute resumes first unplayed clip and still plays two parts");
            wait=Wait(Length(5)+.1); while(wait.MoveNext()) yield return null;
            ResetEvents(); var source=audio.SfxSource; audio.SfxSource=null;
            audio.PlayCharacterVoice("Robot"); audio.SfxSource=source;
            source.enabled=false; audio.PlayCharacterVoice("Robot"); source.enabled=true;
            var clips=(Dictionary<string,AudioClip>)typeof(MutinyAudioManager).GetField("m_SfxClips",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(audio);
            var sixth=clips["daftpunk_06"]; clips.Remove("daftpunk_06"); audio.PlayCharacterVoice("Robot"); clips["daftpunk_06"]=sixth;
            wait=Wait(.15); while(wait.MoveNext()) yield return null;
            Check(heard.Count==0 && root.NextRobotVoiceNumber==6, "Missing/disabled Source and missing clip produce no playback or advancement");
            audio.PlayCharacterVoice("Robot"); wait=Count(1); while(wait.MoveNext()) yield return null;
            source.enabled=false;
            wait=Wait(Length(6)+.65); while(wait.MoveNext()) yield return null;
            source.enabled=true;
            Check(heard.Count==1 && root.NextRobotVoiceNumber==7, "Source disabled during delay cancels second part without consuming clip7");
            ResetEvents(); audio.PlayCharacterVoice("RobotCaptain"); wait=Count(1); while(wait.MoveNext()) yield return null;
            Check(heard[0]=="daftpunk_07", "Captain resumes next unplayed clip7");
            controller.RestartCurrentLevel(); root=controller.CurrentLevel;
            Check(root.NextRobotVoiceNumber==1, "Production restart resets new level counter");
            wait=Wait(Length(7)+.65); while(wait.MoveNext()) yield return null;
            Check(heard.Count==1 && root.NextRobotVoiceNumber==1, "Restart cancels old pending second clip without advancing new root");
            ResetEvents(); root.Team2.SelectCharacter(root.Team2.Characters[0]);
            wait=Count(2); while(wait.MoveNext()) yield return null;
            Check(heard.SequenceEqual(new[]{"daftpunk_01","daftpunk_02"}), "First selection after restart plays1 then2");
            wait=Wait(Length(2)+.1); while(wait.MoveNext()) yield return null;
            ResetEvents(); audio.PlaySFX("Robot"); audio.PlaySFX("RobotCaptain");
            wait=Count(1); while(wait.MoveNext()) yield return null;
            Check(gm.ExecuteCommand("enterlevel 1"), "Production GM leaves space level while pair and queue pending");
            audio.PlayCharacterVoice("Robot");
            wait=Wait(2); while(wait.MoveNext()) yield return null;
            Check(heard.Count(n => n.StartsWith("daftpunk_"))==1 && controller.CurrentLevel.NextRobotVoiceNumber==1, "Leaving cancels both pending part and queued old-level request; ordinary level startup audio remains allowed");
            Check(gm.ExecuteCommand("enterlevel 2_16"), "GM dual16 regression");
            audio.PlayCharacterVoice("RobotCaptain"); wait=Wait(.15); while(wait.MoveNext()) yield return null;
            Check(heard.Count(n => n.StartsWith("daftpunk_"))==1, "Dual16 does not use single16 voice sequence");
            Check(gm.ExecuteCommand("enterlevel 16"), "GM re-entry");
            root=controller.CurrentLevel; ResetEvents(); audio.PlayCharacterVoice("Robot");
            wait=Count(2); while(wait.MoveNext()) yield return null;
            Check(heard.SequenceEqual(new[]{"daftpunk_01","daftpunk_02"}), "Re-entry starts fresh pair1,2");
            Check(root.GravityScale==.5f && root.Team2.Characters.Count==8 && root.Team2.Characters.All(c=>c.Luck==50), "Audio change preserves gravity, eight robots and Luck50");
        }
        static void Tick()
        {
            if(++frames<20) return;
            try
            {
                test ??= Run();
                if (!test.MoveNext()) Finish(true,null);
            }
            catch(Exception ex) { Finish(false,ex); }
        }
        static void Finish(bool ok, Exception ex)
        {
            EditorApplication.update-=Tick; SessionState.SetBool(Key,false);
            Time.timeScale=1;
            if(audio!=null) { audio.SfxPlayed-=Observe; if(audio.SfxEnabled!=savedSfx) audio.ToggleSFX(); }
            File.AppendAllText(Report,(ok?"PASS all ":"FAIL ")+checks+" assertions.\n"+(ex==null?"":ex.ToString())+"\nScope: real production submissions and realtime coroutine delays; no claim of manual listening or full-match turn flow.\n");
            EditorApplication.Exit(ok?0:1);
        }
    }
}
