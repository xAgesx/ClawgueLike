using System.Collections.Generic;
using UnityEngine;

// A "cursed relic": every relic is a boon paired with a curse. This asset only holds
// the shop info and a list of effects - the actual behaviour lives in RelicEffect
// scripts (Assets/Scripts/Shop/Effects/Relics), exactly like CoinData / CoinEffect.
[CreateAssetMenu(fileName = "RelicData", menuName = "Data/Items/Relics/Cursed Relic Data")]
public class RelicData : ItemData {
    [Header("Info")]
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public int basePrice;

    [Header("Effects")]
    [Tooltip("Every boon and curse this relic applies. The relic is inert without one.")]
    public List<RelicEffect> effects = new List<RelicEffect>();
}
