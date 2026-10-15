using UnityEngine;

// Overstock: a wider coin row in the shop, one relic slot narrower.
[CreateAssetMenu(fileName = "Overstock", menuName = "Data/Effects/Relics/Overstock")]
public class OverstockEffect : RelicEffect {
    [SerializeField] private int extraCoinSlots = 1;
    [SerializeField] private int extraRelicSlots = -1;

    public override int ExtraCoinSlots() => extraCoinSlots;
    public override int ExtraRelicSlots() => extraRelicSlots;
}
