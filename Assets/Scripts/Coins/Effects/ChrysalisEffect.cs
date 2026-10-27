using UnityEngine;

// Chrysalis: survives a full turn without being disturbed, then transforms into a
// random special coin from the shop pool. The first turn is always a grace period;
// after that, any movement resets the countdown.
[CreateAssetMenu(fileName = "ChrysalisCoin", menuName = "Data/Effects/SpecialCoins/Chrysalis")]
public class ChrysalisEffect : CoinEffect {
    [Tooltip("Value the replacement starts with; 0 means carry the chrysalis's base value.")]
    [SerializeField] private int replacementValue = 0;

    [SerializeField] private float spawnHeight = 0.35f;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null || source == null) return;

        // The first call has no previous position, so it reports "moved" and gives the
        // coin a full turn of grace. Later calls report the truth.
        if (source.HasMovedSinceLastTurn(this)) return;

        CoinData replacement = ShopManager.Instance != null ? ShopManager.Instance.GetRandomSpecialCoin() : null;
        if (replacement == null) return;

        int value = replacementValue > 0 ? replacementValue : coin.BaseValue;
        ItemSpawner spawner = ItemSpawner.Instance;

        if (spawner != null) {
            spawner.SpawnCoinAt(replacement, coin.transform.position + Vector3.up * spawnHeight, value);
            spawner.DespawnCoin(coin.gameObject);
        }
    }
}
