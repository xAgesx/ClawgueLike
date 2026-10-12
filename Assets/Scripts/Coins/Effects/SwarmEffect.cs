using UnityEngine;

// Swarm: brushing against any coin shakes loose a litter of TinyCoins. Capped by
// maxApplications on the asset so a swarm coin sitting in a pile cannot fill the
// machine with pocket change.
[CreateAssetMenu(fileName = "SwarmCoin", menuName = "Data/Effects/SpecialCoins/Swarm")]
public class SwarmEffect : CoinEffect {
    [Tooltip("The little coin left behind on contact.")]
    [SerializeField] private CoinData tinyCoin;

    [Tooltip("How many spawn per contact.")]
    [SerializeField] private int count = 3;

    [Tooltip("Value each tiny coin is worth.")]
    [SerializeField] private int tinyValue = 1;

    [Tooltip("How far from the swarm coin the litter is scattered.")]
    [SerializeField] private float scatterRadius = 0.3f;

    [Tooltip("Height the tiny coins pop out at.")]
    [SerializeField] private float spawnHeight = 0.4f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (tinyCoin == null || source == null) return;

        ItemSpawner spawner = ItemSpawner.Instance;
        if (spawner == null) {
            Debug.LogWarning("[SwarmEffect] No ItemSpawner in the scene - cannot spawn TinyCoins.");
            return;
        }

        Vector3 origin = source.transform.position;
        for (int i = 0; i < count; i++) {
            Vector3 offset = Random.insideUnitSphere * scatterRadius;
            Vector3 position = origin + new Vector3(offset.x, spawnHeight, offset.z);
            spawner.SpawnCoinAt(tinyCoin, position, tinyValue);
        }
    }
}
