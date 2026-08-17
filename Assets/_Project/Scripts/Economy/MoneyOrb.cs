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

        [SerializeField] private float startDelay = 0.6f; // Yerde biraz sekmesi için süre
        [SerializeField] private float startSpeed = 5f;
        [SerializeField] private float acceleration = 25f;
        [Tooltip("How close counts as 'arrived'.")]
        [SerializeField] private float catchDistance = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip collectClip;
        [Range(0f, 1f)]
        [SerializeField] private float collectVolume = 0.35f;

        private Transform target;
        private int value;
        private float speed;
        private float timer;
        
        private Rigidbody rb;
        private bool isMagnetic = false;

        private void Awake()
        {
            Active.Add(this);
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.linearDamping = 1f; // havada çok uçmasın
            }
            
            // Altın rengi verelim
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.yellow;
            }
        }
        
        private void OnDestroy() => Active.Remove(this);

        public void Launch(Transform spinner, int reward)
        {
            target = spinner;
            value = reward;
            speed = startSpeed;
            
            // Pop up!
            if (rb != null)
            {
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y) + 1f; // Yukarı doğru ağırlıklı
                rb.AddForce(randomDir.normalized * Random.Range(3f, 6f), ForceMode.Impulse);
            }
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject); 
                return;
            }

            timer += Time.deltaTime;
            
            if (timer > startDelay)
            {
                isMagnetic = true;
                if (rb != null) rb.isKinematic = true; // Manyetik çekime girince fiziği kapat
            }

            if (isMagnetic)
            {
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
}
