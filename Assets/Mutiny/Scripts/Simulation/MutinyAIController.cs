using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    public enum AIMoveType
    {
        Pass,
        SelfThrow,
        ShootWeapon
    }

    public struct AIMove
    {
        public AIMoveType MoveType;
        public MutinyCharacter Character;
        public string WeaponType;
        public Vector2 LaunchVelocity;
        public Vector2 TargetPosition;
        public MutinyCharacter TargetCharacter;
        public float Score;
        public float SeagullFlightY;
        public int CannonRotationDegrees;
        public float[] SeagullShotXs;
        public Vector2[] BoxPossibilities;
        public bool UsesForcedWeaponSupply;
    }

    public enum MutinyAITurnGate
    {
        Cancel,
        Wait,
        Execute
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MutinyTeam))]
    public sealed partial class MutinyAIController : MonoBehaviour
    {
        public const float OriginalChestMoveRadiusPixels = 40f;
        public const float OriginalChestMoveBonus = 0.5f;

        // GM-only overlay. Keep the real per-character inventory untouched so
        // disabling the command restores the exact ammunition state.
        private static readonly string[] ForceableWeaponTypes =
        {
            "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle",
            "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull",
            "mine", "cannon", "anchor", "voodooDoll", "tidalWave"
        };

        public static int ForcedWeaponId { get; private set; }
        public static string ForcedWeaponType => ForcedWeaponId == 0 ? null : ForceableWeaponTypes[ForcedWeaponId - 1];
        public static bool ActionLogEnabled { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForcedWeaponOnPlayStart()
        {
            ForcedWeaponId = 0;
            ActionLogEnabled = false;
        }

        public static void SetActionLogEnabled(bool enabled) => ActionLogEnabled = enabled;

        public static bool TrySetForcedWeaponId(int weaponId)
        {
            if (weaponId < 0 || weaponId > ForceableWeaponTypes.Length)
                return false;
            ForcedWeaponId = weaponId;
            return true;
        }

        [Header("AI Settings")]
        [Tooltip("Retained for existing scenes. Flash uses exactly 50 character throw samples.")]
        public int TrajectorySamples = 50;

        [Header("AI Decision Replay")]
        public bool UseFixedDecisionSeed;
        public int FixedDecisionSeed;
        [Tooltip("Optional JSON trace created by a previous AI decision. Empty uses a fresh or fixed seed.")]
        public string ReplayDecisionTracePath;
        public bool SaveDecisionTrace = true;

        private MutinyTeam m_Team;
        private MutinyTurnManager m_TurnManager;
        private Coroutine m_TurnCoroutine;
        private bool m_LoggedMissingManager;
        private int m_DecisionSequence;
        private MutinyAIRandomStream m_Random;
        private MutinyAIDecisionTrace m_ReplayOverride;
        public MutinyAIDecisionTrace LastDecisionTrace { get; private set; }
        // GM-08 only: never mutate the level-authored per-character Luck.
        internal float? TakeoverLuckOverride { get; private set; }

        internal void SetTakeoverLuckOverride(float? luck) => TakeoverLuckOverride = luck;

        public const float MaxGmLuck = 99999f;
        public float? LevelLuckOverride { get; private set; }
        public static bool IsValidGmLuck(float luck) =>
            !float.IsNaN(luck) && !float.IsInfinity(luck) && luck >= 0f && luck <= MaxGmLuck;
        internal void SetLevelLuckOverride(float? luck) => LevelLuckOverride = luck;
        internal float GetEffectiveLuck(MutinyCharacter character) =>
            TakeoverLuckOverride ?? LevelLuckOverride ?? character.Luck;

        private sealed class DecisionWork
        {
            public int Id;
            public string Phase;
            public AIMove Best;
            public int CandidateCount;
            public string TracePath;
            public List<MutinyCharacter> Enemies;
            public List<MutinyCharacter> Allies;
            public string[,] Terrain;
            public int GridW;
            public int GridH;
            public float WaterY;
            public MutinyCharacter ContinuationCharacter;
            public IEnumerator Steps;
            public bool ForceNextFrame;
            public string ForcedWeaponType;
            public DecisionSnapshot Snapshot;
            public MutinyAITraceWriter Writer;
            public bool Streaming;
            public readonly Dictionary<string, float> BestByAction = new Dictionary<string, float>(StringComparer.Ordinal);
            public readonly List<string> ActionOrder = new List<string>();
            public readonly Dictionary<MutinyCharacter, float> EffectiveLucks = new Dictionary<MutinyCharacter, float>();
        }

        private struct SelfThrowSample
        {
            public Vector2 Velocity;
            public Vector2 Landing;
        }

        public bool IsEvaluatingCandidates { get; private set; }

        private void Awake()
        {
            m_Team = GetComponent<MutinyTeam>();
        }

        private void Start()
        {
            BindTurnManager();
            TryStartCurrentTurn("Start");
        }

        private void Update()
        {
            PollTraceWrites();
            if (m_TurnManager == null)
                BindTurnManager();

            if (m_TurnCoroutine == null &&
                ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Execute)
            {
                BeginTurnRoutine("watchdog");
            }
        }

        private void OnDestroy()
        {
            CancelTurnRoutine();
            if (m_TurnManager != null) m_TurnManager.OnTurnStarted -= HandleTurnStarted;
        }

        private void OnDisable()
        {
            // Disabling a MonoBehaviour alone does not stop its coroutines.
            CancelTurnRoutine();
        }

        private void HandleTurnStarted(MutinyTeam activeTeam)
        {
            MutinyDebugLog.Info("AI",
                $"turn event activeTeam={TeamLabel(activeTeam)} self={TeamLabel(m_Team)} phase={m_TurnManager?.CurrentPhase}", this);
            if (isActiveAndEnabled && activeTeam == m_Team && m_Team.IsAiControlled && !m_Team.IsDefeated)
                BeginTurnRoutine("turn event");
        }

        private IEnumerator ExecuteAITurnRoutine()
        {
            IsEvaluatingCandidates = true;
            // Always suspend once so StartCoroutine cannot finish before its handle
            // is assigned, including an invalid/no-candidate decision.
            yield return null;
            MutinyDebugLog.Info("AI", $"thinking started team={TeamLabel(m_Team)} budget={DecisionBudgetMilliseconds}ms/frame", this);
            while (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Wait) yield return null;
            if (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Cancel)
            { CancelDecisionWork(); m_TurnCoroutine = null; yield break; }

            DecisionWork work = null;
            Exception error = null;
            try { work = PrepareDecisionWork(streaming: true); m_ActiveWork = work; }
            catch (Exception exception) { error = exception; }
            MaximumSearchSliceMilliseconds = 0;
            SearchSliceCount = 0;
            bool complete = false;
            while (error == null && !complete)
            {
                var gate = ResolveTurnGate(m_TurnManager, m_Team);
                if (gate == MutinyAITurnGate.Cancel)
                { CancelDecisionWork(); m_TurnCoroutine = null; yield break; }
                if (gate == MutinyAITurnGate.Wait) { yield return null; continue; }
                if (!DecisionIsCurrent(work))
                {
                    InvalidatedDecisionCount++;
                    MutinyDebugLog.Info("AI", "board changed; discarding incomplete decision reason=" + LastDecisionInvalidationReason, this);
                    CancelDecisionWork(); m_TurnCoroutine = null; yield break;
                }
                try { complete = AdvanceDecisionSlice(work); }
                catch (Exception exception) { error = exception; }
                if (!complete && error == null) yield return null;
            }
            AIMove move = CreatePassMove();
            if (error == null)
            {
                try { move = FinishDecisionWork(work); }
                catch (Exception exception) { error = exception; }
            }
            if (error != null)
            {
                // An error must never commit an action in a different/new turn.
                var gate = ResolveTurnGate(m_TurnManager, m_Team);
                CancelDecisionWork();
                Debug.LogException(error, this);
                if (gate == MutinyAITurnGate.Execute)
                {
                    if (ActionLogEnabled) Debug.Log($"[Mutiny:AI-Action] team={m_Team?.TeamNumber ?? 0} decision={work?.Id ?? 0} phase={work?.Phase ?? "unprepared"} action=Pass score=NA reason=evaluation-error:{error.GetType().Name} candidates={work?.CandidateCount ?? 0}", this);
                    m_TurnManager.PassTurn();
                }
                m_TurnCoroutine = null; yield break;
            }
            if (!DecisionIsCurrent(work))
            { InvalidatedDecisionCount++; CancelDecisionWork(); m_TurnCoroutine = null; yield break; }
            IsEvaluatingCandidates = false;
            if (move.MoveType == AIMoveType.Pass || move.Character == null)
            {
                LogCommittedAction(work, move);
                m_TurnManager.PassTurn();
                m_ActiveWork = null; m_TurnCoroutine = null; yield break;
            }
            m_Team.SelectCharacter(move.Character);
            var camera = FindAnyObjectByType<Mutiny.Presentation.MutinyCameraController>();
            if (camera != null)
            {
                BeginWinnerCameraPan(camera, move.Character);
                while (camera.IsPanningToTurnTarget)
                {
                    yield return null;
                    if (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Cancel)
                    { CancelDecisionWork(); m_TurnCoroutine = null; yield break; }
                }
            }
            while (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Wait) yield return null;
            if (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Execute &&
                DecisionIsCurrent(work, ignoreSelection: true) && move.Character.IsAlive)
            {
                LogCommittedAction(work, move);
                ExecuteMove(move);
            }
            else InvalidatedDecisionCount++;
            CancelDecisionWork();
            m_TurnCoroutine = null;
        }

        private bool DecisionIsCurrent(DecisionWork work, bool ignoreSelection = false)
        {
            string reason;
            if (work == null) reason = "missing-decision";
            else if (work.ForcedWeaponType != ForcedWeaponType) reason = "forced-weapon";
            else if (work.Snapshot.Matches(this, out reason, ignoreSelection)) return true;
            LastDecisionInvalidationReason = reason;
            return false;
        }

        private static bool BeginWinnerCameraPan(
            Mutiny.Presentation.MutinyCameraController camera,
            MutinyCharacter character)
        {
            if (camera == null || character == null)
                return false;
            camera.PanToCharacter(character);
            return camera.IsPanningToTurnTarget;
        }

        internal static bool BeginWinnerCameraPanForVerification(
            Mutiny.Presentation.MutinyCameraController camera,
            MutinyCharacter character)
            => BeginWinnerCameraPan(camera, character);

        public static MutinyAITurnGate ResolveTurnGate(MutinyTurnManager manager, MutinyTeam team)
        {
            if (manager == null || team == null || manager.CurrentTeam != team ||
                !team.IsAiControlled || team.IsDefeated ||
                manager.CurrentPhase == TurnPhase.GameOver ||
                manager.CurrentPhase == TurnPhase.NotStarted)
                return MutinyAITurnGate.Cancel;

            return manager.CurrentPhase == TurnPhase.TurnActive
                ? MutinyAITurnGate.Execute
                : MutinyAITurnGate.Wait;
        }

        public AIMove EvaluateBestMove()
        {
            DecisionWork work = PrepareDecisionWork();
            try
            {
                while (work.Steps.MoveNext()) { }
                return FinishDecisionWork(work);
            }
            finally
            {
                (work.Steps as IDisposable)?.Dispose();
                work.Writer?.Abort();
                m_Random = null;
            }
        }

        internal static bool ShouldYieldDecisionFrame(bool forceNextFrame, long elapsedMilliseconds)
            => forceNextFrame || elapsedMilliseconds >= DefaultDecisionBudgetMilliseconds;

        internal IEnumerator EvaluateBestMoveStepsForVerification(Action<AIMove> completed)
        {
            DecisionWork work = PrepareDecisionWork();
            while (work.Steps.MoveNext())
                yield return null;
            completed?.Invoke(FinishDecisionWork(work));
        }

        internal void SetReplayTraceForVerification(MutinyAIDecisionTrace trace)
        {
            m_ReplayOverride = trace;
        }

        private DecisionWork PrepareDecisionWork(bool streaming = false)
        {
            if (m_Team == null)
                m_Team = GetComponent<MutinyTeam>();
            int decisionId = ++m_DecisionSequence;
            var work = new DecisionWork
            {
                Id = decisionId,
                Streaming = streaming,
                Best = new AIMove { MoveType = AIMoveType.Pass, Score = float.NegativeInfinity },
                ForcedWeaponType = ForcedWeaponType
            };

            // Flash keeps dead characters in Team.characters when calculating the
            // opposing centroid; individual distance scoring then checks alive.
            var allChars = FindObjectsByType<MutinyCharacter>();
            var enemies = new List<MutinyCharacter>();
            var allies = new List<MutinyCharacter>();

            for (int i = 0; i < allChars.Length; i++)
            {
                var ch = allChars[i];
                if (ch != null && ch.PhysicsBody != null)
                {
                    if (ch.TeamIndex != m_Team.TeamNumber)
                        enemies.Add(ch);
                    else
                        allies.Add(ch);
                }
            }

            // Team.advance concatenates candidates in Team.characters order.
            allies.Clear();
            foreach (MutinyCharacter character in m_Team.Characters)
                if (character != null && character.PhysicsBody != null)
                    allies.Add(character);

            work.Enemies = enemies;
            work.Allies = allies;
            work.Snapshot = new DecisionSnapshot(this, enemies, allies);
            foreach (MutinyCharacter character in allies)
                work.EffectiveLucks[character] = GetEffectiveLuck(character);

            if (enemies.Count == 0 || allies.Count == 0 || !ContainsAlive(enemies) || !ContainsAlive(allies))
            {
                work.Phase = "invalid-board";
                work.Steps = CompleteDecisionSteps(work, EmptyDecisionSteps());
                BeginDecisionRandom(work);
                return work;
            }

            // Retrieve terrain and water level
            string[,] terrainGrid = null;
            int gridW = 50;
            int gridH = 20;

            var controller = FindAnyObjectByType<MutinyLevelController>();
            if (controller != null && controller.LevelXml != null)
            {
                var lvlData = MutinyLevelXmlParser.Parse(controller.LevelXml.text);
                terrainGrid = lvlData.Terrain;
                gridW = lvlData.Width;
                gridH = lvlData.Height;
            }

            float waterPixelY = float.PositiveInfinity;
            var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
            if (levelRoot != null)
            {
                waterPixelY = -levelRoot.WaterLevelY * MutinyPhysics.PixelsPerUnit;
            }

            // Two-Phase check:
            // If a character was already chosen and moved (CanThrow consumed, CanShoot remaining),
            // only evaluate shooting for this selected character.
            work.Terrain = terrainGrid;
            work.GridW = gridW;
            work.GridH = gridH;
            work.WaterY = waterPixelY;
            MutinyCharacter activeChar = m_Team.SelectedCharacter;
            work.ContinuationCharacter = activeChar != null && activeChar.IsAlive &&
                !activeChar.CanThrow && activeChar.CanShoot ? activeChar : null;
            work.Phase = work.ContinuationCharacter != null ? "continuation" : "first-action";
            BeginDecisionRandom(work);
            work.Steps = CompleteDecisionSteps(work, EvaluateDecisionSteps(work));
            return work;
        }

        private static IEnumerator EmptyDecisionSteps() { yield break; }

        private IEnumerator CompleteDecisionSteps(DecisionWork work, IEnumerator steps)
        {
            try
            {
                while (true)
                {
                    while (work.Writer != null && !work.Writer.CanAcceptRecords) yield return null;
                    if (!steps.MoveNext()) break;
                    yield return null;
                }
                if (m_Random != null)
                {
                    m_Random.AssertReplayComplete();
                    AIMove move = ResolveDecisionWinner(work);
                    m_Random.Trace.Winner = $"{move.MoveType}:{move.Character?.name}:{move.WeaponType}:{move.Score:R}";
                    while (work.Writer != null && !work.Writer.TryComplete(m_Random.Trace)) yield return null;
                }
            }
            finally { (steps as IDisposable)?.Dispose(); }
        }

        private static AIMove ResolveDecisionWinner(DecisionWork work) =>
            work.Phase == "continuation" && (work.Best.Character == null || work.Best.Score <= 0f)
                ? CreatePassMove() : work.Best;

        private IEnumerator EvaluateDecisionSteps(DecisionWork work)
        {
            var actors = work.ContinuationCharacter != null
                ? new List<MutinyCharacter> { work.ContinuationCharacter } : work.Allies;
            for (int actorIndex = 0; actorIndex < actors.Count; actorIndex++)
            {
                MutinyCharacter actor = actors[actorIndex];
                CharacterSnapshot state = StateOf(actor, work);
                if (state.Alive)
                {
                    if (work.ContinuationCharacter == null && state.CanThrow)
                    {
                        // All 50 trajectories are generated before the first score/RNG follow-up.
                        var throws = new List<SelfThrowSample>(50);
                        foreach (object step in BuildSelfThrowSteps(actor, work, throws)) yield return null;
                        foreach (var sample in throws)
                        {
                            EvaluateSelfThrowSample(actor, sample, work, ref work.Best, ref work.CandidateCount);
                            yield return null;
                        }
                    }
                    if (state.CanShoot)
                    {
                        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (string weapon in WeaponChoices(actor, work))
                        {
                            if (!HasEffectiveWeapon(actor, weapon, work) || !seen.Add(weapon)) continue;
                            foreach (object step in EvaluateWeaponSteps(actor, weapon, work)) yield return null;
                        }
                    }
                }
                if (actorIndex < actors.Count - 1) { work.ForceNextFrame = true; yield return null; }
            }
        }

        private AIMove FinishDecisionWork(DecisionWork work)
        {
            AIMove move = ResolveDecisionWinner(work);
            LastDecisionCandidateCount = work.CandidateCount;
            LastDecisionTracePath = work.TracePath;
            if (m_Random != null)
            {
                m_Random.AssertReplayComplete();
                m_Random.Trace.Winner = $"{move.MoveType}:{move.Character?.name}:{move.WeaponType}:{move.Score:R}";
                LastDecisionTrace = m_Random.Trace;
                m_Random = null;
            }
            LogDecisionSummary(work.Id, work.Phase, work.CandidateCount, move);
            return move;
        }

        private void BeginDecisionRandom(DecisionWork work)
        {
            LastDecisionTrace = null;
            MutinyAIDecisionTrace replay = m_ReplayOverride;
            m_ReplayOverride = null;
            if (!string.IsNullOrWhiteSpace(ReplayDecisionTracePath))
            {
                replay = JsonUtility.FromJson<MutinyAIDecisionTrace>(File.ReadAllText(ReplayDecisionTracePath));
                ReplayDecisionTracePath = null; // A trace describes one decision, not the next phase.
                if (replay == null || replay.TeamNumber != m_Team.TeamNumber || replay.Phase != work.Phase)
                    throw new InvalidDataException("AI replay trace team or phase does not match this decision");
            }
            if (replay != null && (replay.TeamNumber != m_Team.TeamNumber || replay.Phase != work.Phase))
                throw new InvalidDataException("AI replay trace team or phase does not match this decision");
            int seed = replay != null ? replay.Seed :
                UseFixedDecisionSeed ? FixedDecisionSeed : UnityEngine.Random.Range(1, int.MaxValue);
            m_Random = new MutinyAIRandomStream(seed, work.Id, m_Team.TeamNumber, work.Phase, replay);
            m_Random.RetainRecords = !work.Streaming;
            m_Random.Trace.RecordsStreamed = work.Streaming;
            if (SaveDecisionTrace)
            {
                string path = Path.Combine(Application.persistentDataPath, "ai-decisions",
                    $"team-{m_Team.TeamNumber}-decision-{work.Id}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.json");
                work.Writer = new MutinyAITraceWriter(path);
                work.TracePath = path;
                m_Random.Writer = work.Writer;
                m_PendingTraceWrites.Add(work.Writer);
            }
        }

        private void LogDecisionSummary(int decisionId, string phase, int candidateCount, AIMove move)
        {
            string character = move.Character != null ? move.Character.name : "none";
            string weapon = string.IsNullOrEmpty(move.WeaponType) ? "none" : move.WeaponType;
            MutinyDebugLog.Info("AI-Decision",
                $"id={decisionId} phase={phase} candidates={candidateCount} type={move.MoveType} character={character} weapon={weapon} score={move.Score:0.0000} velocity=({move.LaunchVelocity.x:0.00},{move.LaunchVelocity.y:0.00}) target=({move.TargetPosition.x:0.0},{move.TargetPosition.y:0.0})",
                this);
        }

        private void LogCommittedAction(DecisionWork work, AIMove move)
        {
            if (!ActionLogEnabled)
                return;

            MutinyAIDecisionTrace trace = LastDecisionTrace;
            var line = new StringBuilder(384);
            line.Append("[Mutiny:AI-Action] team=").Append(m_Team.TeamNumber)
                .Append(" decision=").Append(work.Id)
                .Append(" seed=").Append(trace != null ? trace.Seed.ToString(CultureInfo.InvariantCulture) : "NA")
                .Append(" phase=").Append(work.Phase)
                .Append(" candidates=").Append(work.CandidateCount)
                .Append(" action=").Append(move.MoveType)
                .Append(" character=").Append(move.Character != null ? move.Character.name : "none")
                .Append(" weapon=").Append(string.IsNullOrEmpty(move.WeaponType) ? "none" : move.WeaponType)
                .Append(" score=").Append(Number(move.Score));

            if (move.MoveType != AIMoveType.Pass)
            {
                float luck = work.EffectiveLucks[move.Character];
                line.Append(" luck=").Append(Number(luck))
                    .Append(" start=").Append(Point(PositionOf(move.Character)))
                    .Append(" forcedWeapon=").Append(work.ForcedWeaponType ?? "none")
                    .Append(" forcedSupply=").Append(move.UsesForcedWeaponSupply);
                bool usesArcSamples = move.MoveType == AIMoveType.ShootWeapon &&
                    (MutinyWeaponFactoryCanFire(move.WeaponType) ||
                     string.Equals(move.WeaponType, "anchor", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(move.WeaponType, "cannon", StringComparison.OrdinalIgnoreCase));
                if (usesArcSamples)
                {
                    int samples = Mathf.FloorToInt(luck * m_Team.Characters.Count / Mathf.Max(1, m_Team.AliveCount));
                    line.Append(" weaponSamples=").Append(samples);
                }
                bool hasVelocity = move.MoveType == AIMoveType.SelfThrow ||
                    MutinyWeaponFactoryCanFire(move.WeaponType) ||
                    string.Equals(move.WeaponType, "cannon", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(move.WeaponType, "voodooDoll", StringComparison.OrdinalIgnoreCase);
                if (hasVelocity)
                    line.Append(" velocity=").Append(Point(move.LaunchVelocity));
                if (move.MoveType == AIMoveType.SelfThrow || hasVelocity ||
                    string.Equals(move.WeaponType, "anchor", StringComparison.OrdinalIgnoreCase))
                    line.Append(" predictedTarget=").Append(Point(move.TargetPosition));
                if (move.TargetCharacter != null)
                    line.Append(" targetCharacter=").Append(move.TargetCharacter.name);
                if (string.Equals(move.WeaponType, "cannon", StringComparison.OrdinalIgnoreCase))
                    line.Append(" cannonAngle=").Append(move.CannonRotationDegrees);
                if (string.Equals(move.WeaponType, "seagull", StringComparison.OrdinalIgnoreCase))
                    line.Append(" seagullFlightY=").Append(Number(move.SeagullFlightY))
                        .Append(" seagullShotXs=").Append(Numbers(move.SeagullShotXs));
                if (move.BoxPossibilities != null && move.BoxPossibilities.Length > 0)
                    line.Append(" boxPositions=").Append(Points(move.BoxPossibilities));
                if (string.Equals(move.WeaponType, "tidalWave", StringComparison.OrdinalIgnoreCase))
                    line.Append(" waveStart=(-550,")
                        .Append(Number(move.Character.PhysicsBody.WaterPixelY)).Append(')');
            }

            line.Append(" bestByAction=").Append(BestScoresByAction(work));
            if (!string.IsNullOrEmpty(work.TracePath))
                line.Append(" trace=").Append(work.TracePath).Append(" traceStatus=")
                    .Append(work.Writer.Error != null ? "failed" : work.Writer.Completion.IsCompleted ? "ready" : "pending");
            if (move.MoveType == AIMoveType.Pass)
            {
                if (work.Best.Character != null)
                    line.Append(" bestRejectedAction=").Append(work.Best.MoveType)
                        .Append(" bestRejectedCharacter=").Append(work.Best.Character.name)
                        .Append(" bestRejectedWeapon=")
                        .Append(string.IsNullOrEmpty(work.Best.WeaponType) ? "none" : work.Best.WeaponType)
                        .Append(" bestRejectedTarget=").Append(Point(work.Best.TargetPosition))
                        .Append(" luck=").Append(Number(work.EffectiveLucks[work.Best.Character]));
                line.Append(" bestRejectedScore=")
                    .Append(work.Best.Character != null ? Number(work.Best.Score) : "NA")
                    .Append(" reason=")
                    .Append(work.Phase == "continuation" && work.Best.Character != null
                        ? "continuation-best-score-not-positive"
                        : "no-valid-candidate");
            }
            else
            {
                line.Append(" reason=").Append(work.Phase == "continuation"
                    ? "highest-score-above-zero"
                    : "highest-score-first-action-no-positive-threshold")
                    .Append(" tieRule=first-candidate-wins");
            }
            // GM-10 must remain visible in a player build; the ordinary debug
            // logger is intentionally disabled outside Editor/development builds.
            Debug.Log(line.ToString(), this);
        }

        private static string BestScoresByAction(DecisionWork work)
        {
            var result = new StringBuilder("[");
            for (int i = 0; i < work.ActionOrder.Count; i++)
            {
                if (i > 0) result.Append(',');
                string action = work.ActionOrder[i];
                result.Append(action).Append(':').Append(Number(work.BestByAction[action]));
            }
            return result.Append(']').ToString();
        }

        private static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        private static string Point(Vector2 point) => "(" + Number(point.x) + "," + Number(point.y) + ")";

        private static string Numbers(float[] values)
        {
            if (values == null || values.Length == 0) return "[]";
            var result = new StringBuilder("[");
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) result.Append(',');
                result.Append(Number(values[i]));
            }
            return result.Append(']').ToString();
        }

        private static string Points(Vector2[] values)
        {
            if (values == null || values.Length == 0) return "[]";
            var result = new StringBuilder("[");
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) result.Append(',');
                result.Append(Point(values[i]));
            }
            return result.Append(']').ToString();
        }

        private void EvaluateCharacterWeapons(
            MutinyCharacter shooter, List<MutinyCharacter> enemies, List<MutinyCharacter> allies,
            string[,] terrainGrid, int gridW, int gridH, float waterPixelY,
            ref AIMove bestMove, ref int candidateCount, string onlyWeaponType = null,
            string forcedWeaponType = null, float? decisionLuck = null)
        {
            if (m_Team == null) m_Team = GetComponent<MutinyTeam>();
            var work = new DecisionWork
            {
                Best = bestMove, CandidateCount = candidateCount, Enemies = enemies, Allies = allies,
                Terrain = terrainGrid, GridW = gridW, GridH = gridH, WaterY = waterPixelY,
                ForcedWeaponType = forcedWeaponType, Snapshot = new DecisionSnapshot(this, enemies, allies)
            };
            work.EffectiveLucks[shooter] = decisionLuck ?? GetEffectiveLuck(shooter);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string weapon in WeaponChoices(shooter, work))
            {
                if (onlyWeaponType != null && !string.Equals(onlyWeaponType, weapon, StringComparison.OrdinalIgnoreCase)) continue;
                if (!HasEffectiveWeapon(shooter, weapon, work) || !seen.Add(weapon)) continue;
                foreach (object step in EvaluateWeaponSteps(shooter, weapon, work)) { }
            }
            bestMove = work.Best;
            candidateCount = work.CandidateCount;
        }

        private static IEnumerable<string> EnumerateWeaponChoices(MutinyCharacter character, string forcedWeaponType)
        {
            if (forcedWeaponType != null)
            {
                yield return forcedWeaponType;
                yield break;
            }
            foreach (KeyValuePair<string, int> entry in character.WeaponInventory)
                yield return entry.Key;
        }

        private static bool HasEffectiveWeapon(MutinyCharacter character, string weaponType, string forcedWeaponType)
            => forcedWeaponType != null
                ? string.Equals(weaponType, forcedWeaponType, StringComparison.OrdinalIgnoreCase)
                : character.HasWeapon(weaponType);

        // Verification seam for the production inventory dispatcher and candidate
        // generators. Tests supply an isolated board but do not duplicate any AI
        // formula or bypass the real per-weapon routing above.
        internal AIMove EvaluateCharacterWeaponsForVerification(
            MutinyCharacter shooter,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            string[,] terrainGrid,
            int gridW,
            int gridH,
            float waterPixelY,
            out int candidateCount)
        {
            var bestMove = new AIMove
            {
                MoveType = AIMoveType.Pass,
                Score = float.NegativeInfinity
            };
            candidateCount = 0;
            EvaluateCharacterWeapons(shooter, enemies, allies, terrainGrid, gridW, gridH,
                waterPixelY, ref bestMove, ref candidateCount, forcedWeaponType: ForcedWeaponType);
            return bestMove;
        }

        private static Vector2 SimulateSeagullShotImpact(
            float startX, float startY, string[,] terrainGrid, int gridW, int gridH,
            float waterPixelY, IReadOnlyList<PhysicsBoxObstacle> boxes)
        {
            var body = PhysicsBodyState.CreateDefault(startX, startY);
            body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = MutinySeagullFire.OriginalExtent;
            body.Weight = MutinySeagullFire.OriginalWeight;
            body.VelocityX = MutinySeagull.OriginalFlightSpeed;
            body.HitsBoxes = true;
            var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Seagull, body, null,
                terrainGrid, gridW, gridH, waterPixelY, boxes);
            while (prediction.Advance()) { }
            return prediction.Impact;
        }

        private static float ScoreSeagullShot(
            Vector2 impact,
            List<MutinyCharacter> characters,
            bool allies, DecisionWork work)
        {
            float score = 0f;
            for (int i = 0; i < characters.Count; i++)
            {
                MutinyCharacter character = characters[i];
                if (character == null || !StateOf(character, work).Alive || character.PhysicsBody == null)
                    continue;

                Vector2 target = PositionOf(character, work);
                float distance = Vector2.Distance(impact, target);
                score += ScoreSeagullDistance(distance, allies);
            }
            return score;
        }

        internal static float ScoreSeagullDistanceForVerification(float distance, bool allies)
            => ScoreSeagullDistance(distance, allies);

        private static float ScoreSeagullDistance(float distance, bool allies)
        {
            if (distance >= 40f)
                return 0f;
            return allies
                ? -(1.5f - distance / 40f)
                : 1f - distance / 40f;
        }

        private IEnumerable<object> BuildSelfThrowSteps(MutinyCharacter character, DecisionWork work, List<SelfThrowSample> samples)
        {
            for (int sample = 0; sample < 50; sample++)
            {
                Vector2 velocity = RandomArc(20f);
                PhysicsBodyState body = StateOf(character, work).Body;
                body.VelocityX = velocity.x;
                body.VelocityY = velocity.y;
                body.HitsBoxes = true;
                var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Character, body, null,
                    work.Terrain, work.GridW, work.GridH, work.WaterY, work.Snapshot.Boxes);
                while (prediction.Advance()) yield return null;
                samples.Add(new SelfThrowSample { Velocity = velocity, Landing = prediction.Impact });
                yield return null;
            }
        }

        private void EvaluateSelfThrowSample(MutinyCharacter character, SelfThrowSample sample,
            DecisionWork work, ref AIMove bestMove, ref int candidateCount)
        {
            Vector2 start = PositionOf(character, work);
            Vector2 centroid = Vector2.zero;
            for (int i = 0; i < work.Enemies.Count; i++)
                centroid += PositionOf(work.Enemies[i], work);
            centroid /= work.Enemies.Count;

            // Character.aiThink calls cherryBomb.randomThrows(10) for each move
            // even though the decompiled score ignores those ten impact points.
            // Consume the same random draws so subsequent weapons retain order.
            if (HasEffectiveWeapon(character, "cherryBomb", work))
                for (int i = 0; i < 10; i++)
                    RandomArc(MutinyWeaponFactory.GetTwangMaxForce("cherryBomb"));

            float score = ScoreSelfThrow(character, start, sample.Landing, centroid,
                work.Enemies, work.Allies, work.WaterY, work.ForcedWeaponType, work);
            ConsiderCandidate(ref bestMove, new AIMove
            {
                MoveType = AIMoveType.SelfThrow,
                Character = character,
                LaunchVelocity = sample.Velocity,
                TargetPosition = sample.Landing,
                Score = score
            }, ref candidateCount, work);
        }

        private static AIMove CreatePassMove()
        {
            return new AIMove { MoveType = AIMoveType.Pass, Score = 0f };
        }

        private static bool ContainsAlive(List<MutinyCharacter> characters)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].IsAlive)
                    return true;
            }
            return false;
        }

        private static Vector2 PositionOf(MutinyCharacter character)
        {
            PhysicsBodyState state = character.PhysicsBody.State;
            return new Vector2(state.X, state.Y);
        }

        private Vector2 RandomArc(float maxForce)
        {
            // Character.randomThrows and Weapon.randomThrows: 180 + int(random * 180).
            int degrees = 180 + NextRandomInt(0, 180);
            float force = NextRandomFloat(5f, maxForce);
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians) * force, Mathf.Sin(radians) * force);
        }

        private int NextRandomInt(int minimum, int maximum)
            => m_Random != null ? m_Random.NextInt(minimum, maximum) : UnityEngine.Random.Range(minimum, maximum);

        private float NextRandomFloat(float minimum, float maximum)
            => m_Random != null ? m_Random.NextFloat(minimum, maximum) : UnityEngine.Random.Range(minimum, maximum);

        private static bool MutinyWeaponFactoryCanFire(string weaponType)
        {
            return string.Equals(weaponType, "cherryBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "dynamite", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "banana", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "cannonball", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "boulder", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "mine", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "parachuteBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "rumBottle", StringComparison.OrdinalIgnoreCase);
        }

        private static PhysicsBodyState CreateWeaponSimulation(Vector2 start, string weaponType, Vector2 velocity)
        {
            PhysicsBodyState body = PhysicsBodyState.CreateDefault(start.x, start.y);
            body.VelocityX = velocity.x;
            body.VelocityY = velocity.y;
            body.Bounce = 0.2f;
            body.Friction = 0.3f;
            body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 9f;
            body.HitsBoxes = true;

            if (string.Equals(weaponType, "dynamite", StringComparison.OrdinalIgnoreCase))
            {
                body.Friction = 1.7f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 11f;
            }
            else if (string.Equals(weaponType, "banana", StringComparison.OrdinalIgnoreCase))
            {
                body.Bounce = 0.8f;
                body.Friction = 0.5f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 7f;
            }
            else if (string.Equals(weaponType, "boulder", StringComparison.OrdinalIgnoreCase))
            {
                // AI reaches Boulder through Weapon.aiPerform -> Weapon.fire, not
                // Boulder.release.  Candidate velocity is therefore the launch
                // velocity used by the actual weapon without a .5 conversion.
                body.Weight = 1.5f;
                body.Friction = 0.25f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 31f;
            }
            else if (string.Equals(weaponType, "mine", StringComparison.OrdinalIgnoreCase))
            {
                body.Friction = 1.5f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 14f;
            }
            else if (string.Equals(weaponType, "parachuteBomb", StringComparison.OrdinalIgnoreCase))
            {
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 11f;
            }
            else if (string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase))
            {
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 7f;
            }
            else if (string.Equals(weaponType, "rumBottle", StringComparison.OrdinalIgnoreCase))
            {
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 14f;
            }
            else if (string.Equals(weaponType, "cannonball", StringComparison.OrdinalIgnoreCase))
            {
                body.Weight = 0f;
                body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = 10f;
            }
            return body;
        }

        private static PhysicsBodyState CreateFormalWeaponPredictionTemplate(MutinyCharacter shooter, string weaponType)
        {
            // Initialize the real class once per weapon, then copy its pure state.
            // The probe is never fired or stepped, so contact/explosion/inventory
            // callbacks cannot run during candidate scoring.
            MutinyWeapon probe = MutinyWeaponFactory.SpawnWeapon(weaponType, shooter);
            if (probe == null || probe.PhysicsBody == null)
                throw new InvalidOperationException($"AI cannot initialize prediction weapon {weaponType}");
            try
            {
                probe.PhysicsBody.IsActive = false;
                return probe.PhysicsBody.State;
            }
            finally
            {
                probe.gameObject.SetActive(false);
                Destroy(probe.gameObject);
            }
        }

        internal static PhysicsBodyState CreateFormalWeaponPredictionTemplateForVerification(
            MutinyCharacter shooter, string weaponType)
            => CreateFormalWeaponPredictionTemplate(shooter, weaponType);

        // Legacy isolated-body seam used by existing motion tests. Production
        // candidates copy the formally initialized weapon state above.
        internal static PhysicsBodyState CreateWeaponSimulationForVerification(
            Vector2 start, string weaponType, Vector2 velocity)
        {
            return CreateWeaponSimulation(start, weaponType, velocity);
        }

        private static float ScoreGenericWeaponCandidate(
            string weaponType,
            Vector2 impact,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies, DecisionWork work)
        {
            float score = -0.01f;
            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy == null || !StateOf(enemy, work).Alive)
                    continue;

                float distance = Vector2.Distance(impact, PositionOf(enemy, work));
                if (distance < 70f)
                {
                    score += 1.5f - distance / 70f;
                    score *= 1f + StateOf(enemy, work).Evilness;
                }
            }

            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally == null || !StateOf(ally, work).Alive)
                    continue;

                float distance = Vector2.Distance(impact, PositionOf(ally, work));
                if (distance < 40f)
                    score -= 1.5f - distance / 40f;
            }

            if (string.Equals(weaponType, "anchor", StringComparison.OrdinalIgnoreCase))
                score *= 0.5f;
            if (string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase))
                score = (score - 0.5f) * 1.2f;
            return score;
        }

        private float ScoreSelfThrow(
            MutinyCharacter character,
            Vector2 start,
            Vector2 landing,
            Vector2 enemyCentroid,
            List<MutinyCharacter> enemies,
            List<MutinyCharacter> allies,
            float waterPixelY,
            string forcedWeaponType, DecisionWork work)
        {
            float score = -(landing.y - start.y) * 0.003f;
            score += (Mathf.Abs(landing.x - enemyCentroid.x) - Mathf.Abs(start.x - enemyCentroid.x)) / -500f;
            score += (Mathf.Abs(landing.y - enemyCentroid.y) - Mathf.Abs(start.y - enemyCentroid.y)) / -500f;
            if (landing.y >= waterPixelY)
                score -= 2f;

            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy == null || !StateOf(enemy, work).Alive)
                    continue;

                float distance = Vector2.Distance(landing, PositionOf(enemy, work));
                float proximity;
                if (distance < 200f)
                {
                    proximity = 0.2f * (1f - distance / 200f);
                    if (distance < 100f)
                    {
                        proximity *= distance / 100f;
                        score -= 3f * (1f - distance / 100f);
                    }
                }
                else
                {
                    proximity = 0.3f * Mathf.Pow(0.75f, distance / 200f);
                }
                score += proximity;
                score *= 1f + StateOf(enemy, work).Evilness;
            }

            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally == null || !StateOf(ally, work).Alive)
                    continue;

                float distance = Vector2.Distance(landing, PositionOf(ally, work));
                if (ally == character && distance < 80f)
                    score -= 1f - distance / 80f;
                else if (ally != character && distance < 40f)
                    score -= 0.1f * (1f - distance / 40f);
            }

            foreach (var chest in work.Snapshot.Chests)
                if (!chest.Finished && Vector2.Distance(landing, new Vector2(chest.X, chest.FloorY)) < OriginalChestMoveRadiusPixels)
                    score += OriginalChestMoveBonus;

            if (HasEffectiveWeapon(character, "cherryBomb", work))
                score = ScoreCherryBombFollowUpAtMoveLanding(score, character, landing, enemies, work);

            float movement = Vector2.Distance(landing, start);
            score += movement < 300f ? 0.3f * movement / 300f - 0.3f : -0.3f;
            return score;
        }

        public static float ScoreChestMoveLanding(Vector2 landing, MutinyTreasureChest chest)
        {
            if (chest == null || chest.IsFinished)
                return 0f;
            return Vector2.Distance(landing, new Vector2(chest.PixelX, chest.FloorPixelY)) <
                   OriginalChestMoveRadiusPixels
                ? OriginalChestMoveBonus
                : 0f;
        }

        private float ScoreCherryBombFollowUpAtMoveLanding(
            float score,
            MutinyCharacter character,
            Vector2 landing,
            List<MutinyCharacter> enemies, DecisionWork work)
        {
            // Character.aiThink calls cherryBomb.randomThrows(10), but then (as
            // confirmed in pcode) scores the character movement landing every time.
            // Preserve that scoring quirk without instantiating ten simulation objects.
            for (int sample = 0; sample < 10; sample++)
            {
                float followUp = 0f;
                for (int i = 0; i < enemies.Count; i++)
                {
                    MutinyCharacter enemy = enemies[i];
                    if (enemy == null || !StateOf(enemy, work).Alive)
                        continue;
                    float distance = Vector2.Distance(landing, PositionOf(enemy, work));
                    if (distance < 100f)
                        followUp += (1f - distance / 100f) * (1f + StateOf(enemy, work).Evilness) * 0.5f;
                }
                float selfDistance = Vector2.Distance(landing, PositionOf(character, work));
                if (selfDistance < 80f)
                    followUp -= 1f - selfDistance / 80f;
                if (followUp > 0f)
                    score += followUp;
            }
            return score;
        }

        private static float ScoreTidalWave(List<MutinyCharacter> enemies, List<MutinyCharacter> allies, float waterPixelY, DecisionWork work)
        {
            float score = 0f;
            for (int i = 0; i < enemies.Count; i++)
            {
                MutinyCharacter enemy = enemies[i];
                if (enemy != null && StateOf(enemy, work).Alive && PositionOf(enemy, work).y >= waterPixelY - 300f)
                    score += StateOf(enemy, work).Health / StateOf(enemy, work).MaxHealth * 0.5f;
            }
            for (int i = 0; i < allies.Count; i++)
            {
                MutinyCharacter ally = allies[i];
                if (ally != null && StateOf(ally, work).Alive && PositionOf(ally, work).y >= waterPixelY - 300f)
                    score -= 1.5f;
            }
            return score;
        }



        private void ConsiderCandidate(ref AIMove bestMove, AIMove candidate, ref int candidateCount, DecisionWork work = null)
        {
            candidateCount++;
            if (work != null)
            {
                string action = candidate.MoveType == AIMoveType.SelfThrow ? "jump" :
                    string.IsNullOrEmpty(candidate.WeaponType) ? candidate.MoveType.ToString() : candidate.WeaponType;
                if (!work.BestByAction.TryGetValue(action, out float previous))
                { work.ActionOrder.Add(action); work.BestByAction.Add(action, candidate.Score); }
                else if (candidate.Score > previous) work.BestByAction[action] = candidate.Score;
            }
            if (m_Random != null)
                m_Random.RecordCandidate(new MutinyAICandidateRecord
                {
                    Character = candidate.Character != null ? candidate.Character.name : "none",
                    Weapon = candidate.WeaponType,
                    MoveType = candidate.MoveType.ToString(),
                    Score = candidate.Score,
                    Velocity = candidate.LaunchVelocity,
                    Target = candidate.TargetPosition,
                    TargetCharacter = candidate.TargetCharacter != null ? candidate.TargetCharacter.name : null,
                    SeagullFlightY = candidate.SeagullFlightY,
                    SeagullShotXs = candidate.SeagullShotXs,
                    BoxPossibilities = candidate.BoxPossibilities,
                    CannonRotationDegrees = candidate.CannonRotationDegrees
                });
            // Flash Team.advance uses strict >, so the first candidate wins a tie.
            if (bestMove.Character == null || candidate.Score > bestMove.Score)
                bestMove = candidate;
        }

        private static Vector2 SimulateWeaponImpact(
            PhysicsBodyState body, string weaponType, string[,] grid, int gridW, int gridH,
            float waterPixelY, IReadOnlyList<PhysicsBoxObstacle> boxes, out int steps,
            IReadOnlyList<MutinyCharacter> bananaCharacters = null)
        {
            var positions = new List<Vector2>();
            if (bananaCharacters != null)
                foreach (var character in bananaCharacters)
                    if (character != null && character.PhysicsBody != null) positions.Add(PositionOf(character));
            var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Weapon, body, weaponType,
                grid, gridW, gridH, waterPixelY, boxes, positions);
            while (prediction.Advance()) { }
            steps = prediction.Steps;
            return prediction.Impact;
        }

        private static bool TerminatesSimulationOnContact(string weaponType)
        {
            return string.Equals(weaponType, "cherryBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "rumBottle", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "piecesOfEight", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "parachuteBomb", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(weaponType, "cannonball", StringComparison.OrdinalIgnoreCase);
        }

        private static Vector2 SimulateCharacterLanding(
            PhysicsBodyState body, string[,] grid, int gridW, int gridH, float waterPixelY,
            IReadOnlyList<PhysicsBoxObstacle> boxes, out int steps)
        {
            var prediction = new MutinyAIPrediction(MutinyAIPrediction.Kind.Character, body, null,
                grid, gridW, gridH, waterPixelY, boxes);
            while (prediction.Advance()) { }
            steps = prediction.Steps;
            return prediction.Impact;
        }

        internal static Vector2 SimulateWeaponImpactForVerification(
            PhysicsBodyState body,
            string weaponType,
            string[,] grid,
            int gridW,
            int gridH,
            float waterPixelY,
            out int steps)
        {
            return SimulateWeaponImpact(body, weaponType, grid, gridW, gridH, waterPixelY,
                MutinyBoxRegistry.GetObstacles(), out steps,
                string.Equals(weaponType, "banana", StringComparison.OrdinalIgnoreCase)
                    ? FindObjectsByType<MutinyCharacter>() : null);
        }

        internal static Vector2 SimulateCharacterLandingForVerification(
            PhysicsBodyState body,
            string[,] grid,
            int gridW,
            int gridH,
            float waterPixelY,
            out int steps)
        {
            return SimulateCharacterLanding(body, grid, gridW, gridH, waterPixelY,
                MutinyBoxRegistry.GetObstacles(), out steps);
        }

        private void ExecuteMove(AIMove move)
        {
            var ch = move.Character;
            if (ch == null || !ch.IsAlive)
                return;

            if (move.MoveType == AIMoveType.ShootWeapon && !string.IsNullOrEmpty(move.WeaponType))
            {
                bool consumeInventory = !move.UsesForcedWeaponSupply;
                MutinyDebugLog.Info("AI",
                    $"firing weapon={move.WeaponType} character={ch.name} velocity={move.LaunchVelocity}", this);
                if (string.Equals(move.WeaponType, "tidalWave", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon waveWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (waveWeapon is MutinyTidalWave wave)
                    {
                        float waterY = ch.PhysicsBody.WaterPixelY;
                        wave.StartWave(-550f, waterY);
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else if (string.Equals(move.WeaponType, "seagull", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon birdWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (birdWeapon is MutinySeagull seagull)
                    {
                        seagull.PlaceForAi(move.SeagullFlightY, move.SeagullShotXs);
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else if (string.Equals(move.WeaponType, "gunpowderBarrel", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon barrelWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (barrelWeapon is MutinyGunpowderBarrel barrel && barrel.BeginAiPlacement(move.BoxPossibilities))
                    {
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                        MutinyDebugLog.Info("AI", $"gunpowder barrel AI sequence armed candidates={move.BoxPossibilities.Length}", this);
                    }
                }
                else if (string.Equals(move.WeaponType, "woodenCrate", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon crateWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (crateWeapon is MutinyWoodenCrate crate && crate.BeginAiPlacement(move.BoxPossibilities))
                    {
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                        MutinyDebugLog.Info("AI", $"wooden crate AI sequence armed candidates={move.BoxPossibilities.Length}", this);
                    }
                }
                else if (string.Equals(move.WeaponType, "anchor", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon anchorWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (anchorWeapon is MutinyAnchor anchor && anchor.DropForAi(move.TargetPosition.x))
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                }
                else if (string.Equals(move.WeaponType, "cannon", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon cannonWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (cannonWeapon is MutinyCannon cannon)
                    {
                        cannon.BeginAiFire(move.TargetPosition, move.CannonRotationDegrees, move.LaunchVelocity);
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else if (string.Equals(move.WeaponType, "voodooDoll", StringComparison.OrdinalIgnoreCase))
                {
                    MutinyWeapon dollWeapon = MutinyWeaponFactory.SpawnWeapon(move.WeaponType, ch);
                    if (dollWeapon is MutinyVoodooDoll doll)
                    {
                        doll.BindTarget(move.TargetCharacter);
                        doll.FireForAi(move.LaunchVelocity);
                        if (consumeInventory) ch.ConsumeWeapon(move.WeaponType);
                    }
                }
                else
                {
                    MutinyWeaponFactory.SpawnAndFire(move.WeaponType, ch, move.LaunchVelocity,
                        consumeInventory);
                }
                ch.CanThrow = false;
                ch.CanShoot = false;
                if (m_TurnManager != null)
                {
                    m_TurnManager.NotifyActionStarted();
                }
            }
            else if (move.MoveType == AIMoveType.SelfThrow)
            {
                MutinyDebugLog.Info("AI",
                    $"jumping character={ch.name} velocity={move.LaunchVelocity}; preserves CanShoot for phase 2; original Character.twang has no direct SFX", this);
                ch.PhysicsBody.SetVelocity(move.LaunchVelocity.x, move.LaunchVelocity.y);
                ch.MarkSelfThrown("AI");
                ch.CanThrow = false;
                // Authentic Flash parity: CanShoot remains true for phase 2!
                if (m_TurnManager != null)
                {
                    m_TurnManager.NotifyActionStarted();
                }
            }
            else
            {
                MutinyDebugLog.Info("AI", $"passing turn character={ch.name}", this);
                if (m_TurnManager != null)
                {
                    m_TurnManager.PassTurn();
                }
            }
        }

        // Test seam for the same execution method used by the turn coroutine.
        internal void ExecuteMoveForVerification(AIMove move, MutinyTurnManager turnManager = null)
        {
            if (turnManager != null)
                m_TurnManager = turnManager;
            else if (m_TurnManager == null)
                BindTurnManager();
            ExecuteMove(move);
        }

        private void BindTurnManager()
        {
            MutinyTurnManager found = FindAnyObjectByType<MutinyTurnManager>();
            if (found == null)
            {
                if (!m_LoggedMissingManager)
                {
                    m_LoggedMissingManager = true;
                    MutinyDebugLog.Warning("AI", $"team={TeamLabel(m_Team)} cannot find turn manager", this);
                }
                return;
            }

            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            m_TurnManager = found;
            m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            m_TurnManager.OnTurnStarted += HandleTurnStarted;
            m_LoggedMissingManager = false;
            MutinyDebugLog.Info("AI", $"bound turn manager team={TeamLabel(m_Team)}", this);
        }

        private void TryStartCurrentTurn(string source)
        {
            if (ResolveTurnGate(m_TurnManager, m_Team) == MutinyAITurnGate.Execute)
                BeginTurnRoutine(source);
        }

        private void BeginTurnRoutine(string source)
        {
            if (!isActiveAndEnabled) return;
            CancelTurnRoutine();
            MutinyDebugLog.Info("AI", $"starting turn routine source={source}", this);
            m_TurnCoroutine = StartCoroutine(ExecuteAITurnRoutine());
        }

        private static string TeamLabel(MutinyTeam team)
        {
            return team == null ? "none" : $"T{team.TeamNumber}";
        }
    }
}
