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
        
        [Header("Skin Evolution (Afilli Modeller)")]
        [Tooltip("Farklı seviyelerdeki Spinner modelleri (Hiyerarşide alt obje olmalılar)")]
        [SerializeField] private GameObject[] skinModels;
        [Tooltip("Modellerin açılması için gereken Power seviyeleri (Örn: 1, 10, 20)")]
        [SerializeField] private int[] skinUnlockLevels;

        private Vector3 initialScale;
        private Color initialTrailColor;
        private int currentSkinIndex = -1;
        
        private void Start()
        {
            initialScale = transform.localScale; 
            
            if (trail != null)
            {
                initialTrailColor = trail.startColor;
            }
        }
        
        private void Update()
        {
            float powerLevel = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.Power.Level : 0f;
            float rotateLevel = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.Rotate.Level : 0f;
            
            UpdateScale(powerLevel);
            UpdateTrail(powerLevel, rotateLevel);
            UpdateSkin((int)powerLevel);
        }
        
        private void UpdateSkin(int powerLevel)
        {
            if (skinModels == null || skinModels.Length == 0 || skinUnlockLevels == null || skinUnlockLevels.Length != skinModels.Length) return;

            int targetSkinIndex = 0;
            for (int i = 0; i < skinUnlockLevels.Length; i++)
            {
                if (powerLevel >= skinUnlockLevels[i])
                {
                    targetSkinIndex = i;
                }
            }

            if (currentSkinIndex != targetSkinIndex)
            {
                currentSkinIndex = targetSkinIndex;
                for (int i = 0; i < skinModels.Length; i++)
                {
                    if (skinModels[i] != null)
                    {
                        skinModels[i].SetActive(i == currentSkinIndex);
                    }
                }
            }
        }
        
        private void UpdateScale(float powerLevel)
        {
            float scaleMultiplier = 1f + (powerLevel * scalePerPowerLevel);
            scaleMultiplier = Mathf.Min(scaleMultiplier, maxScaleMultiplier);
            
            // Fever Mode devasa büyüme bonusu!
            if (SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive)
            {
                scaleMultiplier *= 1.5f; // Fever modunda %50 daha büyük!
            }
            
            // Frenzy mode hafif büyüme
            if (SpinnerController.Instance != null && SpinnerController.Instance.IsFrenzyActive)
            {
                scaleMultiplier *= 1.25f;
            }
            
            Vector3 targetScale = initialScale * scaleMultiplier;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 5f);
        }
        
        private void UpdateTrail(float powerLevel, float rotateLevel)
        {
            if (trail == null) return;
            
            float speedPercent = rotateLevel / 20f; 
            trail.time = Mathf.Lerp(0.5f, 1.5f, speedPercent);
            
            Color targetColor = initialTrailColor;
            
            // Fever Mode Alev Efekti!
            bool isFever = SpinForward.Core.ComboManager.Instance != null && SpinForward.Core.ComboManager.Instance.IsFeverActive;
            
            if (isFever)
            {
                targetColor = new Color(1f, 0.2f, 0f); // Ateşli turuncu/kırmızı
            }
            else if (powerLevel > 5)
            {
                targetColor = Color.Lerp(initialTrailColor, highPowerColor, powerLevel / 15f);
            }
            else if (rotateLevel > 5)
            {
                targetColor = Color.Lerp(initialTrailColor, highSpeedColor, rotateLevel / 15f);
            }
            
            trail.startColor = Color.Lerp(trail.startColor, targetColor, Time.deltaTime * (isFever ? 10f : 2f));
            trail.endColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }
    }
}
