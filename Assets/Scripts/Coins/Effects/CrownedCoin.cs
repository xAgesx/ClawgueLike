using UnityEngine;

// A single live crown. The King effect puts this on a coin and multiplies its value;
// only one may exist at a time, so crowning a new coin removes the old crown first.
// The marker owns its own undo so the operation is reversible without the King asset
// having to remember anything.
//
// The component stays on a pooled coin after it is collected. OnDisable therefore
// undoes the value boost and marks the crown inert, and OnEnable only re-registers a
// crown that is actually still applied - otherwise a reused coin would come back
// wearing a phantom crown.
public class CrownedCoin : MonoBehaviour {
    public static CrownedCoin Current { get; private set; }

    private CoinInstance coin;
    private float factor = 1.5f;
    private bool applied;

    private void OnEnable() {
        if (applied) Current = this;
    }

    private void OnDisable() {
        if (applied && coin != null) coin.MultiplyMultiplier(1f / factor);
        applied = false;
        if (Current == this) Current = null;
    }

    public void Apply(float multiplier) {
        factor = Mathf.Max(0.01f, multiplier);
        coin = GetComponent<CoinInstance>();
        if (coin != null) {
            coin.MultiplyMultiplier(factor);
            applied = true;
            Current = this;
        }
    }

    public void RemoveCrown() {
        if (applied && coin != null) coin.MultiplyMultiplier(1f / factor);
        applied = false;
        if (Current == this) Current = null;
        Destroy(this);
    }
}
