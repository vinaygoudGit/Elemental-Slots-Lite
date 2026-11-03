using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalSlots
{
    /// <summary>
    /// Maps each <see cref="SymbolId"/> to the sprite and tint used to draw it.
    /// A symbol without a sprite is drawn as a tinted tile with its name, which keeps the
    /// game playable before the art exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Elemental Slots/Symbol Sprite Library", fileName = "SymbolSpriteLibrary")]
    public sealed class SymbolSpriteLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public SymbolId Id;
            public Sprite Sprite;
            public Color Tint;
        }

        [SerializeField] private Entry[] entries;

        [NonSerialized] private Dictionary<SymbolId, Entry> lookup;

        public Entry Get(SymbolId id)
        {
            // The dictionary is built on first use and rebuilt whenever the entries change.
            if (lookup == null || lookup.Count != entries.Length)
            {
                lookup = new Dictionary<SymbolId, Entry>();
                for (int i = 0; i < entries.Length; i++)
                    lookup[entries[i].Id] = entries[i];
            }

            Entry entry;
            return lookup.TryGetValue(id, out entry) ? entry : new Entry { Id = id, Tint = Color.gray };
        }

        /// <summary>Drops the cached lookup when an entry is edited in the Inspector, even during Play mode.</summary>
        private void OnValidate()
        {
            lookup = null;
        }

        /// <summary>Called when the asset is created: fills one entry per symbol with a placeholder tint.</summary>
        private void Reset()
        {
            var tints = new[]
            {
                new Color(0.93f, 0.42f, 0.28f),
                new Color(0.25f, 0.55f, 0.90f),
                new Color(0.45f, 0.70f, 0.30f),
                new Color(0.60f, 0.85f, 0.85f),
                new Color(0.80f, 0.25f, 0.15f),
                new Color(0.55f, 0.80f, 0.95f),
                new Color(0.30f, 0.60f, 0.25f),
                new Color(0.75f, 0.78f, 0.85f),
                new Color(0.70f, 0.40f, 0.85f),
                new Color(0.60f, 0.55f, 0.40f),
                new Color(0.95f, 0.78f, 0.20f),
                new Color(0.90f, 0.30f, 0.60f),
                new Color(0.50f, 0.50f, 0.50f)
            };

            entries = new Entry[SlotConfig.SymbolCount];
            for (int i = 0; i < entries.Length; i++)
                entries[i] = new Entry { Id = (SymbolId)i, Tint = tints[i] };
        }
    }
}