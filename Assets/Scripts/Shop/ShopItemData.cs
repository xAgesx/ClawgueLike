using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "ClawgueLike/Shop/Shop Item Data")]
public class ShopItemData : ScriptableObject {
    public string itemName;
    public GameObject prefab;
    public Sprite icon;
    public int basePrice;
    public ItemType itemType;
    public ItemData itemData; // Reference to the actual ItemData for spawning
}