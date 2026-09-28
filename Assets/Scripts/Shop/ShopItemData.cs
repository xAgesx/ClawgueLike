using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "ClawgueLike/Shop/Shop Item Data")]
public class ShopItemData : ItemData {
    public GameObject prefab;
    public Sprite icon;
    public int basePrice;
    public ItemType itemType;
}