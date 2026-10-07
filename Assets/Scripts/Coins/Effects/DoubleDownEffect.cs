using UnityEngine;

[CreateAssetMenu(fileName = "DoubleDown", menuName = "Data/Effects/SpecialCoins/Double Down")]
public class DoubleDownEffect : CoinEffect {
    [Tooltip("Factor applied to the target coin's baseValue. 2 = double it.")]
    [SerializeField] private float factor = 2f;
    [SerializeField] private Color tint = Color.red;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        target.MultiplyBaseValue(factor);
        target.SetTint(tint);
    }
}
