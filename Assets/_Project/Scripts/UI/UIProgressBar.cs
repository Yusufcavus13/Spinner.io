using SpinForward.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.UI
{
    public class UIProgressBar : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("The Image component representing the level progress. Set Image Type to Filled.")]
        [SerializeField] private Image fillImage;
        [Tooltip("Optional text label to show percentage (e.g. '50%').")]
        [SerializeField] private TMP_Text percentageLabel;
        
        [Header("Settings")]
        [SerializeField] private float smoothSpeed = 10f;

        private void Update()
        {
            if (LevelManager.Instance == null || LevelManager.Instance.Wall == null || fillImage == null) return;

            int total = LevelManager.Instance.Wall.TotalCubes;
            int remaining = LevelManager.Instance.Wall.Remaining;

            if (total == 0) return;

            int broken = total - remaining;
            float targetFill = (float)broken / total;

            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

            if (percentageLabel != null)
            {
                int percentage = Mathf.RoundToInt(targetFill * 100f);
                percentageLabel.text = percentage + "%";
            }
        }
    }
}
