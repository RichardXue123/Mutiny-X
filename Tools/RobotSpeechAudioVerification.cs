using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class RobotSpeechAudioVerification
    {
        const string Key = "Mutiny.RobotSpeechAudioVerify";
        static string Report => Path.GetFullPath(Path.Combine(Application.dataPath, "../robot-speech-audio-verification.txt"));
        static IEnumerator test;
        static MutinyAudioManager audio;
        static bool savedSfx;
        static int checks, frames;
        static readonly List<string> heard = new();
        static readonly List<double> times = new();
        static RobotSpeechAudioVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                { frames = 0; EditorApplication.update += Tick; }
            };
        }
        public static void RunBatch()
        {
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; XML / natural speech / actual death-settling / coroutine / AudioSource submissions, unscaled realtime; 2026-10-01\n");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer,16), out string error)) throw new Exception(error);
            if (!AssetDatabase.IsValidFolder("Assets/RobotSpeechAudioVerification")) AssetDatabase.CreateFolder("Assets","RobotSpeechAudioVerification");
            EditorSceneManager.SaveScene(scene,"Assets/RobotSpeechAudioVerification/Main.unity");
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
        static string[] RobotEvents() => heard.Where(n => n.StartsWith("daftpunk_")).ToArray();
        static IEnumerator Until(Func<bool> condition, string description)
        {
            double end=Time.realtimeSinceStartupAsDouble+25;
            while(!condition() && Time.realtimeSinceStartupAsDouble<end) yield return null;
            Check(condition(),description);
        }
        static void Advance(MutinySpeechController speech,float seconds) =>
            typeof(MutinySpeechController).GetMethod("AdvanceSpeechForVerification",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(speech,new object[]{seconds});
        static MutinySpeechController Response(MutinyLevelRoot root)
        {
            var speech=root.GetComponent<MutinySpeechController>();
            root.GetComponent<MutinyTurnManager>().StartGame();
            Advance(speech,1); speech.Click(); Advance(speech,6.5f);
            Check(speech.Speaker != null && speech.Speaker.TeamIndex==2 && !speech.IsPlayingEndingLine,"Production speech ticks/click advance opening line0 to response1");
            return speech;
        }
        static void FinishIntro(MutinySpeechController speech)
        {
            Advance(speech,1); speech.Click(); Advance(speech,.12f); speech.Click(); Advance(speech,.08f);
            Check(!speech.HasPendingOrActiveSpeech,"Production click and ticks complete introduction");
        }
        static void Defeat(MutinyLevelRoot root,bool player,bool enemy)
        {
            foreach(var actor in root.Characters)
            {
                actor.PhysicsBody.TryGetTerrain(out _,out _,out _);
                for(int t=0;t<50;t++) actor.PhysicsBody.AdvanceSimulationTick();
            }
            if(player) foreach(var actor in root.Team1.Characters) actor.Drown();
            if(enemy) foreach(var actor in root.Team2.Characters) actor.Drown();
            var manager=root.GetComponent<MutinyTurnManager>(); manager.PassTurn();
            for(int t=0;t<11;t++) manager.AdvanceSimulationTick();
        }
        static void AssertFour(int first)
        {
            Check(RobotEvents().SequenceEqual(Enumerable.Range(first,4).Select(i=>"daftpunk_"+i.ToString("D2"))),"Fixed sequence exactly "+first+".."+(first+3));
            var indices=Enumerable.Range(0,heard.Count).Where(i=>heard[i].StartsWith("daftpunk_")).ToArray();
            for(int i=0;i<3;i++)
            {
                double gap=times[indices[i+1]]-times[indices[i]]-Length(first+i);
                Check(gap>=.499 && gap<.75,"Speech measured post-clip gap="+gap.ToString("F4"));
            }
        }
        static IEnumerator Run()
        {
            audio=MutinyAudioManager.Instance; savedSfx=audio.SfxEnabled;
            if(!audio.SfxEnabled) audio.ToggleSFX();
            audio.SfxPlayed+=Observe; Time.timeScale=0;
            var controller=Object.FindObjectsByType<MutinyLevelController>().Single(c=>c.StartInGameplay);
            var gm=MutinyGMManager.Instance ?? new GameObject("SpeechAudioGM").AddComponent<MutinyGMManager>();
            Check(controller.CurrentLevel.SpeechAudio.Count==2,"Saved/reopened Scene Play retains both XML speechAudio configurations");
            string xml=Resources.Load<TextAsset>("Data/Levels/level_1_16").text;
            var parsed=MutinyLevelXmlParser.Parse(xml);
            Check(parsed.SpeechAudio.Count==2 && parsed.SpeechAudio[0].Line==1 && parsed.SpeechAudio[1].Line==3 && parsed.SpeechAudio.All(c=>c.GapSeconds==.5f),"Parser loads line1/3 and half-second gaps");
            foreach(string bad in new[]{xml.Replace("line=\"1\"","line=\"4\""),xml.Replace("line=\"3\"","line=\"1\""),xml.Replace("gapSeconds=\"0.5\"","gapSeconds=\"NaN\""),xml.Replace("gapSeconds=\"0.5\"","gapSeconds=\"-1\""),xml.Replace("daftpunk_01,"," ,")})
            {
                bool rejected=false; try{MutinyLevelXmlParser.Parse(bad);}catch(FormatException){rejected=true;}
                Check(rejected,"Parser rejects malformed speech audio configuration");
            }
            Check(gm.ExecuteCommand("enterlevel 16"),"GM starts natural intro fixture");
            var root=controller.CurrentLevel; ResetEvents();
            var wait=Until(()=>RobotEvents().Length==1,"Natural opening response starts fixed clip1"); while(wait.MoveNext()) yield return null;
            Check(root.GetComponent<MutinySpeechController>().Speaker.TeamIndex==2,"Fixed intro starts on enemy response, not pirate line0");
            string language=MutinyLocalization.Code;
            gm.ExecuteCommand("lang en"); gm.ExecuteCommand("lang zh-cn");
            root.Team2.SelectCharacter(root.Team2.Characters[0]);
            wait=Until(()=>RobotEvents().Length==4,"Natural speech submits four clips"); while(wait.MoveNext()) yield return null;
            AssertFour(1);
            Check(root.NextRobotVoiceNumber==1,"Scripted speech and language switches leave selection cursor untouched");
            wait=Until(()=>RobotEvents().Length==6,"Queued selection resumes after speech audio finishes"); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().SequenceEqual(new[]{"daftpunk_01","daftpunk_02","daftpunk_03","daftpunk_04","daftpunk_01","daftpunk_02"}) && root.NextRobotVoiceNumber==3,"Selection retains two-part cycle independently after fixed sequence");
            var speech=root.GetComponent<MutinySpeechController>(); FinishIntro(speech);
            wait=Wait(.7); while(wait.MoveNext()) yield return null;
            ResetEvents(); Defeat(root,true,false);
            Check(root.GetComponent<MutinyTurnManager>().GameResult==GameOverResult.Team2Wins && speech.IsPlayingEndingLine,"Actual drowning/pass/settling emits enemy victory speech3");
            wait=Until(()=>RobotEvents().Length==4,"Enemy victory submits four fixed clips"); while(wait.MoveNext()) yield return null;
            AssertFour(5); Check(root.NextRobotVoiceNumber==3,"Enemy victory does not advance selection cycle");
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets for skip fixture"); root=controller.CurrentLevel; ResetEvents(); speech=Response(root);
            wait=Until(()=>RobotEvents().Length==1,"Skip fixture starts first clip"); while(wait.MoveNext()) yield return null;
            Advance(speech,1); speech.Click(); Advance(speech,.12f); speech.Click(); Advance(speech,.08f);
            wait=Wait(1.4); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().Length==1 && !speech.HasPendingOrActiveSpeech,"Production skip cancels pending speech clips");
            root.Team2.SelectCharacter(root.Team2.Characters[0]);
            wait=Until(()=>RobotEvents().Length==3,"Ordinary pair resumes after skipped speech"); while(wait.MoveNext()) yield return null;
            Check(root.NextRobotVoiceNumber==3,"Skipped fixed speech did not consume ordinary sequence");
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets for priority fixture"); root=controller.CurrentLevel; ResetEvents();
            audio.PlaySFX("Robot"); audio.PlaySFX("RobotCaptain");
            wait=Until(()=>RobotEvents().Length==1,"Ordinary selection begins before speech"); while(wait.MoveNext()) yield return null;
            speech=Response(root); ResetEvents();
            wait=Until(()=>RobotEvents().Length==4,"Scripted speech preempts ordinary pending queue"); while(wait.MoveNext()) yield return null;
            AssertFour(1); Check(root.NextRobotVoiceNumber==2,"Priority cancellation leaves previously submitted ordinary clip counted once");
            FinishIntro(speech);
            wait=Wait(1.2); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().Length==4,"Cancelled ordinary queue does not resume stale pending clips");
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets mute fixture"); root=controller.CurrentLevel; ResetEvents(); speech=Response(root);
            wait=Until(()=>RobotEvents().Length==1,"Mute fixture starts clip1"); while(wait.MoveNext()) yield return null;
            audio.ToggleSFX(); wait=Wait(1.4); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().Length==1 && root.NextRobotVoiceNumber==1,"Mute cancels fixed sequence without consuming cycle"); audio.ToggleSFX(); FinishIntro(speech);
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets restart fixture"); root=controller.CurrentLevel; ResetEvents(); speech=Response(root);
            wait=Until(()=>RobotEvents().Length==1,"Restart fixture starts clip1"); while(wait.MoveNext()) yield return null;
            controller.RestartCurrentLevel(); root=controller.CurrentLevel;
            wait=Wait(1.4); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().Length==1 && root.NextRobotVoiceNumber==1,"Restart cancels old fixed sequence and resets ordinary cursor");
            speech=Response(root); ResetEvents(); wait=Until(()=>RobotEvents().Length==1,"Rebuilt root starts new fixed speech"); while(wait.MoveNext()) yield return null;
            Check(gm.ExecuteCommand("enterlevel 1"),"GM leaves during fixed speech");
            wait=Wait(1.4); while(wait.MoveNext()) yield return null;
            Check(RobotEvents().Length==1 && controller.CurrentLevel.SpeechAudio.Count==0,"Leaving cancels fixed speech; unconfigured level remains unconfigured");
            Check(heard.Contains("redPirate"),"Unconfigured level retains normal team voice");
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets player victory fixture"); root=controller.CurrentLevel; speech=Response(root); FinishIntro(speech); ResetEvents(); Defeat(root,false,true);
            wait=Wait(1.3); while(wait.MoveNext()) yield return null;
            Check(root.GetComponent<MutinyTurnManager>().GameResult==GameOverResult.Team1Wins && RobotEvents().Length==0 && heard.Contains("redPirate"),"Actual player victory preserves pirate speech2 without robot fixed sequence");
            Check(gm.ExecuteCommand("enterlevel 16"),"GM resets draw fixture"); root=controller.CurrentLevel; speech=Response(root); FinishIntro(speech); ResetEvents(); Defeat(root,true,true);
            wait=Wait(1.3); while(wait.MoveNext()) yield return null;
            Check(root.GetComponent<MutinyTurnManager>().GameResult==GameOverResult.Draw && RobotEvents().Length==0 && !speech.HasActiveBubble,"Actual draw has no fixed ending speech");
            MutinyLocalization.Select(language);
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
