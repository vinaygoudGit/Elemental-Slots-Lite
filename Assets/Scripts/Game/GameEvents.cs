using System;
using UnityEngine;

namespace ElementalSlots
{
    /// <summary>
    /// Static event hub that decouples the systems from each other. The controller raises
    /// events, and the UI and analytics only subscribe. Nothing here knows who listens.
    /// Events can only be invoked from the class that declares them, so each one has a
    /// matching Raise method.
    /// </summary>
    public static class GameEvents
    {
        // Lifecycle and analytics events.
        public static event Action OnGameStart;
        public static event Action<int, bool> OnSpinStart;
        public static event Action<SpinResultData> OnSpinResult;
        public static event Action<int> OnBonusTriggered;
        public static event Action<int> OnBonusComplete;
        public static event Action<int> OnBetChanged;
        public static event Action<int> OnSessionEnd;

        // Input requests, raised by the UI buttons and handled by the game controller.
        public static event Action OnSpinRequested;
        public static event Action<int> OnBetChangeRequested;
        public static event Action OnQuitRequested;

        // State updates used to keep the HUD in sync.
        public static event Action<int, int> OnHudInit;
        public static event Action<int> OnCreditsChanged;
        public static event Action<bool> OnBusyChanged;
        public static event Action<int> OnFreeSpinsRemaining;

        /// <summary>
        /// Clears all subscribers when entering Play mode, so that with domain reload
        /// disabled no handler from a previous run stays attached.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSubscribers()
        {
            OnGameStart = null;
            OnHudInit = null;
            OnSpinRequested = null;
            OnBetChangeRequested = null;
            OnQuitRequested = null;
            OnSpinStart = null;
            OnSpinResult = null;
            OnBonusTriggered = null;
            OnFreeSpinsRemaining = null;
            OnBonusComplete = null;
            OnBetChanged = null;
            OnCreditsChanged = null;
            OnBusyChanged = null;
            OnSessionEnd = null;
        }

        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaiseHudInit(int credits, int bet) => OnHudInit?.Invoke(credits, bet);
        public static void RaiseSpinRequested() => OnSpinRequested?.Invoke();
        public static void RaiseBetChangeRequested(int direction) => OnBetChangeRequested?.Invoke(direction);
        public static void RaiseQuitRequested() => OnQuitRequested?.Invoke();
        public static void RaiseSpinStart(int bet, bool isFreeSpin) => OnSpinStart?.Invoke(bet, isFreeSpin);
        public static void RaiseSpinResult(SpinResultData result) => OnSpinResult?.Invoke(result);
        public static void RaiseBonusTriggered(int freeSpins) => OnBonusTriggered?.Invoke(freeSpins);
        public static void RaiseFreeSpinsRemaining(int remaining) => OnFreeSpinsRemaining?.Invoke(remaining);
        public static void RaiseBonusComplete(int totalWin) => OnBonusComplete?.Invoke(totalWin);
        public static void RaiseBetChanged(int bet) => OnBetChanged?.Invoke(bet);
        public static void RaiseCreditsChanged(int credits) => OnCreditsChanged?.Invoke(credits);
        public static void RaiseBusyChanged(bool busy) => OnBusyChanged?.Invoke(busy);
        public static void RaiseSessionEnd(int credits) => OnSessionEnd?.Invoke(credits);
    }
}