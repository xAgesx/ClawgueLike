using UnityEngine;

// Roulette: a coin-flip payout. On a win the value is doubled; on a loss the whole
// payout is zero. It is deliberately all-or-nothing, not a partial payout.
[CreateAssetMenu(fileName = "RouletteCoin", menuName = "Data/Effects/SpecialCoins/Roulette")]
public class RouletteEffect : CoinEffect {
    [Tooltip("Chance to win, 0.5 = 50/50.")]
    [SerializeField, Range(0f, 1f)] private float winChance = 0.5f;

    [Tooltip("Multiplier applied to the payout on a win.")]
    [SerializeField] private int winMultiplier = 2;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        return Random.value <= winChance ? payout * Mathf.Max(1, winMultiplier) : 0;
    }
}
