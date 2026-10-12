using UnityEngine;

// Crystal: grows a point of value for every full turn it goes undisturbed. Never
// fires on contact, so TriggersOnContact is off on the asset - a bump cannot burn
// one of its stacks.
[CreateAssetMenu(fileName = "CrystalCoin", menuName = "Data/Effects/SpecialCoins/Crystal")]
public class CrystalEffect : CoinEffect {
    [Tooltip("Value gained per undisturbed turn.")]
    [SerializeField] private int valuePerTurn = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null || source == null) return;

        // Anything that shifted the coin last turn counts as touching it, so the
        // streak breaks and it earns nothing this time round.
        if (source.HasMovedSinceLastTurn(this)) return;

        coin.AddBaseValue(valuePerTurn);
    }
}
