using UnityEngine;

// Sits only on special coin prefabs. Reads its abilities from the same CoinData
// asset the shop and spawner use, so there is one place to wire a coin up.
[RequireComponent(typeof(ItemPickup))]
[RequireComponent(typeof(CoinInstance))]
public class SpecialCoin : MonoBehaviour {
    private ItemPickup pickup;

    private void Awake() {
        pickup = GetComponent<ItemPickup>();
    }

    private void OnTriggerEnter(Collider other) {
        TryApplyTo(other);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider != null) TryApplyTo(collision.collider);
    }

    private void TryApplyTo(Collider other) {
        if (pickup == null) return;

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || !data.IsSpecial) return;

        // Ignore our own child colliders (mesh + trigger on the same object).
        if (other.transform.IsChildOf(transform)) return;

        CoinInstance target = other.GetComponentInParent<CoinInstance>();
        if (target == null || target == GetComponent<CoinInstance>()) return;

        foreach (CoinEffect effect in data.effects) {
            if (!effect.Affects(target)) continue;

            // One application per coin per effect. Without this a DoubleDown that
            // keeps resting against the same coin would compound every frame.
            if (target.HasApplied(effect)) continue;

            effect.Apply(target);
            target.MarkApplied(effect);
        }
    }
}
