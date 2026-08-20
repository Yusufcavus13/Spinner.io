using SpinForward.Level;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// Updates the HUD (Energy Bar and Progress Bar) based on LevelManager state.
    /// UI elements are expected to be set up in the Unity Editor and assigned here.
    /// </summary>
    public class AutoHUD : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The Image component for the Energy Bar (Image Type must be Filled).")]
        [SerializeField] private Image energyBarFill;
        [Tooltip("The Image component for the Progress Bar (Image Type must be Filled).")]
        [SerializeField] private Image progressBarFill;
        
        [Header("Colors")]
        [SerializeField] private Color energyColor = new Color(1f, 0.78f, 0.18f);
        [SerializeField] private Color lowEnergyColor = new Color(0.96f, 0.26f, 0.2f);

        private void Update()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            if (energyBarFill != null)
            {
                float max = lm.MaxEnergy;
                float t = max > 0f ? Mathf.Clamp01(lm.CurrentEnergy / max) : 0f;
                energyBarFill.fillAmount = t;
                
                // Enerji azaldığında rengi kırmızıya dönük yap
                energyBarFill.color = Color.Lerp(lowEnergyColor, energyColor, Mathf.Clamp01(t / 0.35f));
            }

            if (progressBarFill != null && lm.Wall != null)
            {
                int total = lm.Wall.TotalCubes;
                float t = total > 0 ? Mathf.Clamp01((float)(total - lm.Wall.Remaining) / total) : 0f;
                progressBarFill.fillAmount = t;
            }
        }
    }
}
