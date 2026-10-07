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

    [Tooltip("A paired act: only fires while the target still has its own copy of this effect, and spends that copy too. Two rabbits make one baby, then both go grey and can never breed again.")]
    [SerializeField] private bool spendsPartner = false;

    [Header("Self behaviour")]
    [Tooltip("How often the coin carrying this effect leaps toward the nearest coin it can act on. 0 = it never moves on its own.")]
    [SerializeField] private float hopInterval = 0f;

    [Tooltip("Speed in m/s added by a leap. The direction is flat toward the target plus a fixed upward kick.")]
    [SerializeField] private float hopForce = 4f;

    [Tooltip("Tint the carrying coin takes once it has used up all of this effect's applications.")]
    [SerializeField] private Color spentTint = Color.gray;

    public int MaxApplications => maxApplications;
    public bool OncePerTarget => oncePerTarget;
    public bool SpendsPartner => spendsPartner;
    public string TargetFamily => targetFamily;
    public float HopInterval => hopInterval;
    public float HopForce => hopForce;
    public Color SpentTint => spentTint;

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
