using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ElementalSlots
{
    /// <summary>
    /// Draws and animates one reel. The reel is a masked column of symbol cells: the visible
    /// rows plus a few hidden cells above. While spinning, the cells scroll down and are
    /// recycled; to stop, the target symbols are fed in from the top so they land exactly on
    /// the visible rows, followed by a short DOTween ease out with a small bounce.
    /// </summary>
    public sealed class ReelController : MonoBehaviour
    {
        // Hidden cells above the visible rows. Two are needed so no gap shows at the top during the bounce.
        private const int TopBuffer = 2;

        [SerializeField] private SymbolView symbolPrefab;
        [SerializeField] private float spinSpeed = 18f;
        [SerializeField] private float settleTime = 0.35f;

        private RectTransform rect;
        private SymbolView[] cells;
        private SymbolSpriteLibrary library;
        private float cellHeight;
        private float offset;

        /// <summary>Creates the cells under this reel and sizes them to the reel's rectangle.</summary>
        public void Initialize(SymbolSpriteLibrary spriteLibrary)
        {
            library = spriteLibrary;
            rect = (RectTransform)transform;
            Canvas.ForceUpdateCanvases();

            cells = new SymbolView[SlotConfig.Rows + TopBuffer];
            for (int i = 0; i < cells.Length; i++)
            {
                SymbolView cell = Instantiate(symbolPrefab, transform);
                RectTransform cellRect = cell.Rect;
                cellRect.anchorMin = new Vector2(0.5f, 1f);
                cellRect.anchorMax = new Vector2(0.5f, 1f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);
                cell.SetSymbol(RandomFiller(), library);
                cells[i] = cell;
            }

            Layout();
        }

        /// <summary>Returns the cell on a visible row (0 is the top row).</summary>
        public SymbolView GetCell(int row)
        {
            return cells[TopBuffer + row];
        }

        /// <summary>Shows the given symbols on the visible rows without any animation.</summary>
        public void SetColumn(SymbolId[] column)
        {
            offset = 0f;
            for (int row = 0; row < SlotConfig.Rows; row++)
                cells[TopBuffer + row].SetSymbol(column[row], library);
            Apply();
        }

        /// <summary>
        /// Spins the reel and finishes showing <paramref name="target"/> on the visible rows.
        /// </summary>
        /// <param name="target">The symbols to land on, from the top row to the bottom row.</param>
        /// <param name="spinTime">Seconds of free spinning before the reel starts to stop.</param>
        public IEnumerator Spin(SymbolId[] target, float spinTime)
        {
            if (cellHeight <= 0.01f)
            {
                Layout();
                if (cellHeight <= 0.01f)
                {
                    Debug.LogError("Reel has no height, check the reel layout.");
                    yield break;
                }
            }

            // The target rows plus the extra hidden cells have to be fed in before the reel can settle.
            int totalFeeds = SlotConfig.Rows + TopBuffer - 1;
            int fed = 0;
            float elapsed = 0f;
            bool stopping = false;
            offset = 0f;

            while (true)
            {
                elapsed += Time.deltaTime;
                if (!stopping && elapsed >= spinTime)
                    stopping = true;

                offset += spinSpeed * cellHeight * Time.deltaTime;

                // Every time the content has scrolled one cell, shift the symbols down by one.
                while (offset >= cellHeight && fed < totalFeeds)
                {
                    offset -= cellHeight;
                    if (stopping)
                    {
                        // Symbols move down, so the bottom row has to enter first: feed the target in reverse.
                        Shift(fed < SlotConfig.Rows ? target[SlotConfig.Rows - 1 - fed] : RandomFiller());
                        fed++;
                    }
                    else
                    {
                        Shift(RandomFiller());
                    }
                }

                if (fed >= totalFeeds)
                    break;

                if (offset >= cellHeight)
                    offset -= cellHeight;

                Apply();
                yield return null;
            }

            // Ease the last bit of scrolling into place. OutBack overshoots slightly and settles back.
            offset = Mathf.Min(offset, cellHeight);
            yield return DOVirtual.Float(offset, cellHeight, settleTime, SetOffset)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject)
                .WaitForCompletion();

            // The content has now moved one full cell: apply that as a final shift and snap to rest.
            offset = 0f;
            Shift(RandomFiller());
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (cells != null)
                Layout();
        }

        /// <summary>Sizes every cell to one row of the reel and repositions them.</summary>
        private void Layout()
        {
            cellHeight = rect.rect.height / SlotConfig.Rows;
            var size = new Vector2(rect.rect.width, cellHeight);
            for (int i = 0; i < cells.Length; i++)
                cells[i].Rect.sizeDelta = size;
            Apply();
        }

        /// <summary>Places each cell according to its slot and the current scroll offset.</summary>
        private void Apply()
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i].Rect.anchoredPosition = new Vector2(0f, -(i - TopBuffer + 0.5f) * cellHeight - offset);
        }

        /// <summary>Moves every symbol down one slot and puts a new symbol in the top slot.</summary>
        private void Shift(SymbolId incoming)
        {
            for (int i = cells.Length - 1; i > 0; i--)
                cells[i].SetSymbol(cells[i - 1].Symbol, library);
            cells[0].SetSymbol(incoming, library);
        }

        private void SetOffset(float value)
        {
            offset = value;
            Apply();
        }

        /// <summary>A random regular symbol, used for the blur while spinning. It is never part of the result.</summary>
        private static SymbolId RandomFiller()
        {
            return (SymbolId)UnityEngine.Random.Range(0, SlotConfig.RegularSymbolCount);
        }
    }
}