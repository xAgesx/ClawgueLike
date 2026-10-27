using UnityEngine;

// Black Hole: swallows any coin worth enough to be interesting and keeps half of its
// base value. Cheap coins pass through untouched, so a pile of pocket change does not
// turn the black hole into an infinite value sink.
[CreateAssetMenu(fileName = "BlackHoleCoin", menuName = "Data/Effects/SpecialCoins/BlackHole")]
public class BlackHoleEffect : CoinEffect {
    [Tooltip("Only coins at or above this TotalValue are swallowed.")]
    [SerializeField] private int minValue = 4;

    [Tooltip("Fraction of the swallowed coin's base value kept by the black hole.")]
    [SerializeField, Range(0f, 1f)] private float keepFraction = 0.5f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null || source == null || source.Self == null) return;
        if (target.TotalValue < minValue) return;

        int keep = Mathf.Max(1, Mathf.FloorToInt(target.BaseValue * keepFraction));
        source.Self.AddBaseValue(keep);

        ItemSpawner spawner = ItemSpawner.Instance;
        if (spawner != null) spawner.DespawnCoin(target.gameObject);
        else Destroy(target.gameObject);
    }
}
