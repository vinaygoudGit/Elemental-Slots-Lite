using System.Collections.Generic;

namespace ElementalSlots
{
    /// <summary>A position in the grid: reel (column) and row.</summary>
    public struct CellPos
    {
        public readonly int Reel;
        public readonly int Row;

        public CellPos(int reel, int row)
        {
            Reel = reel;
            Row = row;
        }
    }

    /// <summary>One winning symbol of a spin, with the cells that took part in it.</summary>
    public sealed class WinLine
    {
        public SymbolId Symbol;
        public int ReelsMatched;
        public int Ways;
        public double Multiplier;
        public List<CellPos> Cells = new List<CellPos>();
    }

    /// <summary>
    /// The complete outcome of one spin. It is created by <see cref="WaysEvaluator"/>,
    /// completed by the game controller (bet and win in credits), and sent to the UI and
    /// analytics through <c>GameEvents.OnSpinResult</c>.
    /// </summary>
    public sealed class SpinResultData
    {
        /// <summary>The visible symbols, indexed [reel][row].</summary>
        public SymbolId[][] Grid;
        public List<WinLine> Lines = new List<WinLine>();

        /// <summary>Total win as a multiple of the bet.</summary>
        public double WinMultiplier;
        public int ScatterCount;

        /// <summary>Bet used to price this spin, and the win in credits.</summary>
        public int Bet;
        public int Win;
        public bool IsFreeSpin;

        public bool IsWin => WinMultiplier > 0d;
        public bool TriggersFreeSpins => ScatterCount >= SlotConfig.ScatterTrigger;
    }
}