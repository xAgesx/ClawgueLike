using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Poolable : MonoBehaviour {
    [SerializeField] private float sleepVelocity = 0.15f;
    [SerializeField] private float sleepAngularVelocity = 0.15f;
    [SerializeField] private float sleepDelay = 0.5f;

    private Rigidbody rb;
    private float sleepTimer;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        if (rb.isKinematic) return;

        bool isSlow = rb.linearVelocity.sqrMagnitude < sleepVelocity * sleepVelocity
            && rb.angularVelocity.sqrMagnitude < sleepAngularVelocity * sleepAngularVelocity;

        if (isSlow) {
            sleepTimer += Time.fixedDeltaTime;
            if (sleepTimer >= sleepDelay) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        } else {
            sleepTimer = 0f;
        }
    }

    public void OnGetFromPool() {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();
        rb.isKinematic = false;
        sleepTimer = 0f;
    }

    public void OnReturnToPool() {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        sleepTimer = 0f;
    }

    public void WakeUp() {
        rb.WakeUp();
        sleepTimer = 0f;
    }

    public void Disturb(Vector3 force) {
        rb.WakeUp();
        rb.AddForce(force, ForceMode.Impulse);
        sleepTimer = 0f;
    }
}
