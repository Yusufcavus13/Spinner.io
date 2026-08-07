using UnityEngine;
using SpinForward.Player;

namespace SpinForward.Level
{
    public class VortexHazard : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The base force at which the vortex pulls the spinner.")]
        [SerializeField] private float pullForce = 100f;
        [Tooltip("The maximum distance the vortex can pull from.")]
        [SerializeField] private float pullRange = 12f;

        private void Start()
        {
            // Görsel olarak bir Sphere (Küre) oluştur ve bu objeye ekle
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 1.5f; // Normal küplerden biraz daha büyük
            
            // SphereCollider'daki Trigger'ı kapat (veya tamamen sil) ki fiziksel bir engel olsun
            // Aslında içinden geçilmemesi için kalabilir.
            
            // Siyah materyal ata
            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material blackMat = new Material(Shader.Find("Standard"));
                blackMat.color = Color.black;
                rend.material = blackMat;
            }
        }

        private void FixedUpdate()
        {
            // Kendi etrafında sürekli dön (Girdap efekti)
            transform.Rotate(Vector3.up * 360f * Time.fixedDeltaTime, Space.World);
            transform.Rotate(Vector3.right * 180f * Time.fixedDeltaTime, Space.Self);
            
            if (SpinnerController.Instance != null)
            {
                Rigidbody spinnerRb = SpinnerController.Instance.GetComponent<Rigidbody>();
                if (spinnerRb != null)
                {
                    Vector3 toSpinner = spinnerRb.position - transform.position;
                    float distance = toSpinner.magnitude;
                    
                    if (distance < pullRange)
                    {
                        // Uzaklık azaldıkça çekim gücü eksponansiyel olarak artar (Merkeze yaklaştıkça kaçmak zorlaşır)
                        float normalizedDistance = distance / pullRange;
                        float pullMultiplier = 1f - normalizedDistance; 
                        pullMultiplier = pullMultiplier * pullMultiplier; // Karesini alarak eksponansiyel yapıyoruz
                        
                        Vector3 pullDir = -toSpinner.normalized;
                        spinnerRb.AddForce(pullDir * pullForce * pullMultiplier, ForceMode.Force);
                    }
                }
            }
        }
    }
}
