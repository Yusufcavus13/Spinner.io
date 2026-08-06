using UnityEngine;
using UnityEngine.Pool;

namespace SpinForward.UI
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [Header("Ayarlar")]
        [Tooltip("Ekrana çıkacak hasar yazısı Prefab'i")]
        [SerializeField] private FloatingText textPrefab;

        private ObjectPool<FloatingText> pool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Performans için Object Pool (Obje Havuzu) oluşturuyoruz
            pool = new ObjectPool<FloatingText>(
                createFunc: () => Instantiate(textPrefab, transform),
                actionOnGet: (obj) => obj.gameObject.SetActive(true),
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj.gameObject),
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
        }

        public void ShowDamage(int amount, Vector3 position)
        {
            if (textPrefab == null) return;

            Vector3 spawnPos = position + Vector3.up * 0.5f;
            
            // Havuzdan hazır bir obje çekiyoruz (Instantiate yerine)
            FloatingText textObj = pool.Get();
            textObj.transform.position = spawnPos;
            textObj.Play(amount, this);
        }

        // Yazı kaybolunca kendini yok etmek yerine havuza geri döner
        public void ReturnToPool(FloatingText textObj)
        {
            pool.Release(textObj);
        }
    }
}
