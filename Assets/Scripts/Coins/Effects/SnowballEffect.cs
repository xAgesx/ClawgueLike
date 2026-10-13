using UnityEngine;

// Snowball: every distinct coin it touches packs on another point of value, and once
// it has rolled into enough new coins the ball itself swells a little. The "distinct"
// part is free - oncePerTarget stops it ever doubling back on the same partner. Value
// is uncapped; only the size has a ceiling, set here so the ball cannot grow forever.
[CreateAssetMenu(fileName = "SnowballCoin", menuName = "Data/Effects/SpecialCoins/Snowball")]
public class SnowballEffect : CoinEffect {
    [Tooltip("Value gained for each new coin it rolls into.")]
    [SerializeField] private int valuePerTouch = 1;

    [Tooltip("How many distinct coins it must roll into before it grows once.")]
    [SerializeField] private int coinsPerGrowth = 3;

    [Tooltip("Fraction of its current size added each time it grows.")]
    [SerializeField] private float sizeGainPerTouch = 0.1f;

    [Tooltip("Hard cap on the ball's size, as a multiple of its normal size.")]
    [SerializeField] private float maxSizeMultiplier = 3f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (source == null || source.Self == null) return;

        source.Self.AddBaseValue(valuePerTouch);

        // The counter is bumped after Apply returns, so this application is the next one.
        int touches = source.ApplicationsFor(this) + 1;
        if (coinsPerGrowth > 0 && touches % coinsPerGrowth == 0) {
            float grown = source.Self.SizeMultiplier * (1f + sizeGainPerTouch);
            source.Self.SetSizeMultiplier(Mathf.Min(maxSizeMultiplier, grown));
        }
    }
}
