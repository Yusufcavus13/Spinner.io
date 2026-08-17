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
        
        [Header("Lean (tilt toward movement)")]
        [Tooltip("Max degrees the spinner tilts into its travel direction. ~8 reads as a gentle bank; big values look like wobbling in place.")]
        [SerializeField] private float maxLeanAngle = 8f;
        [SerializeField] private float leanSmooth = 6f;

        // Debuff ve Buff mekanikleri
        private float iceDebuffTimer = 0f;
        private Renderer visualRenderer;
        private Color originalVisualColor;
        private bool isIceVisualActive = false;

        // Spin + lean state
        private float spinAngle;
        private Quaternion currentLean = Quaternion.identity;
        private Quaternion visualBaseRot = Quaternion.identity;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody>();
            visualRenderer = GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
                originalVisualColor = visualRenderer.material.color;

            // Kullanıcı yanlışlıkla ayarları bozmuşsa düzelt:
            if (rb != null)
            {
                rb.isKinematic = false;
                // Freeze rotation AND vertical position: the spinner stays at a fixed
                // height so it can never climb up onto the cubes - it always hits them
                // side-on and plows through instead of riding over the top.
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                rb.useGravity = false; // Y is locked, so no gravity needed
                // Stop the fast spinner from tunnelling INTO the static cubes.
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother visual motion
            }

            // Spinner'ın Y pozisyonunu tam küplerin merkezine (0.5f) sabitle ki üstlerinden uçmasın.
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

            // Çarpışmalarda geri sekmeyi ve takılmayı önlemek için pürüzsüz PhysicsMaterial
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                PhysicsMaterial smoothMat = new PhysicsMaterial("SpinnerSmooth");
                smoothMat.bounciness = 0f;
                smoothMat.dynamicFriction = 0f;
                smoothMat.staticFriction = 0f;
                smoothMat.frictionCombine = PhysicsMaterialCombine.Minimum;
                smoothMat.bounceCombine = PhysicsMaterialCombine.Minimum;
                col.material = smoothMat;
            }

            // Eğer "Afilli model" için bir Animator eklendiyse ve Root Motion açıksa, fizik hareketini kitler!
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
            }

            if (visual != null)
            {
                visualBaseRot = visual.localRotation; // preserve the model's resting orientation
                visualRenderer = visual.GetComponentInChildren<Renderer>();
                if (visualRenderer != null)
                {
                    originalVisualColor = visualRenderer.material.color;
                }
            }
        }

        private void Start() => MatchColliderToVisual();

        /// <summary>Sizes the physics collider to the current visual so cubes break where the
        /// model touches them. Y is frozen, so this is a pure horizontal hit volume CENTERED
        /// on the spinner - no vertical offset (the old offset shoved the spinner backwards).</summary>
        public void MatchColliderToVisual()
        {
            if (visual == null || !TryGetComponent(out SphereCollider sc))
                return;

            Renderer[] rends = visual.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0)
                return;

            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);

            float worldRadius = Mathf.Max(b.extents.x, b.extents.z);
            float lossy = Mathf.Max(0.0001f, transform.lossyScale.x);
            sc.center = Vector3.zero;
            sc.radius = Mathf.Clamp(worldRadius / lossy, 0.4f, 1.1f);
        }

        // Frenzy ve Direnç Değişkenleri
        public bool IsFrenzyActive => frenzyTimer > 0f;
        private float frenzyTimer = 0f;
        private Vector3 grindPushBack = Vector3.zero;

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
            else if (frenzyTimer > 0f)
            {
                frenzyTimer -= Time.deltaTime;
                if (visualRenderer != null)
                {
                    // Frenzy boyunca altın/kırmızı arası titreşen renk
                    float t = Mathf.PingPong(Time.time * 10f, 1f);
                    visualRenderer.material.color = Color.Lerp(Color.yellow, Color.red, t);
                }
            }
            else
            {
                if ((isIceVisualActive || visualRenderer?.material.color != originalVisualColor) && visualRenderer != null)
                {
                    visualRenderer.material.color = originalVisualColor;
                    isIceVisualActive = false;
                }
            }
            
            float spin = spinSpeed; // fallback so it always spins
            if (UpgradeSystem.Instance != null)
                spin = UpgradeSystem.Instance.Rotate.Value;
                
            // Fever veya Frenzy Mode hızı katlar!
            if ((SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive) || IsFrenzyActive)
            {
                spin *= 2f;
            }
            
            // Buz yavaşlatması (%75 yavaşlar)
            if (iceDebuffTimer > 0f)
            {
                spin *= 0.25f;
            }

            // Accumulate the spin angle, then lean the whole spinner toward its
            // travel direction (banking into movement) for a bit of life.
            spinAngle += spin * Time.deltaTime;

            Quaternion targetLean = Quaternion.identity;
            Vector3 flatVel = rb.linearVelocity;
            flatVel.y = 0f;
            if (flatVel.sqrMagnitude > 0.02f)
            {
                Vector3 tiltAxis = Vector3.Cross(Vector3.up, flatVel.normalized);
                float amount = Mathf.Clamp01(flatVel.magnitude / Mathf.Max(0.01f, moveSpeed * 0.6f)) * maxLeanAngle;
                targetLean = Quaternion.AngleAxis(amount, tiltAxis);
            }
            currentLean = Quaternion.Slerp(currentLean, targetLean, Time.deltaTime * leanSmooth);

            if (visual != null)
                visual.localRotation = currentLean * Quaternion.AngleAxis(spinAngle, Vector3.up) * visualBaseRot;
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
                currentMoveSpeed *= 2.5f; 
            }
            
            if (IsFrenzyActive)
            {
                currentMoveSpeed *= 1.5f; // Frenzy hızı %50 artırır
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
            
            // Fiziksel Direnç (Push-back) eklentisi
            horizNext += grindPushBack;
            grindPushBack = Vector3.Lerp(grindPushBack, Vector3.zero, Time.fixedDeltaTime * 10f); // Hızlıca sönümlenir

            rb.linearVelocity = new Vector3(horizNext.x, vel.y, horizNext.z);
        }
        
        public void ApplyGrindResistance(Vector3 direction, float resistance)
        {
            if (IsFrenzyActive) return; // Frenzy modunda direnç hissetmez, yarıp geçer!
            grindPushBack += direction * resistance * 0.2f;
            if (grindPushBack.magnitude > resistance) grindPushBack = Vector3.ClampMagnitude(grindPushBack, resistance);
        }

        public void ActivateFrenzy(float duration)
        {
            frenzyTimer = duration;
        }
        
        public void ApplyIceDebuff(float duration)
        {
            iceDebuffTimer = duration;
        }
        
        public void SpawnClones(int count, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                // Klon için ana fiziksel obje
                GameObject clone = new GameObject("SpinnerClone");
                clone.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                clone.transform.localScale = transform.localScale * 0.6f; // Yarı boyutunda
                clone.tag = gameObject.tag; // Cube scriptinin tanıması için aynı tag (Örn: "Spinner")

                // Fizik özellikleri 
                Rigidbody cloneRb = clone.AddComponent<Rigidbody>();
                cloneRb.mass = 2f;
                cloneRb.linearVelocity = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                cloneRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

                // Zıplaklık için PhysicsMaterial ve Collider
                SphereCollider col = clone.AddComponent<SphereCollider>();
                PhysicsMaterial bMat = new PhysicsMaterial("BouncyClone");
                bMat.bounciness = 1f;
                bMat.dynamicFriction = 0f;
                bMat.staticFriction = 0f;
                bMat.bounceCombine = PhysicsMaterialCombine.Maximum;
                col.material = bMat;
                
                // Oyuncu ile klon çarpışmasın (fiziksel fırlama hatasını önler)
                Collider myCol = GetComponent<Collider>();
                if (myCol != null)
                {
                    Physics.IgnoreCollision(myCol, col);
                }

                // Oyuncunun kendi görselini kopyalayıp klona ekle
                if (visual != null)
                {
                    GameObject cloneVisual = Instantiate(visual.gameObject, clone.transform);
                    cloneVisual.transform.localPosition = Vector3.zero;
                    
                    // Klonun SpinnerVisuals componenti varsa silelim (kendi kendini scale etmesin)
                    SpinnerVisuals sv = cloneVisual.GetComponent<SpinnerVisuals>();
                    if (sv != null) Destroy(sv);

                    // Klonların Trail rengini ateşli kırmızı/turuncu yapalım ki ayırt edilsinler
                    TrailRenderer tr = cloneVisual.GetComponentInChildren<TrailRenderer>();
                    if (tr != null)
                    {
                        tr.startColor = new Color(1f, 0.4f, 0f);
                    }
                }

                Destroy(clone, lifetime); // Belirli bir süre sonra klon yok olsun
            }
        }
    }
}
