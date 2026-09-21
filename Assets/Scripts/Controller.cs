using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour {
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private Transform horizontalAxis;
    [SerializeField] private Transform forwardAxis;
    [SerializeField] private Transform verticalAxis;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 2f;

    [Header("Horizontal Limits")]
    [SerializeField] private float horizontalMin;
    [SerializeField] private float horizontalMax;
    [SerializeField] private bool invertHorizontal;

    [Header("Forward Limits")]
    [SerializeField] private float forwardMin;
    [SerializeField] private float forwardMax;
    [SerializeField] private bool invertForward;

    [Header("Drop")]
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float dropDuration = 2f;
    [SerializeField] private float dropSpeed = 5f;
    [SerializeField] private float returnSpeed = 3f;

    [Header("Drop Chute")]
    [SerializeField] private Vector3 dropChutePosition;
    [SerializeField] private float chuteReturnSpeed = 3f;

    [Header("Chain Segment")]
    [SerializeField] private Rigidbody dynamicSegment;
    [SerializeField] private Transform segmentRestPosition;
    [SerializeField] private float segmentReturnSpeed = 3f;

    [Header("Visual Segments")]
    [SerializeField] private Transform visualSegmentsParent;
    [SerializeField] private float visualAppearEarlier = 0.5f;
    [SerializeField] private float visualDisappearLater = 1f;

    [Header("Magnet")]
    [SerializeField] private Magnet magnet;

    [Header("Joystick")]
    [SerializeField] private Transform joystick;
    [SerializeField] private float joystickRotationSpeed = 10f;
    [SerializeField] private float maxJoystickAngle = 30f;
    [SerializeField] private bool invertHorizontalAxis = false;
    [SerializeField] private bool invertVerticalAxis = false;

    [Header("Button")]
    [SerializeField] private Transform dropButton;
    [SerializeField] private float buttonPressDepth = 0.1f;
    [SerializeField] private float buttonPressSpeed = 15f;

    private Vector2 input;
    private Vector2 velocity;
    private bool isDropping;
    private Vector3 buttonStartPos;

    public bool IsDropping => isDropping;

    private void SetVisualRenderers(bool enabled) {
        if (visualSegmentsParent == null) return;
        for (int i = 0; i < visualSegmentsParent.childCount; i++) {
            Renderer r = visualSegmentsParent.GetChild(i).GetComponent<Renderer>();
            if (r != null) r.enabled = enabled;
        }
    }

    private void SetVisualRenderer(int index, bool enabled) {
        if (visualSegmentsParent == null || index < 0 || index >= visualSegmentsParent.childCount) return;
        Renderer r = visualSegmentsParent.GetChild(index).GetComponent<Renderer>();
        if (r != null) r.enabled = enabled;
    }

    private void Start() {
        if (dropButton != null) {
            buttonStartPos = dropButton.localPosition;
        }
    }

    private void OnEnable() {
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;
        dropAction.action.performed += OnDrop;
    }

    private void OnDisable() {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        dropAction.action.performed -= OnDrop;
    }

    private void OnMove(InputAction.CallbackContext ctx) {
        input = ctx.ReadValue<Vector2>();
    }

    private void OnDrop(InputAction.CallbackContext ctx) {
        if (!isDropping && verticalAxis != null) {
            StartCoroutine(DropRoutine());
        }
    }

    private void Update() {
        if (isDropping) return;

        float accel = input.sqrMagnitude > 0.01f ? acceleration : deceleration;

        velocity.x = Mathf.MoveTowards(velocity.x, input.x * speed, accel * Time.deltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, input.y * speed, accel * Time.deltaTime);

        if (horizontalAxis != null) {
            Vector3 pos = horizontalAxis.position;
            float dir = invertHorizontal ? -1f : 1f;
            pos.x += velocity.x * dir * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, horizontalMin, horizontalMax);
            horizontalAxis.position = pos;

            if (pos.x <= horizontalMin || pos.x >= horizontalMax) {
                velocity.x = 0f;
            }
        }

        if (forwardAxis != null) {
            Vector3 pos = forwardAxis.position;
            float dir = invertForward ? -1f : 1f;
            pos.z += velocity.y * dir * Time.deltaTime;
            pos.z = Mathf.Clamp(pos.z, forwardMin, forwardMax);
            forwardAxis.position = pos;

            if (pos.z <= forwardMin || pos.z >= forwardMax) {
                velocity.y = 0f;
            }
        }

        UpdateJoystickRotation();
    }

    private void UpdateJoystickRotation() {
        if (joystick == null) return;

        float horizontal = invertHorizontalAxis ? -velocity.x : velocity.x;
        float vertical = invertVerticalAxis ? -velocity.y : velocity.y;

        float targetX = -vertical / speed * maxJoystickAngle;
        float targetY = horizontal / speed * maxJoystickAngle;

        Quaternion targetRotation = Quaternion.Euler(targetX, targetY, 0f);
        joystick.localRotation = Quaternion.Slerp(joystick.localRotation, targetRotation, joystickRotationSpeed * Time.deltaTime);

        if (Mathf.Abs(velocity.x) > 0.01f || Mathf.Abs(velocity.y) > 0.01f) {
            Debug.Log($"[Joystick] vel=({velocity.x:F2}, {velocity.y:F2}) target=({targetX:F1}, {targetY:F1})");
        }
    }

    private IEnumerator DropRoutine() {
        isDropping = true;
        velocity = Vector2.zero;

        Vector3 verticalStartPos = verticalAxis.position;
        Vector3 dropTarget = verticalStartPos + Vector3.down * dropDistance;

        if (dynamicSegment != null) dynamicSegment.isKinematic = false;
        if (magnet != null) magnet.EnableMagneticField();
        SetVisualRenderers(false);
        StartCoroutine(PressButton(true));

        int segmentsShown = 0;
        float dropTime = dropDistance / dropSpeed;
        float appearOffset = visualAppearEarlier / dropTime;

        while (Vector3.Distance(verticalAxis.position, dropTarget) > 0.01f) {
            verticalAxis.position = Vector3.MoveTowards(verticalAxis.position, dropTarget, dropSpeed * Time.deltaTime);

            if (visualSegmentsParent != null) {
                float elapsed = Mathf.Clamp01(Vector3.Distance(verticalStartPos, verticalAxis.position) / dropDistance + appearOffset);
                int target = Mathf.FloorToInt(elapsed * visualSegmentsParent.childCount);
                while (segmentsShown < target && segmentsShown < visualSegmentsParent.childCount) {
                    SetVisualRenderer(segmentsShown, true);
                    segmentsShown++;
                }
            }

            yield return null;
        }

        SetVisualRenderers(true);

        yield return new WaitForSeconds(dropDuration);

        int segmentsHidden = 0;
        float returnTime = dropDistance / returnSpeed;
        float disappearOffset = visualDisappearLater / returnTime;

        while (Vector3.Distance(verticalAxis.position, verticalStartPos) > 0.01f) {
            verticalAxis.position = Vector3.MoveTowards(verticalAxis.position, verticalStartPos, returnSpeed * Time.deltaTime);

            if (visualSegmentsParent != null) {
                float elapsed = Mathf.Clamp01(1f - (Vector3.Distance(verticalStartPos, verticalAxis.position) / dropDistance) - disappearOffset);
                int target = Mathf.FloorToInt(elapsed * visualSegmentsParent.childCount);
                int index = visualSegmentsParent.childCount - 1 - segmentsHidden;
                while (segmentsHidden < target && index >= 0) {
                    SetVisualRenderer(index, false);
                    segmentsHidden++;
                    index = visualSegmentsParent.childCount - 1 - segmentsHidden;
                }
            }

            yield return null;
        }

        SetVisualRenderers(false);
        verticalAxis.position = verticalStartPos;

        if (dynamicSegment != null && segmentRestPosition != null) {
            dynamicSegment.isKinematic = true;
            while (Mathf.Abs(dynamicSegment.position.y - segmentRestPosition.position.y) > 0.01f) {
                dynamicSegment.transform.position = Vector3.MoveTowards(
                    dynamicSegment.transform.position,
                    segmentRestPosition.position,
                    segmentReturnSpeed * Time.deltaTime);
                yield return null;
            }
            dynamicSegment.transform.position = segmentRestPosition.position;
        }

        while (Mathf.Abs(horizontalAxis.position.x - dropChutePosition.x) > 0.01f) {
            float dir = dropChutePosition.x > horizontalAxis.position.x ? 1f : -1f;
            Vector3 pos = horizontalAxis.position;
            pos.x += dir * chuteReturnSpeed * Time.deltaTime;
            if ((dir > 0 && pos.x > dropChutePosition.x) || (dir < 0 && pos.x < dropChutePosition.x)) {
                pos.x = dropChutePosition.x;
            }
            horizontalAxis.position = pos;
            yield return null;
        }

        while (Mathf.Abs(forwardAxis.position.z - dropChutePosition.z) > 0.01f) {
            float dir = dropChutePosition.z > forwardAxis.position.z ? 1f : -1f;
            Vector3 pos = forwardAxis.position;
            pos.z += dir * chuteReturnSpeed * Time.deltaTime;
            if ((dir > 0 && pos.z > dropChutePosition.z) || (dir < 0 && pos.z < dropChutePosition.z)) {
                pos.z = dropChutePosition.z;
            }
            forwardAxis.position = pos;
            yield return null;
        }

        if (magnet != null) magnet.DisableMagneticField();
        StartCoroutine(PressButton(false));
        RoundsManager.Instance?.OnPullUsed();
        isDropping = false;
    }

    private IEnumerator PressButton(bool press) {
        if (dropButton == null) yield break;

        Vector3 targetPos = press
            ? buttonStartPos - Vector3.up * buttonPressDepth
            : buttonStartPos;

        while (Vector3.Distance(dropButton.localPosition, targetPos) > 0.001f) {
            dropButton.localPosition = Vector3.MoveTowards(dropButton.localPosition, targetPos, buttonPressSpeed * Time.deltaTime);
            yield return null;
        }
        dropButton.localPosition = targetPos;
    }
}
