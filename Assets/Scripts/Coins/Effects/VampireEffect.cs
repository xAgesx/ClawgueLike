using UnityEngine;

// Vampire: drinks a point of value out of every coin it touches and keeps it for
// good. Nothing can be drained below one, so a pile of pocket change feeds it slowly
// instead of evaporating. Applications are uncapped on the asset - the whole point is
// that it fattens up for as long as it keeps meeting new coins.
[CreateAssetMenu(fileName = "VampireCoin", menuName = "Data/Effects/SpecialCoins/Vampire")]
public class VampireEffect : CoinEffect {
    [Tooltip("Base value taken per coin it drinks from.")]
    [SerializeField] private int stealAmount = 1;

    [Tooltip("A donor is never drained below this, so no coin can be emptied out of existence.")]
    [SerializeField] private int minTargetValue = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null || source == null || source.Self == null) return;
        if (target == source.Self) return;

        int available = target.BaseValue - minTargetValue;
        if (available <= 0) return;

        int stolen = Mathf.Min(stealAmount, available);

        // Drain first, then take what the donor could actually spare - AddBaseValue
        // clamps at one, so asking for more than the donor has would create value.
        target.AddBaseValue(-stolen);
        source.Self.AddBaseValue(stolen);
    }
}
