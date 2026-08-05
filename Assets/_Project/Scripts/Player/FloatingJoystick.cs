using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SpinForward.Player
{
    /// <summary>
    /// On-screen "floating" joystick. The player touches anywhere; the stick
    /// spawns at the finger, and dragging away from that point gives a direction
    /// vector (-1..1 on each axis). Reads the pointer directly through the new
    /// Input System, so no EventSystem / GraphicRaycaster wiring is needed.
    /// </summary>
    public class FloatingJoystick : MonoBehaviour
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [Tooltip("How far (in canvas units) the handle can travel from center.")]
        [SerializeField] private float handleRange = 110f;

        /// <summary>Normalized input. Magnitude 0 = idle, 1 = fully pushed.</summary>
        public Vector2 Direction { get; private set; }

        private RectTransform canvasRect;
        private Vector2 startLocal;
        private bool active;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            Hide();
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || canvasRect == null)
                return;

            bool pressed = pointer.press.isPressed;
            Vector2 screenPos = pointer.position.ReadValue();

            if (pressed && !active)
            {
                // Ignore presses that land on a UI element (e.g. upgrade buttons),
                // so tapping the shop doesn't also steer the spinner.
                if (IsPointerOverUI())
                    return;

                active = true;
                Show();
                startLocal = ScreenToCanvas(screenPos);
                if (background != null) background.anchoredPosition = startLocal;
                if (handle != null) handle.anchoredPosition = Vector2.zero;
                Direction = Vector2.zero;
            }
            else if (pressed && active)
            {
                Vector2 nowLocal = ScreenToCanvas(screenPos);
                Vector2 delta = nowLocal - startLocal;
                Vector2 clamped = Vector2.ClampMagnitude(delta, handleRange);
                if (handle != null) handle.anchoredPosition = clamped;
                Direction = clamped / handleRange;
            }
            else if (!pressed && active)
            {
                active = false;
                Direction = Vector2.zero;
                Hide();
            }
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private Vector2 ScreenToCanvas(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out Vector2 local);
            return local;
        }

        private void Show()
        {
            if (background != null) background.gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (background != null) background.gameObject.SetActive(false);
            Direction = Vector2.zero;
        }
    }
}
