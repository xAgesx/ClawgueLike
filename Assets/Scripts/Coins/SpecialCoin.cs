using System.Collections.Generic;
using UnityEngine;

// Sits only on special coin prefabs. Reads its abilities from the same CoinData
// asset the shop and spawner use, so there is one place to wire a coin up.
[RequireComponent(typeof(ItemPickup))]
[RequireComponent(typeof(CoinInstance))]
public class SpecialCoin : MonoBehaviour {
    // Abilities are held back for this long after the coin spawns, so a fresh drop
    // cannot bite the pile it lands in before it has even settled.
    [SerializeField] private float spawnAbilityDelay = 1f;

    private ItemPickup pickup;
    private CoinInstance self;
    private Rigidbody body;

    // How many times THIS coin has fired each of its effects. Lives here because
    // the effect asset is shared by every copy and must stay stateless.
    private readonly Dictionary<CoinEffect, int> applications = new Dictionary<CoinEffect, int>();

    // Seconds left before this coin's abilities may fire. Reset to the spawn delay
    // every time the coin leaves the pool, then ticked down in Update.
    private float spawnLockTimer;

    // Coins this one touched while it was still arming. A coin that falls straight
    // onto the pile makes its one meaningful contact during the delay, and because
    // it stays in contact no fresh event ever arrives - so those touches are held
    // here and replayed the moment the coin wakes up instead of being discarded.
    private readonly List<CoinInstance> pendingTargets = new List<CoinInstance>();

    // Seconds left until the next leap. Starts negative so a freshly pooled coin
    // waits a full interval before its first hop instead of jumping on spawn.
    private float hopTimer = -1f;

    // Where each turn-based effect last saw this coin. An effect compares this against
    // its current spot to decide whether the coin was disturbed during the turn.
    private readonly Dictionary<CoinEffect, Vector3> lastTurnPositions = new Dictionary<CoinEffect, Vector3>();

    // Drift under this is settling, not a touch. Coins are ~15cm across, so 2cm is
    // small enough that a real nudge always counts but a pile breathing does not.
    private const float DisturbanceThreshold = 0.02f;

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
        lastTurnPositions.Clear();
        hopTimer = -1f;
        spawnLockTimer = spawnAbilityDelay;
        pendingTargets.Clear();
    }

    private void Update() {
        // Abilities stay dormant for a moment after the coin appears.
        if (spawnLockTimer > 0f) {
            spawnLockTimer -= Time.deltaTime;
            if (spawnLockTimer <= 0f) FlushPendingTargets();
            return;
        }

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
        TryImpact(collision);
    }

    // A landing rather than a coin-on-coin bump: the thing we hit carries no
    // CoinInstance of its own and the surface faces upward. Apply can only ever fire
    // for coin targets, so effects about hitting the floor hook in here instead.
    private void TryImpact(Collision collision) {
        if (spawnLockTimer > 0f) return;
        if (pickup == null || collision.collider == null) return;

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || !data.IsSpecial) return;

        // Landing on the pile is not landing on the floor.
        if (collision.collider.GetComponentInParent<CoinInstance>() != null) return;

        if (collision.contactCount == 0) return;
        if (collision.GetContact(0).normal.y < 0.5f) return;

        foreach (CoinEffect effect in data.effects) {
            if (effect == null) continue;
            if (IsExhausted(effect)) continue;

            effect.OnImpact(Self, this, collision);
        }
    }

    private void TryApplyTo(Collider other) {
        if (pickup == null) return;

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || !data.IsSpecial) return;

        // Ignore our own child colliders (mesh + trigger on the same object).
        if (other.transform.IsChildOf(transform)) return;

        CoinInstance target = other.GetComponentInParent<CoinInstance>();
        if (target == null || target == Self) return;

        // Still arming: hold the touch and act on it the instant the coin wakes up.
        if (spawnLockTimer > 0f) {
            if (!pendingTargets.Contains(target)) pendingTargets.Add(target);
            return;
        }

        ApplyTo(target, data);
    }

    // Anything this coin bumped into while it was still arming gets run through here
    // once the spawn delay is over, so the touch is delayed rather than lost.
    private void FlushPendingTargets() {
        if (pickup == null) {
            pendingTargets.Clear();
            return;
        }

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || !data.IsSpecial) {
            pendingTargets.Clear();
            return;
        }

        for (int i = 0; i < pendingTargets.Count; i++) {
            CoinInstance target = pendingTargets[i];
            if (target != null) ApplyTo(target, data);
        }
        pendingTargets.Clear();
    }

    private void ApplyTo(CoinInstance target, CoinData data) {
        foreach (CoinEffect effect in data.effects) {
            // Turn-only effects (a sleeping coin) must never be driven by a bump.
            if (!effect.TriggersOnContact) continue;

            if (!effect.Affects(target)) continue;

            // One application per coin per effect. Without this an Infection that
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
            if (IsExhausted(effect)) Self.SetTint(effect.SpentTint, effect.SpentTintStrength);

            // Both rabbits report the same contact, so the second one has to see the
            // first one's spend - otherwise one litter comes out as two babies.
            if (effect.SpendsPartner) SpendPartner(target, effect);
        }
    }

    // Called once by RoundsManager when a turn's coins have landed. Every special coin
    // still in the machine gets its turn-start effects fired.
    public static void DispatchTurnStart() {
        foreach (SpecialCoin coin in FindObjectsByType<SpecialCoin>(FindObjectsSortMode.None)) {
            coin.TurnStart();
        }
    }

    private void TurnStart() {
        if (pickup == null) return;

        CoinData data = pickup.ItemData as CoinData;
        if (data == null || !data.IsSpecial) return;

        foreach (CoinEffect effect in data.effects) {
            if (effect == null) continue;
            if (IsExhausted(effect)) continue;

            effect.OnTurnStart(Self, this);
            applications[effect] = ApplicationsFor(effect) + 1;

            if (IsExhausted(effect)) Self.SetTint(effect.SpentTint, effect.SpentTintStrength);
        }
    }

    // Turn-based effects ask this instead of watching the physics themselves. Records
    // where the coin is and reports whether it drifted since the last turn boundary.
    // The first call has nothing to compare against, so it reports "moved" - a coin
    // bought this turn gets no credit for the turn it arrived in.
    public bool HasMovedSinceLastTurn(CoinEffect effect) {
        Vector3 now = transform.position;

        if (!lastTurnPositions.TryGetValue(effect, out Vector3 before)) {
            lastTurnPositions[effect] = now;
            return true;
        }

        lastTurnPositions[effect] = now;
        return (now - before).sqrMagnitude > DisturbanceThreshold * DisturbanceThreshold;
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
        special.Self.SetTint(effect.SpentTint, effect.SpentTintStrength);
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
        if (spawnLockTimer > 0f) return;

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

    // Public so effects that need to know how many times they have landed (a snowball
    // that only grows every few coins) can read their own running total.
    public int ApplicationsFor(CoinEffect effect) {
        return applications.TryGetValue(effect, out int count) ? count : 0;
    }
}
