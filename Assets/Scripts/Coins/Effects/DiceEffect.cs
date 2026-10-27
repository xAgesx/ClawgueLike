using UnityEngine;

// Dice: the payout is replaced by a roll from 1 to 6. Its own base value never
// matters, which makes it a cheap lottery ticket rather than a value carrier.
[CreateAssetMenu(fileName = "DiceCoin", menuName = "Data/Effects/SpecialCoins/Dice")]
public class DiceEffect : CoinEffect {
    [SerializeField] private int minRoll = 1;
    [SerializeField] private int maxRoll = 6;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        int low = Mathf.Min(minRoll, maxRoll);
        int high = Mathf.Max(minRoll, maxRoll);
        return Random.Range(low, high + 1);
    }
}
