using UnityEngine;

// Seed: when it reaches the chute it plants a small pod at that spot. On the next
// turn boundary the pod sprouts a normal coin worth 3. The seed itself still pays
// its own value now; the pod is the delayed bonus.
[CreateAssetMenu(fileName = "SeedCoin", menuName = "Data/Effects/SpecialCoins/Seed")]
public class SeedEffect : CoinEffect {
    [Tooltip("The coin the pod grows into. Normally the plain Coin asset.")]
    [SerializeField] private CoinData sproutCoin;

    [Tooltip("Value of the coin that sprouts next turn.")]
    [SerializeField] private int sproutValue = 3;

    [SerializeField] private Color sproutTint = new Color(0.3f, 0.9f, 0.35f);

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        if (coin == null) return payout;

        DelayedCoinPod.Schedule(sproutCoin, coin.transform.position, sproutValue, 1, sproutTint);
        return payout;
    }
}
