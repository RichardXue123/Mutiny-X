using System.Collections;
using UnityEngine;

namespace Mutiny.Simulation
{
    public enum MutinyAIStrategyMode { Legacy, Enhanced }

    // Immutable identity of one decision/committed action. A live GM change must
    // not change the operator of a weapon sequence that has already started.
    public readonly struct MutinyAIStrategyContext
    {
        public readonly MutinyAIStrategyMode Mode;
        public readonly int ConfigurationVersion;
        public readonly string StrategyId;
        public readonly string AlgorithmId;
        public readonly bool UsesFallback;
        public bool IsBound => !string.IsNullOrEmpty(StrategyId);
        public string ModeId => Mode == MutinyAIStrategyMode.Enhanced ? "enhanced" : "legacy";

        internal MutinyAIStrategyContext(MutinyAIStrategyMode mode, int version,
            string strategyId, string algorithmId, bool fallback)
        {
            Mode = mode;
            ConfigurationVersion = version;
            StrategyId = strategyId;
            AlgorithmId = algorithmId;
            UsesFallback = fallback;
        }
    }

    public sealed partial class MutinyAIController
    {
        public static bool EnhancementEnabled { get; private set; }
        public static int StrategyConfigurationVersion { get; private set; }
        public static bool EnhancedPlannerImplemented => false;
        public MutinyAIStrategyContext CurrentDecisionStrategy => m_ActiveWork?.StrategyContext ?? default;
        public MutinyAIStrategyContext LastDecisionStrategy { get; private set; }
        public MutinyAIStrategyContext LastCommittedStrategy { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEnhancementOnPlayStart()
        {
            EnhancementEnabled = false;
            StrategyConfigurationVersion = 0;
        }

        public static void SetEnhancementEnabled(bool enabled)
        {
            if (EnhancementEnabled == enabled) return;
            EnhancementEnabled = enabled;
            // Version rather than only a bool also rejects off -> on -> off
            // changes that occur between two search/commit validation points.
            unchecked { StrategyConfigurationVersion++; }
        }

        // Input is the controller's captured DecisionWork (board, legal actions,
        // settings and RNG); output is AIMove. Strategies only evaluate: the
        // common turn coroutine owns camera/validation/real action submission.
        // This is a main-thread adapter, NOT a thread-safe full-effect simulator.
        private interface IAIDecisionStrategy
        {
            string Id { get; }
            string AlgorithmId { get; }
            bool UsesFallback { get; }
            IEnumerator Evaluate(MutinyAIController controller, DecisionWork input);
            AIMove ResolveWinner(DecisionWork input);
        }

        private sealed class LegacyDecisionStrategy : IAIDecisionStrategy
        {
            public string Id => "legacy";
            public string AlgorithmId => "legacy";
            public bool UsesFallback => false;
            public IEnumerator Evaluate(MutinyAIController controller, DecisionWork input) =>
                controller.EvaluateLegacyDecisionSteps(input);
            public AIMove ResolveWinner(DecisionWork input) =>
                input.Phase == "continuation" && (input.Best.Character == null || input.Best.Score <= 0f)
                    ? CreatePassMove() : input.Best;
        }

        private sealed class EnhancedBootstrapStrategy : IAIDecisionStrategy
        {
            public string Id => "enhanced-bootstrap";
            public string AlgorithmId => "legacy";
            public bool UsesFallback => true;
            // Intentionally explicit until the separately specified full-effect
            // planner exists. No duplicate formulas or extra RNG draws here.
            public IEnumerator Evaluate(MutinyAIController controller, DecisionWork input) =>
                LegacyStrategy.Evaluate(controller, input);
            public AIMove ResolveWinner(DecisionWork input) => LegacyStrategy.ResolveWinner(input);
        }

        private static readonly IAIDecisionStrategy LegacyStrategy = new LegacyDecisionStrategy();
        private static readonly IAIDecisionStrategy BootstrapStrategy = new EnhancedBootstrapStrategy();

        private static void BindDecisionStrategy(DecisionWork work)
        {
            work.Strategy = EnhancementEnabled ? BootstrapStrategy : LegacyStrategy;
            work.StrategyContext = new MutinyAIStrategyContext(
                EnhancementEnabled ? MutinyAIStrategyMode.Enhanced : MutinyAIStrategyMode.Legacy,
                StrategyConfigurationVersion, work.Strategy.Id, work.Strategy.AlgorithmId, work.Strategy.UsesFallback);
        }

        private static bool StrategyIsCurrent(DecisionWork work) => work != null &&
            work.StrategyContext.ConfigurationVersion == StrategyConfigurationVersion;

        private static AIMove ResolveDecisionWinner(DecisionWork work)
        {
            AIMove move = work.Strategy.ResolveWinner(work);
            move.StrategyContext = work.StrategyContext;
            return move;
        }
    }
}
