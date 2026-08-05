using UnityEngine;

namespace SpinForward.Core
{
    /// <summary>
    /// Smoothly follows the spinner from a diagonal top-down angle, matching the
    /// GDD's "camera from above, character in the middle" framing.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -9f);
        [SerializeField] private float followSmooth = 8f;
        [SerializeField] private float lookHeight = 0.5f;

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
