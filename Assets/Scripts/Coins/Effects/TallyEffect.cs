using UnityEngine;

// Tally: pays a bonus based on how many coins have already reached the chute this
// turn, doubled. The chute increments its counter before asking for a payout, so the
// tally includes the coin that is being collected.
[CreateAssetMenu(fileName = "TallyCoin", menuName = "Data/Effects/SpecialCoins/Tally")]
public class TallyEffect : CoinEffect {
    [Tooltip("Coins paid per coin already collected this turn.")]
    [SerializeField] private int coinsPerPayout = 1;

    [Tooltip("Multiplier applied to the tally bonus.")]
    [SerializeField] private int bonusMultiplier = 2;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        int bonus = coinsPerPayout * bonusMultiplier * ChuteDrop.CoinsCollectedThisTurn;
        return payout + Mathf.Max(0, bonus);
    }
}
