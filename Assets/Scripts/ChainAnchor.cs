using UnityEngine;

public class ChainAnchor : MonoBehaviour {
    [SerializeField] private Rigidbody rb;
    public Transform carriage;
    public Vector3 offset;

    private void FixedUpdate() {
        rb.MovePosition(carriage.position + offset);
    }
}
