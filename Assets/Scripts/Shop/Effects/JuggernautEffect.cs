using UnityEngine;

// Juggernaut: a much stronger magnet, paid for with one pull every turn.
[CreateAssetMenu(fileName = "Juggernaut", menuName = "Data/Effects/Relics/Juggernaut")]
public class JuggernautEffect : RelicEffect {
    [SerializeField] private float forceMultiplier = 1.4f;
    [SerializeField] private float radiusMultiplier = 1.25f;
    [SerializeField] private int pullDelta = -1;

    public override float MagnetForceMultiplier(int activeCoins) => forceMultiplier;
    public override float MagnetRadiusMultiplier() => radiusMultiplier;
    public override int ExtraPulls() => pullDelta;

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        return $"+{Mathf.RoundToInt((forceMultiplier - 1f) * 100f)}% force";
    }
}
