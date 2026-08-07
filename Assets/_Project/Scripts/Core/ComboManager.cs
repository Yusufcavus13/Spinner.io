using UnityEngine;
using SpinForward.Level;
using System;

namespace SpinForward.Core
{
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        public Action<float> OnComboUpdated; // Returns combo percentage (0 to 1)
        public Action<bool> OnFeverModeChanged;

        [Header("Combo Settings")]
        [SerializeField] private int maxCombo = 5; // Çok daha az küp!
        [SerializeField] private float comboDecayRate = 0.5f; // Neredeyse hiç düşmeyecek!
        [SerializeField] private float feverDuration = 5f;

        private float currentCombo = 0f;
        public bool IsFeverActive { get; private set; }
        private float feverTimer = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to any cube smashed event
            Cube.AnyCubeSmashed += HandleCubeSmashed;
        }

        private void OnDestroy()
        {
            Cube.AnyCubeSmashed -= HandleCubeSmashed;
        }

        private void Update()
        {
            if (IsFeverActive)
            {
                feverTimer -= Time.deltaTime;
                OnComboUpdated?.Invoke(feverTimer / feverDuration);

                if (feverTimer <= 0)
                {
                    EndFeverMode();
                }
            }
            else if (currentCombo > 0)
            {
                // Bölüm geçişlerinde (yeni küpler gelirken) kombomuzun düşmesini DONDURUYORUZ!
                bool isPlaying = SpinForward.Level.LevelManager.Instance != null && SpinForward.Level.LevelManager.Instance.IsPlaying;
                
                if (isPlaying)
                {
                    currentCombo -= comboDecayRate * Time.deltaTime;
                    currentCombo = Mathf.Max(0, currentCombo);
                    OnComboUpdated?.Invoke(currentCombo / maxCombo);
                }
            }
        }

        private void HandleCubeSmashed(Vector3 position)
        {
            if (IsFeverActive) return;

            currentCombo += 1f;
            
            if (currentCombo >= maxCombo)
            {
                StartFeverMode();
            }
            else
            {
                OnComboUpdated?.Invoke(currentCombo / maxCombo);
            }
        }

        private void StartFeverMode()
        {
            IsFeverActive = true;
            feverTimer = feverDuration;
            currentCombo = maxCombo;
            OnFeverModeChanged?.Invoke(true);
            
            // Optionally shake the camera here or play a sound
        }

        private void EndFeverMode()
        {
            IsFeverActive = false;
            currentCombo = 0;
            OnFeverModeChanged?.Invoke(false);
            OnComboUpdated?.Invoke(0);
        }
    }
}
