using UnityEngine;
using System.Collections;

namespace SpinForward.UI
{
    public class UIAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Animasyonun kaç saniye süreceği")]
        [SerializeField] private float duration = 0.4f;
        
        [Tooltip("Panelin başlangıç küçüklüğü")]
        [SerializeField] private Vector3 startScale = Vector3.zero;
        
        [Tooltip("Panelin orijinal/hedef büyüklüğü")]
        [SerializeField] private Vector3 endScale = Vector3.one;

        private void OnEnable()
        {
            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            transform.localScale = startScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; 
                
                float t = elapsed / duration; // 0 ile 1 arası zaman
                if (t > 1f) t = 1f;
                
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

                transform.localScale = Vector3.LerpUnclamped(startScale, endScale, easedT);
                
                yield return null;
            }

            transform.localScale = endScale;
        }
    }
}
