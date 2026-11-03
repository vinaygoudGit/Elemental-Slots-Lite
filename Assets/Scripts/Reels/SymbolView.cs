using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalSlots
{
    /// <summary>
    /// One symbol cell on a reel. Shows the symbol's sprite, or a tinted tile with its name
    /// when no sprite is assigned, and can play the win pulse.
    /// </summary>
    public sealed class SymbolView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private float pulseScale = 1.18f;
        [SerializeField] private float pulsePeriod = 0.5f;

        private RectTransform rect;
        private Tween pulse;

        public SymbolId Symbol { get; private set; }

        public RectTransform Rect
        {
            get
            {
                if (rect == null)
                    rect = (RectTransform)transform;
                return rect;
            }
        }

        /// <summary>Changes the symbol shown by this cell.</summary>
        public void SetSymbol(SymbolId symbol, SymbolSpriteLibrary library)
        {
            Symbol = symbol;
            SymbolSpriteLibrary.Entry entry = library.Get(symbol);

            bool hasSprite = entry.Sprite != null;

            // The tinted tile and the name are only a fallback for symbols without a sprite.
            background.color = entry.Tint;
            background.enabled = !hasSprite;
            icon.sprite = entry.Sprite;
            icon.enabled = hasSprite;
            label.enabled = !hasSprite;
            label.text = symbol.ToString();
        }

        /// <summary>Starts the scale up and scale down animation used to show a winning cell.</summary>
        public void PlayWin()
        {
            StopWin();

            // Draw this cell above its neighbours so the enlarged symbol is not covered.
            transform.SetAsLastSibling();

            // A yoyo loop grows to the pulse scale and back, until StopWin kills it.
            pulse = transform.DOScale(pulseScale, pulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        /// <summary>Stops the pulse and restores the normal size.</summary>
        public void StopWin()
        {
            pulse?.Kill();
            pulse = null;
            transform.localScale = Vector3.one;
        }
    }
}