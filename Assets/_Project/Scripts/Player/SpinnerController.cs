using SpinForward.Economy;
using SpinForward.Level;
using UnityEngine;

namespace SpinForward.Player
{
    
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
        [Tooltip("Fallback spin speed (deg/sec) used when no UpgradeSystem is present.")]
        [SerializeField] private float spinSpeed = 720f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Rotate upgrade controls the spin speed; fall back to the serialized
            // value if the upgrade system isn't in the scene.
            float spin = spinSpeed;
            if (UpgradeSystem.Instance != null)
                spin = UpgradeSystem.Instance.Rotate.Value;

            if (visual != null)
                visual.Rotate(0f, spin * Time.deltaTime, 0f, Space.World);
        }

        private void FixedUpdate()
        {
            // Sadece oyun oynanıyorken hareket etmesine izin ver
            if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

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
