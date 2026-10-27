using UnityEngine;

public class MenuLogoAnimator : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float scaleSpeed = 2f;
    [SerializeField] private float scaleAmount = 0.05f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 1.5f;
    [SerializeField] private float rotationAmount = 3f;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        float time = Time.unscaledTime;

        float scaleFactor = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmount;
        transform.localScale = originalScale * scaleFactor;

        float zRotation = Mathf.Sin(time * rotationSpeed) * rotationAmount;
        transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, zRotation);
    }
}