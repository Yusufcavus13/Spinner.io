using UnityEngine;
using SpinForward.Level;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using Unity.Cinemachine; // Cinemachine 3.x kütüphanesi 
#endif

namespace SpinForward.CameraControl
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Cinemachine Refs")]
        [Tooltip("Sahnedeki CinemachineCamera'yı buraya sürükleyin")]
        [SerializeField] private CinemachineCamera vcam;
        
        [Header("Framing Settings")]
        [Tooltip("Standart 5x5 duvar için varsayılan Lens Açısı (FOV)")]
        [SerializeField] private float defaultFOV = 60f;
        [Tooltip("Duvar büyüdükçe kamera FOV'u en fazla kaç olabilir?")]
        [SerializeField] private float maxFOV = 90f;
        [SerializeField] private float framingPadding = 1.2f;

        // Cinemachine sarsıntı motoru
        private CinemachineImpulseSource impulseSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            Cube.AnyCubeSmashed += OnCubeSmashed;
        }

        private void OnDisable()
        {
            Cube.AnyCubeSmashed -= OnCubeSmashed;
        }

        public void FrameWall(int columns, int rows, float spacing)
        {
            if (vcam == null) return;

            float width = columns * spacing;
            float depth = rows * spacing;
            float maxDim = Mathf.Max(width, depth);

            // Duvar 5 birimden büyükse çarpan artar
            float distanceMultiplier = (maxDim / 5f) * framingPadding; 
            if (distanceMultiplier < 1f) distanceMultiplier = 1f;

            // Kamerayı geriye itmek yerine, Cinemachine Lens açısını (FOV) genişleterek zoom-out yapıyoruz
            float newFOV = defaultFOV * distanceMultiplier;
            
            // Cinemachine 3.x'te Lens ayarlarına erişim
            var lens = vcam.Lens;
            lens.FieldOfView = Mathf.Min(newFOV, maxFOV);
            vcam.Lens = lens;
        }

        private void OnCubeSmashed(Vector3 pos)
        {
            // Normal küp kırılmasında hissedilir ama rahatsız etmeyen bir titreşim.
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithVelocity(Random.insideUnitSphere * 0.3f);
            }
        }
        
        public void HeavyShake(float intensity = 1f)
        {
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithVelocity(Random.insideUnitSphere * intensity);
            }
        }

        /// <summary>Briefly freezes time for a punchy "impact" feel (bombs, big hits).</summary>
        public void HitStop(float duration = 0.06f)
        {
            if (Time.timeScale < 0.9f)
                return; // don't stack, and don't fight the shop's pause
            StartCoroutine(HitStopRoutine(duration));
        }

        private System.Collections.IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            // Resume, unless the shop paused the game in the meantime.
            Time.timeScale = SpinForward.UI.UIShop.IsOpen ? 0f : 1f;
        }
    }
}
