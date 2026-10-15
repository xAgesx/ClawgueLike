using UnityEngine;

// Fence: a couple of free rerolls each visit, but paid rerolls cost more.
[CreateAssetMenu(fileName = "Fence", menuName = "Data/Effects/Relics/Fence")]
public class FenceEffect : RelicEffect {
    [SerializeField] private int freeRerolls = 2;
    [SerializeField] private float paidRerollMultiplier = 2f;

    public override int FreeRerolls() => freeRerolls;
    public override float PaidRerollCostMultiplier() => paidRerollMultiplier;

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        int remaining = ShopManager.Instance != null ? ShopManager.Instance.FreeRerollsRemainingNow() : freeRerolls;
        return remaining + " rerolls left";
    }
}
