using UnityEngine;

// Mutation: every new coin it touches gets one permanent change, rolled from a table
// that is deliberately half blessing and half curse. The change is applied to the
// target, not the mutation coin, so a mutation is a gamble for the pile around it.
[CreateAssetMenu(fileName = "MutationCoin", menuName = "Data/Effects/SpecialCoins/Mutation")]
public class MutationEffect : CoinEffect {
    [Tooltip("Amount added or removed by the flat-value outcomes.")]
    [SerializeField] private int valueDelta = 1;

    [Tooltip("Factor used by the multiplicative base-value outcomes.")]
    [SerializeField] private float baseFactor = 1.5f;

    [Tooltip("Factor used by the multiplicative multiplier outcomes.")]
    [SerializeField] private float multiplierFactor = 1.25f;

    [SerializeField] private Color tint = new Color(0.6f, 0.2f, 0.9f);

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null) return;

        switch (Random.Range(0, 6)) {
            case 0:
                target.AddBaseValue(valueDelta);
                break;
            case 1:
                target.MultiplyBaseValue(baseFactor);
                break;
            case 2:
                target.MultiplyMultiplier(multiplierFactor);
                break;
            case 3:
                target.DrainBaseValue(valueDelta);
                break;
            case 4:
                target.MultiplyBaseValue(1f / baseFactor);
                break;
            default:
                target.MultiplyMultiplier(1f / multiplierFactor);
                break;
        }

        target.SetTint(tint, 0.45f);
    }
}
