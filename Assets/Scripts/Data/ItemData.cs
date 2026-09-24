using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ClawgueLike/Item Data")]
public class ItemData : ScriptableObject {
    public string itemName;
    public GameObject prefab;
    public int value;
    public float weight = 1f;
    [Range(0f, 1f)]
    public float magneticAttraction = 1f;
    public ItemType itemType;
}

public enum ItemType {
    Coin,
    Fragment,
    Artifact
}