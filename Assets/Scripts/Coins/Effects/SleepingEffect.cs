using UnityEngine;

// Sleeping: quietly gains a point of value on every turn boundary, from the turn it
// arrives. Never fires on contact, so TriggersOnContact is off on the asset - a bump
// cannot burn one of its stacks.
[CreateAssetMenu(fileName = "SleepingCoin", menuName = "Data/Effects/SpecialCoins/Sleeping")]
public class SleepingEffect : CoinEffect {
    [Tooltip("Value gained per turn since it spawned.")]
    [SerializeField] private int valuePerTurn = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null || source == null) return;

        coin.AddBaseValue(valuePerTurn);
    }
}
