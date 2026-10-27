using UnityEngine;

// Persistent field that halves the pull of gravity on every coin inside it for a few
// seconds. The upward acceleration is exactly half of standard gravity, so a coin in
// free fall drops at half speed while the field is alive.
public class AntigravField : MonoBehaviour {
    private readonly Collider[] hits = new Collider[128];

    private float radius = 4f;
    private float duration = 5f;
    private float lift = 4.905f;

    public void Configure(float fieldRadius, float fieldDuration, float upwardAcceleration) {
        radius = Mathf.Max(0.1f, fieldRadius);
        duration = Mathf.Max(0.1f, fieldDuration);
        lift = Mathf.Max(0f, upwardAcceleration);
    }

    private void Update() {
        duration -= Time.deltaTime;
        if (duration <= 0f) Destroy(gameObject);
    }

    private void FixedUpdate() {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, radius, hits, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++) {
            CoinInstance coin = hits[i].GetComponentInParent<CoinInstance>();
            if (coin == null) continue;

            Rigidbody body = coin.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) continue;

            body.WakeUp();
            body.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }
    }
}
