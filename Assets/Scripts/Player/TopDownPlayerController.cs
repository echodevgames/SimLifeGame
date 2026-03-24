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
            moveDirection *= MoveSpeed * Time.deltaTime;

            // Apply minimal constant gravity to keep the player grounded on flat terrain.
            moveDirection.y = Gravity * Time.deltaTime;

            characterController.Move(moveDirection);
        }
    }
}
