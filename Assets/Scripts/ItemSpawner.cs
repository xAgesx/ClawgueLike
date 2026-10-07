using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    public static ItemSpawner Instance { get; private set; }

    [Header("Normal Coins")]
    [SerializeField] private CoinData[] normalCoinPool;
    [SerializeField] private int normalCoinsPerRound = 10;

    [Header("Inventory Coins (spawned per turn)")]
    [SerializeField] private int inventoryCoinsPerTurn = 4;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private Transform parentContainer;
    [SerializeField] private int initialPoolSize = 100;

    [Header("Debug")]
    public int spawnedCount;

    private bool isSpawning;
    private Dictionary<ItemData, ObjectPool> pools = new Dictionary<ItemData, ObjectPool>();
    private List<GameObject> activeItems = new List<GameObject>();

    private void OnValidate() {
    }

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Start() {
        if (spawnPoints == null || spawnPoints.Length < 3) {
            Debug.LogError("ItemSpawner requires at least 3 spawn points.");
            return;
        }

        if (normalCoinPool == null || normalCoinPool.Length == 0) {
            Debug.LogError("ItemSpawner requires normal coin pool.");
            return;
        }

        InitializePools();
    }

    private void InitializePools() {
        if (normalCoinPool != null) {
            foreach (CoinData item in normalCoinPool) {
                if (!pools.ContainsKey(item)) {
                    GameObject poolParent = new GameObject($"Pool_{item.itemName}");
                    poolParent.transform.SetParent(transform);
                    pools[item] = new ObjectPool(item.prefab, initialPoolSize, poolParent.transform);
                }
            }
        }
    }

    // initialSize < 0 falls back to the scene-wide pool size. Effect-spawned coins pass
    // a small number instead: ObjectPool pre-creates every instance in its constructor,
    // so instantly building 100 prefabs in the middle of a physics callback is a hitch.
    private void EnsurePoolExists(ItemData itemData, int initialSize = -1) {
        if (!pools.ContainsKey(itemData)) {
            GameObject poolParent = new GameObject($"Pool_{itemData.itemName}");
            poolParent.transform.SetParent(transform);
            int size = initialSize >= 0 ? initialSize : initialPoolSize;
            pools[itemData] = new ObjectPool(itemData.prefab, size, poolParent.transform);
        }
    }

    /// <summary>
    /// Spawns normal coins only (called at start of each round)
    /// </summary>
    public void SpawnNormalCoins() {
        if (isSpawning) return;
        if (normalCoinPool == null || normalCoinPool.Length == 0) return;

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnNormalCoinsRoutine());
    }

    /// <summary>
    /// Spawns coins from player inventory (called when shop exits / new turn starts)
    /// </summary>
    public void SpawnInventoryCoins() {
        if (GameManager.Instance == null) return;

        var inventory = GameManager.Instance.Inventory;
        if (inventory == null || inventory.Count == 0) {
            Debug.Log("[ItemSpawner] No inventory items to spawn");
            return;
        }

        StartCoroutine(SpawnInventoryWhenFree());
    }

    // SpawnNormalCoins is still running for ~5s at round start. Exiting the shop
    // before it finishes used to silently drop the whole request, so wait our turn.
    private IEnumerator SpawnInventoryWhenFree() {
        while (isSpawning) yield return null;

        isSpawning = true;
        yield return SpawnInventoryCoinsRoutine();
        isSpawning = false;
    }

    /// <summary>
    /// Spawns normal coins first, then inventory coins (called at end of round)
    /// </summary>
    public void SpawnRoundAndInventoryCoins() {
        if (isSpawning) return;

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnRoundAndInventoryRoutine());
    }

    public void StopSpawning() {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void ClearAllItems() {
        foreach (GameObject item in activeItems) {
            if (item != null) {
                ItemData itemData = GetItemDataByName(item.name);
                if (itemData != null && pools.ContainsKey(itemData)) {
                    pools[itemData].Return(item);
                }
            }
        }
        activeItems.Clear();
        spawnedCount = 0;
    }

    private IEnumerator SpawnNormalCoinsRoutine() {
        int count = normalCoinsPerRound;
        if (normalCoinPool == null || normalCoinPool.Length == 0) {
            isSpawning = false;
            yield break;
        }

        while (spawnedCount < count) {
            SpawnNormalCoin();
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private IEnumerator SpawnInventoryCoinsRoutine() {
        // Special coins carry effects and are the whole point of buying them, so
        // they always spawn. Regular bonus coins stay capped per turn.
        foreach (CoinData special in GetInventoryCoins(true)) {
            SpawnInventoryCoin(special);
            GameManager.Instance.SpendItem(special, 1);
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        List<CoinData> coins = GetInventoryCoins(false);
        int count = Mathf.Min(inventoryCoinsPerTurn, coins.Count);
        for (int i = 0; i < count; i++) {
            int index = Random.Range(0, coins.Count);
            CoinData itemData = coins[index];
            coins.RemoveAt(index);
            SpawnInventoryCoin(itemData);
            GameManager.Instance.SpendItem(itemData, 1);
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private List<CoinData> GetInventoryCoins(bool special) {
        List<CoinData> result = new List<CoinData>();
        var inventory = GameManager.Instance?.Inventory;
        if (inventory == null) return result;

        foreach (var invItem in inventory) {
            CoinData coin = invItem.itemData as CoinData;
            if (coin == null || coin.IsSpecial != special) continue;
            for (int i = 0; i < invItem.quantity; i++) {
                result.Add(coin);
            }
        }
        return result;
    }

    private IEnumerator SpawnRoundAndInventoryRoutine() {
        // First spawn normal coins
        if (normalCoinPool != null && normalCoinPool.Length > 0) {
            while (spawnedCount < normalCoinsPerRound) {
                SpawnNormalCoin();
                spawnedCount++;
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawning = false;
    }

    private void SpawnNormalCoin() {
        if (normalCoinPool == null || normalCoinPool.Length == 0) return;

        CoinData itemData = normalCoinPool[Random.Range(0, normalCoinPool.Length)];
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(spawnPosition, spawnRotation);
        item.name = itemData.itemName;
        BindItemData(item, itemData);
        activeItems.Add(item);
    }

    private void SpawnInventoryCoin(CoinData itemData) {
        if (itemData == null) return;

        EnsurePoolExists(itemData);
        
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(spawnPosition, spawnRotation);
        item.name = itemData.itemName;
        BindItemData(item, itemData);
        activeItems.Add(item);
    }

    /// <summary>
    /// Spawns a coin at an exact spot with an exact value. Used by effects that create
    /// coins mid-game (a rabbit breeding, a mine bursting) rather than by the shop.
    /// </summary>
    public GameObject SpawnCoinAt(CoinData itemData, Vector3 position, int baseValue) {
        if (itemData == null) return null;

        EnsurePoolExists(itemData, 4);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        item.name = itemData.itemName;
        BindItemData(item, itemData);
        activeItems.Add(item);

        // BindItemData has just reset the value from the asset, so override it after.
        if (baseValue > 0) {
            CoinInstance coin = item.GetComponent<CoinInstance>();
            if (coin != null) coin.SetBaseValue(baseValue);
        }
        return item;
    }

    // Guarantees the spawned object points back at the asset it came from and that
    // its runtime CoinInstance has this round's baseValue. Adding CoinInstance for
    // anyone who forgot it on the prefab is cheaper than a silent payout of 1.
    private void BindItemData(GameObject item, ItemData itemData) {
        ItemPickup pickup = item.GetComponent<ItemPickup>();
        if (pickup != null) pickup.SetItemData(itemData);

        CoinData coinData = itemData as CoinData;
        if (coinData == null) return;

        CoinInstance coin = item.GetComponent<CoinInstance>();
        if (coin == null) coin = item.AddComponent<CoinInstance>();
        coin.Initialize(coinData);
    }

    private ItemData GetItemDataByName(string name) {
        foreach (CoinData item in normalCoinPool) {
            if (item.itemName == name) return item;
        }
        foreach (var kvp in pools) {
            if (kvp.Key.itemName == name) return kvp.Key;
        }
        return null;
    }

    private Vector3 CalculateSpawnPosition() {
        Transform pointA = spawnPoints[0];
        Transform pointB = spawnPoints[1];
        Transform pointC = spawnPoints[2];

        float t = Random.Range(0f, 1f);
        Vector3 ab = Vector3.Lerp(pointA.position, pointB.position, t);
        Vector3 bc = Vector3.Lerp(pointB.position, pointC.position, t);
        Vector3 ac = Vector3.Lerp(pointA.position, pointC.position, t);

        float u = Random.Range(0f, 1f);
        Vector3 abToBc = Vector3.Lerp(ab, bc, u);
        Vector3 abToAc = Vector3.Lerp(ab, ac, u);

        return Vector3.Lerp(abToBc, abToAc, Random.Range(0f, 1f));
    }
}