using UnityEngine;

// An effect is a data-only strategy: the asset itself is SHARED by every copy of a
// coin and survives between play sessions in the editor. Never store runtime state
// (timers, stacks, "already touched" flags) in a field here or it will leak between
// runs. State belongs on CoinInstance instead.
public abstract class CoinEffect : ScriptableObject {
    [SerializeField] private ItemType[] affects = { ItemType.Coin };

    public bool Affects(CoinInstance target) {
        if (target == null || target.Data == null) return false;
        if (affects == null || affects.Length == 0) return true;

        for (int i = 0; i < affects.Length; i++) {
            if (affects[i] == target.Data.itemType) return true;
        }
        return false;
    }

    public abstract void Apply(CoinInstance target);
}
