using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
                // Ignore presses that land on an actual BUTTON (upgrade / shop buttons),
                // so tapping them doesn't also steer the spinner. A plain decorative panel
                // or full-screen backdrop must NOT block steering (that froze the spinner).
                if (IsPointerOverButton(screenPos))
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

        private static readonly List<RaycastResult> uiRaycastHits = new List<RaycastResult>();

        // True only if the press is over an interactive control (Button/Toggle/etc.), NOT a
        // plain image or backdrop. Using IsPointerOverGameObject() here was wrong: any
        // raycast-catching panel over the play area would block the joystick entirely.
        private static bool IsPointerOverButton(Vector2 screenPos)
        {
            if (EventSystem.current == null)
                return false;

            var data = new PointerEventData(EventSystem.current) { position = screenPos };
            uiRaycastHits.Clear();
            EventSystem.current.RaycastAll(data, uiRaycastHits);

            for (int i = 0; i < uiRaycastHits.Count; i++)
            {
                GameObject go = uiRaycastHits[i].gameObject;
                if (go != null && go.GetComponentInParent<Selectable>() != null)
                    return true; // a real button/toggle - don't steer
            }
            return false;
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
