using UnityEngine;

// Skeleton: worthless on its own, but collecting it banks a free shop reroll for the
// next visit. The reroll is banked because the shop is closed while the coin drops.
[CreateAssetMenu(fileName = "SkeletonCoin", menuName = "Data/Effects/SpecialCoins/Skeleton")]
public class SkeletonEffect : CoinEffect {
    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        ShopManager.Instance?.GrantFreeReroll();
        return 0;
    }
}
