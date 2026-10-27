using System.Collections.Generic;
using UnityEngine;

// Meteor: a one-shot impact coin. The first coin it touches - or the floor it lands
// on - sets off a wide, hard shove that scatters the whole pile. It uses the same
// velocity-change blast as the Mine, but reaches further and throws harder.
[CreateAssetMenu(fileName = "MeteorCoin", menuName = "Data/Effects/SpecialCoins/Meteor")]
public class MeteorEffect : CoinEffect {
    [Tooltip("Everything inside this radius is thrown.")]
    [SerializeField] private float blastRadius = 2f;

    [Tooltip("Speed handed to a coin right at the impact point, scaled down by distance.")]
    [SerializeField] private float blastForce = 6f;

    [Tooltip("Upward bias so coins pop and tumble instead of skidding along the floor.")]
    [SerializeField] private float upwardBias = 0.9f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        Explode(source);
    }

    public override void OnImpact(CoinInstance coin, SpecialCoin source, Collision collision) {
        Explode(source);
    }

    private void Explode(SpecialCoin source) {
        if (source == null || source.Self == null) return;

        Vector3 origin = source.transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, blastRadius, ~0, QueryTriggerInteraction.Ignore);
        HashSet<CoinInstance> seen = new HashSet<CoinInstance>();

        foreach (Collider hit in hits) {
            CoinInstance coin = hit.GetComponentInParent<CoinInstance>();
            if (coin == null || coin == source.Self) continue;
            if (!seen.Add(coin)) continue;

            Rigidbody body = coin.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) continue;

            Vector3 offset = coin.transform.position - origin;
            float distance = offset.magnitude;
            float falloff = blastRadius > 0f ? 1f - Mathf.Clamp01(distance / blastRadius) : 1f;
            Vector3 direction = distance > 0.0001f ? offset / distance : Vector3.up;
            direction = (direction + Vector3.up * upwardBias).normalized;

            body.WakeUp();
            body.AddForce(direction * blastForce * falloff, ForceMode.VelocityChange);
        }
    }
}
