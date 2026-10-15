using UnityEngine;

// Long Arm: the magnet reaches further, but pulls weaker.
[CreateAssetMenu(fileName = "LongArm", menuName = "Data/Effects/Relics/Long Arm")]
public class LongArmEffect : RelicEffect {
    [SerializeField] private float radiusMultiplier = 1.4f;
    [SerializeField] private float forceMultiplier = 0.75f;

    public override float MagnetRadiusMultiplier() => radiusMultiplier;
    public override float MagnetForceMultiplier(int activeCoins) => forceMultiplier;

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        return "+" + Mathf.RoundToInt((radiusMultiplier - 1f) * 100f) + "% range";
    }
}
