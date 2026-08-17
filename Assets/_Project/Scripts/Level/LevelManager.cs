using SpinForward.Core;
using SpinForward.Economy;
using TMPro;
using UnityEngine;

namespace SpinForward.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        public bool IsPlaying => state == State.Playing;
        public bool IsWaitingToStart => state == State.WaitingToStart;

        private enum State { WaitingToStart, Playing, Won, Lost }

        [Header("Scene refs")]
        [SerializeField] private CubeWall wall;
        public CubeWall Wall => wall;
        [SerializeField] private Transform spinner;

        [Header("Levels")]
        [Tooltip("Designed levels, played in order. Past the last one, it repeats the last level.")]
        [SerializeField] private LevelData[] levels;

        [Header("UI")]
        [SerializeField] private GameObject tapToPlayPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text progressLabel;

        [Header("Audio")]
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        [Header("Events (For Camera & UI)")]
        public UnityEngine.Events.UnityEvent onMenuState;
        public UnityEngine.Events.UnityEvent onGameState;

        private const float DefaultDuration = 20f;
        private const int DefaultSize = 5;

        private State state;
        private int level = 1;
        [Tooltip("Energy refunded per cube smashed. Lets an active player sustain and finish a level.")]
        [SerializeField] private float energyPerCube = 0.6f;
        [Tooltip("Cap on refund energy PER SECOND. Keep it below the drain so fast clearing can't out-pace it - energy always trends down.")]
        [SerializeField] private float maxRefundPerSecond = 2f;
        private float refundThisSecond;
        private float refundTimer;

        private float currentEnergy;
        private float maxEnergy;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        private Vector3 spinnerStart;
        private Rigidbody spinnerBody;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (spinner != null)
            {
                // Kullanıcı spinner objesini değiştirip Tag'lemeyi unutursa diye otomatik Tag ataması yap:
                spinner.tag = "Spinner";

                spinnerStart = spinner.position;
                spinnerBody = spinner.GetComponent<Rigidbody>();
            }
        }

        private void Start()
        {
            if (spinner != null)
            {
                spinner.position = spinnerStart;
            }

            // Old top-right "Energy: 100" text is replaced by the vertical energy bar; hide it.
            if (timerLabel != null)
                timerLabel.gameObject.SetActive(false);

            SetupEnvironment();

            if (wall != null)
                wall.Cleared += OnWallCleared;
            Cube.AnyCubeSmashed += OnAnyCubeSmashed;
            if (UpgradeSystem.Instance != null)
                UpgradeSystem.Instance.Energy.Changed += OnEnergyUpgraded;
            StartLevel();
        }

        private void SetupEnvironment()
        {
            // 1. Gökyüzünü güzel bir mavi yap
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.7f, 1f); // Canlı okyanus/gökyüzü mavisi
            }

            // 2. Sahnedeki devasa beyaz zemini bul ve "Kum (Sand)" rengine boya!
            bool groundFound = false;
            MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            foreach (var rend in allRenderers)
            {
                // Y ekseninde 0'a yakın olan, ismi 'Plane' veya 'Ground' olan, veya devasa objeler zeminimizdir.
                if (rend.transform.position.y <= 0.2f && rend.gameObject.name != "OceanWater" && rend.gameObject.name != "WoodenDock")
                {
                    if (rend.gameObject.name.ToLower().Contains("plane") || rend.gameObject.name.ToLower().Contains("ground") || rend.transform.localScale.x > 3f)
                    {
                        rend.material.color = new Color(0.93f, 0.84f, 0.55f); // Sıcak kum sarısı
                        
                        // Zemin çok küçükse küplerin altı boş kalır ve havada uçuyor gibi görünür.
                        // O yüzden zemini devasa bir ada boyutuna (200x200 metre) getirelim:
                        rend.transform.localScale = new Vector3(20f, 1f, 30f);
                        rend.transform.position = new Vector3(0f, 0f, 30f); // İleriye doğru uzat
                        groundFound = true;
                    }
                }
            }
            
            // Eğer sahnede hiç zemin yoksa (kullanıcı silmişse) biz oluşturalım:
            if (!groundFound)
            {
                GameObject sand = GameObject.CreatePrimitive(PrimitiveType.Plane);
                sand.name = "SandGround_Auto";
                sand.transform.position = new Vector3(0f, 0f, 30f);
                sand.transform.localScale = new Vector3(20f, 1f, 30f);
                Renderer sr = sand.GetComponent<Renderer>();
                if (sr != null)
                {
                    Shader urp = Shader.Find("Universal Render Pipeline/Lit");
                    sr.material = new Material(urp != null ? urp : Shader.Find("Standard"));
                    sr.material.color = new Color(0.93f, 0.84f, 0.55f);
                }
            }

            // 3. Ada hissi yaratmak için aşağıya devasa bir su/okyanus zemini ekle
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "OceanWater";
            water.transform.position = new Vector3(0f, -0.8f, 20f); // Kum zeminin hemen altına
            water.transform.localScale = new Vector3(200f, 1f, 200f); // Uçsuz bucaksız
            
            Renderer r = water.GetComponent<Renderer>();
            if (r != null)
            {
                Shader urp = Shader.Find("Universal Render Pipeline/Lit");
                r.material = new Material(urp != null ? urp : Shader.Find("Standard"));
                r.material.color = new Color(0.15f, 0.65f, 0.9f, 0.9f); // Turkuaz okyanus mavisi
                r.material.SetFloat("_Smoothness", 0.9f);
            }

            // Sınır duvarlarını oluştur (Görünmez duvarlar, aşağı düşmeyi engeller)
            CreateInvisibleWall(new Vector3(0f, 2f, -10f), new Vector3(100f, 10f, 2f)); // Arka duvar (genişletildi)
            CreateInvisibleWall(new Vector3(-50f, 2f, 30f), new Vector3(2f, 10f, 100f)); // Sol duvar
            CreateInvisibleWall(new Vector3(50f, 2f, 30f), new Vector3(2f, 10f, 100f)); // Sağ duvar
        }

        private void CreateInvisibleWall(Vector3 pos, Vector3 scale)
        {
            GameObject wall = new GameObject("InvisibleBarrier");
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            BoxCollider col = wall.AddComponent<BoxCollider>();
            // MeshRenderer eklemiyoruz, bu yüzden tamamen görünmez! Sadece düşmeyi engeller.
        }

        private void OnDestroy()
        {
            if (wall != null)
                wall.Cleared -= OnWallCleared;
            Cube.AnyCubeSmashed -= OnAnyCubeSmashed;
            if (UpgradeSystem.Instance != null)
                UpgradeSystem.Instance.Energy.Changed -= OnEnergyUpgraded;
        }

        // Buying the Energy upgrade raises max energy - top up current energy by the gained
        // amount so the right-side bar visibly jumps up the moment you press the button.
        private void OnEnergyUpgraded()
        {
            if (UpgradeSystem.Instance == null)
                return;
            float newMax = UpgradeSystem.Instance.Energy.Value;
            float added = newMax - maxEnergy;
            maxEnergy = newMax;
            if (added > 0f)
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + added);
        }

        // Smashing cubes refunds energy, so actively clearing keeps you alive.
        private void OnAnyCubeSmashed(Vector3 pos)
        {
            if (state != State.Playing)
                return;

            // Refund is capped PER SECOND, so no matter how many cubes you smash the
            // refund can't out-pace the drain - energy always trends down, but clearing
            // buys you time. Dawdling drains at the full rate.
            if (refundThisSecond >= maxRefundPerSecond)
                return;
            float refund = Mathf.Min(energyPerCube, maxRefundPerSecond - refundThisSecond);
            refundThisSecond += refund;
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + refund);
        }

        private void Update()
        {
            if (state == State.WaitingToStart)
            {
                // While the shop is open, ignore taps so buying doesn't start the game.
                if (!SpinForward.UI.UIShop.IsOpen &&
                    (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
                {
                    BeginPlaying();
                }
                return;
            }

            if (state != State.Playing)
                return;

            // Enerji Tüketimi (Hareket ederken daha hızlı tükenebilir ama şimdilik sabit)
            // Faster drain again so there is real pressure: the per-cube refund only
            // sustains you if you keep clearing efficiently (dawdling = you run out).
            float energyDrainRate = 2.5f;
            if (spinnerBody != null && spinnerBody.linearVelocity.magnitude > 0.5f)
            {
                energyDrainRate = 4f; // moving costs more
            }

            currentEnergy -= energyDrainRate * Time.deltaTime;

            // Refill the per-second refund cap window.
            refundTimer -= Time.deltaTime;
            if (refundTimer <= 0f)
            {
                refundTimer = 1f;
                refundThisSecond = 0f;
            }
            
            if (timerLabel != null)
            {
                // UI'ı Enerji Barı gibi güncelliyoruz, ileride gerçek Image fill amount kullanılabilir
                timerLabel.text = "Energy: " + Mathf.CeilToInt(Mathf.Max(0f, currentEnergy)).ToString();
            }

            if (currentEnergy <= 0f)
                Lose(); // Enerji bittiğinde tur biter
                
            // Update Progress
            if (progressLabel != null && wall != null && wall.TotalCubes > 0)
            {
                int destroyed = wall.TotalCubes - wall.Remaining;
                float pct = (float)destroyed / wall.TotalCubes;
                progressLabel.text = $"% {Mathf.FloorToInt(pct * 100f)}";
            }
        }

        // Called by the tap-to-play input and by the Shop's PLAY button.
        public void BeginPlaying()
        {
            if (state != State.WaitingToStart)
                return;
            state = State.Playing;
            if (tapToPlayPanel != null) tapToPlayPanel.SetActive(false);
            onGameState?.Invoke();
        }

        private void StartLevel(bool autoStart = false)
        {
            state = autoStart ? State.Playing : State.WaitingToStart;
            Time.timeScale = 1f;

            if (tapToPlayPanel != null) tapToPlayPanel.SetActive(!autoStart);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
            
            if (!autoStart)
                onMenuState?.Invoke();
            else
                onGameState?.Invoke();

            MoneyOrb.ClearAll(); 
            ResetSpinner();

            LevelData data = GetLevelData();
            
            // Maksimum Enerjiyi UpgradeSystem'den al
            if (UpgradeSystem.Instance != null)
                maxEnergy = UpgradeSystem.Instance.Energy.Value;
            else
                maxEnergy = 100f; // Fallback
                
            currentEnergy = maxEnergy;

            if (timerLabel != null)
                timerLabel.text = "Energy: " + Mathf.CeilToInt(Mathf.Max(0f, currentEnergy)).ToString();

            if (levelLabel != null)
                levelLabel.text = "Level " + level;

            if (wall != null)
            {
                int cols = data != null ? data.columns : DefaultSize;
                int rows = data != null ? data.rows : DefaultSize;
                wall.Build(cols, rows, data);
            }
        }

        private LevelData GetLevelData()
        {
            if (levels == null || levels.Length == 0)
                return null;

            int index = Mathf.Clamp(level - 1, 0, levels.Length - 1);
            return levels[index];
        }

        private void ResetSpinner()
        {
            if (spinner == null)
                return;

            // Teleporting a dynamic Rigidbody needs more than transform.position:
            // zero the motion, move the physics body itself, then force the physics
            // engine to accept the new position immediately (before the wall builds).
            if (spinnerBody != null)
            {
                spinnerBody.linearVelocity = Vector3.zero;
                spinnerBody.angularVelocity = Vector3.zero;
                spinnerBody.position = spinnerStart;
            }

            spinner.position = spinnerStart;
            Physics.SyncTransforms();
        }

        private void OnWallCleared()
        {
            if (state == State.Playing)
                Win();
        }

        private void Win()
        {
            state = State.Won;
            Time.timeScale = 0f; // freeze the action behind the panel
            if (winPanel != null) winPanel.SetActive(true);
            if (Sfx.Instance != null) Sfx.Instance.Play(winClip, 0.7f, 0f);
        }

        private void Lose()
        {
            state = State.Lost;
            Time.timeScale = 0f;
            if (losePanel != null) losePanel.SetActive(true);
            if (Sfx.Instance != null) Sfx.Instance.Play(loseClip, 0.7f, 0f);
        }


        public void NextLevel()
        {
            if (state != State.Won)
                return;
            level++;
            StartLevel(true); // Next Level'a geçince direkt başla
        }

        public void Retry()
        {
            StartLevel(true); // Tekrar denendiğinde de direkt başla
        }
    }
}
