using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemData[] _itemPool;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int _itemsPerRound = 10;
    [SerializeField] private float _spawnInterval = 0.5f;
    [SerializeField] private Transform _parentContainer;

    private int _spawnedCount;
    private bool _isSpawning;

    private void Start()
    {
        if (_spawnPoints == null || _spawnPoints.Length < 3)
        {
            Debug.LogError("ItemSpawner requires at least 3 spawn points.");
            return;
        }

        if (_itemPool == null || _itemPool.Length == 0)
        {
            Debug.LogError("ItemSpawner requires at least one item in the pool.");
            return;
        }
    }

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;
        _spawnedCount = 0;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (_spawnedCount < _itemsPerRound)
        {
            SpawnItem();
            _spawnedCount++;
            yield return new WaitForSeconds(_spawnInterval);
        }

        _isSpawning = false;
    }

    private void SpawnItem()
    {
        ItemData itemData = _itemPool[Random.Range(0, _itemPool.Length)];
        Vector3 spawnPosition = CalculateSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject item = Instantiate(itemData.prefab, spawnPosition, spawnRotation, _parentContainer);
        item.name = itemData.itemName;
    }

    private Vector3 CalculateSpawnPosition()
    {
        Transform pointA = _spawnPoints[0];
        Transform pointB = _spawnPoints[1];
        Transform pointC = _spawnPoints[2];

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
