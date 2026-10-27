using UnityEngine;

// Hydra: cuts the coin it touches into two half-value copies, then removes the original.
// The pieces use the same CoinData, so a split special coin gets a fresh, unspent copy
// of its ability. Spawn delays stop the pieces from splitting each other immediately.
[CreateAssetMenu(fileName = "HydraCoin", menuName = "Data/Effects/SpecialCoins/Hydra")]
public class HydraEffect : CoinEffect {
    [Tooltip("How many pieces the target is split into.")]
    [SerializeField] private int pieces = 2;

    [Tooltip("Each piece is worth at least this much, so a value-1 coin still divides.")]
    [SerializeField] private int minPieceValue = 1;

    [Tooltip("How far the pieces are scattered from the original.")]
    [SerializeField] private float scatterRadius = 0.25f;

    [Tooltip("Upward nudge given to the pieces so they do not spawn inside the pile.")]
    [SerializeField] private float spawnLift = 0.25f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null || target.Data == null) return;

        ItemSpawner spawner = ItemSpawner.Instance;
        if (spawner == null) return;

        int pieceValue = Mathf.Max(minPieceValue, target.BaseValue / Mathf.Max(1, pieces));
        Vector3 origin = target.transform.position;
        CoinData data = target.Data;

        // Despawn after spawning: SpawnCoinAt may reuse a pooled object, and the target
        // itself must still be valid while its data and position are read.
        for (int i = 0; i < pieces; i++) {
            Vector3 offset = Random.insideUnitSphere * scatterRadius;
            offset.y = Mathf.Abs(offset.y) + spawnLift;
            spawner.SpawnCoinAt(data, origin + offset, pieceValue);
        }

        spawner.DespawnCoin(target.gameObject);
    }
}
