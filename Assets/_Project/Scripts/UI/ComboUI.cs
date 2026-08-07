using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpinForward.Core;

namespace SpinForward.UI
{
    public class ComboUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider comboSlider;
        [SerializeField] private TextMeshProUGUI feverText;
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 0.8f, 0f); // Yellow
        [SerializeField] private Color feverColor = new Color(1f, 0.2f, 0f); // Red/Orange

        private RectTransform feverTextRect;
        private Vector3 targetTextScale = Vector3.one;

        private void Start()
        {
            if (feverText != null)
            {
                feverTextRect = feverText.GetComponent<RectTransform>();
                feverText.gameObject.SetActive(false);
            }

            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboUpdated += UpdateComboBar;
                ComboManager.Instance.OnFeverModeChanged += HandleFeverMode;
            }
        }

        private void OnDestroy()
        {
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboUpdated -= UpdateComboBar;
                ComboManager.Instance.OnFeverModeChanged -= HandleFeverMode;
            }
        }

        private void Update()
        {
            // Juicy Fever Text scaling
            if (feverText != null && feverText.gameObject.activeSelf)
            {
                // Pulsate the text
                float pulse = 1f + Mathf.Sin(Time.time * 20f) * 0.1f;
                feverTextRect.localScale = Vector3.Lerp(feverTextRect.localScale, targetTextScale * pulse, Time.deltaTime * 10f);
            }
        }

        private void UpdateComboBar(float percentage)
        {
            if (comboSlider != null)
            {
                comboSlider.value = percentage;
            }
        }

        private void HandleFeverMode(bool isFever)
        {
            if (feverText != null)
            {
                feverText.gameObject.SetActive(isFever);
                if (isFever)
                {
                    feverTextRect.localScale = Vector3.one * 0.2f; // Pop in effect
                    targetTextScale = Vector3.one;
                }
            }

            if (fillImage != null)
            {
                fillImage.color = isFever ? feverColor : normalColor;
            }
        }
    }
}
