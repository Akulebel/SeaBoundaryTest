using UnityEngine;
using UnityEngine.InputSystem;

namespace SeaBoundary
{
    public sealed class ShipController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0f)] private float turnSpeed = 100f;
        private InputAction move;

        private void OnEnable()
        {
            move = actions != null ? actions.FindAction("Player/Move", false) : null;
            if (move == null)
            {
                Debug.LogError("ShipController: assign InputSystem_Actions with Player/Move.", this);
                enabled = false;
                return;
            }
            move.Enable();
        }

        private void OnDisable() => move?.Disable();

        private void Update()
        {
            Vector2 input = move.ReadValue<Vector2>();
            transform.Rotate(Vector3.up, input.x * turnSpeed * Time.deltaTime, Space.World);
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            transform.position += forward * (input.y * moveSpeed * Time.deltaTime);
        }
    }
}
