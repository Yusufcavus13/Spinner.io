using System.Collections.Generic;
using SpinForward.Core;
using UnityEngine;

namespace SpinForward.Economy
{
    public class MoneyOrb : MonoBehaviour
    {
        // Every live orb registers here so a level change can wipe them all at once.
        private static readonly List<MoneyOrb> Active = new List<MoneyOrb>();

        /// <summary>Destroys every coin currently in flight (called on level change).</summary>
        public static void ClearAll()
        {
            for (int i = Active.Count - 1; i >= 0; i--)
                if (Active[i] != null)
                    Destroy(Active[i].gameObject);
            Active.Clear();
        }

        [SerializeField] private float startDelay = 0.25f;
        [SerializeField] private float startSpeed = 2f;
        [SerializeField] private float acceleration = 16f;
        [Tooltip("How close counts as 'arrived'.")]
        [SerializeField] private float catchDistance = 0.25f;

        [Header("Audio")]
        [SerializeField] private AudioClip collectClip;
        [Range(0f, 1f)]
        [SerializeField] private float collectVolume = 0.35f;

        private Transform target;
        private int value;
        private float speed;
        private float timer;

        private void Awake() => Active.Add(this);
        private void OnDestroy() => Active.Remove(this);

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
                Destroy(gameObject); 
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
                if (Sfx.Instance != null)
                    Sfx.Instance.Play(collectClip, collectVolume);
                Destroy(gameObject);
            }
        }
    }
}
