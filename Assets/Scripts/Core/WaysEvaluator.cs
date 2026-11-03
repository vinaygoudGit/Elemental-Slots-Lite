namespace ElementalSlots
{
    /// <summary>
    /// Scores a grid with the 1024-ways rule. A symbol wins when it appears on consecutive
    /// reels starting from reel 1, in any row. The number of ways is the product of how many
    /// times the symbol (or a Wild) appears on each reel of the chain.
    /// </summary>
    public static class WaysEvaluator
    {
        /// <param name="grid">The symbols indexed [reel][row].</param>
        /// <param name="collectLines">
        /// When true the winning cells are recorded for the win animation. The simulator
        /// passes false because it only needs the total.
        /// </param>
        public static SpinResultData Evaluate(SymbolId[][] grid, bool collectLines = true)
        {
            var result = new SpinResultData { Grid = grid };
            result.ScatterCount = CountScatters(grid);

            ScoreSymbols(grid, result, collectLines);
            return result;
        }

        private static void ScoreSymbols(SymbolId[][] grid, SpinResultData result, bool collectLines)
        {
            var counts = new int[SlotConfig.Reels];

            for (int s = 0; s < SlotConfig.RegularSymbolCount; s++)
            {
                var symbol = (SymbolId)s;
                int reelsMatched = 0;
                int ways = 1;

                // Walk left to right and stop at the first reel without the symbol.
                for (int reel = 0; reel < SlotConfig.Reels; reel++)
                {
                    int count = CountOnReel(grid[reel], symbol);
                    counts[reel] = count;
                    if (count == 0)
                        break;
                    ways *= count;
                    reelsMatched++;
                }

                if (reelsMatched < SlotConfig.MinMatch)
                    continue;

                double multiplier = ways * SlotConfig.Pays[s][reelsMatched - SlotConfig.MinMatch];
                result.WinMultiplier += multiplier;

                if (!collectLines)
                    continue;

                var line = new WinLine
                {
                    Symbol = symbol,
                    ReelsMatched = reelsMatched,
                    Ways = ways,
                    Multiplier = multiplier
                };

                for (int reel = 0; reel < reelsMatched; reel++)
                    for (int row = 0; row < SlotConfig.Rows; row++)
                        if (Matches(grid[reel][row], symbol))
                            line.Cells.Add(new CellPos(reel, row));

                result.Lines.Add(line);
            }
        }

        private static int CountOnReel(SymbolId[] column, SymbolId symbol)
        {
            int count = 0;
            for (int row = 0; row < column.Length; row++)
                if (Matches(column[row], symbol))
                    count++;
            return count;
        }

        /// <summary>A cell matches a symbol if it is that symbol or a Wild.</summary>
        private static bool Matches(SymbolId cell, SymbolId symbol)
        {
            return cell == symbol || cell == SymbolId.Wild;
        }

        private static int CountScatters(SymbolId[][] grid)
        {
            int count = 0;
            for (int reel = 0; reel < grid.Length; reel++)
                for (int row = 0; row < grid[reel].Length; row++)
                    if (grid[reel][row] == SymbolId.Scatter)
                        count++;
            return count;
        }
    }
}