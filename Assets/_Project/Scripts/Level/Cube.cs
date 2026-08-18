using System.Collections.Generic;
using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.Level
{
    public enum CubeType { Normal, Bomb, Steel, Ice, Shield, Split, Frenzy, Laser, Gold, Drain, TimeBomb }

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
        [Tooltip("Physical blast force applied to loose debris when a bomb explodes.")]
        [SerializeField] private float explosionForce = 12f;
        [Tooltip("Energy the spinner loses if caught in a bomb blast.")]
        [SerializeField] private float bombEnergyPenalty = 12f;
        [Tooltip("How hard the spinner is knocked back by a bomb blast.")]
        [SerializeField] private float bombKnockback = 7f;

        [Header("Drain (Trap) Cube")]
        [Tooltip("Energy the glowing drain cube saps when smashed.")]
        [SerializeField] private float drainAmount = 10f;

        [Header("Time Bomb Cube")]
        [Tooltip("Seconds before an ARMED time-bomb cube detonates on its own.")]
        [SerializeField] private float timeBombDuration = 6f;
        [Tooltip("The spinner must come this close before a time-bomb cube ARMS and starts counting down.")]
        [SerializeField] private float timeBombArmDistance = 4.5f;

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
            rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true; 
            rend = GetComponent<Renderer>();
        }

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
            transform.localScale = Vector3.one; 
            
            if (type == CubeType.Steel)
                currentHealth = 8; // sert ama kırılabilir (eskiden 9999 = kırılamazdı)
            else if (type == CubeType.Shield)
            {
                currentHealth = health + 1; 
                transform.localScale = Vector3.one * 1.15f; 
            }
            else
                currentHealth = health;
                
            if (myType == CubeType.Ice)
                SetColor(new Color(0.5f, 0.8f, 1f)); 
            else if (myType == CubeType.Shield)
                SetColor(Color.grey); 
            else if (myType == CubeType.Split)
                SetColor(new Color(1f, 0.5f, 0f)); 
            else if (myType == CubeType.Frenzy)
                SetColor(Color.yellow); // Frenzy = Golden/Yellow
            else if (myType == CubeType.Drain)
                SetGlowColor(new Color(0.72f, 0.1f, 0.95f)); // parlayan mor tuzak - net ayırt edilir
            else if (myType == CubeType.TimeBomb)
            {
                SetGlowColor(new Color(1f, 0.55f, 0.05f)); // dormant: glowing amber, clearly distinct
                StartCoroutine(TimeBombRoutine());
            }
        }

        // Stays DORMANT until the spinner comes close; only then does it arm and count down
        // (so far-off bombs don't all detonate at once at level start). Clear it in time = safe.
        private System.Collections.IEnumerator TimeBombRoutine()
        {
            if (rend == null)
                rend = GetComponent<Renderer>();

            while (!isSmashed)
            {
                var sc = SpinForward.Player.SpinnerController.Instance;
                if (sc != null)
                {
                    Vector3 d = sc.transform.position - transform.position;
                    d.y = 0f;
                    if (d.magnitude <= timeBombArmDistance)
                        break; // spinner is near - arm it
                }
                yield return null;
            }
            if (isSmashed)
                yield break;

            // Armed: pulse red, faster and faster, then detonate.
            Material mat = rend != null ? rend.material : null; // own instance so pulsing doesn't pool
            float t = timeBombDuration;
            while (t > 0f && !isSmashed)
            {
                t -= Time.deltaTime;
                if (mat != null)
                {
                    float k = 1f - Mathf.Clamp01(t / timeBombDuration);
                    float pulse = Mathf.Abs(Mathf.Sin(Time.time * Mathf.Lerp(3f, 16f, k)));
                    mat.SetColor(BaseColorId, Color.Lerp(new Color(1f, 0.6f, 0.05f), new Color(1f, 0.05f, 0.05f), pulse));
                }
                yield return null;
            }

            if (!isSmashed)
            {
                // Time's up: it blows on its own (clears neighbors, hurts the spinner if near).
                Explode();
                Shatter(transform.position);
            }
        }
        
        public void MoveTo(Vector3 targetPos)
        {
            if (isSmashed)
                return;
            if (rb != null && rb.isKinematic)
                rb.MovePosition(targetPos);
            else
                transform.position = targetPos; 
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

        private static readonly Dictionary<Color, Material> GlowMaterials = new Dictionary<Color, Material>();

        // Like SetColor but EMISSIVE (the cube glows) - used for the distinct drain trap cube.
        public void SetGlowColor(Color color)
        {
            if (rend == null)
                rend = GetComponent<Renderer>();
            if (rend == null)
                return;

            if (!GlowMaterials.TryGetValue(color, out Material mat))
            {
                mat = new Material(rend.sharedMaterial);
                mat.SetColor(BaseColorId, color);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2.5f);
                GlowMaterials[color] = mat;
            }
            rend.sharedMaterial = mat;
        }

        private float lastDamageTime = 0f;

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (Time.time - lastDamageTime >= 0.1f)
            {
                HandleCollision(collision);
            }
        }

        private void HandleCollision(Collision collision)
        {
            if (isSmashed)
                return;
                
            // Sadece Spinner (veya klonları) vurabilir. Para orbu / parça / zemin geçmez.
            if (!collision.collider.CompareTag(spinnerTag))
                return;

            lastDamageTime = Time.time;

            // Tough cubes just WOBBLE on hit - no push-back (that shoved the spinner around).
            if (currentHealth > 4)
            {
                transform.localScale = Vector3.one * 0.9f;
                Invoke(nameof(ResetScale), 0.1f);
            }

            int damage = 1;
            if (UpgradeSystem.Instance != null)
                damage = Mathf.CeilToInt(UpgradeSystem.Instance.Power.Value);

            // Equipped shop skin adds flat bonus damage.
            if (SpinForward.Economy.SkinManager.Instance != null)
                damage += SpinForward.Economy.SkinManager.Instance.CurrentBonusDamage;
                
            if (SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive)
            {
                damage = 99999; 
            }
            
            // Frenzy Buff bonus
            if (SpinForward.Player.SpinnerController.Instance != null && SpinForward.Player.SpinnerController.Instance.IsFrenzyActive)
            {
                damage *= 3; // Frenzy aktifse x3 hasar!
            }
                
            TakeDamage(damage, collision.transform.position);
        }

        private void ResetScale()
        {
            if (!isSmashed) transform.localScale = (myType == CubeType.Shield) ? Vector3.one * 1.15f : Vector3.one;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            bool isFever = SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive;
            
            if (isSmashed) return;

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

            // Ekrana hasar miktarını yazdır (Fever modunda 9999 yazmasın diye gizledik!)
            if (!isFever && SpinForward.UI.FloatingTextManager.Instance != null)
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
                else if (myType == CubeType.Laser)
                {
                    FireLaser();
                }
                else if (myType == CubeType.Gold)
                {
                    // Altın küp, mevcut income'un 15 katını verir.
                    float incomeVal = SpinForward.Economy.UpgradeSystem.Instance != null ? SpinForward.Economy.UpgradeSystem.Instance.Income.Value : 2f;
                    int bonusGold = Mathf.Max(10, Mathf.RoundToInt(incomeVal * 15f));
                    if (SpinForward.Economy.Wallet.Instance != null)
                        SpinForward.Economy.Wallet.Instance.Add(bonusGold);
                        
                    if (SpinForward.UI.FloatingTextManager.Instance != null)
                        SpinForward.UI.FloatingTextManager.Instance.ShowDamage(bonusGold, transform.position + Vector3.up); // Yazı azıcık yukarıda çıksın
                }
                else if (myType == CubeType.Drain && LevelManager.Instance != null)
                {
                    LevelManager.Instance.DrainEnergy(drainAmount); // tuzak küp: enerji emer
                }
            }
        }

        private void FireLaser()
        {
            if (SpinForward.CameraControl.CameraController.Instance != null)
                SpinForward.CameraControl.CameraController.Instance.HeavyShake(1f);
                
            // Haç şeklinde (Yatay ve Dikey) BoxCast/OverlapBox kullanarak sıradaki her şeyi sil!
            Vector3 extentsH = new Vector3(30f, 1f, 0.5f);
            Vector3 extentsV = new Vector3(0.5f, 1f, 30f);

            Collider[] hitsH = Physics.OverlapBox(transform.position, extentsH, Quaternion.identity);
            foreach (var h in hitsH)
            {
                if (h.TryGetComponent(out Cube c) && c != this && !c.isSmashed)
                {
                    c.TakeDamage(999, transform.position); // Lazer anında yok eder
                }
            }

            Collider[] hitsV = Physics.OverlapBox(transform.position, extentsV, Quaternion.identity);
            foreach (var h in hitsV)
            {
                if (h.TryGetComponent(out Cube c) && c != this && !c.isSmashed)
                {
                    c.TakeDamage(999, transform.position);
                }
            }
        }

        private void Explode()
        {
            if (SpinForward.CameraControl.CameraController.Instance != null)
            {
                SpinForward.CameraControl.CameraController.Instance.HeavyShake(2f);
                SpinForward.CameraControl.CameraController.Instance.HitStop(0.07f);
            }
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider c in colliders)
            {
                if (c.TryGetComponent(out Cube neighborCube) && neighborCube != this)
                {
                    // Bomba patladığında etrafındaki küplere yüksek hasar verir
                    neighborCube.TakeDamage(explosionDamage, transform.position);
                }
            }

            // Physical shockwave: shove any loose debris (now dynamic) outward.
            foreach (Collider c in colliders)
            {
                Rigidbody body = c.attachedRigidbody;
                if (body != null && !body.isKinematic)
                    body.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.5f, ForceMode.Impulse);
            }

            // BOMB RISK: if the spinner is caught in the blast, it loses energy and is knocked back.
            if (SpinForward.Player.SpinnerController.Instance != null)
            {
                Vector3 toSpinner = SpinForward.Player.SpinnerController.Instance.transform.position - transform.position;
                toSpinner.y = 0f;
                if (toSpinner.magnitude < explosionRadius)
                {
                    if (LevelManager.Instance != null)
                        LevelManager.Instance.DrainEnergy(bombEnergyPenalty);
                    SpinForward.Player.SpinnerController.Instance.Knockback(toSpinner.normalized, bombKnockback);
                }
            }
        }

        private void Shatter(Vector3 hitFrom)
        {
            isSmashed = true;

            // Efekt (Particle) oluştur
            GameObject psObj = new GameObject("CubeShatterEffect");
            psObj.transform.position = transform.position;
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Ayarlar yapılırken oynatılmasını durdur
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 1f;
            main.startSpeed = 8f;
            main.startSize = 0.4f;
            main.startColor = (rend != null) ? rend.sharedMaterial.color : Color.white;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10, 15) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            ps.Play();
            Destroy(psObj, 2f);

            // Frozen cubes carry no Rigidbody; add one now so the debris can fly.
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            // Collider stays ENABLED so the debris physically bounces off the ground
            // and other chunks (and gets shoved by bomb blasts) instead of phasing through.
            // But it must NOT shove the spinner, or clearing would fight the player.
            if (SpinForward.Player.SpinnerController.Instance != null && TryGetComponent(out Collider myCol))
            {
                Collider spinnerCol = SpinForward.Player.SpinnerController.Instance.GetComponent<Collider>();
                if (spinnerCol != null)
                    Physics.IgnoreCollision(myCol, spinnerCol);
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

            // Frenzy Mekaniği
            if (myType == CubeType.Frenzy && SpinForward.Player.SpinnerController.Instance != null)
            {
                SpinForward.Player.SpinnerController.Instance.ActivateFrenzy(5f); // 5 saniye Frenzy buff
            }

            float force = knockForce;
            Vector3 dir = (transform.position - hitFrom).normalized + Vector3.up * 0.5f;
            rb.AddForce(dir * force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * force, ForceMode.Impulse);

            Smashed?.Invoke(this);
            AnyCubeSmashed?.Invoke(transform.position);
            StartCoroutine(ShrinkAndDie());
        }

        // Quickly shrinks the debris to nothing so a broken cube never lingers in the
        // spinner's path (which read as "phasing through the cube").
        private System.Collections.IEnumerator ShrinkAndDie()
        {
            Vector3 start = transform.localScale;
            float dur = Mathf.Min(debrisLifetime, 0.6f);
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t / dur);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
