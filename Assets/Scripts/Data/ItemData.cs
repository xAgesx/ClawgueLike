using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ClawgueLike/Item Data")]
public class ItemData : ScriptableObject {
    public string itemName;
    public GameObject prefab;
    public int value;
    public float weight = 1f;
}
