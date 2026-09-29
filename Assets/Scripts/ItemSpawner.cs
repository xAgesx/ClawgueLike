using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    [Header("Normal Coins")]
    [SerializeField] private ItemData[] normalCoinPool;
    [SerializeField] private int normalCoinsPerRound = 10;

    [Header("Special Coins")]
    [SerializeField] private ItemData[] specialCoinPool;
    [SerializeField] private int specialCoinsPerTurn = 4;

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
        // Removed spawnItems logic
    }

    private void Start() {
        if (spawnPoints == null || spawnPoints.Length < 3) {
            Debug.LogError("ItemSpawner requires at least 3 spawn points.");
            return;
        }

        if ((normalCoinPool == null || normalCoinPool.Length == 0) && 
            (specialCoinPool == null || specialCoinPool.Length == 0)) {
            Debug.LogError("ItemSpawner requires at least one item pool.");
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

        if (specialCoinPool != null) {
            foreach (ItemData item in specialCoinPool) {
                if (!pools.ContainsKey(item)) {
                    GameObject poolParent = new GameObject($"Pool_{item.itemName}");
                    poolParent.transform.SetParent(transform);
                    pools[item] = new ObjectPool(item.prefab, initialPoolSize, poolParent.transform);
                }
            }
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
    /// Spawns SpecialCoins only (called when shop exits / new turn starts)
    /// </summary>
    public void SpawnSpecialCoins() {
        if (isSpawning) return;
        if (specialCoinPool == null || specialCoinPool.Length == 0) return;

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnSpecialCoinsRoutine());
    }

    /// <summary>
    /// Spawns normal coins first, then SpecialCoins (called at end of round)
    /// </summary>
    public void SpawnRoundAndSpecialCoins() {
        if (isSpawning) return;

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnRoundAndSpecialRoutine());
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

    private IEnumerator SpawnSpecialCoinsRoutine() {
        int count = specialCoinsPerTurn;
        if (specialCoinPool == null || specialCoinPool.Length == 0) {
            isSpawning = false;
            yield break;
        }

        while (spawnedCount < count) {
            SpawnSpecialCoin();
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private IEnumerator SpawnRoundAndSpecialRoutine() {
        // First spawn normal coins
        if (normalCoinPool != null && normalCoinPool.Length > 0) {
            while (spawnedCount < normalCoinsPerRound) {
                SpawnNormalCoin();
                spawnedCount++;
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // Then spawn special coins
        if (specialCoinPool != null && specialCoinPool.Length > 0) {
            int specialSpawned = 0;
            while (specialSpawned < specialCoinsPerTurn) {
                SpawnSpecialCoin();
                specialSpawned++;
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

    private void SpawnSpecialCoin() {
        if (specialCoinPool == null || specialCoinPool.Length == 0) return;

        ItemData itemData = specialCoinPool[Random.Range(0, specialCoinPool.Length)];
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
        foreach (ItemData item in specialCoinPool) {
            if (item.itemName == name) return item;
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