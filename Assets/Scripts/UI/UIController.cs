using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalSlots
{
    /// <summary>
    /// Owns the HUD: credits, bet and win text, the bonus banner, and the button states.
    /// It never changes game state itself. Button clicks (delivered by Unity's EventSystem)
    /// are forwarded as requests, and the screen is updated by listening to game events.
    /// </summary>
    public sealed class UIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text winText;
        [SerializeField] private TMP_Text bonusText;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button betUpButton;
        [SerializeField] private Button betDownButton;
        [SerializeField] private Button quitButton;

        private Tween winCountTween;
        private int credits;
        private int bet;
        private bool busy;

        private void Awake()
        {
            spinButton.onClick.AddListener(GameEvents.RaiseSpinRequested);
            quitButton.onClick.AddListener(GameEvents.RaiseQuitRequested);
            betUpButton.onClick.AddListener(() => GameEvents.RaiseBetChangeRequested(1));
            betDownButton.onClick.AddListener(() => GameEvents.RaiseBetChangeRequested(-1));
        }

        private void OnEnable()
        {
            GameEvents.OnHudInit += HandleHudInit;
            GameEvents.OnCreditsChanged += HandleCreditsChanged;
            GameEvents.OnBetChanged += HandleBetChanged;
            GameEvents.OnBusyChanged += HandleBusyChanged;
            GameEvents.OnSpinStart += HandleSpinStart;
            GameEvents.OnSpinResult += HandleSpinResult;
            GameEvents.OnBonusTriggered += HandleBonusTriggered;
            GameEvents.OnFreeSpinsRemaining += HandleFreeSpinsRemaining;
            GameEvents.OnBonusComplete += HandleBonusComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnHudInit -= HandleHudInit;
            GameEvents.OnCreditsChanged -= HandleCreditsChanged;
            GameEvents.OnBetChanged -= HandleBetChanged;
            GameEvents.OnBusyChanged -= HandleBusyChanged;
            GameEvents.OnSpinStart -= HandleSpinStart;
            GameEvents.OnSpinResult -= HandleSpinResult;
            GameEvents.OnBonusTriggered -= HandleBonusTriggered;
            GameEvents.OnFreeSpinsRemaining -= HandleFreeSpinsRemaining;
            GameEvents.OnBonusComplete -= HandleBonusComplete;
        }

        private void HandleHudInit(int startCredits, int startBet)
        {
            credits = startCredits;
            bet = startBet;
            winText.text = string.Empty;
            bonusText.text = string.Empty;
            RefreshTexts();
            RefreshButtons();
        }

        private void HandleCreditsChanged(int value)
        {
            credits = value;
            RefreshTexts();
            RefreshButtons();
        }

        private void HandleBetChanged(int value)
        {
            bet = value;
            RefreshTexts();
            RefreshButtons();
        }

        private void HandleBusyChanged(bool value)
        {
            busy = value;
            RefreshButtons();
        }

        private void HandleSpinStart(int spinBet, bool isFreeSpin)
        {
            winCountTween?.Kill();
            winText.text = string.Empty;
        }

        /// <summary>Shows the result of the spin: the amount counts up to the win, or a loss message.</summary>
        private void HandleSpinResult(SpinResultData result)
        {
            string prefix = result.IsFreeSpin ? "FREE SPIN " : string.Empty;
            winCountTween?.Kill();

            if (result.Win <= 0)
            {
                winText.text = $"{prefix}NO WIN";
                return;
            }

            winCountTween = DOVirtual.Float(0f, result.Win, 0.6f,
                    value => winText.text = $"{prefix}WIN {Mathf.RoundToInt(value):N0}")
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            winText.transform.DOKill(true);
            winText.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 6, 0.5f).SetLink(gameObject);
        }

        /// <summary>Shows the bonus banner with a pop-in animation.</summary>
        private void HandleBonusTriggered(int freeSpins)
        {
            bonusText.text = $"FREE SPINS: {freeSpins}";

            bonusText.transform.DOKill();
            bonusText.transform.localScale = Vector3.zero;
            bonusText.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetLink(gameObject);
        }

        private void HandleFreeSpinsRemaining(int remaining)
        {
            bonusText.text = remaining > 0 ? $"FREE SPINS LEFT: {remaining}" : string.Empty;
        }

        private void HandleBonusComplete(int totalWin)
        {
            winCountTween?.Kill();
            winText.text = $"FREE SPINS TOTAL {totalWin:N0}";
        }

        private void RefreshTexts()
        {
            creditsText.text = $"CREDITS {credits:N0}";
            betText.text = $"BET {bet:N0}";
        }

        /// <summary>Buttons are locked while a round is running, and Spin also needs enough credits for the bet.</summary>
        private void RefreshButtons()
        {
            spinButton.interactable = !busy && credits >= bet;
            betUpButton.interactable = !busy;
            betDownButton.interactable = !busy;
            quitButton.interactable = !busy;
        }
    }
}