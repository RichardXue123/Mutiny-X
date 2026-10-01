using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    public sealed partial class MutinyAIController
    {
        private IEnumerable<object> EvaluateWeaponSteps(MutinyCharacter shooter, string weaponType, DecisionWork work)
        {
            string weapon = weaponType.ToLowerInvariant();
            int samples = Mathf.FloorToInt(work.EffectiveLucks[shooter] * work.Snapshot.TeamCharacterCount /
                Mathf.Max(1, work.Snapshot.AliveCount));
            if (work.Enhanced != null) samples = Mathf.Clamp(samples, 4, EnhancedMaxWeaponSamples);
            if (work.Enhanced != null && weapon == "piecesofeight")
            {
                // A first-shot distance score cannot gate an eight-shot weapon.
                // Its one primary rollout owns the per-coin searches itself.
                ConsiderCandidate(ref work.Best, new AIMove { MoveType = AIMoveType.ShootWeapon,
                    Character = shooter, WeaponType = weaponType }, ref work.CandidateCount, work);
                yield return null;
                yield break;
            }
            IEnumerable<object> special = null;
            switch (weapon)
            {
                case "tidalwave":
                    ConsiderCandidate(ref work.Best, new AIMove { MoveType = AIMoveType.ShootWeapon,
                        Character = shooter, WeaponType = weaponType,
                        Score = ScoreTidalWave(work.Enemies, work.Allies, work.WaterY, work) }, ref work.CandidateCount, work);
                    yield return null;
                    break;
                case "voodoodoll": special = EvaluateVoodooSteps(shooter, work.Enemies, work.Terrain, work.GridW, work.GridH, work.WaterY, work); break;
                case "seagull": special = EvaluateSeagullSteps(shooter, work.Enemies, work.Allies, work.Terrain, work.GridW, work.GridH, work.WaterY, work); break;
                case "woodencrate":
                case "gunpowderbarrel": special = EvaluateBoxWeaponSteps(shooter, weaponType, work); break;
                case "anchor": special = EvaluateAnchorSteps(shooter, work.Enemies, work.Allies, work.Terrain, work.GridW, work.GridH, work.WaterY, samples, work); break;
                case "cannon": special = EvaluateCannonSteps(shooter, work.Enemies, work.Allies, work.Terrain, work.GridW, work.GridH, work.WaterY, samples, work); break;
                default:
                    if (!MutinyWeaponFactoryCanFire(weaponType)) yield break;
                    PhysicsBodyState template = work.Enhanced != null
                        ? MutinyAIEffectWorld.ProjectileBody(weapon, StateOf(shooter, work).Body)
                        : CreateFormalWeaponPredictionTemplate(shooter, weaponType);
                    yield return null;
                    for (int sample = 0; sample < samples; sample++)
                    {
                        Vector2 velocity = RandomArc(MutinyWeaponFactory.GetTwangMaxForce(weaponType));
                        PhysicsBodyState body = template;
                        body.VelocityX = velocity.x;
                        body.VelocityY = velocity.y;
                        var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Weapon, body, weaponType,
                            work.Terrain, work.GridW, work.GridH, work.WaterY, work.Snapshot.Boxes, work.Snapshot.BananaPositions);
                        while (prediction.Advance()) yield return null;
                        ConsiderCandidate(ref work.Best, new AIMove
                        {
                            MoveType = AIMoveType.ShootWeapon, Character = shooter, WeaponType = weaponType,
                            LaunchVelocity = velocity, TargetPosition = prediction.Impact,
                            Score = ScoreGenericWeaponCandidate(weaponType, prediction.Impact, work.Enemies, work.Allies, work)
                        }, ref work.CandidateCount, work);
                        yield return null;
                    }
                    break;
            }
            if (special != null) foreach (object step in special) yield return null;
            if (work.Best.MoveType == AIMoveType.ShootWeapon &&
                string.Equals(work.Best.WeaponType, work.ForcedWeaponType, StringComparison.OrdinalIgnoreCase))
                work.Best.UsesForcedWeaponSupply = true;
        }

        private IEnumerable<object> EvaluateBoxWeaponSteps(MutinyCharacter shooter, string weaponType, DecisionWork work)
        {
            if (work.Terrain == null || work.GridW <= 0 || work.GridH <= 0) yield break;
            float averageX = 0;
            int allyCount = 0;
            foreach (var ally in work.Allies)
                if (StateOf(ally, work).Alive) { averageX += PositionOf(ally, work).x; allyCount++; }
            if (allyCount == 0) yield break;
            averageX /= allyCount;
            var enemies = new List<MutinyCharacter>();
            foreach (var enemy in work.Enemies) if (StateOf(enemy, work).Alive) enemies.Add(enemy);
            if (enemies.Count == 0) yield break;
            var possibilities = new List<Vector2>(10);
            for (int sample = 0; sample < 10; sample++)
            {
                Vector2 target = PositionOf(enemies[NextRandomInt(0, enemies.Count)], work);
                float sign = Mathf.Approximately(averageX, target.x) ? 0f : Mathf.Sign(averageX - target.x);
                float offsetX = sign * NextRandomInt(16, 64);
                float offsetY = NextRandomInt(-50, 50);
                Vector2 candidate = new Vector2(target.x + offsetX, target.y + offsetY);
                if (MutinyBoxPlacementRules.CanPlace(candidate, work.Terrain, work.GridW, work.GridH,
                    work.Snapshot.Boxes, work.Snapshot.ChestPositions, work.Snapshot.LivingBodies))
                    possibilities.Add(candidate);
                yield return null;
            }
            if (possibilities.Count >= 3)
                ConsiderCandidate(ref work.Best, new AIMove
                {
                    MoveType = AIMoveType.ShootWeapon, Character = shooter, WeaponType = weaponType,
                    Score = NextRandomFloat(0f, 1f), BoxPossibilities = possibilities.ToArray()
                }, ref work.CandidateCount, work);
            yield return null;
        }

        private IEnumerable<object> EvaluateAnchorSteps(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            int samples,
            DecisionWork work)
        {
            // Anchor.randomThrows: choose an x across the whole level, start at
            // y=-200 and fall vertically at 40 px/tick. Only floor contacts become
            // candidates; water misses are discarded before generic scoring.
            if (terrainGrid == null || gridW <= 0 || gridH <= 0 || float.IsInfinity(waterPixelY))
                yield break;

            int levelWidthPixels = gridW * (int)MutinyPhysics.PixelsPerUnit;
            List<PhysicsBoxObstacle> boxes = work.Snapshot.Boxes;
            for (int sample = 0; sample < Mathf.Max(0, samples); sample++)
            {
                int targetX = NextRandomInt(0, levelWidthPixels);
                PhysicsBodyState body = PhysicsBodyState.CreateDefault(targetX, MutinyAnchor.DropStartYPixels);
                body.Weight = 0f;
                body.LeftExtent = body.RightExtent = 48f;
                body.TopExtent = 96f;
                body.BottomExtent = 0f;
                body.HitsBoxes = true;

                var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Anchor, body, null,
                    terrainGrid, gridW, gridH, waterPixelY, boxes);
                while (prediction.Advance()) yield return null;
                if (!prediction.HitFloor) { yield return null; continue; }
                body = prediction.Body;

                Vector2 landing = new Vector2(body.X, body.Y);
                float score = ScoreGenericWeaponCandidate("anchor", landing, enemies, allies, work);
                ConsiderCandidate(ref work.Best, new AIMove
                {
                    MoveType = AIMoveType.ShootWeapon,
                    Character = shooter,
                    WeaponType = "anchor",
                    TargetPosition = landing,
                    Score = score
                }, ref work.CandidateCount, work);
                yield return null;
            }
        }

        private IEnumerable<object> EvaluateCannonSteps(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            int samples,
            DecisionWork work)
        {
            // Cannon.randomThrows: choose a point within dragRange / 2 about
            // owner+(0,-100), advance the 10px cannon body through Solid collision,
            // then simulate a 30-force weight-zero cannonball from the corrected spot.
            // Cannon.aiPerform later repeats the same collision-aware body move.
            Vector2 owner = PositionOf(shooter, work);
            List<PhysicsBoxObstacle> placementBoxes = work.Snapshot.Boxes;
            int count = Mathf.Max(0, samples);
            for (int i = 0; i < count; i++)
            {
                int placementAngle = NextRandomInt(0, 360);
                float distance = NextRandomFloat(0f, 1f) * 65f; // Weapon.dragRange / 2
                float placementRadians = placementAngle * Mathf.Deg2Rad;
                Vector2 requestedPlacement = owner + new Vector2(
                    Mathf.Cos(placementRadians) * distance,
                    MutinyCannon.PlacementOffsetY + Mathf.Sin(placementRadians) * distance);

                // Cannon.randomThrows samples a second independent angle after
                // resolving placement. The placement direction does not aim the shot.
                int firingAngle = NextRandomInt(0, 360);
                float firingRadians = firingAngle * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(firingRadians), Mathf.Sin(firingRadians)) * MutinyCannon.FireStrength;

                PhysicsBodyState cannonBody = PhysicsBodyState.CreateDefault(
                    owner.x, owner.y + MutinyCannon.InitialEquipmentOffsetY);
                cannonBody.Weight = 0f;
                cannonBody.HitsBoxes = true;
                cannonBody.LeftExtent = cannonBody.RightExtent =
                    cannonBody.TopExtent = cannonBody.BottomExtent = 10f;
                cannonBody.VelocityX = requestedPlacement.x - cannonBody.X;
                cannonBody.VelocityY = requestedPlacement.y - cannonBody.Y;
                MutinyPhysics.Step(ref cannonBody, terrainGrid, gridW, gridH, placementBoxes);
                yield return null;
                Vector2 resolvedPlacement = new Vector2(cannonBody.X, cannonBody.Y);

                PhysicsBodyState ball = PhysicsBodyState.CreateDefault(resolvedPlacement.x, resolvedPlacement.y);
                ball.Weight = 0f;
                ball.HitsBoxes = true;
                ball.LeftExtent = ball.RightExtent = ball.TopExtent = ball.BottomExtent = 10f;
                ball.VelocityX = velocity.x;
                ball.VelocityY = velocity.y;
                var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Weapon, ball, "cannonball",
                    terrainGrid, gridW, gridH, waterPixelY, placementBoxes);
                while (prediction.Advance()) yield return null;
                Vector2 impact = prediction.Impact;
                float score = ScoreGenericWeaponCandidate("cannon", impact, enemies, allies, work);
                ConsiderCandidate(ref work.Best, new AIMove
                {
                    MoveType = AIMoveType.ShootWeapon,
                    Character = shooter,
                    WeaponType = "cannon",
                    LaunchVelocity = velocity,
                    // aiPerform receives the requested point and resolves it through
                    // the live cannon body against the current terrain/boxes again.
                    TargetPosition = requestedPlacement,
                    CannonRotationDegrees = firingAngle,
                    Score = score
                }, ref work.CandidateCount, work);
                yield return null;
            }
        }

        private IEnumerable<object> EvaluateSeagullSteps(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            DecisionWork work)
        {
            // Seagull.aiSimulation: fly 100..199 px above the highest living
            // enemy, sample ten sorted x positions, then keep only positive shots.
            float highestEnemyY = float.PositiveInfinity;
            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy != null && StateOf(enemy, work).Alive)
                    highestEnemyY = Mathf.Min(highestEnemyY, StateOf(enemy, work).Body.Y);
            }
            if (float.IsInfinity(highestEnemyY))
                yield break;

            float flightY = highestEnemyY - 100f - NextRandomInt(0, 100);
            int levelWidthPixels = gridW * (int)MutinyPhysics.PixelsPerUnit;
            var candidates = new List<float>(10);
            for (int i = 0; i < 10; i++)
                candidates.Add(NextRandomInt(0, levelWidthPixels));
            candidates.Sort();

            var acceptedShots = new List<float>();
            float success = 0f;
            List<PhysicsBoxObstacle> simulationBoxes = work.Snapshot.Boxes;
            for (int i = 0; i < candidates.Count; i++)
            {
                PhysicsBodyState body = PhysicsBodyState.CreateDefault(
                    candidates[i] + MutinySeagull.OriginalShotXOffset, flightY);
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = MutinySeagullFire.OriginalExtent;
                body.Weight = MutinySeagullFire.OriginalWeight;
                body.GravityScale = StateOf(shooter, work).Body.EffectiveGravityScale;
                body.VelocityX = MutinySeagull.OriginalFlightSpeed;
                body.HitsBoxes = true;
                var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Seagull, body, null,
                    terrainGrid, gridW, gridH, waterPixelY, simulationBoxes);
                while (prediction.Advance()) yield return null;
                Vector2 impact = prediction.Impact;
                float shotScore = ScoreSeagullShot(impact, enemies, false, work) +
                                  ScoreSeagullShot(impact, allies, true, work);
                if (shotScore > 0f)
                {
                    acceptedShots.Add(candidates[i]);
                    success += shotScore;
                }
            }

            // Character.as only creates an AI move when aiSimulation returned
            // more than one accepted firing coordinate.
            if (acceptedShots.Count <= 1)
                yield break;

            ConsiderCandidate(ref work.Best, new AIMove
            {
                MoveType = AIMoveType.ShootWeapon,
                Character = shooter,
                WeaponType = "seagull",
                Score = success,
                SeagullFlightY = flightY,
                SeagullShotXs = acceptedShots.ToArray()
            }, ref work.CandidateCount, work);
            yield return null;
        }

        private IEnumerable<object> EvaluateVoodooSteps(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            DecisionWork work)
        {
            List<PhysicsBoxObstacle> simulationBoxes = work.Snapshot.Boxes;
            for (int enemyIndex = 0; enemyIndex < enemies.Count; enemyIndex++)
            {
                MutinyCharacter enemy = enemies[enemyIndex];
                if (enemy == null || !StateOf(enemy, work).Alive)
                    continue;
                Vector2 start = PositionOf(enemy, work);
                var velocities = new Vector2[2];
                var landings = new Vector2[2];
                // Character.randomThrows(2) generates both trajectories before
                // Character.aiThink draws either success perturbation.
                for (int sample = 0; sample < 2; sample++)
                {
                    velocities[sample] = RandomArc(20f);
                    PhysicsBodyState body = StateOf(enemy, work).Body;
                    body.X = start.x;
                    body.Y = start.y;
                    body.VelocityX = velocities[sample].x;
                    body.VelocityY = velocities[sample].y;
                    body.HitsBoxes = true;
                    var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Character, body, null,
                        terrainGrid, gridW, gridH, waterPixelY, simulationBoxes);
                    while (prediction.Advance()) yield return null;
                    landings[sample] = prediction.Impact;
                    yield return null;
                }
                for (int sample = 0; sample < 2; sample++)
                {
                    Vector2 landing = landings[sample];
                    float score = landing.y >= waterPixelY
                        ? 1f + NextRandomFloat(0f, 0.2f)
                        : NextRandomFloat(0f, 0.2f) - 0.5f;
                    ConsiderCandidate(ref work.Best, new AIMove
                    {
                        MoveType = AIMoveType.ShootWeapon,
                        Character = shooter,
                        WeaponType = "voodooDoll",
                        LaunchVelocity = velocities[sample],
                        TargetCharacter = enemy,
                        Score = score
                    }, ref work.CandidateCount, work);
                yield return null;
                }
            }
        }

    }
}

