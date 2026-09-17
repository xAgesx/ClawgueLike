using UnityEngine;

public class ChainAnchor : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    public Transform carriage;
    public Vector3 offset;
    void FixedUpdate() {
    rb.position = carriage.position + offset;  
}
}
