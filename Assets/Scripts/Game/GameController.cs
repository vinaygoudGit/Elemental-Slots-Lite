using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalSlots
{
    /// <summary>
    /// Runs the main game loop: credits, bet, the spin sequence, win payout and the
    /// Free Spins bonus. The outcome of a spin is decided first (random grid, then
    /// evaluation), and the reels only animate towards that result.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private ReelController[] reels;
        [SerializeField] private SymbolSpriteLibrary spriteLibrary;
        [SerializeField] private int startingCredits = 100000;

        // Bets are multiples of 100 so every payout is an exact whole number of credits.
        [SerializeField] private int[] betLevels = { 100, 200, 500, 1000 };

        [SerializeField] private float baseSpinTime = 0.6f;
        [SerializeField] private float reelStagger = 0.25f;
        [SerializeField] private float winDisplayTime = 1.5f;
        [SerializeField] private float bonusPause = 0.6f;

        // DEBUG: tick in the Inspector to force Free Spins on the next paid spin. Remove before submitting.
        [SerializeField] private bool debugForceFreeSpins;

        private readonly List<SymbolView> highlighted = new List<SymbolView>();

        private SpinGenerator generator;
        private SpinResultData lastResult;
        private int credits;
        private int betIndex;
        private bool busy;

        private int Bet => betLevels[betIndex];

        private void OnEnable()
        {
            GameEvents.OnSpinRequested += HandleSpinRequested;
            GameEvents.OnBetChangeRequested += HandleBetChangeRequested;
            GameEvents.OnQuitRequested += HandleQuitRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnSpinRequested -= HandleSpinRequested;
            GameEvents.OnBetChangeRequested -= HandleBetChangeRequested;
            GameEvents.OnQuitRequested -= HandleQuitRequested;
        }

        private void Start()
        {
            generator = new SpinGenerator();
            credits = startingCredits;
            betIndex = 0;

            // Show a random grid before the first spin so the reels are not empty.
            SymbolId[][] grid = generator.Generate();
            for (int reel = 0; reel < reels.Length; reel++)
            {
                reels[reel].Initialize(spriteLibrary);
                reels[reel].SetColumn(grid[reel]);
            }

            GameEvents.RaiseHudInit(credits, Bet);
            GameEvents.RaiseGameStart();
        }


        /// <summary>
        /// Exits the game. In a build the application closes, and in the editor Play mode stops.
        /// Either way Unity calls OnApplicationQuit, which logs the session_end event.
        /// </summary>
        private void HandleQuitRequested()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         
            Application.Quit();
#endif
        }


        private void HandleBetChangeRequested(int direction)
        {
            if (busy)
                return;

            int next = Mathf.Clamp(betIndex + direction, 0, betLevels.Length - 1);
            if (next == betIndex)
                return;

            betIndex = next;
            GameEvents.RaiseBetChanged(Bet);
        }

        private void HandleSpinRequested()
        {
            if (busy || credits < Bet)
                return;

            StartCoroutine(PlayRound());
        }

        /// <summary>One paid spin, followed by the free spins bonus if it triggered one.</summary>
        private IEnumerator PlayRound()
        {
            SetBusy(true);

            yield return PlaySpin(false);

            if (lastResult.TriggersFreeSpins)
                yield return PlayFreeSpins();

            SetBusy(false);
        }

        /// <summary>
        /// Plays a single spin: charge the bet, decide the result, animate the reels, pay the
        /// win and highlight the winning cells. Free spins skip the bet.
        /// </summary>
        private IEnumerator PlaySpin(bool isFreeSpin)
        {
            ClearHighlights();

            int bet = Bet;
            if (!isFreeSpin)
            {
                credits -= bet;
                GameEvents.RaiseCreditsChanged(credits);
            }

            GameEvents.RaiseSpinStart(bet, isFreeSpin);

            // The result is fully decided here, before any reel moves.
            SymbolId[][] grid = generator.Generate();

            // DEBUG: puts three Scatters on reels 1, 3 and 5, once. Remove before submitting.
            if (debugForceFreeSpins && !isFreeSpin)
            {
                grid[0][1] = SymbolId.Scatter;
                grid[2][1] = SymbolId.Scatter;
                grid[4][1] = SymbolId.Scatter;
                debugForceFreeSpins = false;
            }

            SpinResultData result = WaysEvaluator.Evaluate(grid);
            result.Bet = bet;
            result.IsFreeSpin = isFreeSpin;
            result.Win = (int)System.Math.Round(result.WinMultiplier * bet);

            // Start every reel at once, each one stopping a little later than the one before.
            var routines = new Coroutine[reels.Length];
            for (int reel = 0; reel < reels.Length; reel++)
                routines[reel] = StartCoroutine(reels[reel].Spin(grid[reel], baseSpinTime + reel * reelStagger));

            for (int reel = 0; reel < routines.Length; reel++)
                yield return routines[reel];

            credits += result.Win;
            lastResult = result;

            GameEvents.RaiseCreditsChanged(credits);
            GameEvents.RaiseSpinResult(result);

            bool showScatters = result.TriggersFreeSpins && !isFreeSpin;
            if (result.IsWin || showScatters)
            {
                Highlight(result, showScatters);
                yield return new WaitForSeconds(winDisplayTime);
                ClearHighlights();
            }
        }

        /// <summary>
        /// The Free Spins bonus: a fixed number of automatic spins that cost nothing and pay
        /// normally. They cannot retrigger the bonus.
        /// </summary>
        private IEnumerator PlayFreeSpins()
        {
            int count = SlotConfig.FreeSpinsAwarded;
            int total = 0;

            GameEvents.RaiseBonusTriggered(count);
            yield return new WaitForSeconds(bonusPause);

            for (int i = 0; i < count; i++)
            {
                GameEvents.RaiseFreeSpinsRemaining(count - i);
                yield return PlaySpin(true);
                total += lastResult.Win;
            }

            GameEvents.RaiseFreeSpinsRemaining(0);
            GameEvents.RaiseBonusComplete(total);
        }

        /// <summary>Starts the scale pulse on every cell that is part of a win (and on Scatters that trigger the bonus).</summary>
        private void Highlight(SpinResultData result, bool includeScatters)
        {
            var mask = new bool[SlotConfig.Reels, SlotConfig.Rows];

            foreach (WinLine line in result.Lines)
                foreach (CellPos cell in line.Cells)
                    mask[cell.Reel, cell.Row] = true;

            for (int reel = 0; reel < SlotConfig.Reels; reel++)
                for (int row = 0; row < SlotConfig.Rows; row++)
                {
                    if (includeScatters && result.Grid[reel][row] == SymbolId.Scatter)
                        mask[reel, row] = true;

                    if (!mask[reel, row])
                        continue;

                    SymbolView view = reels[reel].GetCell(row);
                    view.PlayWin();
                    highlighted.Add(view);
                }
        }

        private void ClearHighlights()
        {
            for (int i = 0; i < highlighted.Count; i++)
                highlighted[i].StopWin();
            highlighted.Clear();
        }

        private void SetBusy(bool value)
        {
            busy = value;
            GameEvents.RaiseBusyChanged(value);
        }
    }
}