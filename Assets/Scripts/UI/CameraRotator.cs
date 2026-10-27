using UnityEngine;

[ExecuteAlways]
public class CameraRotator : MonoBehaviour
{
    public enum RotationMode
    {
        ContinuousSpin,
        OscillateLeftRight,
        OrbitAroundTarget
    }

    [Header("Mode Configuration")]
    [SerializeField] private RotationMode rotationMode = RotationMode.ContinuousSpin;

    [Header("Continuous Spin Settings")]
    [SerializeField] private Vector3 spinSpeed = new Vector3(0f, 15f, 0f);
    [SerializeField] private Space spinSpace = Space.Self;

    [Header("Oscillation (Left / Right Pan) Settings")]
    [SerializeField] private float oscillateAngle = 45f;
    [SerializeField] private float oscillateSpeed = 2f;
    [SerializeField] private bool oscillateOnXAxis = false;
    [SerializeField] private bool oscillateOnYAxis = true;
    [SerializeField] private bool oscillateOnZAxis = false;

    [Header("Orbit Settings")]
    [SerializeField] private Transform orbitTarget;
    [SerializeField] private float orbitDistance = 10f;
    [SerializeField] private float orbitSpeed = 20f;
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private Vector3 orbitOffset = Vector3.zero;

    private float currentOscillationTime;
    private Vector3 initialRotation;
    private Vector3 initialPosition;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        initialPosition = transform.position;

        if (rotationMode == RotationMode.OrbitAroundTarget && orbitTarget != null)
        {
            transform.position = orbitTarget.position + orbitOffset + (transform.forward * -orbitDistance);
        }
    }

    void Update()
    {
        // Use delta time in play mode, or a manual time step in edit mode so it updates smoothly when dragging sliders
        float dt = Application.isPlaying ? Time.deltaTime : 0.016f;

        switch (rotationMode)
        {
            case RotationMode.ContinuousSpin:
                HandleContinuousSpin(dt);
                break;
            case RotationMode.OscillateLeftRight:
                HandleOscillation(dt);
                break;
            case RotationMode.OrbitAroundTarget:
                HandleOrbit(dt);
                break;
        }
    }

    private void HandleContinuousSpin(float dt)
    {
        transform.Rotate(spinSpeed * dt, spinSpace);
    }

    private void HandleOscillation(float dt)
    {
        currentOscillationTime += dt * oscillateSpeed;
        float angle = Mathf.Sin(currentOscillationTime) * oscillateAngle;

        Vector3 targetEuler = initialRotation;
        if (oscillateOnXAxis) targetEuler.x += angle;
        if (oscillateOnYAxis) targetEuler.y += angle;
        if (oscillateOnZAxis) targetEuler.z += angle;

        transform.localEulerAngles = targetEuler;
    }

    private void HandleOrbit(float dt)
    {
        if (orbitTarget == null) return;

        transform.RotateAround(orbitTarget.position + orbitOffset, Vector3.up, orbitSpeed * dt);

        if (lookAtTarget)
        {
            transform.LookAt(orbitTarget.position + orbitOffset);
        }
    }
}