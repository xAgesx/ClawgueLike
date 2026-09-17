using UnityEngine;

public class VerletChain : MonoBehaviour {
    [SerializeField] private Transform anchor;
    [SerializeField] private Transform followTarget;
    [SerializeField] private int segments = 8;
    [SerializeField] private float segmentLength = 0.15f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float damping = 0.99f;
    [SerializeField] private int constraintIterations = 8;
    [SerializeField] private Vector3 rotationOffset;

    private Vector3[] positions;
    private Vector3[] previousPositions;

    public Vector3 EndPosition => positions[positions.Length - 1];
    public Vector3[] Positions => positions;
    public int SegmentCount => positions.Length;

    private void Start() {
        positions = new Vector3[segments + 1];
        previousPositions = new Vector3[segments + 1];

        Vector3 origin = anchor != null ? anchor.position : transform.position;

        for (int i = 0; i < positions.Length; i++) {
            positions[i] = origin + Vector3.down * (segmentLength * i);
            previousPositions[i] = positions[i];
        }
    }

    private void FixedUpdate() {
        if (positions == null) return;

        VerletIntegrate();
        ApplyConstraints();
    }

    private void LateUpdate() {
        if (positions == null) return;

        if (followTarget != null) {
            followTarget.position = EndPosition;

            Vector3 chainDir = positions[positions.Length - 2] - EndPosition;
            if (chainDir.sqrMagnitude > 0.0001f) {
                followTarget.rotation = Quaternion.LookRotation(chainDir, Vector3.up) * Quaternion.Euler(rotationOffset);
            }
        }
    }

    private void VerletIntegrate() {
        Vector3 gravityVec = new Vector3(0f, gravity, 0f);

        for (int i = 1; i < positions.Length; i++) {
            Vector3 velocity = (positions[i] - previousPositions[i]) * damping;
            previousPositions[i] = positions[i];
            positions[i] += velocity + gravityVec * (Time.fixedDeltaTime * Time.fixedDeltaTime);
        }
    }

    private void ApplyConstraints() {
        if (anchor != null) {
            positions[0] = anchor.position;
        }

        for (int iteration = 0; iteration < constraintIterations; iteration++) {
            for (int i = 0; i < positions.Length - 1; i++) {
                Vector3 delta = positions[i + 1] - positions[i];
                float dist = delta.magnitude;
                float error = dist - segmentLength;

                if (dist < 0.0001f) continue;

                Vector3 correction = (delta / dist) * error * 0.5f;

                if (i > 0) {
                    positions[i] += correction;
                }
                positions[i + 1] -= correction;
            }
        }
    }
}
