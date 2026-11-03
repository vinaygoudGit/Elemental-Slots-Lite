namespace ElementalSlots
{
    /// <summary>
    /// Every symbol in the game. The numeric order matters: the first ten are the regular
    /// symbols (index = row in <see cref="SlotConfig.Pays"/>), and the index also selects
    /// the column in <see cref="SlotConfig.Weights"/>.
    /// </summary>
    public enum SymbolId
    {
        // Regular symbols, ordered from highest to lowest pay.
        Fire,
        Water,
        Earth,
        Air,
        Lava,
        Ice,
        Leaf,
        Cloud,
        Gem,
        Rune,

        // Special symbols.
        Wild,
        Scatter,
        Bonus
    }
}