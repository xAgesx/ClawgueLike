using UnityEngine;

// Greedy: every coin that reaches the chute while this coin is still in the machine
// feeds it a point of value. The asset's maxApplications of 7 is the "+7" cap; after
// that it goes quiet and grey like any other spent effect.
[CreateAssetMenu(fileName = "GreedyCoin", menuName = "Data/Effects/SpecialCoins/Greedy")]
public class GreedyEffect : CoinEffect {
    [Tooltip("Value gained each time another coin is collected.")]
    [SerializeField] private int valuePerCoin = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnCoinCollected(CoinInstance coin, SpecialCoin source, CoinInstance collected) {
        if (coin == null) return;
        coin.AddBaseValue(valuePerCoin);
    }
}
