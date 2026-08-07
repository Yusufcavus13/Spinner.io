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

        public static SpinnerController Instance { get; private set; }
        
        private Rigidbody rb;
        
        // Debuff ve Buff mekanikleri
        private float iceDebuffTimer = 0f;
        private Renderer visualRenderer;
        private Color originalVisualColor;
        private bool isIceVisualActive = false;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody>();
            if (visual != null)
            {
                visualRenderer = visual.GetComponentInChildren<Renderer>();
                if (visualRenderer != null)
                {
                    originalVisualColor = visualRenderer.material.color;
                }
            }
        }

        private void Update()
        {
            if (iceDebuffTimer > 0f)
            {
                iceDebuffTimer -= Time.deltaTime;
                if (!isIceVisualActive && visualRenderer != null)
                {
                    visualRenderer.material.color = new Color(0.5f, 0.8f, 1f);
                    isIceVisualActive = true;
                }
            }
            else
            {
                if (isIceVisualActive && visualRenderer != null)
                {
                    visualRenderer.material.color = originalVisualColor;
                    isIceVisualActive = false;
                }
            }
            
            float spin = 0f;
            if (UpgradeSystem.Instance != null)
                spin = UpgradeSystem.Instance.Rotate.Value;
                
            // Fever Mode hızı 2'ye katlar!
            if (SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive)
            {
                spin *= 2f;
            }
            
            // Buz yavaşlatması (%75 yavaşlar)
            if (iceDebuffTimer > 0f)
            {
                spin *= 0.25f;
            }

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

            float currentMoveSpeed = moveSpeed;
            if (SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive)
            {
                // Obje büyüdüğü için normal hız yavaş hissettirir. Hızı 2.5 katına çıkarıyoruz!
                currentMoveSpeed *= 2.5f; 
            }
            
            // Buz yavaşlatması (%75 yavaşlar)
            if (iceDebuffTimer > 0f)
            {
                currentMoveSpeed *= 0.25f;
            }

            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;
            Vector3 dir = new Vector3(input.x, 0f, input.y);
            Vector3 targetVel = Vector3.ClampMagnitude(dir, 1f) * currentMoveSpeed;

            Vector3 vel = rb.linearVelocity;
            Vector3 horizNow = new Vector3(vel.x, 0f, vel.z);
            Vector3 horizNext = Vector3.MoveTowards(horizNow, targetVel, acceleration * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(horizNext.x, vel.y, horizNext.z);
        }
        
        public void ApplyIceDebuff(float duration)
        {
            iceDebuffTimer = duration;
        }
        
        public void SpawnClones(int count, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                // Klon için bir sphere oluştur
                GameObject clone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                clone.name = "SpinnerClone";
                clone.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                clone.transform.localScale = transform.localScale * 0.5f; // Yarı boyutunda
                clone.tag = gameObject.tag; // Cube scriptinin tanıması için aynı tag (Örn: "Spinner")

                // Fizik özellikleri (Zıplaması için material ve rigidbody)
                Rigidbody cloneRb = clone.AddComponent<Rigidbody>();
                cloneRb.mass = 2f;
                cloneRb.linearVelocity = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                
                // Zıplaklık için PhysicMaterial
                SphereCollider col = clone.GetComponent<SphereCollider>();
                PhysicsMaterial bMat = new PhysicsMaterial("BouncyClone");
                bMat.bounciness = 1f;
                bMat.dynamicFriction = 0f;
                bMat.staticFriction = 0f;
                bMat.bounceCombine = PhysicsMaterialCombine.Maximum;
                col.material = bMat;
                
                // Görsel olarak kırmızı/turuncu arası bir renk (Ateş efekti gibi)
                Renderer r = clone.GetComponent<Renderer>();
                if(r != null)
                {
                    r.material.color = new Color(1f, 0.5f, 0f);
                }

                Destroy(clone, lifetime); // Belirli bir süre sonra klon yok olsun
            }
        }
    }
}
