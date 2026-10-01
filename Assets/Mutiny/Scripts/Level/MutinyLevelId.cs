using System;
using System.Globalization;

namespace Mutiny.Levels
{
    /// <summary>A mode and its local map number; independent of campaign/menu limits.</summary>
    public readonly struct MutinyLevelId : IEquatable<MutinyLevelId>
    {
        public const int OriginalSinglePlayerCount = 15;
        public const int OriginalTwoPlayerCount = 18;

        public MutinyGameMode Mode { get; }
        public int Number { get; }
        public bool IsValid => Number > 0 &&
            (Mode == MutinyGameMode.SinglePlayer || Mode == MutinyGameMode.LocalTwoPlayer);
        public int ModeNumber => Mode == MutinyGameMode.SinglePlayer ? 1 : 2;
        public string AssetName => $"level_{ModeNumber}_{Number:D2}";
        public string ResourcePath => "Data/Levels/" + AssetName;

        // Only the original visual/evidence tables use the old global numbering.
        public int OriginalNumber => Mode == MutinyGameMode.LocalTwoPlayer && Number <= OriginalTwoPlayerCount
            ? Number + OriginalSinglePlayerCount : Number;

        public MutinyLevelId(MutinyGameMode mode, int number)
        {
            Mode = mode;
            Number = number;
        }

        public static MutinyLevelId FromOriginalNumber(int number) => number > OriginalSinglePlayerCount
            ? new MutinyLevelId(MutinyGameMode.LocalTwoPlayer, number - OriginalSinglePlayerCount)
            : new MutinyLevelId(MutinyGameMode.SinglePlayer, number);

        /// <summary>GM: a bare positive integer means single player; explicit names select either mode.</summary>
        public static bool TryParse(string value, out MutinyLevelId id)
        {
            id = default;
            if (string.IsNullOrEmpty(value)) return false;
            string token = value.StartsWith("level_", StringComparison.OrdinalIgnoreCase)
                ? value.Substring(6) : value;
            string[] parts = token.Split('_');
            MutinyGameMode mode = MutinyGameMode.SinglePlayer;
            if (parts.Length == 2)
            {
                if (parts[0] == "1") mode = MutinyGameMode.SinglePlayer;
                else if (parts[0] == "2") mode = MutinyGameMode.LocalTwoPlayer;
                else return false;
            }
            else if (parts.Length != 1 || !string.Equals(token, value, StringComparison.Ordinal))
                return false;
            if (!int.TryParse(parts[parts.Length - 1], NumberStyles.None, CultureInfo.InvariantCulture,
                    out int number) || number <= 0)
                return false;
            id = new MutinyLevelId(mode, number);
            return true;
        }

        public bool Equals(MutinyLevelId other) => Mode == other.Mode && Number == other.Number;
        public override bool Equals(object obj) => obj is MutinyLevelId other && Equals(other);
        public override int GetHashCode() => ((int)Mode * 397) ^ Number;
        public override string ToString() => AssetName;
    }
}
