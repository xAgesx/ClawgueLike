using UnityEngine;

[CreateAssetMenu(fileName = "CheatData", menuName = "ClawgueLike/Shop/Cheat Data")]
public class CheatData : ScriptableObject {
    public string cheatName;
    public string description;
    public Sprite icon;
    public int basePrice;
    public CheatEffectType effectType;
    public float effectValue; // Duration, power, etc.
    public float duration; // For timed effects
}

public enum CheatEffectType {
    TimeFreeze,
    AutoAim,
    InstantPull,
    MagnetBurst,
    ExtraPull,
    CoinMagnet,
    FragmentAttractor
}