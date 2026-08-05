using UnityEngine;

namespace SpinForward.Level
{

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

        public event System.Action<Cube> Smashed;

        /// <summary>Global signal fired by ANY cube when it shatters, carrying the
        /// world position. Lets money/sfx/effects react without referencing cubes.</summary>
        public static event System.Action<Vector3> AnyCubeSmashed;

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

            rb.isKinematic = false;
            Vector3 dir = (transform.position - hitFrom).normalized + Vector3.up * 0.5f;
            rb.AddForce(dir * knockForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * knockForce, ForceMode.Impulse);

            Smashed?.Invoke(this);
            AnyCubeSmashed?.Invoke(transform.position);
            Destroy(gameObject, debrisLifetime);
        }
    }
}
