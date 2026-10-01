// Isolated editor helper called by SceneLevelPreviewVerification after natural
// Start/Update; drives real GM, weapon factory, physics and AI predictor entries.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mutiny.Levels;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    public static class Level16SpaceSettingsVerification
    {
        const BindingFlags StaticInternal = BindingFlags.Static | BindingFlags.NonPublic;
        public static void Verify(MutinyLevelController controller, Action<bool, string> check)
        {
            var root = controller.CurrentLevel;
            check(root.GravityScale == 0.5f && root.Characters.All(c => c.PhysicsBody.State.EffectiveGravityScale == 0.5f),
                "EXT-SPACE16-GRAVITY-01 serialized scene preview and natural Start retain half gravity on all 21 bodies");
            CheckLuck(root, check);
            var gm = MutinyGMManager.Instance;
            if (gm == null) gm = new GameObject("Space16_GM").AddComponent<MutinyGMManager>();
            check(gm.ExecuteCommand("enterlevel 16") && controller.CurrentLevel.GravityScale == 0.5f,
                "Real GM entry applies XML half gravity");
            root = controller.CurrentLevel;
            CheckLuck(root, check);
            var actor = root.Team1.Characters.Single(c => c.GridX == 11 && c.GridY == 5);
            actor.PhysicsBody.TryGetTerrain(out _, out _, out _);
            var start = new Vector2(actor.PhysicsBody.State.X, actor.PhysicsBody.State.Y);
            actor.PhysicsBody.Twang(start, start + new Vector2(0f, 20f));
            float before = actor.PhysicsBody.State.VelocityY;
            actor.PhysicsBody.AdvanceSimulationTick();
            check(Mathf.Abs(actor.PhysicsBody.State.VelocityY - before - 0.5f) < 0.001f &&
                actor.PhysicsBody.State.Weight == 1f, "Production character Twang gains 0.5 px/tick, retaining Weight=1");
            VerifyWeapon(actor, "cherryBomb", 1f, 0.5f, check);
            VerifyWeapon(actor, "boulder", 1.5f, 0.75f, check);
            VerifyWeapon(actor, "cannonball", 0f, 0f, check);
            controller.RestartCurrentLevel();
            check(controller.CurrentLevel.GravityScale == 0.5f, "Production restart retains half gravity");
            CheckLuck(controller.CurrentLevel, check);
            check(gm.ExecuteCommand("enterlevel 6") && controller.CurrentLevel.GravityScale == 1f &&
                controller.CurrentLevel.Characters.All(c => c.PhysicsBody.State.EffectiveGravityScale == 1f),
                "Switch to ordinary Level6 resets default world/body gravity to 1");
            var ordinaryActor = controller.CurrentLevel.Team1.Characters[0];
            var ordinary = MutinyWeaponFactory.SpawnAndFire("cherryBomb", ordinaryActor, new Vector2(0f, -10f), false);
            try
            {
                before = ordinary.PhysicsBody.State.VelocityY;
                ordinary.PhysicsBody.AdvanceSimulationTick();
                check(Mathf.Abs(ordinary.PhysicsBody.State.VelocityY - before - 1f) < 0.001f,
                    "Actual ordinary-level fired bomb gains original 1 px/tick");
            }
            finally { UnityEngine.Object.DestroyImmediate(ordinary.gameObject); }
            check(gm.ExecuteCommand("enterlevel 16"), "GM returns to scene-preview Level16");
            CheckLuck(controller.CurrentLevel, check);
        }
        static void CheckLuck(MutinyLevelRoot root, Action<bool, string> check)
        {
            var ai = root.Team2.GetComponent<MutinyAIController>();
            var effective = typeof(MutinyAIController).GetMethod("GetEffectiveLuck", BindingFlags.Instance | BindingFlags.NonPublic);
            check(root.Team2.Characters.Count == 11 && root.Team2.Characters.All(c => c.Luck == 50f) &&
                ai.LevelLuckOverride == null && root.Team2.Characters.All(c => (float)effective.Invoke(ai, new object[] { c }) == 50f) &&
                root.Team1.Characters.All(c => c.Luck == 5f), "EXT-SPACE16-LUCK-01 all 11 robots use native/effective Luck50, player Luck5, no GM override");
        }
        static void VerifyWeapon(MutinyCharacter owner, string kind, float weight, float acceleration, Action<bool, string> check)
        {
            var weapon = MutinyWeaponFactory.SpawnAndFire(kind, owner, new Vector2(0f, -10f), false);
            try
            {
                var body = weapon.PhysicsBody;
                body.TryGetTerrain(out string[,] terrain, out int width, out int height);
                check(body.State.Weight == weight && body.State.EffectiveGravityScale == 0.5f,
                    "Real weapon " + kind + " preserves original weight and receives half gravity");
                float vy = body.State.VelocityY;
                body.AdvanceSimulationTick();
                check(Mathf.Abs(body.State.VelocityY - vy - acceleration) < 0.001f,
                    "Fired " + kind + " actual acceleration " + acceleration);
                var formal = typeof(MutinyAIController).GetMethod("CreateFormalWeaponPredictionTemplate", StaticInternal);
                var template = (PhysicsBodyState)formal.Invoke(null, new object[] { owner, kind });
                check(template.Weight == weight && template.EffectiveGravityScale == 0.5f,
                    "Native AI formal prediction template agrees with runtime: " + kind);
                var effects = typeof(MutinyAIController).Assembly.GetType("Mutiny.Simulation.MutinyAIEffectWorld");
                var enhanced = (PhysicsBodyState)effects.GetMethod("ProjectileBody", StaticInternal)
                    .Invoke(null, new object[] { kind.ToLowerInvariant(), owner.PhysicsBody.State });
                check(enhanced.Weight == weight && enhanced.EffectiveGravityScale == 0.5f,
                    "Enhanced AI projectile inherits world gravity: " + kind);
                if (weight > 0f)
                {
                    var launch = new Vector2(enhanced.X, enhanced.Y);
                    var destination = launch + new Vector2(90f, 0f);
                    var aimed = (Vector2)effects.GetMethod("AimedVelocity", StaticInternal).Invoke(null,
                        new object[] { launch, destination, 20f, 1f, enhanced.GravityPerTick });
                    enhanced.VelocityX = aimed.x; enhanced.VelocityY = aimed.y;
                    var empty = new string[100, 115];
                    for (int tick = 0; tick < 10; tick++) MutinyPhysics.Step(ref enhanced, empty, 115, 100);
                    check(Vector2.Distance(new Vector2(enhanced.X, enhanced.Y), destination) < 0.001f,
                        "Enhanced directed arc reaches target with actual half-gravity physics: " + kind);
                }
                // Start the actual sliced/native predictor at the current live state;
                // compare several physics ticks, without rewriting its formulas.
                var type = typeof(MutinyAIController).Assembly.GetType("Mutiny.Simulation.MutinyAIPrediction");
                var kindType = type.GetNestedType("Kind", BindingFlags.NonPublic);
                var prediction = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new object[] { Enum.Parse(kindType, "Weapon"), body.State, kind, terrain, width, height,
                        body.WaterPixelY, null, null }, null);
                var advance = type.GetMethod("Advance");
                for (int tick = 0; tick < 5; tick++)
                {
                    check((bool)advance.Invoke(prediction, null), "Production AI predictor advances " + kind + " tick " + tick);
                    body.AdvanceSimulationTick();
                }
                var predicted = (PhysicsBodyState)type.GetField("Body").GetValue(prediction);
                check(Mathf.Abs(predicted.X - body.State.X) < 0.001f && Mathf.Abs(predicted.Y - body.State.Y) < 0.001f &&
                    Mathf.Abs(predicted.VelocityY - body.State.VelocityY) < 0.001f,
                    "Five native AI prediction ticks match real low-gravity flight: " + kind);
            }
            finally { UnityEngine.Object.DestroyImmediate(weapon.gameObject); }
        }
    }
}
