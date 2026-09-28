using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "ClawgueLike/Shop/Relic Data")]
public class RelicData : ScriptableObject {
    public string relicName;
    public string description;
    public Sprite icon;
    public int basePrice;
    public RelicEffectType effectType;
    public float effectValue; // e.g., 1.5 for 1.5x coins, 1 for +1 turn
}

public enum RelicEffectType {
    ExtraTurnPerRound,
    CoinMultiplier,
    FragmentChance,
    MagnetRangeBoost,
    MagnetPowerBoost,
    DropSpeedBoost,
    ResupplyDiscount
}