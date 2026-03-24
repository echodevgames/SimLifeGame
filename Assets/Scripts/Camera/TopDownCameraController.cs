using UnityEngine;

namespace SeedyRoots.Camera
{
    /// <summary>
    /// Positions the camera in a fixed top-down isometric offset above the player Transform.
    /// Smoothly follows the target via LateUpdate.
    /// </summary>
    public class TopDownCameraController : MonoBehaviour
    {
        [Tooltip("The Transform to follow (assign the Player Transform).")]
        [SerializeField] private Transform target;

        [Tooltip("Position offset from the target. Controls camera height and angle distance.")]
        public Vector3 Offset = new Vector3(0f, 15f, -10f);

        [Tooltip("Lerp speed for camera follow smoothing.")]
        public float SmoothSpeed = 5f;

        private void Awake()
        {
            // Set a fixed isometric downward rotation once at startup.
            transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("[TopDownCameraController] Target is not assigned.", this);
                return;
            }

            Vector3 desiredPosition = target.position + Offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed * Time.deltaTime);
        }
    }
}
