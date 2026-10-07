using UnityEngine;

// An effect is a data-only strategy: the asset itself is SHARED by every copy of a
// coin and survives between play sessions in the editor. Never store runtime state
// (timers, stacks, "already touched" flags) in a field here or it will leak between
// runs. State belongs on CoinInstance instead.
public abstract class CoinEffect : ScriptableObject {
    [SerializeField] private ItemType[] affects = { ItemType.Coin };

    [Tooltip("If set, only coins whose CoinData.family matches this exactly.")]
    [SerializeField] private string targetFamily = "";

    [Tooltip("How many times the coin carrying this effect may fire it before it goes quiet. 0 = never runs out.")]
    [SerializeField] private int maxApplications = 0;

    [Tooltip("If set, this effect can only ever land once on any given coin - a coin can only be doubled once. Turn it off when the source's own cap is the real limit, so any coin can act on any partner (a rabbit breeding with any other rabbit).")]
    [SerializeField] private bool oncePerTarget = true;

    public int MaxApplications => maxApplications;
    public bool OncePerTarget => oncePerTarget;

    public bool Affects(CoinInstance target) {
        if (target == null || target.Data == null) return false;

        if (!string.IsNullOrEmpty(targetFamily) && target.Data.family != targetFamily) return false;

        if (affects == null || affects.Length == 0) return true;

        for (int i = 0; i < affects.Length; i++) {
            if (affects[i] == target.Data.itemType) return true;
        }
        return false;
    }

    public abstract void Apply(CoinInstance target, SpecialCoin source);
}
