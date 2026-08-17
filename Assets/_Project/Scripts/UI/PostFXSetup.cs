using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpinForward.UI
{
    /// <summary>
    /// Adds a premium look at runtime with URP post-processing - Bloom (glow), a soft
    /// vignette and a touch of color grading - with no manual Volume/asset setup.
    /// Drop this on ONE GameObject. Emissive spinner parts and effects will glow.
    /// </summary>
    public class PostFXSetup : MonoBehaviour
    {
        [Header("Bloom")]
        [SerializeField] private float bloomIntensity = 0.9f;
        [SerializeField] private float bloomThreshold = 0.9f;

        [Header("Grade")]
        [SerializeField] private float vignette = 0.28f;
        [SerializeField] private float saturation = 12f;
        [SerializeField] private float contrast = 8f;

        private void Start()
        {
            // Post-processing has to be enabled on the camera to show at all.
            if (Camera.main != null)
            {
                UniversalAdditionalCameraData data = Camera.main.GetUniversalAdditionalCameraData();
                if (data != null)
                    data.renderPostProcessing = true;
            }

            var go = new GameObject("PostFX_Volume");
            go.transform.SetParent(transform, false);
            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(bloomIntensity);
            bloom.threshold.Override(bloomThreshold);
            bloom.scatter.Override(0.6f);

            Vignette vig = profile.Add<Vignette>(true);
            vig.intensity.Override(vignette);
            vig.smoothness.Override(0.5f);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(saturation);
            color.contrast.Override(contrast);
        }
    }
}
