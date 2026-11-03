namespace ElementalSlots
{
    /// <summary>
    /// Every tunable number of the slot math in one place: grid size, paytable and reel weights.
    /// Changing a pay or a weight changes the RTP, so rerun the simulator after any edit.
    /// This class is plain C# and has no Unity dependency.
    /// </summary>
    public static class SlotConfig
    {
        public const int Reels = 5;
        public const int Rows = 4;

        /// <summary>Minimum number of consecutive reels (from reel 1) needed to pay.</summary>
        public const int MinMatch = 3;

        /// <summary>Scatters anywhere on the grid needed to trigger free spins.</summary>
        public const int ScatterTrigger = 3;
        public const int FreeSpinsAwarded = 3;

        public const int SymbolCount = 13;
        public const int RegularSymbolCount = 10;

        /// <summary>RTP measured by the simulator (average of 60M spins), shown next to the session RTP in the log.</summary>
        public const double ReferenceRtp = 0.956;

        /// <summary>
        /// Pay per way as a multiple of the total bet. Indexed [symbol][reelsMatched - 3],
        /// so the three values are the pay for 3, 4 and 5 reels.
        /// </summary>
        public static readonly double[][] Pays =
        {
            new[] { 0.78, 2.33, 7.75 }, // Fire
            new[] { 0.62, 1.86, 6.20 }, // Water
            new[] { 0.46, 1.55, 4.65 }, // Earth
            new[] { 0.39, 1.24, 3.88 }, // Air
            new[] { 0.31, 0.78, 2.33 }, // Lava
            new[] { 0.23, 0.62, 1.86 }, // Ice
            new[] { 0.16, 0.46, 1.55 }, // Leaf
            new[] { 0.16, 0.46, 1.55 }, // Cloud
            new[] { 0.08, 0.31, 0.93 }, // Gem
            new[] { 0.08, 0.31, 0.93 }  // Rune
        };

        /// <summary>
        /// Relative frequency of each symbol on each reel, in <see cref="SymbolId"/> order.
        /// A bigger weight means the symbol appears more often. Wild is 0 on reels 1 and 5
        /// (it only lands on the middle reels) and Bonus is 0 everywhere (it never appears).
        /// </summary>
        public static readonly int[][] Weights =
        {
            new[] { 6, 7, 8, 9, 10, 11, 12, 12, 13, 14, 0, 3, 0 },
            new[] { 6, 7, 8, 9, 10, 11, 12, 12, 13, 14, 4, 3, 0 },
            new[] { 6, 7, 8, 9, 10, 11, 12, 12, 13, 14, 4, 3, 0 },
            new[] { 6, 7, 8, 9, 10, 11, 12, 12, 13, 14, 4, 3, 0 },
            new[] { 6, 7, 8, 9, 10, 11, 12, 12, 13, 14, 0, 3, 0 }
        };

        public static bool IsRegular(SymbolId symbol)
        {
            return (int)symbol < RegularSymbolCount;
        }
    }
}