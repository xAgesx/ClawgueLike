using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChainLink : MonoBehaviour {
    [SerializeField] private int solverIterations = 10;
    [SerializeField] private float mass = 2f;
    [SerializeField] private float angularDrag = 1f;
    [SerializeField] private RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    private void Awake() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.solverIterations = solverIterations;
        rb.mass = mass;
        rb.angularDamping = angularDrag;
        rb.interpolation = interpolation;
    }
}
