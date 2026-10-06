using System.Collections.Generic;
using UnityEngine;

// The one and only coin asset: shop info, prefab reference and gameplay value in
// a single ScriptableObject. Replaces the old ItemData -> ShopItemData -> SpecialCoinData chain.
[CreateAssetMenu(fileName = "CoinData", menuName = "Data/Items/SpecialCoins/Coin Data")]
public class CoinData : ItemData {
    [Header("Shop")]
    public Sprite icon;
    public int basePrice;
    [TextArea] public string description;

    [Header("Value")]
    [Tooltip("Coins paid out per unit before any effects touch it.")]
    public int baseValue = 1;

    [Header("Effects")]
    [Tooltip("Abilities this coin applies to other coins it physically touches.")]
    public List<CoinEffect> effects = new List<CoinEffect>();

    // Coins carrying effects are the "special" ones. Derived rather than a manual
    // enum toggle, so it can never fall out of sync with the effects list.
    public bool IsSpecial => effects != null && effects.Count > 0;
}
