using UnityEngine;

// Caterpillar: a fresh coin that reaches the chute on its first turn cocoons instead
// of paying. The cocoon hatches next turn into a normal coin worth three times the
// original payout. A caterpillar that has already survived a turn pays normally.
[CreateAssetMenu(fileName = "CaterpillarCoin", menuName = "Data/Effects/SpecialCoins/Caterpillar")]
public class CaterpillarEffect : CoinEffect {
    [Tooltip("The coin the cocoon hatches into. Normally the plain Coin asset.")]
    [SerializeField] private CoinData hatchCoin;

    [Tooltip("Value multiplier applied to the cocooned payout.")]
    [SerializeField] private float valueMultiplier = 3f;

    [Tooltip("The hatched coin is never worth less than this.")]
    [SerializeField] private int minHatchValue = 1;

    [SerializeField] private Color cocoonTint = new Color(1f, 0.85f, 0.2f);

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        if (coin == null || source == null) return payout;

        // turnsSurvived is 1 for a coin that was present at the turn boundary it
        // spawned into. If it survived into a second turn, it is no longer "fresh".
        if (source.TurnsSurvived > 1) return payout;

        int hatchValue = Mathf.Max(minHatchValue, Mathf.RoundToInt(payout * valueMultiplier));
        DelayedCoinPod.Schedule(hatchCoin, coin.transform.position, hatchValue, 1, cocoonTint);
        return 0;
    }
}
