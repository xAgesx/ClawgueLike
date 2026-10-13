using UnityEngine;

[CreateAssetMenu(fileName = "Infection", menuName = "Data/Effects/SpecialCoins/Infection")]
public class InfectionEffect : CoinEffect {
    [Tooltip("Factor applied to the target coin's baseValue. 2 = double it.")]
    [SerializeField] private float factor = 2f;
    [SerializeField] private Color tint = Color.red;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        target.MultiplyBaseValue(factor);
        target.SetTint(tint);
    }
}
