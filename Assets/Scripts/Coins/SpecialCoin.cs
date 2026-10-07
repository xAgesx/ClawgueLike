using System.Collections.Generic;
using UnityEngine;

// Sits only on special coin prefabs. Reads its abilities from the same CoinData
// asset the shop and spawner use, so there is one place to wire a coin up.
[RequireComponent(typeof(ItemPickup))]
[RequireComponent(typeof(CoinInstance))]
public class SpecialCoin : MonoBehaviour {
    private ItemPickup pickup;
    private CoinInstance self;
    private Rigidbody body;

    // How many times THIS coin has fired each of its effects. Lives here because
    // the effect asset is shared by every copy and must stay stateless.
    private readonly Dictionary<CoinEffect, int> applications = new Dictionary<CoinEffect, int>();

    // Seconds left until the next leap. Starts negative so a freshly pooled coin
    // waits a full interval before its first hop instead of jumping on spawn.
    private float hopTimer = -1f;

    public CoinInstance Self {
        get {
            if (self == null) self = GetComponent<CoinInstance>();
            return self;
        }
    }

    private Rigidbody Body {
        get {
            if (body == null) body = GetComponent<Rigidbody>();
            return body;
        }
    }

    private void Awake() {
        pickup = GetComponent<ItemPickup>();
    }

    private void OnEnable() {
        // Pooled coins come back to life, so their effects have to come back too.
        applications.Clear();
        hopTimer = -1f;
    }

    private void Update() {
        CoinEffect driver = FindHopDriver();
        if (driver == null) {
            hopTimer = -1f;
            return;
        }

        if (hopTimer < 0f) {
            hopTimer = driver.HopInterval;
            return;
        }

        hopTimer -= Time.deltaTime;
        if (hopTimer > 0f) return;

        hopTimer = driver.HopInterval;
        TryHop(driver);
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

            // A paired act needs a partner that still has its own copy left, so a
            // grey rabbit can never be bred with again.
            if (effect.SpendsPartner && IsExhaustedFor(target, effect)) continue;

            effect.Apply(target, this);
            if (effect.OncePerTarget) target.MarkApplied(effect);
            applications[effect] = ApplicationsFor(effect) + 1;

            // A coin that has used up an effect goes grey so it reads as "spent"
            // at a glance rather than looking like it is still waiting to fire.
            if (IsExhausted(effect)) Self.SetTint(effect.SpentTint);

            // Both rabbits report the same contact, so the second one has to see the
            // first one's spend - otherwise one litter comes out as two babies.
            if (effect.SpendsPartner) SpendPartner(target, effect);
        }
    }

    private static bool IsExhaustedFor(CoinInstance coin, CoinEffect effect) {
        if (effect.MaxApplications <= 0) return false;

        SpecialCoin special = coin.GetComponent<SpecialCoin>();
        if (special == null) return false;

        return special.ApplicationsFor(effect) >= effect.MaxApplications;
    }

    private static void SpendPartner(CoinInstance coin, CoinEffect effect) {
        if (effect.MaxApplications <= 0) return;

        SpecialCoin special = coin.GetComponent<SpecialCoin>();
        if (special == null) return;

        special.applications[effect] = effect.MaxApplications;
        special.Self.SetTint(effect.SpentTint);
    }

    // The first effect on this coin that still leaps and has not run out yet.
    private CoinEffect FindHopDriver() {
        if (pickup == null) return null;

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || data.effects == null) return null;

        for (int i = 0; i < data.effects.Count; i++) {
            CoinEffect effect = data.effects[i];
            if (effect == null || effect.HopInterval <= 0f) continue;
            if (IsExhausted(effect)) continue;
            return effect;
        }
        return null;
    }

    private void TryHop(CoinEffect driver) {
        Rigidbody rb = Body;
        if (rb == null || rb.isKinematic) return;

        CoinInstance target = FindNearestHopTarget(driver);
        if (target == null) return;

        Vector3 flat = target.transform.position - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.0001f) return;

        Vector3 launch = (flat.normalized + Vector3.up * 0.75f).normalized * driver.HopForce;

        // Drop the vertical component first, otherwise a coin in free fall spends
        // the whole leap cancelling out its own downward speed.
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(launch, ForceMode.VelocityChange);
    }

    // Only coins this effect could actually land on are worth hopping toward - a
    // grey rabbit with nothing left to breed with is not a destination, so the last
    // active rabbit sits still instead of bouncing off spent partners forever.
    private CoinInstance FindNearestHopTarget(CoinEffect effect) {
        CoinInstance nearest = null;
        float bestSqr = float.MaxValue;
        Vector3 origin = transform.position;
        CoinInstance selfInstance = Self;

        foreach (CoinInstance coin in FindObjectsByType<CoinInstance>(FindObjectsSortMode.None)) {
            if (coin == null || coin == selfInstance) continue;
            if (!effect.Affects(coin)) continue;
            if (effect.SpendsPartner && IsExhaustedFor(coin, effect)) continue;

            float sqr = (coin.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr) {
                bestSqr = sqr;
                nearest = coin;
            }
        }
        return nearest;
    }

    private bool IsExhausted(CoinEffect effect) {
        return effect.MaxApplications > 0 && ApplicationsFor(effect) >= effect.MaxApplications;
    }

    private int ApplicationsFor(CoinEffect effect) {
        return applications.TryGetValue(effect, out int count) ? count : 0;
    }
}
