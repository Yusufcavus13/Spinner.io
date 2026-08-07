using System.Collections.Generic;
using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.Level
{
    public enum CubeType { Normal, Bomb, Steel, Ice, Shield, Split }

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
        public bool IsSmashed => isSmashed;
        private bool isSmashed;
        
        private int currentHealth = 1;
        public CubeType MyType => myType;
        private CubeType myType = CubeType.Normal;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Dictionary<Color, Material> ColorMaterials = new Dictionary<Color, Material>();

        private void Awake()
        {
            // Frozen cubes are cheap STATIC colliders - no Rigidbody. One is only
            // added when the cube must slide (moving walls) or fly (on shatter).
            // This is what makes big voxel images affordable on mobile.
            rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true; // if the prefab still ships one, keep it frozen
            rend = GetComponent<Renderer>();
        }

        /// <summary>Gives the cube a kinematic Rigidbody so a moving/breathing wall can
        /// slide it cheaply (dragging a static collider by transform is expensive).</summary>
        public void MakeMovable()
        {
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public void Init(CubeType type, int health)
        {
            myType = type;
            transform.localScale = Vector3.one; // Reset scale
            
            if (type == CubeType.Steel)
                currentHealth = 9999; 
            else if (type == CubeType.Shield)
            {
                currentHealth = health + 1; 
                transform.localScale = Vector3.one * 1.15f; // Shield cubes are slightly larger
            }
            else
                currentHealth = health;
                
            if (myType == CubeType.Ice)
                SetColor(new Color(0.5f, 0.8f, 1f)); 
            else if (myType == CubeType.Shield)
                SetColor(Color.grey); 
            else if (myType == CubeType.Split)
                SetColor(new Color(1f, 0.5f, 0f)); 
        }
        
        public void MoveTo(Vector3 targetPos)
        {
            if (isSmashed)
                return;
            if (rb != null && rb.isKinematic)
                rb.MovePosition(targetPos);
            else
                transform.position = targetPos; // fallback (static cube, shouldn't happen)
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
                
            // Fever Modundayken TEK ATAR!
            if (SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive)
            {
                damage = 99999; // Çelik küplerin bile canı yetmez!
            }
                
            TakeDamage(damage, collision.transform.position);
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            bool isFever = SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive;
            
            if (isSmashed) return;
            
            // Çelik küpler normalde kırılmaz, ama Fever Modu aktifse acımaz!
            if (myType == CubeType.Steel && !isFever) return;
            
            // Kalkanlı Küp (Shield) Mekaniği: Fever modunda değilsek, kalkan bütün hasarı emmeli!
            if (myType == CubeType.Shield && currentHealth > 1 && !isFever)
            {
                currentHealth = 1; // Kalkan kırıldı, asıl cana (1) düştü
                SetColor(Color.white); // Kalkan kırıldığında rengi beyaza dönsün
                
                // Kıvılcım veya ses efekti eklenebilir
                if (SpinForward.UI.FloatingTextManager.Instance != null)
                {
                    SpinForward.UI.FloatingTextManager.Instance.ShowDamage(0, transform.position); // "Shield Broken" da yazdırılabilir
                }
                return; // Kalkan kırıldığı için asıl hasarı alma
            }

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

            // Frozen cubes carry no Rigidbody; add one now so the debris can fly.
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            if (TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = false;
            }
            
            // Buz Küpü Mekaniği
            if (myType == CubeType.Ice && SpinForward.Player.SpinnerController.Instance != null)
            {
                SpinForward.Player.SpinnerController.Instance.ApplyIceDebuff(3f); // 3 saniye yavaşlat
            }
            
            // Klonlama (Split) Mekaniği
            if (myType == CubeType.Split && SpinForward.Player.SpinnerController.Instance != null)
            {
                SpinForward.Player.SpinnerController.Instance.SpawnClones(2, 4f); // 2 klon, 4 saniye
            }

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
