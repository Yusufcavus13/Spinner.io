using UnityEngine;
using TMPro;
using System.Collections;

namespace SpinForward.UI
{
    public class FloatingText : MonoBehaviour
    {
        [Tooltip("Yazının havaya uçma hızı")]
        [SerializeField] private float floatSpeed = 2f;
        [Tooltip("Yazının ekranda kalma süresi")]
        [SerializeField] private float lifetime = 1f;
        
        private TMP_Text textComponent;
        private FloatingTextManager myManager;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        public void Play(int amount, FloatingTextManager manager)
        {
            myManager = manager;
            if (textComponent != null)
            {
                textComponent.text = amount.ToString();
                // Rengini ve saydamlığını sıfırla (Çünkü önceki kullanımda solmuştu)
                Color c = textComponent.color;
                c.a = 1f;
                textComponent.color = c;
            }
            StartCoroutine(FloatAndFade());
        }

        private IEnumerator FloatAndFade()
        {
            float elapsed = 0f;
            Color startColor = textComponent != null ? textComponent.color : Color.white;
            
            // Kamera açısına göre döndür (Billboarding) ki yazı hep kameraya düz baksın
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            while (elapsed < lifetime)
            {
                // Yukarı doğru hareket et
                transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
                
                // Yavaşça saydamlaş (Fade Out)
                if (textComponent != null)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
                    textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Yok etmek (Destroy) yerine havuza geri gönderiyoruz
            if (myManager != null)
            {
                myManager.ReturnToPool(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
