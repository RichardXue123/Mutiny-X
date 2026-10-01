// Isolated Unity Editor verification entry. Copy to Assets/Editor in a disposable
// project and execute Mutiny.Verification.Editor.Level16DraftVerification.RunBatch.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mutiny.Levels;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class Level16DraftVerification
    {
        const string Pending = "Mutiny.Level16Draft.Pending";
        static readonly StringBuilder Report = new StringBuilder();
        static int checks;
        static Level16DraftVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false)) Verify();
            };
        }
        public static void RunBatch()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition, string description)
        {
            if (!condition) throw new Exception(description);
            checks++;
            Report.AppendLine("PASS " + description);
        }
        static void Verify()
        {
            SessionState.EraseBool(Pending);
            Report.AppendLine("Unity " + Application.unityVersion + " isolated batchmode/nographics Play Mode; 2026-10-01");
            bool success = false;
            var host = new GameObject("Level16Draft_Controller");
            var frontendHost = new GameObject("Level16Draft_Frontend");
            var cameraHost = new GameObject("Level16Draft_Camera");
            var controller = host.AddComponent<MutinyLevelController>();
            var frontend = frontendHost.AddComponent<MutinyFrontendController>();
            cameraHost.AddComponent<Camera>().orthographic = true;
            cameraHost.AddComponent<MutinyCameraController>();
            MutinyGMManager gm = MutinyGMManager.Instance;
            GameObject gmHost = null;
            if (gm == null) { gmHost = new GameObject("Level16Draft_GM"); gm = gmHost.AddComponent<MutinyGMManager>(); }
            MutinyLevelRoot oldRoot = null;
            try
            {
                TextAsset xml = Resources.Load<TextAsset>("Data/Levels/level_1_16");
                Check(xml != null && xml.text == File.ReadAllText(Path.Combine(Application.dataPath, "Mutiny/Data/Levels/level_1_16.xml")),
                    "EXT-SPACE16-XML-01 authoring and runtime XML are identical");
                var data = MutinyLevelXmlParser.Parse(xml.text, xml.name);
                var six = MutinyLevelXmlParser.Parse(Resources.Load<TextAsset>("Data/Levels/level_1_06").text);
                Check(data.Width == 115 && data.Height == 36 && data.Players == 1 && data.Width == six.Width,
                    "EXT-SPACE16-XML-01 parser resolves 115x36 single-player, same width as Level 6");
                var types = new HashSet<string>();
                int terrainCount = 0, backgroundCount = 0;
                for (int y = 0; y < data.Height; y++) for (int x = 0; x < data.Width; x++)
                {
                    if (!string.IsNullOrEmpty(data.Terrain[y,x])) { terrainCount++; types.Add(data.Terrain[y,x]); }
                    if (!string.IsNullOrEmpty(data.Background[y,x]) && data.Background[y,x] != "antichest") { backgroundCount++; types.Add(data.Background[y,x]); }
                }
                foreach (string type in types) Check(MutinyLevelBuilder.ResolveTileSprite(type) != null, "Existing tile resolves: " + type);
                var regions = Regions(data.Terrain, data.Width, data.Height);
                Check(regions.Count == 3 && regions.Any(r => r == "4,28,6,10") && regions.Any(r => r == "87,111,6,10") &&
                    regions.Any(r => r == "18,96,12,29"), "EXT-SPACE16-LAYOUT-01 exactly three ships in intended positions");
                frontend.Initialize(controller);
                Check(gm.ExecuteCommand("enterlevel 16") && frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                    controller.CurrentLevelIndex == 16 && controller.ActiveGameMode == MutinyGameMode.SinglePlayer,
                    "EXT-SPACE16-ENTRY-01 real GM enters draft single-player 16");
                MutinyLevelRoot root = controller.CurrentLevel;
                Check(root.LevelName == "Orbital Convoy - Draft 01" && root.Width == 115 && root.Height == 36,
                    "Production root uses new draft identity and dimensions");
                Check(root.TerrainHolder.Cast<Transform>().Count(t => t.name.StartsWith("tile_")) == terrainCount &&
                    root.BackgroundHolder.Cast<Transform>().Count(t => t.name.StartsWith("bg_")) == backgroundCount,
                    "EXT-SPACE16-LAYOUT-01 builder creates every terrain/background tile without silent missing assets");
                Check(root.Team1.Characters.Count == 10 && root.Team2.Characters.Count == 11 && root.Characters.Count == 21 &&
                    !root.Team1.IsAiControlled && root.Team2.IsAiControlled && root.Team2.GetComponent<MutinyAIController>() != null,
                    "EXT-SPACE16-SPAWN-01 ten humans versus eleven AI robots");
                Check(root.Team1.Characters.Count(c => c.CharacterType == "redPirateCaptain") == 1 &&
                    root.Team1.Characters.Count(c => c.CharacterType == "redPirate") == 9 &&
                    root.Team2.Characters.Count(c => c.CharacterType == "RobotCaptain") == 1 &&
                    root.Team2.Characters.Count(c => c.CharacterType == "Robot") == 10,
                    "Each team has exactly one captain and the expected troop types");
                Check(root.Team1.Characters.Count(c => c.GridY == 5) == 2 && root.Team2.Characters.Count(c => c.GridY == 5) == 2,
                    "Small ship crews are two players and two robots; main ship holds 8 versus 9");
                var coordinates = new HashSet<string>();
                foreach (var character in root.Characters)
                {
                    int x = character.GridX, y = character.GridY;
                    Check(coordinates.Add(x + "," + y) && data.Terrain[y,x] == null && data.Terrain[y-1,x] == null &&
                        data.Terrain[y+1,x] != null && Fits(character, data.Terrain),
                        "Birth head/body clear, unique coordinate, supporting floor: " + character.name);
                    Check(character.GetComponent<SpriteRenderer>().sprite != null && MutinyCharacterAnimator.LoadFrames(character.CharacterType).Length == 35,
                        "Character sprite and complete animation load: " + character.name);
                    Check(character.PhysicsBody.TryGetTerrain(out _, out _, out _), "Physics uses production terrain: " + character.name);
                }
                var chest = root.GetComponent<MutinyTreasureChestManager>();
                Check(chest.ValidDropColumns.Count > 0 && chest.ValidDropColumns.All(x => Enumerable.Range(0, 34).Any(y => data.Terrain[y,x] != null)),
                    "Chest drop columns all have landable ship terrain above the fall boundary");
                for (int tick = 0; tick < 125; tick++) foreach (var character in root.Characters) character.PhysicsBody.AdvanceSimulationTick();
                foreach (var character in root.Characters)
                    Check(character.IsAlive && !character.IsDrowned && character.Health == 100 && character.PhysicsBody.IsAtRest &&
                        Mathf.Abs(character.PhysicsBody.State.X - (character.GridX * 32 + 16)) < 0.01f &&
                        // Terrain contact uses a 0.1px separation buffer in production physics.
                        Mathf.Abs(character.PhysicsBody.State.Y - (character.GridY * 32 + 24)) < 0.11f,
                        "125 actual physics ticks preserve alive settled initial placement: " + character.name +
                        " alive=" + character.IsAlive + " drowned=" + character.IsDrowned + " health=" + character.Health +
                        " rest=" + character.PhysicsBody.IsAtRest + " xy=" + character.PhysicsBody.State.X + "," + character.PhysicsBody.State.Y +
                        " velocity=" + character.PhysicsBody.State.VelocityX + "," + character.PhysicsBody.State.VelocityY);
                oldRoot = root;
                controller.RestartCurrentLevel();
                Check(controller.CurrentLevel != oldRoot && !oldRoot.gameObject.activeSelf && controller.CurrentLevelIndex == 16 &&
                    controller.CurrentLevel.Team1.Characters.Count == 10 && controller.CurrentLevel.Team2.Characters.Count == 11 &&
                    controller.CurrentLevel.LevelName == data.Name, "EXT-SPACE16-ENTRY-01 production restart keeps new draft and crews");
                success = true;
            }
            catch (Exception exception) { Report.AppendLine("FAIL " + exception); Debug.LogException(exception); }
            finally
            {
                if (controller.CurrentLevel != null) Object.DestroyImmediate(controller.CurrentLevel.gameObject);
                if (oldRoot != null) Object.DestroyImmediate(oldRoot.gameObject);
                Object.DestroyImmediate(host); Object.DestroyImmediate(frontendHost); Object.DestroyImmediate(cameraHost);
                if (gmHost != null) Object.DestroyImmediate(gmHost);
            }
            if (success)
            {
                try
                {
                    var entry = MutinyLevelEntryVerificationTest.Run();
                    Check(entry.Passed, "Existing production level identity / GM regression " + entry.PassedAssertions + "/" + entry.TotalAssertions);
                    Report.AppendLine(string.Join("\n", entry.Logs));
                    Report.AppendLine(string.Join("\n", entry.Failures));
                }
                catch (Exception exception) { success = false; Report.AppendLine("FAIL " + exception); }
            }
            Report.AppendLine((success ? "PASS " : "FAIL ") + checks + " draft assertions (plus existing GM/identity assertions above).");
            Report.AppendLine("Scope: XML/resource/production GM construction and settled spawn checks. No full-match balance, visual sci-fi material/sky or low-gravity claim.");
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../level16-draft-verification.txt")), Report.ToString());
            Debug.Log("[LEVEL16-DRAFT] " + (success ? "PASS " : "FAIL ") + checks);
            EditorApplication.Exit(success ? 0 : 1);
        }
        static bool Fits(MutinyCharacter character, string[,] terrain)
        {
            var s = character.PhysicsBody.State;
            int left = Mathf.FloorToInt((s.X-s.LeftExtent+0.01f)/32), right = Mathf.FloorToInt((s.X+s.RightExtent-0.01f)/32);
            int top = Mathf.FloorToInt((s.Y-s.TopExtent+0.01f)/32), bottom = Mathf.FloorToInt((s.Y+s.BottomExtent-0.01f)/32);
            for (int y = top; y <= bottom; y++) for (int x = left; x <= right; x++) if (terrain[y,x] != null) return false;
            return true;
        }
        static List<string> Regions(string[,] terrain, int width, int height)
        {
            var seen = new bool[height,width]; var result = new List<string>();
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                if (seen[y,x] || terrain[y,x] == null) continue;
                int minX=x, maxX=x, minY=y, maxY=y;
                var queue = new Queue<Vector2Int>(); queue.Enqueue(new Vector2Int(x,y)); seen[y,x]=true;
                while (queue.Count > 0)
                {
                    var p=queue.Dequeue(); minX=Math.Min(minX,p.x);maxX=Math.Max(maxX,p.x);minY=Math.Min(minY,p.y);maxY=Math.Max(maxY,p.y);
                    foreach(var d in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})
                    {
                        var n=p+d;if(n.x<0||n.x>=width||n.y<0||n.y>=height||seen[n.y,n.x]||terrain[n.y,n.x]==null)continue;
                        seen[n.y,n.x]=true;queue.Enqueue(n);
                    }
                }
                result.Add(minX+","+maxX+","+minY+","+maxY);
            }
            return result;
        }
    }
}
