using UnityEngine;

// Lives for a few seconds and keeps shoving every coin inside its radius: hard
// upward, swung sideways around the centre, and tugged slightly inward so the pile
// spirals up instead of spraying outward. Built at runtime by TornadoEffect - there
// is no prefab to keep in sync.
public class TornadoField : MonoBehaviour {
    // Reused every physics step; resizing would allocate inside a hot loop.
    private readonly Collider[] hits = new Collider[128];

    private float radius = 1.2f;
    private float duration = 5f;
    private float lift = 12f;
    private float swirl = 6f;
    private float inward = 2f;
    private float maxSpeed = 4f;

    // The coin that opened the whirlwind sits at the eye and should not fling itself.
    private CoinInstance source;

    public void Configure(float fieldRadius, float fieldDuration, float fieldLift, float fieldSwirl, float fieldInward, float fieldMaxSpeed, CoinInstance owner) {
        radius = fieldRadius;
        duration = fieldDuration;
        lift = fieldLift;
        swirl = fieldSwirl;
        inward = fieldInward;
        maxSpeed = fieldMaxSpeed;
        source = owner;
    }

    private void Start() {
        Destroy(gameObject, duration);
    }

    private void FixedUpdate() {
        // Triggers are skipped so each coin is pushed once, by its solid collider.
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++) {
            CoinInstance coin = hits[i].GetComponentInParent<CoinInstance>();
            if (coin == null || coin == source) continue;

            Rigidbody body = coin.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) continue;

            // Poolable puts any coin that sits still for half a second to sleep, and a
            // sleeping body ignores forces - a whirlwind over a settled pile would do
            // nothing at all without this.
            body.WakeUp();

            Vector3 offset = coin.transform.position - transform.position;
            offset.y = 0f;

            Vector3 force = Vector3.up * lift;

            if (offset.sqrMagnitude > 0.0001f) {
                Vector3 away = offset.normalized;
                force += Vector3.Cross(Vector3.up, away) * swirl;
                force -= away * inward;
            }

            // Acceleration, not force: coins are tiny (mass 0.1) and a force-based
            // push would fling them at absurd speed.
            body.AddForce(force, ForceMode.Acceleration);

            // Constant acceleration with no ceiling would build up to a coin bouncing
            // off the glass at silly speeds, so clamp what the whirlwind can impart.
            Vector3 velocity = body.linearVelocity;
            if (velocity.sqrMagnitude > maxSpeed * maxSpeed) {
                body.linearVelocity = velocity.normalized * maxSpeed;
            }
        }
    }
}
