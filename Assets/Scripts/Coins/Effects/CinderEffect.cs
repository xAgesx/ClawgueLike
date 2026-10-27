using UnityEngine;

// Cinder: a support coin. Every distinct neighbour it touches gets a permanent point
// of value, but the cinder itself pays nothing when it finally drops into the chute.
[CreateAssetMenu(fileName = "CinderCoin", menuName = "Data/Effects/SpecialCoins/Cinder")]
public class CinderEffect : CoinEffect {
    [Tooltip("Value handed to each new coin it touches.")]
    [SerializeField] private int valuePerTouch = 1;

    [SerializeField] private Color tint = new Color(1f, 0.55f, 0.1f);

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null) return;

        target.AddBaseValue(valuePerTouch);
        target.SetTint(tint, 0.35f);
    }

    public override int OnCollected(CoinInstance coin, SpecialCoin source, int payout) {
        return 0;
    }
}
