using System;

namespace ElementalSlots
{
    /// <summary>
    /// Produces random grids using the per-reel weights from <see cref="SlotConfig"/>.
    /// Each cell is an independent weighted draw.
    /// </summary>
    public sealed class SpinGenerator
    {
        private readonly Random _rng;
        private readonly int[][] _cumulative;
        private readonly int[] _totals;

        public SpinGenerator(Random rng = null)
        {
            _rng = rng ?? new Random();
            _cumulative = new int[SlotConfig.Reels][];
            _totals = new int[SlotConfig.Reels];

            // Turn the weights into running totals so one random roll can be matched
            // against them. For weights 6, 7, 8 the running totals are 6, 13, 21.
            for (int reel = 0; reel < SlotConfig.Reels; reel++)
            {
                int[] weights = SlotConfig.Weights[reel];
                _cumulative[reel] = new int[weights.Length];
                int sum = 0;
                for (int s = 0; s < weights.Length; s++)
                {
                    sum += weights[s];
                    _cumulative[reel][s] = sum;
                }
                _totals[reel] = sum;
            }
        }

        /// <summary>Creates an empty grid with the right dimensions.</summary>
        public static SymbolId[][] CreateGrid()
        {
            var grid = new SymbolId[SlotConfig.Reels][];
            for (int reel = 0; reel < SlotConfig.Reels; reel++)
                grid[reel] = new SymbolId[SlotConfig.Rows];
            return grid;
        }

        /// <summary>Creates a new grid and fills it with random symbols.</summary>
        public SymbolId[][] Generate()
        {
            var grid = CreateGrid();
            Fill(grid);
            return grid;
        }

        /// <summary>Fills an existing grid in place. Used by the simulator to avoid allocations.</summary>
        public void Fill(SymbolId[][] grid)
        {
            for (int reel = 0; reel < SlotConfig.Reels; reel++)
                for (int row = 0; row < SlotConfig.Rows; row++)
                    grid[reel][row] = Pick(reel);
        }

        private SymbolId Pick(int reel)
        {
            int roll = _rng.Next(_totals[reel]);
            int[] cumulative = _cumulative[reel];
            for (int s = 0; s < cumulative.Length; s++)
                if (roll < cumulative[s])
                    return (SymbolId)s;
            return SymbolId.Fire;
        }
    }
}