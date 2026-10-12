using UnityEngine;

// Snowball: every distinct coin it touches packs on another point of value. The
// "distinct" part is free - oncePerTarget stops it ever doubling back on the same
// partner - and maxApplications on the asset supplies the ceiling.
[CreateAssetMenu(fileName = "SnowballCoin", menuName = "Data/Effects/SpecialCoins/Snowball")]
public class SnowballEffect : CoinEffect {
    [Tooltip("Value gained for each new coin it rolls into.")]
    [SerializeField] private int valuePerTouch = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (source == null || source.Self == null) return;

        source.Self.AddBaseValue(valuePerTouch);
    }
}
