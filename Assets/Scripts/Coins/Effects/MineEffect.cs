using System.Collections.Generic;
using UnityEngine;

// Mine: the first bump sets it off. Everything around it is thrown outward and up so
// a settled pile gets one violent stir, then the mine burns out on its own - the
// asset's maxApplications of 1 is what paints it grey afterwards.
[CreateAssetMenu(fileName = "MineCoin", menuName = "Data/Effects/SpecialCoins/Mine")]
public class MineEffect : CoinEffect {
    [Tooltip("Everything inside this radius gets thrown.")]
    [SerializeField] private float blastRadius = 1.1f;

    [Tooltip("Speed in m/s handed to a coin right on top of the blast, scaled down with distance.")]
    [SerializeField] private float blastForce = 3.5f;

    [Tooltip("Upward bias, so coins pop and tumble instead of skidding along the floor.")]
    [SerializeField] private float upwardBias = 0.6f;

    public override void Apply(CoinInstance target, SpecialCoin source) {
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

            // VelocityChange, not Impulse: coins weigh 0.1 and an impulse would launch
            // them through the glass.
            body.WakeUp();
            body.AddForce(direction * blastForce * falloff, ForceMode.VelocityChange);
        }
    }
}
