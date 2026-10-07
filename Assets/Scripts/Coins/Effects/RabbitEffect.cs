using UnityEngine;

// Breeding: two rabbits meet and the one using the ability leaves a smaller rabbit
// behind, then goes quiet (maxApplications on the asset handles the "once only").
// The effect asset stays stateless - the counter lives on SpecialCoin.
[CreateAssetMenu(fileName = "RabbitCoin", menuName = "Data/Effects/SpecialCoins/Rabbit")]
public class RabbitEffect : CoinEffect {
    [Tooltip("The coin left behind when two rabbits meet.")]
    [SerializeField] private CoinData offspring;

    [Tooltip("Fraction of the breeding rabbit's TotalValue the newborn starts with.")]
    [SerializeField, Range(0f, 1f)] private float offspringFraction = 0.5f;

    [Tooltip("Newborns are never worth less than this.")]
    [SerializeField] private int minOffspringValue = 1;

    [Tooltip("Lifts the newborn clear of the two parents so it does not spawn inside them.")]
    [SerializeField] private float spawnHeight = 0.5f;

    [Tooltip("Both parents breed from the same contact, so each newborn is nudged sideways or they would spawn on top of each other.")]
    [SerializeField] private float spawnJitter = 0.4f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (offspring == null || target == null) return;
        if (source == null || source.Self == null) return;

        ItemSpawner spawner = ItemSpawner.Instance;
        if (spawner == null) {
            Debug.LogWarning("[RabbitEffect] No ItemSpawner in the scene - cannot spawn an offspring.");
            return;
        }

        int value = Mathf.Max(minOffspringValue,
            Mathf.FloorToInt(source.Self.TotalValue * offspringFraction));

        // Born between the two parents, lifted clear and nudged sideways.
        Vector3 position = Vector3.Lerp(source.transform.position, target.transform.position, 0.5f);
        position += new Vector3(
            Random.Range(-spawnJitter, spawnJitter),
            spawnHeight,
            Random.Range(-spawnJitter, spawnJitter));

        spawner.SpawnCoinAt(offspring, position, value);
    }
}
