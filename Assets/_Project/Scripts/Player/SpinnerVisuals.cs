using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.Player
{
    public class SpinnerVisuals : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The visual model of the spinner (should be a child of this object)")]
        [SerializeField] private Transform visualModel;
        
        [Header("Upgrade Scaling")]
        [Tooltip("How much the power upgrade increases the spinner's size. Default is 0.05 (5% bigger per level)")]
        [SerializeField] private float scalePerPowerLevel = 0.05f;
        
        [Tooltip("The maximum size the spinner can reach")]
        [SerializeField] private float maxScaleMultiplier = 2.0f;
        
        [Header("Trail Settings")]
        [Tooltip("The trail renderer component")]
        [SerializeField] private TrailRenderer trail;
        
        [Tooltip("Color of the trail when power is high")]
        [SerializeField] private Color highPowerColor = Color.red;
        
        [Tooltip("Color of the trail when rotate speed is high")]
        [SerializeField] private Color highSpeedColor = Color.cyan;
        
        private Vector3 initialScale;
        private Color initialTrailColor;
        
        private void Start()
        {
            initialScale = transform.localScale; // Artık çarkın kendisini (Collider dahil) baz alıyoruz
            
            if (trail != null)
            {
                initialTrailColor = trail.startColor;
            }
            
            InvokeRepeating(nameof(UpdateVisuals), 0.5f, 0.5f);
        }
        
        private void UpdateVisuals()
        {
            if (UpgradeSystem.Instance == null) return;
            
            Upgrade powerUpgrade = UpgradeSystem.Instance.Power;
            Upgrade rotateUpgrade = UpgradeSystem.Instance.Rotate;
            
            UpdateScale(powerUpgrade);
            UpdateTrail(powerUpgrade, rotateUpgrade);
        }
        
        private void UpdateScale(Upgrade powerUpgrade)
        {
            float scaleMultiplier = 1f + (powerUpgrade.Level * scalePerPowerLevel);
            scaleMultiplier = Mathf.Min(scaleMultiplier, maxScaleMultiplier);
            
            Vector3 targetScale = initialScale * scaleMultiplier;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 5f);
        }
        
        private void UpdateTrail(Upgrade powerUpgrade, Upgrade rotateUpgrade)
        {
            if (trail == null) return;
            
            float speedPercent = rotateUpgrade.Level / 20f; // Assuming 20 is max or high level
            trail.time = Mathf.Lerp(0.5f, 1.5f, speedPercent);
            
            Color targetColor = initialTrailColor;
            if (powerUpgrade.Level > 5)
            {
                targetColor = Color.Lerp(initialTrailColor, highPowerColor, powerUpgrade.Level / 15f);
            }
            else if (rotateUpgrade.Level > 5)
            {
                targetColor = Color.Lerp(initialTrailColor, highSpeedColor, rotateUpgrade.Level / 15f);
            }
            
            trail.startColor = Color.Lerp(trail.startColor, targetColor, Time.deltaTime * 2f);
            trail.endColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }
    }
}
