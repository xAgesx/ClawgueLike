using UnityEngine;

public class Magnet : MonoBehaviour {
    [SerializeField] private float attractionRadius = 1f;
    [SerializeField] private float attractionForce = 10f;
    [SerializeField] private float rotationForce = 5f;
    [SerializeField] private LayerMask itemsLayer;

    private bool magneticFieldActive;

    public void EnableMagneticField() {
        magneticFieldActive = true;
        Debug.Log("[Magnet] Magnetic field enabled");
    }

    public void DisableMagneticField() {
        magneticFieldActive = false;
        Debug.Log("[Magnet] Magnetic field disabled");
    }

    private void FixedUpdate() {
        if (!magneticFieldActive) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attractionRadius, itemsLayer);

        foreach (Collider hit in hits) {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null) continue;

            ItemData itemData = GetItemData(hit.gameObject);
            float multiplier = itemData != null ? itemData.magneticAttraction : 1f;
            if (multiplier <= 0f) continue;

            Vector3 direction = transform.position - hit.transform.position;
            float distance = direction.magnitude;
            float forceMagnitude = attractionForce * multiplier * (attractionRadius / Mathf.Max(distance, 0.01f));

            rb.AddForce(direction.normalized * forceMagnitude, ForceMode.Acceleration);

            Vector3 toMagnet = (transform.position - hit.transform.position).normalized;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, toMagnet);
            float tiltAngle = Vector3.Angle(Vector3.up, toMagnet);
            float rotMagnitude = rotationForce * multiplier * (attractionRadius / Mathf.Max(distance, 0.01f));
            rb.AddTorque(tiltAxis * tiltAngle * rotMagnitude * 0.01f, ForceMode.Acceleration);
        }
    }

    private ItemData GetItemData(GameObject obj) {
        ItemPickup pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null) return pickup.ItemData;
        return null;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = magneticFieldActive ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
