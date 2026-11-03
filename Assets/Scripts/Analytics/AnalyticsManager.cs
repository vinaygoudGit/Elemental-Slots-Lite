using System.Globalization;
using System.Text;
using UnityEngine;

namespace ElementalSlots
{
    /// <summary>
    /// Lightweight analytics: listens to the game events and prints each one to the Console
    /// as a JSON string. It also tracks the session totals and logs the running session RTP
    /// after every spin. There is no backend, the Console is the only output.
    /// </summary>
    public sealed class AnalyticsManager : MonoBehaviour
    {
        private int totalSpins;
        private long totalBet;
        private long totalWon;

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnSpinStart += HandleSpinStart;
            GameEvents.OnSpinResult += HandleSpinResult;
            GameEvents.OnBonusTriggered += HandleBonusTriggered;
            GameEvents.OnBonusComplete += HandleBonusComplete;
            GameEvents.OnBetChanged += HandleBetChanged;
            GameEvents.OnSessionEnd += HandleSessionEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnSpinStart -= HandleSpinStart;
            GameEvents.OnSpinResult -= HandleSpinResult;
            GameEvents.OnBonusTriggered -= HandleBonusTriggered;
            GameEvents.OnBonusComplete -= HandleBonusComplete;
            GameEvents.OnBetChanged -= HandleBetChanged;
            GameEvents.OnSessionEnd -= HandleSessionEnd;
        }

        private void HandleGameStart()
        {
            Log("{\"event\":\"game_start\"}");
        }

        private void HandleSpinStart(int bet, bool isFreeSpin)
        {
            Log($"{{\"event\":\"spin_start\",\"bet\":{bet},\"free_spin\":{Bool(isFreeSpin)}}}");
        }

        /// <summary>Updates the session totals, then logs the result and the session RTP.</summary>
        private void HandleSpinResult(SpinResultData result)
        {
            totalSpins++;

            // Free spins pay but are not paid for, so only paid spins count towards the bet.
            if (!result.IsFreeSpin)
                totalBet += result.Bet;
            totalWon += result.Win;

            string outcome = result.Win > 0 ? "win" : "loss";
            Log($"{{\"event\":\"spin_result\",\"bet\":{result.Bet},\"win\":{result.Win},\"free_spin\":{Bool(result.IsFreeSpin)}," +
                $"\"outcome\":\"{outcome}\",\"scatters\":{result.ScatterCount},\"symbols\":{GridJson(result.Grid)}," +
                $"\"session_rtp\":{Num(SessionRtp())}}}");

            Debug.Log($"[RTP] session={Num(SessionRtp() * 100d, "0.00")}% reference={Num(SlotConfig.ReferenceRtp * 100d, "0.00")}% " +
                      $"spins={totalSpins} total_bet={totalBet} total_won={totalWon}");
        }

        private void HandleBonusTriggered(int freeSpins)
        {
            Log($"{{\"event\":\"bonus_triggered\",\"type\":\"free_spins\",\"spins\":{freeSpins}}}");
        }

        private void HandleBonusComplete(int totalWin)
        {
            Log($"{{\"event\":\"bonus_complete\",\"type\":\"free_spins\",\"win\":{totalWin}}}");
        }

        private void HandleBetChanged(int bet)
        {
            Log($"{{\"event\":\"bet_changed\",\"bet\":{bet}}}");
        }

        private void HandleSessionEnd(int credits)
        {
            Log($"{{\"event\":\"session_end\",\"spins\":{totalSpins},\"total_bet\":{totalBet},\"total_win\":{totalWon}," +
                $"\"session_rtp\":{Num(SessionRtp())},\"credits\":{credits}}}");
        }

        /// <summary>Total won divided by total bet so far. It only settles near the real RTP after many spins.</summary>
        private double SessionRtp()
        {
            return totalBet > 0 ? (double)totalWon / totalBet : 0d;
        }

        private static void Log(string json)
        {
            Debug.Log("[Analytics] " + json);
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        /// <summary>Formats a number with a dot as the decimal separator, whatever the system language is, so the JSON stays valid.</summary>
        private static string Num(double value, string format = "0.0000")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>Writes the grid as one array per reel, each listing its symbols from top to bottom.</summary>
        private static string GridJson(SymbolId[][] grid)
        {
            var sb = new StringBuilder("[");
            for (int reel = 0; reel < grid.Length; reel++)
            {
                if (reel > 0)
                    sb.Append(',');

                sb.Append('[');
                for (int row = 0; row < grid[reel].Length; row++)
                {
                    if (row > 0)
                        sb.Append(',');
                    sb.Append('"').Append(grid[reel][row].ToString()).Append('"');
                }
                sb.Append(']');
            }
            return sb.Append(']').ToString();
        }
    }
}