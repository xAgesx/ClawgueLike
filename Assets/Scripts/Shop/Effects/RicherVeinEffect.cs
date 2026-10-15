using UnityEngine;

// Richer Vein: rounds spawn more coins, but a crowded machine drags the magnet down.
[CreateAssetMenu(fileName = "RicherVein", menuName = "Data/Effects/Relics/Richer Vein")]
public class RicherVeinEffect : RelicEffect {
    [SerializeField] private int extraCoinsPerRound = 15;
    [SerializeField] private int crowdThreshold = 60;
    [SerializeField] private float crowdForceMultiplier = 0.8f;

    public override int ExtraNormalCoinsPerRound() => extraCoinsPerRound;

    public override float MagnetForceMultiplier(int activeCoins) {
        return crowdThreshold > 0 && activeCoins >= crowdThreshold ? crowdForceMultiplier : 1f;
    }

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        return $"+{extraCoinsPerRound} coins";
    }
}
