using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// One breakable cube. Sits frozen (kinematic) so the wall holds its shape,
    /// then wakes up, gets knocked away and shatters when the spinner hits it.
    /// Raises <see cref="Smashed"/> so the wall (and later the economy) can react.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Cube : MonoBehaviour
    {
        [Tooltip("Tag the spinner must have for a hit to count.")]
        [SerializeField] private string spinnerTag = "Spinner";
        [Tooltip("Hits needed to shatter. Power upgrade will lower this later.")]
        [SerializeField] private int hitPoints = 1;
        [Tooltip("How hard the cube is flung when it shatters.")]
        [SerializeField] private float knockForce = 6f;
        [Tooltip("Seconds the shattered debris lives before it is removed.")]
        [SerializeField] private float debrisLifetime = 1.5f;

        /// <summary>Fires once, when this cube shatters. Carries itself so listeners
        /// know where it happened (useful for the money particle in the next step).</summary>
        public event System.Action<Cube> Smashed;

        private Rigidbody rb;
        private bool isSmashed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // frozen in place until hit
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isSmashed)
                return;
            if (!collision.collider.CompareTag(spinnerTag))
                return;

            hitPoints--;
            if (hitPoints <= 0)
                Shatter(collision.transform.position);
        }

        private void Shatter(Vector3 hitFrom)
        {
            isSmashed = true;

            // Wake up the physics and fling the cube away from the spinner.
            rb.isKinematic = false;
            Vector3 dir = (transform.position - hitFrom).normalized + Vector3.up * 0.5f;
            rb.AddForce(dir * knockForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * knockForce, ForceMode.Impulse);

            Smashed?.Invoke(this);
            Destroy(gameObject, debrisLifetime);
        }
    }
}
