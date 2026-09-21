using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    [SerializeField] private ItemData[] itemPool;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int itemsPerRound = 10;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private Transform parentContainer;
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private bool spawnOnStart = true;

    public bool spawnItems;

    [Header("Debug")]
    public int spawnedCount;

    private bool isSpawning;
    private Dictionary<ItemData, ObjectPool> pools = new Dictionary<ItemData, ObjectPool>();
    private List<GameObject> activeItems = new List<GameObject>();

    private void OnValidate() {
        if (spawnItems && !isSpawning) {
            SpawnItems();
        } else if (!spawnItems && isSpawning) {
            StopSpawning();
        }
    }

    private void Start() {
        if (spawnPoints == null || spawnPoints.Length < 3) {
            Debug.LogError("ItemSpawner requires at least 3 spawn points.");
            return;
        }

        if (itemPool == null || itemPool.Length == 0) {
            Debug.LogError("ItemSpawner requires at least one item in the pool.");
            return;
        }

        InitializePools();

        if (spawnOnStart) {
            SpawnItems();
        }
    }

    private void InitializePools() {
        foreach (ItemData item in itemPool) {
            if (!pools.ContainsKey(item)) {
                GameObject poolParent = new GameObject($"Pool_{item.itemName}");
                poolParent.transform.SetParent(transform);
                pools[item] = new ObjectPool(item.prefab, initialPoolSize, poolParent.transform);
            }
        }
    }

    public void SpawnItems() {
        if (isSpawning) return;
        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnRoutine());
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

    private IEnumerator SpawnRoutine() {
        while (spawnedCount < itemsPerRound) {
            SpawnItem();
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnItem() {
        ItemData itemData = itemPool[Random.Range(0, itemPool.Length)];
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        ObjectPool pool = pools[itemData];
        GameObject item = pool.Get(spawnPosition, spawnRotation);
        item.name = itemData.itemName;
        activeItems.Add(item);
    }

    private ItemData GetItemDataByName(string name) {
        foreach (ItemData item in itemPool) {
            if (item.itemName == name) {
                return item;
            }
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
