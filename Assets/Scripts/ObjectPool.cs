using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
    private GameObject prefab;
    private Transform parent;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    private List<GameObject> allObjects = new List<GameObject>();

    public int CountActive => allObjects.Count - availableObjects.Count;
    public int CountInactive => availableObjects.Count;

    public ObjectPool(GameObject prefab, int initialSize, Transform parent) {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++) {
            GameObject obj = CreateObject();
            availableObjects.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation) {
        GameObject obj = availableObjects.Count > 0
            ? availableObjects.Dequeue()
            : CreateObject();

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        Poolable poolable = obj.GetComponent<Poolable>();
        if (poolable != null) {
            poolable.OnGetFromPool();
        }

        return obj;
    }

    public void Return(GameObject obj) {
        Poolable poolable = obj.GetComponent<Poolable>();
        if (poolable != null) {
            poolable.OnReturnToPool();
        }

        obj.SetActive(false);
        availableObjects.Enqueue(obj);
    }

    public void ReturnAll() {
        foreach (GameObject obj in allObjects) {
            if (obj.activeInHierarchy) {
                Return(obj);
            }
        }
    }

    private GameObject CreateObject() {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        allObjects.Add(obj);
        return obj;
    }
}
