using UnityEngine;
using UnityEngine.EventSystems;

namespace SpinForward.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [Tooltip("How big the button gets when hovered (1.0 = normal).")]
        [SerializeField] private float hoverScale = 1.05f;
        
        [Tooltip("How small the button squashes when clicked (1.0 = normal).")]
        [SerializeField] private float clickScale = 0.9f;
        
        [Tooltip("How fast the button animates.")]
        [SerializeField] private float animationSpeed = 20f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector3 targetScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            targetScale = originalScale;
        }

        private void Update()
        {
            // Smoothly lerp towards the target scale for that juicy, springy feel
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = originalScale * clickScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // If the mouse is still over the button, go back to hover scale, else original
            if (eventData.pointerCurrentRaycast.gameObject == gameObject)
            {
                targetScale = originalScale * hoverScale;
            }
            else
            {
                targetScale = originalScale;
            }
        }
    }
}
