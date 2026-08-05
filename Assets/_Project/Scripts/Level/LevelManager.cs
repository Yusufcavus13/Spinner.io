using TMPro;
using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// Runs one level at a time as a tiny state machine: Playing -> Won or Lost.
    /// You get a limited time to clear the wall. Clear it in time -> win -> next
    /// (bigger) level. Run out of time -> lose -> retry the same level. Money is
    /// kept across attempts because we never reload the scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private enum State { Playing, Won, Lost }

        [Header("Scene refs")]
        [SerializeField] private CubeWall wall;
        [SerializeField] private Transform spinner;

        [Header("Rules")]
        [Tooltip("Seconds you get to clear the wall.")]
        [SerializeField] private float attemptDuration = 20f;

        [Header("Level size")]
        [SerializeField] private int startColumns = 5;
        [SerializeField] private int startRows = 5;
        [Tooltip("Extra columns AND rows added each level.")]
        [SerializeField] private int growthPerLevel = 1;

        [Header("UI")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text levelLabel;

        private State state;
        private int level = 1;
        private float timeLeft;
        private Vector3 spinnerStart;
        private Rigidbody spinnerBody;

        private void Awake()
        {
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
            if (state != State.Playing)
                return;

            timeLeft -= Time.deltaTime;
            if (timerLabel != null)
                timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();

            if (timeLeft <= 0f)
                Lose();
        }

        private void StartLevel()
        {
            state = State.Playing;
            Time.timeScale = 1f;

            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

            ResetSpinner();

            timeLeft = attemptDuration;
            if (levelLabel != null)
                levelLabel.text = "Level " + level;

            int size = growthPerLevel * (level - 1);
            if (wall != null)
                wall.Build(startColumns + size, startRows + size);
        }

        private void ResetSpinner()
        {
            if (spinner == null)
                return;

            spinner.position = spinnerStart;
            if (spinnerBody != null)
            {
                spinnerBody.linearVelocity = Vector3.zero;
                spinnerBody.angularVelocity = Vector3.zero;
            }
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
        }

        private void Lose()
        {
            state = State.Lost;
            Time.timeScale = 0f;
            if (losePanel != null) losePanel.SetActive(true);
        }

        // ---- Hooked to the panel buttons in the Inspector ----

        public void NextLevel()
        {
            if (state != State.Won)
                return;
            level++;
            StartLevel();
        }

        public void Retry()
        {
            StartLevel();
        }
    }
}
