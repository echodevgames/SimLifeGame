using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedyRoots.Player
{
    /// <summary>
    /// Top-down player controller using the new Input System.
    /// Moves the player on the XZ plane via a CharacterController.
    /// Requires a CharacterController and PlayerInput component on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class TopDownPlayerController : MonoBehaviour
    {
        [Tooltip("Movement speed in units per second.")]
        public float MoveSpeed = 5f;

        [Tooltip("Degrees per second the player rotates toward the movement direction.")]
        [SerializeField] private float rotationSpeed = 720f;

        private static readonly float Gravity = -9.81f;

        private CharacterController characterController;
        private Vector2 moveInput;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Receives the Move input action via the PlayerInput SendMessages behaviour.
        /// </summary>
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            // Convert 2D input to XZ plane movement.
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // Rotate the player root toward the movement direction.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            moveDirection *= MoveSpeed * Time.deltaTime;

            // Apply minimal constant gravity to keep the player grounded on flat terrain.
            moveDirection.y = Gravity * Time.deltaTime;

            characterController.Move(moveDirection);
        }
    }
}
