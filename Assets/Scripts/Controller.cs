using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour {
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Transform horizontalAxis;
    [SerializeField] private Transform forwardAxis;
    [SerializeField] private Transform verticalAxis;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 2f;

    [Header("Horizontal Limits")]
    [SerializeField] private float horizontalMin;
    [SerializeField] private float horizontalMax;

    [Header("Forward Limits")]
    [SerializeField] private float forwardMin;
    [SerializeField] private float forwardMax;

    private Vector2 input;
    private Vector2 velocity;

    private void OnEnable() {
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;
    }

    private void OnDisable() {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext ctx) {
        input = ctx.ReadValue<Vector2>();
    }

    private void Update() {
        float accel = input.sqrMagnitude > 0.01f ? acceleration : deceleration;

        velocity.x = Mathf.MoveTowards(velocity.x, input.x * speed, accel * Time.deltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, input.y * speed, accel * Time.deltaTime);

        if (horizontalAxis != null) {
            Vector3 pos = horizontalAxis.position;
            pos.x += velocity.x * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, horizontalMin, horizontalMax);
            horizontalAxis.position = pos;
        }

        if (forwardAxis != null) {
            Vector3 pos = forwardAxis.position;
            pos.z += velocity.y * Time.deltaTime;
            pos.z = Mathf.Clamp(pos.z, forwardMin, forwardMax);
            forwardAxis.position = pos;
        }
    }
}
