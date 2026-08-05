using UnityEngine;

namespace SpinForward.Core
{
    /// <summary>
    /// One shared audio player for short sound effects. Anyone calls
    /// <c>Sfx.Instance.Play(clip)</c>. Uses PlayOneShot so many sounds can overlap,
    /// and jitters the pitch a touch so repeated smashes don't sound identical.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Sfx : MonoBehaviour
    {
        public static Sfx Instance { get; private set; }

        [SerializeField] private AudioSource source;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (source == null)
                source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void Play(AudioClip clip, float volume = 1f, float pitchJitter = 0.08f)
        {
            if (clip == null || source == null)
                return;

            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip, volume);
        }
    }
}
