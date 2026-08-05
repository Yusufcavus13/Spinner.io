using UnityEngine;

namespace SpinForward.Economy
{
    /// <summary>
    /// A single coin that pops out of a smashed cube, homes in on the spinner
    /// (accelerating as it goes), and pays into the <see cref="Wallet"/> on arrival.
    /// Moves purely by transform - no physics, no collider needed.
    /// </summary>
    public class MoneyOrb : MonoBehaviour
    {
        [Tooltip("Short pause before it starts flying, so the pop reads.")]
        [SerializeField] private float startDelay = 0.15f;
        [SerializeField] private float startSpeed = 2f;
        [SerializeField] private float acceleration = 22f;
        [Tooltip("How close counts as 'arrived'.")]
        [SerializeField] private float catchDistance = 0.35f;

        private Transform target;
        private int value;
        private float speed;
        private float timer;

        /// <summary>Called by the spawner right after Instantiate.</summary>
        public void Launch(Transform spinner, int reward)
        {
            target = spinner;
            value = reward;
            speed = startSpeed;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject); // spinner gone, don't linger
                return;
            }

            timer += Time.deltaTime;
            if (timer < startDelay)
                return;

            speed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) <= catchDistance)
            {
                if (Wallet.Instance != null)
                    Wallet.Instance.Add(value);
                Destroy(gameObject);
            }
        }
    }
}
