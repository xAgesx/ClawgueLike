using UnityEngine;

// Banker: rounds the turn's running collected total up to the next multiple of ten.
// The bonus is whatever it takes to reach that boundary, so a 9-value banker pays 1
// extra and a 14-value banker pays 6. It checks the total before this coin is added.
[CreateAssetMenu(fileName = "BankerCoin", menuName = "Data/Effects/SpecialCoins/Banker")]
public class BankerEffect : CoinEffect {
    [Tooltip("The total is rounded up to a multiple of this value.")]
    [SerializeField] private int roundTo = 10;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        if (roundTo <= 1) return payout;

        int total = ChuteDrop.ValueCollectedThisTurn + payout;
        int rounded = Mathf.CeilToInt(total / (float)roundTo) * roundTo;
        return payout + Mathf.Max(0, rounded - total);
    }
}
