using System.Collections;
using SpinForward.Level;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// Code-built win / lose result screens with clean Next Level and Retry buttons, plus
    /// a sliding "wipe" transition when moving to the next level or retrying. Self-contained
    /// (no prefabs/wiring) - drop on ONE GameObject. Replaces the old plain win/lose panels;
    /// clear LevelManager's Win Panel / Lose Panel fields so they don't double up.
    /// </summary>
    public class UIResultScreen : MonoBehaviour
    {
        [SerializeField] private Color winColor = new Color(0.16f, 0.78f, 0.4f);
        [SerializeField] private Color loseColor = new Color(0.95f, 0.38f, 0.28f);
        [SerializeField] private Color panelColor = new Color(0.10f, 0.11f, 0.16f, 0.98f);
        [SerializeField] private Color wipeColor = new Color(0.08f, 0.5f, 0.55f);

        private const float RefW = 1080f;

        private Sprite rounded;
        private GameObject winOverlay;
        private GameObject loseOverlay;
        private RectTransform wipeRt;
        private TMP_Text wipeLabel;
        private bool transitioning;
        private int lastState = -1; // 0 none, 1 won, 2 lost

        private void Start()
        {
            EnsureEventSystem();
            rounded = MakeRoundedSprite(24);

            Transform canvas = BuildCanvas();
            winOverlay = BuildOverlay(canvas, "BÖLÜM TAMAM!", "SONRAKİ BÖLÜM", winColor, OnNext);
            loseOverlay = BuildOverlay(canvas, "OLMADI!", "TEKRAR DENE", loseColor, OnRetry);
            BuildWipe(canvas);

            winOverlay.SetActive(false);
            loseOverlay.SetActive(false);
        }

        private void Update()
        {
            if (transitioning || LevelManager.Instance == null)
                return;

            int s = LevelManager.Instance.IsWon ? 1 : LevelManager.Instance.IsLost ? 2 : 0;
            if (s == lastState)
                return;
            lastState = s;
            winOverlay.SetActive(s == 1);
            loseOverlay.SetActive(s == 2);
        }

        private void OnNext()
        {
            if (LevelManager.Instance != null && !transitioning)
                StartCoroutine(Transition(LevelManager.Instance.NextLevel));
        }

        private void OnRetry()
        {
            if (LevelManager.Instance != null && !transitioning)
                StartCoroutine(Transition(LevelManager.Instance.Retry));
        }

        private IEnumerator Transition(System.Action action)
        {
            transitioning = true;
            winOverlay.SetActive(false);
            loseOverlay.SetActive(false);
            lastState = 0;

            wipeRt.gameObject.SetActive(true);
            wipeRt.anchoredPosition = new Vector2(RefW * 1.3f, 0f);
            if (wipeLabel != null)
                wipeLabel.text = "";

            // Sweep in from the right to fully cover the screen.
            yield return Slide(new Vector2(RefW * 1.3f, 0f), Vector2.zero, 0.22f);

            action(); // rebuild the level (hidden behind the wipe) + resume time
            if (wipeLabel != null && LevelManager.Instance != null)
                wipeLabel.text = "LEVEL " + LevelManager.Instance.CurrentLevel;

            yield return WaitUnscaled(0.25f);

            // Sweep out to the left, revealing the new level.
            yield return Slide(Vector2.zero, new Vector2(-RefW * 1.3f, 0f), 0.3f);

            wipeRt.gameObject.SetActive(false);
            transitioning = false;
        }

        private IEnumerator Slide(Vector2 from, Vector2 to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                wipeRt.anchoredPosition = Vector2.LerpUnclamped(from, to, Mathf.SmoothStep(0f, 1f, t / dur));
                yield return null;
            }
            wipeRt.anchoredPosition = to;
        }

        private static IEnumerator WaitUnscaled(float t)
        {
            float e = 0f;
            while (e < t) { e += Time.unscaledDeltaTime; yield return null; }
        }

        // ---------- builders ----------

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Transform BuildCanvas()
        {
            var go = new GameObject("Result_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private GameObject BuildOverlay(Transform canvas, string title, string buttonText, Color accent, System.Action onClick)
        {
            var overlay = new GameObject("Overlay", typeof(RectTransform));
            var oRt = (RectTransform)overlay.transform;
            oRt.SetParent(canvas, false);
            Stretch(oRt);

            RectTransform backdrop = MakeImage(oRt, "Backdrop", new Color(0f, 0f, 0f, 0.72f), Vector2.zero, new Vector2(880f, 640f));
            Stretch(backdrop);

            RectTransform panel = MakeImage(oRt, "Panel", panelColor, Vector2.zero, new Vector2(880f, 640f));
            Round(panel);

            // Accent strip behind the title.
            RectTransform strip = MakeImage(panel, "Strip", accent, new Vector2(0f, 200f), new Vector2(880f, 150f));
            Round(strip);
            strip.GetComponent<Image>().raycastTarget = false;
            MakeText(strip, title, 78f, Color.white, Vector2.zero, new Vector2(840f, 150f), FontStyles.Bold);

            // Big action button.
            RectTransform btn = MakeImage(panel, "Button", accent, new Vector2(0f, -170f), new Vector2(640f, 155f));
            Round(btn);
            var button = btn.gameObject.AddComponent<Button>();
            button.targetGraphic = btn.GetComponent<Image>();
            button.onClick.AddListener(() => onClick());
            MakeText(btn, buttonText, 52f, Color.white, Vector2.zero, new Vector2(640f, 155f), FontStyles.Bold);

            return overlay;
        }

        private void BuildWipe(Transform canvas)
        {
            RectTransform rt = MakeImage(canvas, "Wipe", wipeColor, new Vector2(RefW * 1.3f, 0f), new Vector2(1500f, 2600f));
            wipeLabel = MakeText(rt, "", 96f, Color.white, Vector2.zero, new Vector2(1000f, 240f), FontStyles.Bold);
            wipeRt = rt;
            rt.gameObject.SetActive(false);
        }

        private RectTransform MakeImage(Transform parent, string goName, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private void Round(RectTransform rt)
        {
            var img = rt.GetComponent<Image>();
            img.sprite = rounded;
            img.type = Image.Type.Sliced;
        }

        private TMP_Text MakeText(Transform parent, string text, float size, Color color, Vector2 pos, Vector2 rectSize, FontStyles style)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite MakeRoundedSprite(int radius)
        {
            int size = radius * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float ny = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float d = Mathf.Sqrt((x + 0.5f - nx) * (x + 0.5f - nx) + (y + 0.5f - ny) * (y + 0.5f - ny));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }
    }
}
