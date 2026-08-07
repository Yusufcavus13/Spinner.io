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

        private enum State { WaitingToStart, Playing, Won, Lost }

        [Header("Scene refs")]
        [SerializeField] private CubeWall wall;
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
        private float currentEnergy;
        private float maxEnergy;
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
                spinnerStart = spinner.position;
                spinnerBody = spinner.GetComponent<Rigidbody>();
            }
        }

        private void Start()
        {
            if (wall != null)
                wall.Cleared += OnWallCleared;
            StartLevel();
        }

        private void OnDestroy()
        {
            if (wall != null)
                wall.Cleared -= OnWallCleared;
        }

        private void Update()
        {
            if (state == State.WaitingToStart)
            {
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    state = State.Playing;
                    if (tapToPlayPanel != null) tapToPlayPanel.SetActive(false);
                    onGameState?.Invoke();
                }
                return;
            }

            if (state != State.Playing)
                return;

            // Enerji Tüketimi (Hareket ederken daha hızlı tükenebilir ama şimdilik sabit)
            float energyDrainRate = 5f; // Saniyede 5 birim
            if (spinnerBody != null && spinnerBody.linearVelocity.magnitude > 0.5f)
            {
                energyDrainRate = 10f; // Hareket ederken daha hızlı tükenir
            }

            currentEnergy -= energyDrainRate * Time.deltaTime;
            
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
