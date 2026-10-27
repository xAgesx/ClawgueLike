using UnityEngine;

// Per-frame movement half of Homing. It is added to the coin at the first turn
// boundary and then keeps a target locked as long as that target is worth chasing.
// Lives on the coin so it follows it through the pile and stops when the coin is
// pooled or collected.
public class HomingDrift : MonoBehaviour {
    private HomingEffect effect;
    private CoinInstance self;
    private Rigidbody body;
    private CoinInstance target;
    private float retargetTimer;

    public void Configure(HomingEffect sourceEffect, CoinInstance coin) {
        effect = sourceEffect;
        self = coin;
        body = GetComponent<Rigidbody>();
        target = null;
        retargetTimer = 0f;
    }

    private void FixedUpdate() {
        if (effect == null || self == null) return;
        if (body == null) body = GetComponent<Rigidbody>();
        if (body == null || body.isKinematic) return;

        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f || target == null || !target.gameObject.activeInHierarchy) {
            retargetTimer = effect.RetargetInterval;
            target = FindMostValuable();
        }

        if (target == null) return;

        Vector3 offset = target.transform.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.02f) return;

        body.WakeUp();
        Vector3 force = offset.normalized * effect.Speed + Vector3.up * effect.UpwardDrift;
        body.AddForce(force, ForceMode.Acceleration);
    }

    private CoinInstance FindMostValuable() {
        CoinInstance best = null;
        int bestValue = -1;
        float radiusSqr = effect.SearchRadius * effect.SearchRadius;

        foreach (CoinInstance other in FindObjectsByType<CoinInstance>(FindObjectsInactive.Exclude)) {
            if (other == null || other == self) continue;
            if ((other.transform.position - transform.position).sqrMagnitude > radiusSqr) continue;

            if (other.TotalValue > bestValue) {
                best = other;
                bestValue = other.TotalValue;
            }
        }

        return best;
    }

    private void OnDisable() {
        target = null;
        retargetTimer = 0f;
    }
}
