using UnityEngine;

// Cloner: at the start of each turn it copies the cheapest coin currently in the
// machine. Normal coins make more fodder; a cheap special coin can snowball into a
// second copy of its ability. It never clones itself, so a lone cloner waits.
[CreateAssetMenu(fileName = "ClonerCoin", menuName = "Data/Effects/SpecialCoins/Cloner")]
public class ClonerEffect : CoinEffect {
    [Tooltip("How far above the cloner the copy is placed.")]
    [SerializeField] private float spawnHeight = 0.45f;

    [Tooltip("Sideways scatter so the copy does not spawn inside the original.")]
    [SerializeField] private float scatterRadius = 0.2f;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        CoinInstance cheapest = null;
        foreach (CoinInstance other in FindObjectsByType<CoinInstance>(FindObjectsInactive.Exclude)) {
            if (other == null || other == coin) continue;
            if (cheapest == null || other.TotalValue < cheapest.TotalValue) cheapest = other;
        }

        if (cheapest == null || cheapest.Data == null) return;

        Vector3 offset = Random.insideUnitSphere * scatterRadius;
        offset.y = Mathf.Abs(offset.y) + spawnHeight;
        ItemSpawner.Instance?.SpawnCoinAt(cheapest.Data, coin.transform.position + offset, cheapest.BaseValue);
    }
}
