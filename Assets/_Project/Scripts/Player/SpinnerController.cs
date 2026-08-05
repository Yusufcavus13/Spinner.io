using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// Physics-driven spinner. The joystick steers it around the XZ plane via a
    /// Rigidbody, while a visual child keeps spinning around Y. Because it moves
    /// with real velocity, it will knock cubes around once we add them.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpinnerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FloatingJoystick joystick;
        [Tooltip("Child transform that spins for looks (not the physics body).")]
        [SerializeField] private Transform visual;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float acceleration = 25f;

        [Header("Spin")]
        [Tooltip("Visual spin speed in degrees/second. Driven by the Rotate upgrade later.")]
        [SerializeField] private float spinSpeed = 720f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (visual != null)
                visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
        }

        private void FixedUpdate()
        {
            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;
            Vector3 dir = new Vector3(input.x, 0f, input.y);
            Vector3 targetVel = Vector3.ClampMagnitude(dir, 1f) * moveSpeed;

            Vector3 vel = rb.linearVelocity;
            Vector3 horizNow = new Vector3(vel.x, 0f, vel.z);
            Vector3 horizNext = Vector3.MoveTowards(horizNow, targetVel, acceleration * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(horizNext.x, vel.y, horizNext.z);
        }
    }
}
