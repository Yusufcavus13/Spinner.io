using SpinForward.Core;
using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// Turns the global "a cube was smashed" signal into juice: a smash sound and
    /// a particle burst at the hit point. Listens to the event, so it never needs
    /// a reference to any cube.
    /// </summary>
    public class SmashEffects : MonoBehaviour
    {
        [SerializeField] private AudioClip smashClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.5f;
        [Tooltip("Optional particle system spawned at each smash. Leave empty for sound only.")]
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private float burstLifetime = 2f;

        private void OnEnable()
        {
            Cube.AnyCubeSmashed += OnSmash;
        }

        private void OnDisable()
        {
            Cube.AnyCubeSmashed -= OnSmash;
        }

        private void OnSmash(Vector3 position)
        {
            if (Sfx.Instance != null)
                Sfx.Instance.Play(smashClip, volume);

            if (burstPrefab != null)
            {
                ParticleSystem burst = Instantiate(burstPrefab, position, Quaternion.identity);
                Destroy(burst.gameObject, burstLifetime);
            }
        }
    }
}
