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

        [Header("Audio")]
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        private const float DefaultDuration = 20f;
        private const int DefaultSize = 5;

        private State state;
        private int level = 1;
        private float timeLeft;
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
                // Ekrana dokunulduğunda oyunu başlat
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    state = State.Playing;
                    if (tapToPlayPanel != null) tapToPlayPanel.SetActive(false);
                }
                return;
            }

            if (state != State.Playing)
                return;

            timeLeft -= Time.deltaTime;
            if (timerLabel != null)
                timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();

            if (timeLeft <= 0f)
                Lose();
        }

        private void StartLevel(bool autoStart = false)
        {
            state = autoStart ? State.Playing : State.WaitingToStart;
            Time.timeScale = 1f;

            if (tapToPlayPanel != null) tapToPlayPanel.SetActive(!autoStart);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

            MoneyOrb.ClearAll(); // drop any coins still flying from the previous attempt
            ResetSpinner();

            LevelData data = GetLevelData();
            timeLeft = data != null ? data.attemptDuration : DefaultDuration;

            // Süreyi daha oyun başlamadan ekranda göster
            if (timerLabel != null)
                timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();

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
