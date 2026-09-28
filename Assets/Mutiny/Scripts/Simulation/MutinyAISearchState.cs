using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    public sealed partial class MutinyAIController
    {
        public const float DefaultDecisionBudgetMilliseconds = 3f;
        [Tooltip("Maximum AI search work per rendered frame (soft budget, checked between physics ticks).")]
        [Min(0.1f)] public float DecisionBudgetMilliseconds = DefaultDecisionBudgetMilliseconds;
        public int EvaluatedCandidateCount => m_ActiveWork?.CandidateCount ?? 0;
        public double LastSearchSliceMilliseconds { get; private set; }
        public double MaximumSearchSliceMilliseconds { get; private set; }
        public int SearchSliceCount { get; private set; }
        public int LastDecisionCandidateCount { get; private set; }
        public string LastDecisionTracePath { get; private set; }
        public string CurrentDecisionTracePath => m_ActiveWork?.TracePath;
        public int InvalidatedDecisionCount { get; private set; }
        public string LastDecisionInvalidationReason { get; private set; }
        private DecisionWork m_ActiveWork;
        private readonly List<MutinyAITraceWriter> m_PendingTraceWrites = new List<MutinyAITraceWriter>();

        private sealed class CharacterSnapshot
        {
            public MutinyCharacter Character;
            public PhysicsBodyState Body;
            public bool Alive, CanThrow, CanShoot;
            public int TeamIndex;
            public float Health, MaxHealth, Evilness, Luck;
            public KeyValuePair<string, int>[] Weapons;
            public bool HasWeapon(string weapon)
            {
                foreach (var entry in Weapons)
                    if (entry.Value > 0 && (string.Equals(entry.Key, weapon, StringComparison.OrdinalIgnoreCase) ||
                        weapon.Equals("cannon", StringComparison.OrdinalIgnoreCase) && entry.Key.Equals("cannonball", StringComparison.OrdinalIgnoreCase) ||
                        weapon.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && entry.Key.Equals("cannon", StringComparison.OrdinalIgnoreCase))) return true;
                return false;
            }
        }

        private sealed class ChestSnapshot
        {
            public MutinyTreasureChest Chest;
            public float X, Y, FloorY;
            public bool Finished;
        }

        private sealed class DecisionSnapshot
        {
            public readonly Dictionary<MutinyCharacter, CharacterSnapshot> Characters = new Dictionary<MutinyCharacter, CharacterSnapshot>();
            public readonly List<PhysicsBoxObstacle> Boxes = MutinyBoxRegistry.GetObstacles();
            public readonly List<ChestSnapshot> Chests = new List<ChestSnapshot>();
            public readonly List<Vector2> BananaPositions = new List<Vector2>();
            public readonly List<PhysicsBodyState> LivingBodies = new List<PhysicsBodyState>();
            public readonly List<Vector2> ChestPositions = new List<Vector2>();
            public MutinyCharacter Selected;
            public int TurnNumber, TeamCharacterCount, AliveCount;
            public MutinyCharacter[] TeamCharacters;
            public MutinyLevelController LevelController;
            public TextAsset LevelXml;
            public MutinyLevelRoot LevelRoot;
            public float WaterLevel;

            public DecisionSnapshot(MutinyAIController ai, List<MutinyCharacter> enemies, List<MutinyCharacter> allies)
            {
                Selected = ai.m_Team.SelectedCharacter;
                TurnNumber = ai.m_TurnManager != null ? ai.m_TurnManager.TurnCount : 0;
                TeamCharacters = ai.m_Team.Characters.ToArray();
                TeamCharacterCount = TeamCharacters.Length;
                AliveCount = ai.m_Team.AliveCount;
                foreach (MutinyCharacter character in UnityEngine.Object.FindObjectsByType<MutinyCharacter>())
                {
                    if (character == null || character.PhysicsBody == null) continue;
                    var weapons = new KeyValuePair<string, int>[character.WeaponInventory.Count];
                    int index = 0;
                    foreach (var entry in character.WeaponInventory) weapons[index++] = entry;
                    var state = new CharacterSnapshot
                    {
                        Character = character, Body = character.PhysicsBody.State, Alive = character.IsAlive,
                        CanThrow = character.CanThrow, CanShoot = character.CanShoot, Health = character.Health,
                        MaxHealth = character.MaxHealth, Evilness = character.Evilness, TeamIndex = character.TeamIndex,
                        Luck = ai.GetEffectiveLuck(character), Weapons = weapons
                    };
                    Characters.Add(character, state);
                    if (state.Alive) LivingBodies.Add(state.Body);
                }
                // Retain the original allies-then-enemies order and include dead entries.
                foreach (var character in allies) BananaPositions.Add(PositionOf(character));
                foreach (var character in enemies) BananaPositions.Add(PositionOf(character));
                foreach (var chest in UnityEngine.Object.FindObjectsByType<MutinyTreasureChest>())
                {
                    if (chest == null) continue;
                    Chests.Add(new ChestSnapshot { Chest = chest, X = chest.PixelX, Y = chest.PixelY,
                        FloorY = chest.FloorPixelY, Finished = chest.IsFinished });
                    ChestPositions.Add(new Vector2(chest.PixelX, chest.PixelY));
                }
                LevelController = UnityEngine.Object.FindAnyObjectByType<MutinyLevelController>();
                if (LevelController != null) LevelXml = LevelController.LevelXml;
                LevelRoot = UnityEngine.Object.FindAnyObjectByType<MutinyLevelRoot>();
                if (LevelRoot != null) WaterLevel = LevelRoot.WaterLevelY;
            }

            public bool Matches(MutinyAIController ai, out string reason, bool ignoreSelection = false)
            {
                reason = null;
                if (ai.m_TurnManager != null && ai.m_TurnManager.TurnCount != TurnNumber)
                    return Reject("turn-number", out reason);
                if (!ignoreSelection && ai.m_Team.SelectedCharacter != Selected)
                    return Reject("selected-character", out reason);
                if (ai.m_Team.Characters.Count != TeamCharacters.Length)
                    return Reject("team-character-count", out reason);
                for (int i = 0; i < TeamCharacters.Length; i++)
                    if (ai.m_Team.Characters[i] != TeamCharacters[i]) return Reject("team-character-order", out reason);
                if (UnityEngine.Object.FindAnyObjectByType<MutinyLevelController>() != LevelController ||
                    UnityEngine.Object.FindAnyObjectByType<MutinyLevelRoot>() != LevelRoot)
                    return Reject("level-identity", out reason);
                if (LevelController != null && LevelController.LevelXml != LevelXml)
                    return Reject("level-xml", out reason);
                if (LevelRoot != null && LevelRoot.WaterLevelY != WaterLevel)
                    return Reject("water-level", out reason);
                int characters = 0;
                foreach (var character in UnityEngine.Object.FindObjectsByType<MutinyCharacter>())
                {
                    if (character == null || character.PhysicsBody == null) continue;
                    characters++;
                    if (!Characters.TryGetValue(character, out var saved))
                        return Reject("character-added:" + character.name, out reason);
                    if (character.IsAlive != saved.Alive)
                        return Reject("character-alive:" + character.name + ":" + saved.Alive + "->" + character.IsAlive, out reason);
                    if (character.TeamIndex != saved.TeamIndex ||
                        character.CanThrow != saved.CanThrow || character.CanShoot != saved.CanShoot ||
                        character.Health != saved.Health || character.MaxHealth != saved.MaxHealth ||
                        character.Evilness != saved.Evilness)
                        return Reject("character-state:" + character.name, out reason);
                    if (ai.GetEffectiveLuck(character) != saved.Luck)
                        return Reject("character-luck:" + character.name + ":" + Number(saved.Luck) + "->" + Number(ai.GetEffectiveLuck(character)), out reason);
                    // Corpses continue 25 Hz physics (including indefinite water sinking),
                    // while the production turn-settlement gate deliberately skips them.
                    // Retain their frozen positions for centroid/Banana scoring; only
                    // an alive-state transition, not passive corpse motion, invalidates.
                    if (saved.Alive && !SamePhysics(character.PhysicsBody.State, saved.Body))
                        return Reject("character-physics:" + character.name + ":" + PhysicsDifference(saved.Body, character.PhysicsBody.State), out reason);
                    if (character.WeaponInventory.Count != saved.Weapons.Length)
                        return Reject("character-inventory-count:" + character.name, out reason);
                    int index = 0;
                    foreach (var entry in character.WeaponInventory)
                    {
                        var expected = saved.Weapons[index++];
                        if (entry.Key != expected.Key || entry.Value != expected.Value)
                            return Reject("character-inventory:" + character.name + ":" + entry.Key, out reason);
                    }
                }
                if (characters != Characters.Count) return Reject("character-removed", out reason);
                var boxes = MutinyBoxRegistry.GetObstacles();
                if (boxes.Count != Boxes.Count) return Reject("box-count", out reason);
                for (int i = 0; i < boxes.Count; i++)
                {
                    if (boxes[i].Body != Boxes[i].Body) return Reject("box-identity:" + i, out reason);
                    if (!SamePhysics(boxes[i].State, Boxes[i].State))
                        return Reject("box-physics:" + i + ":" + PhysicsDifference(Boxes[i].State, boxes[i].State), out reason);
                }
                int activeChests = 0, expectedChests = 0;
                foreach (var chest in UnityEngine.Object.FindObjectsByType<MutinyTreasureChest>())
                {
                    if (chest == null || chest.IsFinished) continue;
                    activeChests++;
                    bool found = false;
                    foreach (var saved in Chests)
                        if (saved.Chest == chest && !saved.Finished && saved.X == chest.PixelX && saved.FloorY == chest.FloorPixelY)
                        { found = true; break; }
                    if (!found) return Reject("chest-state:" + chest.name, out reason);
                }
                foreach (var saved in Chests) if (!saved.Finished) expectedChests++;
                // Descent and visual fading alone do not invalidate a decision.
                return activeChests == expectedChests || Reject("chest-removed-or-finished", out reason);
            }

            private static bool Reject(string value, out string reason) { reason = value; return false; }
        }

        private static string PhysicsDifference(PhysicsBodyState before, PhysicsBodyState after)
        {
            var text = new System.Text.StringBuilder();
            AddPhysicsDifference(text, "x", before.X, after.X);
            AddPhysicsDifference(text, "y", before.Y, after.Y);
            AddPhysicsDifference(text, "vx", before.VelocityX, after.VelocityX);
            AddPhysicsDifference(text, "vy", before.VelocityY, after.VelocityY);
            AddPhysicsDifference(text, "weight", before.Weight, after.Weight);
            AddPhysicsDifference(text, "bounce", before.Bounce, after.Bounce);
            AddPhysicsDifference(text, "friction", before.Friction, after.Friction);
            AddPhysicsDifference(text, "left", before.LeftExtent, after.LeftExtent);
            AddPhysicsDifference(text, "right", before.RightExtent, after.RightExtent);
            AddPhysicsDifference(text, "top", before.TopExtent, after.TopExtent);
            AddPhysicsDifference(text, "bottom", before.BottomExtent, after.BottomExtent);
            if (before.HitsTiles != after.HitsTiles) text.Append(" hitsTiles=").Append(before.HitsTiles).Append("->").Append(after.HitsTiles);
            if (before.HitsBoxes != after.HitsBoxes) text.Append(" hitsBoxes=").Append(before.HitsBoxes).Append("->").Append(after.HitsBoxes);
            return text.ToString();
        }

        private static void AddPhysicsDifference(System.Text.StringBuilder text, string field, float before, float after)
        {
            if (before != after) text.Append(' ').Append(field).Append('=').Append(Number(before)).Append("->").Append(Number(after));
        }

        private static bool SamePhysics(PhysicsBodyState a, PhysicsBodyState b) =>
            a.X == b.X && a.Y == b.Y && a.VelocityX == b.VelocityX && a.VelocityY == b.VelocityY &&
            a.Weight == b.Weight && a.Bounce == b.Bounce && a.Friction == b.Friction &&
            a.LeftExtent == b.LeftExtent && a.RightExtent == b.RightExtent &&
            a.TopExtent == b.TopExtent && a.BottomExtent == b.BottomExtent &&
            a.HitsTiles == b.HitsTiles && a.HitsBoxes == b.HitsBoxes;

        private static CharacterSnapshot StateOf(MutinyCharacter character, DecisionWork work) => work.Snapshot.Characters[character];
        private static Vector2 PositionOf(MutinyCharacter character, DecisionWork work) =>
            new Vector2(StateOf(character, work).Body.X, StateOf(character, work).Body.Y);
        private static bool HasEffectiveWeapon(MutinyCharacter character, string weapon, DecisionWork work) =>
            work.ForcedWeaponType != null ? string.Equals(weapon, work.ForcedWeaponType, StringComparison.OrdinalIgnoreCase) :
                StateOf(character, work).HasWeapon(weapon);

        private static IEnumerable<string> WeaponChoices(MutinyCharacter character, DecisionWork work)
        {
            if (work.ForcedWeaponType != null) { yield return work.ForcedWeaponType; yield break; }
            foreach (var entry in StateOf(character, work).Weapons) yield return entry.Key;
        }

        private void CancelDecisionWork()
        {
            IsEvaluatingCandidates = false;
            if (m_ActiveWork != null)
            {
                (m_ActiveWork.Steps as IDisposable)?.Dispose();
                m_ActiveWork.Writer?.Abort();
                m_ActiveWork = null;
            }
            m_Random = null;
        }

        private void CancelTurnRoutine()
        {
            if (m_TurnCoroutine != null) StopCoroutine(m_TurnCoroutine);
            m_TurnCoroutine = null;
            CancelDecisionWork();
        }

        private void PollTraceWrites()
        {
            for (int i = m_PendingTraceWrites.Count - 1; i >= 0; i--)
            {
                var writer = m_PendingTraceWrites[i];
                if (!writer.Completion.IsCompleted) continue;
                if (writer.Error != null) Mutiny.Diagnostics.MutinyDebugLog.Warning("AI-Decision", "cannot save trace: " + writer.Error.Message, this);
                else if (File.Exists(writer.Path)) Mutiny.Diagnostics.MutinyDebugLog.Info("AI-Decision", "trace ready=" + writer.Path, this);
                m_PendingTraceWrites.RemoveAt(i);
            }
        }

        private bool AdvanceDecisionSlice(DecisionWork work)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            double limit = float.IsNaN(DecisionBudgetMilliseconds) || float.IsInfinity(DecisionBudgetMilliseconds)
                ? DefaultDecisionBudgetMilliseconds : Math.Max(0.1, DecisionBudgetMilliseconds);
            bool complete = false;
            do
            {
                if (work.Writer != null && !work.Writer.CanAcceptRecords) break;
                if (!work.Steps.MoveNext()) { complete = true; break; }
                if (work.ForceNextFrame) { work.ForceNextFrame = false; break; }
            } while (timer.Elapsed.TotalMilliseconds < limit);
            LastSearchSliceMilliseconds = timer.Elapsed.TotalMilliseconds;
            MaximumSearchSliceMilliseconds = Math.Max(MaximumSearchSliceMilliseconds, LastSearchSliceMilliseconds);
            SearchSliceCount++;
            return complete;
        }

        // Runs the production budget pump, not a second evaluator. Real-turn
        // cancellation/execution are tested separately through Start/Update.
        internal IEnumerator EvaluateBestMoveBudgetedForVerification(Action<AIMove> completed, bool streaming = false)
        {
            var work = PrepareDecisionWork(streaming);
            m_ActiveWork = work;
            SearchSliceCount = 0;
            MaximumSearchSliceMilliseconds = 0;
            try
            {
                while (!AdvanceDecisionSlice(work)) yield return null;
                completed?.Invoke(FinishDecisionWork(work));
            }
            finally { CancelDecisionWork(); }
        }
    }
}
