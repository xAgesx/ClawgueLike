using System.Collections.Generic;
using UnityEngine;

// Sits only on special coin prefabs. Reads its abilities from the same CoinData
// asset the shop and spawner use, so there is one place to wire a coin up.
[RequireComponent(typeof(ItemPickup))]
[RequireComponent(typeof(CoinInstance))]
public class SpecialCoin : MonoBehaviour {
    private ItemPickup pickup;
    private CoinInstance self;

    // How many times THIS coin has fired each of its effects. Lives here because
    // the effect asset is shared by every copy and must stay stateless.
    private readonly Dictionary<CoinEffect, int> applications = new Dictionary<CoinEffect, int>();

    public CoinInstance Self {
        get {
            if (self == null) self = GetComponent<CoinInstance>();
            return self;
        }
    }

    private void Awake() {
        pickup = GetComponent<ItemPickup>();
    }

    private void OnEnable() {
        // Pooled coins come back to life, so their effects have to come back too.
        applications.Clear();
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
        if (target == null || target == Self) return;

        foreach (CoinEffect effect in data.effects) {
            if (!effect.Affects(target)) continue;

            // One application per coin per effect. Without this a DoubleDown that
            // keeps resting against the same coin would compound every frame.
            if (effect.OncePerTarget && target.HasApplied(effect)) continue;

            // Effects with a cap (a rabbit breeds once) fall silent once spent.
            if (IsExhausted(effect)) continue;

            effect.Apply(target, this);
            if (effect.OncePerTarget) target.MarkApplied(effect);
            applications[effect] = ApplicationsFor(effect) + 1;
        }
    }

    private bool IsExhausted(CoinEffect effect) {
        return effect.MaxApplications > 0 && ApplicationsFor(effect) >= effect.MaxApplications;
    }

    private int ApplicationsFor(CoinEffect effect) {
        return applications.TryGetValue(effect, out int count) ? count : 0;
    }
}
