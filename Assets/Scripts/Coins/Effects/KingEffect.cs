using UnityEngine;

// King: crowns the first coin it touches for a 50% value boost. Crowns are exclusive -
// crowning a new coin strips the old one, so exactly one crowned coin exists at a time.
[CreateAssetMenu(fileName = "KingCoin", menuName = "Data/Effects/SpecialCoins/King")]
public class KingEffect : CoinEffect {
    [Tooltip("Multiplier applied to the crowned coin.")]
    [SerializeField] private float crownMultiplier = 1.5f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null) return;

        // Remove any crown already on this target, then any crown elsewhere. The
        // previous holder keeps its other modifiers; only the crown's factor is undone.
        // GetComponents rather than GetComponent also clears inert leftovers from a
        // pooled coin that was crowned in a previous life.
        foreach (CrownedCoin stale in target.GetComponents<CrownedCoin>()) stale.RemoveCrown();
        if (CrownedCoin.Current != null) CrownedCoin.Current.RemoveCrown();

        target.gameObject.AddComponent<CrownedCoin>().Apply(crownMultiplier);
    }
}
