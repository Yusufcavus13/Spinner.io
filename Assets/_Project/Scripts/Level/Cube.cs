using System.Collections.Generic;
using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.Level
{
    public enum CubeType { Normal, Bomb, Steel }

    [RequireComponent(typeof(Rigidbody))]
    public class Cube : MonoBehaviour
    {
        [Tooltip("Tag the spinner must have for a hit to count.")]
        [SerializeField] private string spinnerTag = "Spinner";
        [Tooltip("How hard the cube is flung when it shatters.")]
        [SerializeField] private float knockForce = 6f;
        [Tooltip("Seconds the shattered debris lives before it is removed.")]
        [SerializeField] private float debrisLifetime = 1.2f;
        
        [Header("Bomb Settings")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private int explosionDamage = 5;

        public event System.Action<Cube> Smashed;
        public static event System.Action<Vector3> AnyCubeSmashed;

        private Rigidbody rb;
        private Renderer rend;
        private bool isSmashed;
        
        private int currentHealth = 1;
        private CubeType myType = CubeType.Normal;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Dictionary<Color, Material> ColorMaterials = new Dictionary<Color, Material>();

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // frozen in place until hit
            rend = GetComponent<Renderer>();
        }

        public void Init(CubeType type, int health)
        {
            myType = type;
            currentHealth = type == CubeType.Steel ? 9999 : health; // Steel is practically invincible
        }

        public void SetColor(Color color)
        {
            if (rend == null)
                rend = GetComponent<Renderer>();
            if (rend == null)
                return;

            if (!ColorMaterials.TryGetValue(color, out Material mat))
            {
                mat = new Material(rend.sharedMaterial);
                mat.SetColor(BaseColorId, color);
                ColorMaterials[color] = mat;
            }
            rend.sharedMaterial = mat;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isSmashed)
                return;
                
            // Sadece Spinner vurabilir (Zincirleme patlamalar TakeDamage üzerinden yürüyecek)
            if (!collision.collider.CompareTag(spinnerTag))
                return;

            // Power upgrade allows doing more damage per hit
            int damage = 1;
            if (UpgradeSystem.Instance != null)
                damage = Mathf.CeilToInt(UpgradeSystem.Instance.Power.Value);
                
            TakeDamage(damage, collision.transform.position);
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (isSmashed || myType == CubeType.Steel)
                return;

            currentHealth -= amount;

            // Ekrana hasar miktarını yazdır
            if (SpinForward.UI.FloatingTextManager.Instance != null)
            {
                SpinForward.UI.FloatingTextManager.Instance.ShowDamage(amount, transform.position);
            }
            
            // Eğer can 0'ın altına düştüyse kırıl!
            if (currentHealth <= 0)
            {
                Shatter(hitPoint);
                
                // Eğer bomba ise etrafındakileri de patlat!
                if (myType == CubeType.Bomb)
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider c in colliders)
            {
                if (c.TryGetComponent(out Cube neighborCube) && neighborCube != this)
                {
                    // Bomba patladığında etrafındaki küplere yüksek hasar verir
                    neighborCube.TakeDamage(explosionDamage, transform.position);
                }
            }
        }

        private void Shatter(Vector3 hitFrom)
        {
            isSmashed = true;
            rb.isKinematic = false;

            float force = knockForce;
            Vector3 dir = (transform.position - hitFrom).normalized + Vector3.up * 0.5f;
            rb.AddForce(dir * force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * force, ForceMode.Impulse);

            Smashed?.Invoke(this);
            AnyCubeSmashed?.Invoke(transform.position);
            Destroy(gameObject, debrisLifetime);
        }
    }
}
