using System.Collections.Generic;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Verification
{
    public static class MutinyAiPredictionVerificationTest
    {
        public static MutinyLevel1VerificationResult Run()
        {
            var result = new MutinyLevel1VerificationResult();
            Verify(result);
            return result;
        }

        public static void Verify(MutinyLevel1VerificationResult result)
        {
            VerifyBananaStops(result);
            VerifyEquippedOrigins(result);
        }

        private static void VerifyBananaStops(MutinyLevel1VerificationResult result)
        {
            var host = new GameObject("AI-PHY-04_Fixture");
            MutinyBanana actual = null;
            try
            {
                MutinyTeam team = host.AddComponent<MutinyTeam>();
                team.TeamNumber = 2;
                team.IsAiControlled = true;
                MutinyCharacter owner = AddCharacter(host, "Owner", 2, new Vector2(1000f, 1000f));
                team.RegisterCharacter(owner);
                MutinyCharacter target = AddCharacter(host, "Target", 1, new Vector2(20f, 0f));
                PhysicsBodyState state = MutinyAIController.CreateWeaponSimulationForVerification(
                    Vector2.zero, "banana", Vector2.right);
                state.Weight = 0f; // Isolated constant-speed board for strict distance boundaries.
                Vector2 impact = MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out int steps);
                result.Assert(steps == 1 && impact == Vector2.right,
                    "AI-PHY-04 Banana stops after motion at strictly less than 20px from enemy");
                target.TeamIndex = 2;
                team.RegisterCharacter(target);
                impact = MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                result.Assert(steps == 1 && impact == Vector2.right,
                    "AI-PHY-04 Banana includes allies in immediate proximity termination");
                target.TakeDamage(1000f);
                impact = MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                result.Assert(!target.IsAlive && team.Characters.Contains(target) && steps == 1,
                    "AI-PHY-04 Banana includes dead characters retained in the team");
                target.PhysicsBody.State.X = 21f;
                MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                result.Assert(steps == 2,
                    "AI-PHY-04 exactly 20px does not terminate, next tick at 19px does");
                target.PhysicsBody.State.X = -20f;
                impact = MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                result.Assert(steps == 101 && impact.x == 101f,
                    "AI-PHY-04 moving away within 50px does not invent a last-distance history rule");
                state.VelocityX = 0f;
                MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                result.Assert(steps == 1, "AI-PHY-04 Banana retains its original at-rest termination");

                owner.PhysicsBody.State = PhysicsBodyState.CreateDefault(128f, 128f);
                actual = MutinyWeaponFactory.SpawnWeapon("banana", owner) as MutinyBanana;
                actual.PhysicsBody.SetTerrain(null, 0, 0);
                state = actual.PhysicsBody.State;
                state.VelocityX = 1f;
                state.VelocityY = 0f;
                impact = MutinyAIController.SimulateWeaponImpactForVerification(
                    state, "banana", null, 0, 0, float.PositiveInfinity, out steps);
                actual.Fire(Vector2.right);
                actual.PhysicsBody.AdvanceSimulationTick();
                result.Assert(steps == 1 && actual.IsFinished &&
                              Vector2.Distance(impact, new Vector2(actual.PhysicsBody.State.X, actual.PhysicsBody.State.Y)) < 0.001f,
                    "AI-PHY-04 prediction includes owner and matches real Banana Fire/physics/detonation position");
            }
            finally
            {
                if (actual != null) Object.DestroyImmediate(actual.gameObject);
                foreach (MutinyExplosion explosion in Object.FindObjectsByType<MutinyExplosion>())
                    if (explosion.Caster != null && explosion.Caster.transform.IsChildOf(host.transform))
                        Object.DestroyImmediate(explosion.gameObject);
                Object.DestroyImmediate(host);
            }
        }

        private static void VerifyEquippedOrigins(MutinyLevel1VerificationResult result)
        {
            var host = new GameObject("AI-PHY-05_Fixture");
            int savedForce = MutinyAIController.ForcedWeaponId;
            Random.State savedRandom = Random.state;
            try
            {
                MutinyBoxRegistry.ResetForLevel();
                MutinyTeam team = host.AddComponent<MutinyTeam>();
                team.TeamNumber = 2;
                team.IsAiControlled = true;
                MutinyCharacter owner = AddCharacter(host, "Owner", 2, new Vector2(128f, 160f));
                owner.Luck = 1f;
                team.RegisterCharacter(owner);
                MutinyCharacter target = AddCharacter(host, "Enemy", 1, new Vector2(400f, 160f));
                var enemies = new List<MutinyCharacter> { target };
                var allies = new List<MutinyCharacter> { owner };
                MutinyAIController ai = host.AddComponent<MutinyAIController>();
                ai.SaveDecisionTrace = false;
                string[,] terrain = new string[12, 16];
                for (int x = 0; x < 16; x++) terrain[6, x] = "ground";
                terrain[3, 3] = "ground";
                owner.PhysicsBody.SetTerrain(terrain, 16, 12);
                var obstacle = new GameObject("Box");
                obstacle.transform.SetParent(host.transform);
                MutinyPhysicsBody box = obstacle.AddComponent<MutinyPhysicsBody>();
                box.State = PhysicsBodyState.CreateDefault(168f, 156f);
                box.State.LeftExtent = box.State.RightExtent = box.State.TopExtent = box.State.BottomExtent = 16f;
                MutinyBoxRegistry.Register(box);

                string[] types = { "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle", "banana", "parachuteBomb", "mine" };
                int[] ids = { 1, 2, 3, 4, 5, 6, 7, 11 };
                bool catchesCenterBug = false;
                for (int i = 0; i < types.Length; i++)
                {
                    MutinyAIController.TrySetForcedWeaponId(ids[i]);
                    Random.InitState(731 + i);
                    AIMove move = ai.EvaluateCharacterWeaponsForVerification(owner, enemies, allies,
                        terrain, 16, 12, float.PositiveInfinity, out int count);
                    PhysicsBodyState equipped = MutinyAIController.CreateFormalWeaponPredictionTemplateForVerification(owner, types[i]);
                    bool equipCorrect = equipped.X == 128f && equipped.Y == (types[i] == "boulder" ? 130f : 150f);
                    equipped.VelocityX = move.LaunchVelocity.x;
                    equipped.VelocityY = move.LaunchVelocity.y;
                    Vector2 expected = MutinyAIController.SimulateWeaponImpactForVerification(equipped, types[i],
                        terrain, 16, 12, float.PositiveInfinity, out _);
                    PhysicsBodyState centered = equipped;
                    centered.X = owner.PhysicsBody.State.X;
                    centered.Y = owner.PhysicsBody.State.Y;
                    Vector2 oldImpact = MutinyAIController.SimulateWeaponImpactForVerification(centered, types[i],
                        terrain, 16, 12, float.PositiveInfinity, out _);
                    catchesCenterBug |= Vector2.Distance(expected, oldImpact) > 0.01f;
                    result.Assert(equipCorrect && count == 1 && move.WeaponType == types[i] &&
                                  Vector2.Distance(expected, move.TargetPosition) < 0.001f,
                        "AI-PHY-05 production " + types[i] + " candidate preserves formal equip origin near terrain/box");
                }
                result.Assert(catchesCenterBug,
                    "AI-PHY-05 near-obstacle fixture distinguishes formal equip origin from former character-center origin");
                // Also cover Boulder/Banana in open space: obstacles can make
                // both bad/good starts converge to the same endpoint by chance.
                MutinyBoxRegistry.ResetForLevel();
                owner.PhysicsBody.SetTerrain(null, 0, 0);
                foreach (int index in new[] { 1, 5 })
                {
                    MutinyAIController.TrySetForcedWeaponId(ids[index]);
                    Random.InitState(812 + index);
                    AIMove move = ai.EvaluateCharacterWeaponsForVerification(owner, enemies, allies,
                        null, 0, 0, float.PositiveInfinity, out int count);
                    PhysicsBodyState equipped = MutinyAIController.CreateFormalWeaponPredictionTemplateForVerification(owner, types[index]);
                    equipped.VelocityX = move.LaunchVelocity.x;
                    equipped.VelocityY = move.LaunchVelocity.y;
                    Vector2 expected = MutinyAIController.SimulateWeaponImpactForVerification(equipped, types[index],
                        null, 0, 0, float.PositiveInfinity, out _);
                    PhysicsBodyState centered = equipped;
                    centered.Y = owner.PhysicsBody.State.Y;
                    Vector2 oldImpact = MutinyAIController.SimulateWeaponImpactForVerification(centered, types[index],
                        null, 0, 0, float.PositiveInfinity, out _);
                    result.Assert(count == 1 && Vector2.Distance(expected, oldImpact) > 0.01f &&
                                  Vector2.Distance(expected, move.TargetPosition) < 0.001f,
                        "AI-PHY-05 open-space " + types[index] + " candidate detects center-origin regression");
                }
            }
            finally
            {
                MutinyBoxRegistry.ResetForLevel();
                MutinyAIController.TrySetForcedWeaponId(savedForce);
                Random.state = savedRandom;
                Object.DestroyImmediate(host);
            }
        }

        private static MutinyCharacter AddCharacter(GameObject host, string name, int team, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(host.transform);
            MutinyCharacter character = go.AddComponent<MutinyCharacter>();
            character.TeamIndex = team;
            character.PhysicsBody.State = PhysicsBodyState.CreateDefault(position.x, position.y);
            character.PhysicsBody.SetTerrain(null, 0, 0);
            return character;
        }
    }
}
