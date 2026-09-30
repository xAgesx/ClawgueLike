using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    [Header("Normal Coins")]
    [SerializeField] private ItemData[] normalCoinPool;
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
            foreach (ItemData item in normalCoinPool) {
                if (!pools.ContainsKey(item)) {
                    GameObject poolParent = new GameObject($"Pool_{item.itemName}");
                    poolParent.transform.SetParent(transform);
                    pools[item] = new ObjectPool(item.prefab, initialPoolSize, poolParent.transform);
                }
            }
        }
    }

    private void EnsurePoolExists(ItemData itemData) {
        if (!pools.ContainsKey(itemData)) {
            GameObject poolParent = new GameObject($"Pool_{itemData.itemName}");
            poolParent.transform.SetParent(transform);
            pools[itemData] = new ObjectPool(itemData.prefab, initialPoolSize, poolParent.transform);
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
        if (isSpawning) return;
        if (GameManager.Instance == null) return;

        var inventory = GameManager.Instance.Inventory;
        if (inventory == null || inventory.Count == 0) {
            Debug.Log("[ItemSpawner] No inventory items to spawn");
            return;
        }

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnInventoryCoinsRoutine());
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
        List<ItemData> coinItems = GetInventoryCoinItems();
        if (coinItems.Count == 0) {
            isSpawning = false;
            yield break;
        }

        int count = Mathf.Min(inventoryCoinsPerTurn, coinItems.Count);
        for (int i = 0; i < count; i++) {
            ItemData itemData = coinItems[Random.Range(0, coinItems.Count)];
            SpawnInventoryCoin(itemData);
            GameManager.Instance.SpendItem(itemData, 1);
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private List<ItemData> GetInventoryCoinItems() {
        List<ItemData> coinItems = new List<ItemData>();
        var inventory = GameManager.Instance?.Inventory;
        if (inventory == null) return coinItems;

        foreach (var invItem in inventory) {
            if (invItem.itemData != null && invItem.itemData.itemType == ItemType.Coin) {
                for (int i = 0; i < invItem.quantity; i++) {
                    coinItems.Add(invItem.itemData);
                }
            }
        }
        return coinItems;
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

        ItemData itemData = normalCoinPool[Random.Range(0, normalCoinPool.Length)];
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(spawnPosition, spawnRotation);
        item.name = itemData.itemName;
        activeItems.Add(item);
    }

    private void SpawnInventoryCoin(ItemData itemData) {
        if (itemData == null) return;

        EnsurePoolExists(itemData);
        
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(spawnPosition, spawnRotation);
        item.name = itemData.itemName;
        activeItems.Add(item);
    }

    private ItemData GetItemDataByName(string name) {
        foreach (ItemData item in normalCoinPool) {
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