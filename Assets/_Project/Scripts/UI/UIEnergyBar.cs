using SpinForward.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.UI
{
    public class UIEnergyBar : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("The Image component that represents the fill bar. Set Image Type to Filled.")]
        [SerializeField] private Image fillImage;
        [Tooltip("Optional text label to show energy percentage or exact values.")]
        [SerializeField] private TMP_Text valueLabel;
        
        [Header("Settings")]
        [Tooltip("Smooth transition speed for the fill bar.")]
        [SerializeField] private float smoothSpeed = 10f;

        private void Update()
        {
            if (LevelManager.Instance == null || fillImage == null) return;

            float currentEnergy = LevelManager.Instance.CurrentEnergy;
            float maxEnergy = LevelManager.Instance.MaxEnergy;

            // Calculate target fill amount
            float targetFill = maxEnergy > 0 ? (currentEnergy / maxEnergy) : 0f;

            // Smoothly interpolate the fill amount
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

            // Update text label if assigned
            if (valueLabel != null)
            {
                valueLabel.text = Mathf.CeilToInt(currentEnergy).ToString();
            }
        }
    }
}
