using System.Collections.Generic;
using UnityEngine;

// Spring: kicks off whatever it lands on. The bounce hands back a fraction of the
// speed it arrived with rather than a flat boost, so it rattles the pile for a moment
// and then settles instead of pinging around the machine forever.
//
// Landings only - a spring resting on the pile is not hitting the floor, and it would
// be reloading itself every physics step if a coin bump counted.
[CreateAssetMenu(fileName = "SpringCoin", menuName = "Data/Effects/SpecialCoins/Spring")]
public class SpringEffect : CoinEffect {
    [Tooltip("Landings slower than this are settling, not a bounce.")]
    [SerializeField] private float minImpactSpeed = 1f;

    [Tooltip("Fraction of the landing speed handed back as upward speed.")]
    [SerializeField, Range(0f, 1f)] private float restitution = 0.7f;

    [Tooltip("Ceiling on bounce speed, so a long drop cannot fire it into the glass.")]
    [SerializeField] private float maxBounceSpeed = 5f;

    [Tooltip("Radius of the shockwave that shoves the coins it lands among.")]
    [SerializeField] private float knockRadius = 0.6f;

    [Tooltip("Speed in m/s given to the coins caught in the landing.")]
    [SerializeField] private float knockForce = 1.5f;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnImpact(CoinInstance coin, SpecialCoin source, Collision collision) {
        if (coin == null || source == null) return;

        Rigidbody body = source.GetComponent<Rigidbody>();
        if (body == null || body.isKinematic) return;

        // relativeVelocity is the speed the collision was solved with. By the time the
        // callback runs the solver has already zeroed linearVelocity, so it is the only
        // place the landing speed still exists.
        float impact = Mathf.Abs(collision.relativeVelocity.y);
        if (impact < minImpactSpeed) return;

        body.WakeUp();

        Vector3 velocity = body.linearVelocity;
        velocity.y = Mathf.Min(maxBounceSpeed, impact * restitution);
        body.linearVelocity = velocity;

        KnockNeighbours(source);
    }

    private void KnockNeighbours(SpecialCoin source) {
        Vector3 origin = source.transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, knockRadius, ~0, QueryTriggerInteraction.Ignore);

        HashSet<CoinInstance> seen = new HashSet<CoinInstance>();

        foreach (Collider hit in hits) {
            CoinInstance coin = hit.GetComponentInParent<CoinInstance>();
            if (coin == null || coin == source.Self) continue;
            if (!seen.Add(coin)) continue;

            Rigidbody body = coin.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) continue;

            Vector3 offset = coin.transform.position - origin;
            offset.y = 0f;
            Vector3 direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector3.up;

            body.WakeUp();
            body.AddForce((direction + Vector3.up * 0.3f).normalized * knockForce, ForceMode.VelocityChange);
        }
    }
}
