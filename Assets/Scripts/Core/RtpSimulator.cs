using System;

namespace ElementalSlots
{
    /// <summary>Statistics collected by <see cref="RtpSimulator"/>. All amounts are in units of the bet.</summary>
    public sealed class RtpReport
    {
        public long PaidSpins;
        public double TotalBet;
        public double TotalWon;
        public double FreeSpinWon;
        public long Hits;
        public long Triggers;
        public double MaxSingleWin;

        /// <summary>Return to player: total won divided by total bet.</summary>
        public double Rtp => TotalBet > 0 ? TotalWon / TotalBet : 0;
        public double BaseRtp => TotalBet > 0 ? (TotalWon - FreeSpinWon) / TotalBet : 0;
        public double FreeSpinRtp => TotalBet > 0 ? FreeSpinWon / TotalBet : 0;

        /// <summary>Share of paid spins that win anything.</summary>
        public double HitFrequency => PaidSpins > 0 ? (double)Hits / PaidSpins : 0;
        public double TriggerRate => PaidSpins > 0 ? (double)Triggers / PaidSpins : 0;

        public override string ToString()
        {
            return $"spins={PaidSpins} rtp={Rtp:P3} base={BaseRtp:P3} free={FreeSpinRtp:P3} " +
                   $"hit={HitFrequency:P2} trigger=1/{(Triggers > 0 ? PaidSpins / Triggers : 0)} maxWin={MaxSingleWin:F1}x";
        }
    }

    /// <summary>
    /// Plays a large number of spins with the real generator and evaluator, without any
    /// Unity objects, and measures the RTP. Free spins triggered by Scatters are included,
    /// with no bet charged for them.
    /// </summary>
    public static class RtpSimulator
    {
        public static RtpReport Run(long paidSpins, int seed)
        {
            var generator = new SpinGenerator(new Random(seed));
            var grid = SpinGenerator.CreateGrid();
            var report = new RtpReport { PaidSpins = paidSpins };

            for (long i = 0; i < paidSpins; i++)
            {
                generator.Fill(grid);
                var spin = WaysEvaluator.Evaluate(grid, false);

                report.TotalBet += 1d;
                double spinWin = spin.WinMultiplier;
                if (spin.IsWin)
                    report.Hits++;

                // Free spins pay but cost nothing. They do not retrigger.
                if (spin.TriggersFreeSpins)
                {
                    report.Triggers++;
                    for (int f = 0; f < SlotConfig.FreeSpinsAwarded; f++)
                    {
                        generator.Fill(grid);
                        double freeWin = WaysEvaluator.Evaluate(grid, false).WinMultiplier;
                        report.FreeSpinWon += freeWin;
                        spinWin += freeWin;
                    }
                }

                report.TotalWon += spinWin;
                if (spinWin > report.MaxSingleWin)
                    report.MaxSingleWin = spinWin;
            }

            return report;
        }
    }
}