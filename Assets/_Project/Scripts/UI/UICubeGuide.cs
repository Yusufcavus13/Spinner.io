using SpinForward.Level;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// A code-built "how to play" guide that explains the special cube types (color swatch +
    /// name + what it does). Auto-shows once on first launch, and reopens from a "?" button in
    /// the menu. Self-contained - drop on ONE GameObject, no prefabs.
    /// </summary>
    public class UICubeGuide : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }

        private struct Entry
        {
            public Color color;
            public string name;
            public string desc;
            public Entry(Color c, string n, string d) { color = c; name = n; desc = d; }
        }

        private static readonly Entry[] Entries =
        {
            new Entry(Color.red,                          "Bomba",          "Patlar, çevreyi temizler — yakınındaysan enerji kaybedersin!"),
            new Entry(new Color(0.35f, 0.85f, 0.1f),      "Tuzak (yeşil)",  "Parlayan zehirli yeşil. Vurunca enerjini emer — kaçın!"),
            new Entry(new Color(1f, 0.55f, 0.05f),        "Zaman Bombası",  "Yaklaşınca sayar; 6 sn'de kırmazsan patlar!"),
            new Entry(Color.cyan,                         "Laser (avantaj)","Parlayan camgöbeği. Satır + sütunu komple temizler!"),
            new Entry(new Color(0.32f, 0.35f, 0.42f),     "Çelik",          "Sert (çok vuruş). Kırmak zorunlu değil, yolu tıkar."),
            new Entry(new Color(0.5f, 0.8f, 1f),          "Buz",            "Vurunca spinner bir süre yavaşlar."),
            new Entry(Color.grey,                         "Kalkan",         "2 vuruş ister: önce kalkanı, sonra çekirdeği kır."),
        };

        [SerializeField] private Color panelColor = new Color(0.10f, 0.11f, 0.16f, 0.99f);
        [SerializeField] private Color accent = new Color(0.2f, 0.85f, 1f);

        [SerializeField] private Sprite rounded;
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject openButton;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            EnsureEventSystem();

            if (root != null)
            {
                // UI sahnede zaten var — sadece butonları bağla
                if (rounded == null) rounded = MakeRoundedSprite(20);
                if (openButton != null)
                {
                    var btn = openButton.GetComponent<Button>();
                    if (btn != null) btn.onClick.AddListener(Show);
                }
                if (closeButton != null) closeButton.onClick.AddListener(Close);
            }
            else
            {
                // UI yok — koddan oluştur (orijinal davranış)
                rounded = MakeRoundedSprite(20);
                Transform canvas = BuildCanvas();
                BuildOpenButton(canvas);
                BuildGuide(canvas);
            }

            root.SetActive(false);

            if (PlayerPrefs.GetInt("SeenGuide", 0) == 0) // show once on first launch
            {
                PlayerPrefs.SetInt("SeenGuide", 1);
                PlayerPrefs.Save();
                Show();
            }
        }

        private void Update()
        {
            if (openButton == null)
                return;
            bool show = !IsOpen && LevelManager.Instance != null && !LevelManager.Instance.IsPlaying;
            if (openButton.activeSelf != show)
                openButton.SetActive(show);
        }

        private void Show()
        {
            IsOpen = true;
            root.SetActive(true);
            var gr = GetComponentInParent<GraphicRaycaster>();
            if (gr == null) gr = GetComponent<GraphicRaycaster>();
            if (gr != null) gr.enabled = true;
        }

        private void Close()
        {
            IsOpen = false;
            root.SetActive(false);
            var gr = GetComponentInParent<GraphicRaycaster>();
            if (gr == null) gr = GetComponent<GraphicRaycaster>();
            if (gr != null) gr.enabled = false;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Transform BuildCanvas()
        {
            var go = new GameObject("Guide_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1100; // above the shop
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private void BuildOpenButton(Transform canvas)
        {
            // Sol üst köşeye (Level yazısının biraz altına) sabitleyelim
            RectTransform rt = MakeImage(canvas, "GuideButton", accent, new Vector2(100f, -250f), new Vector2(110f, 110f));
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            
            Round(rt);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = rt.GetComponent<Image>();
            btn.onClick.AddListener(Show);
            MakeText(rt, "?", 70f, Color.white, Vector2.zero, new Vector2(110f, 110f), TextAlignmentOptions.Center);
            openButton = rt.gameObject;
        }

        private void BuildGuide(Transform canvas)
        {
            root = new GameObject("GuideRoot", typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(canvas, false);
            Stretch(rootRt);

            RectTransform backdrop = MakeImage(rootRt, "Backdrop", new Color(0f, 0f, 0f, 0.78f), Vector2.zero, Vector2.zero);
            Stretch(backdrop);

            RectTransform panel = MakeImage(rootRt, "Panel", panelColor, Vector2.zero, new Vector2(940f, 1560f));
            Round(panel);

            MakeText(panel, "NASIL OYNANIR", 74f, accent, new Vector2(0f, 680f), new Vector2(880f, 110f), TextAlignmentOptions.Center, FontStyles.Bold);

            for (int i = 0; i < Entries.Length; i++)
                BuildRow(panel, Entries[i], 540f - i * 168f);

            RectTransform ok = MakeImage(panel, "OK", accent, new Vector2(0f, -690f), new Vector2(620f, 130f));
            Round(ok);
            var okBtn = ok.gameObject.AddComponent<Button>();
            okBtn.targetGraphic = ok.GetComponent<Image>();
            okBtn.onClick.AddListener(Close);
            MakeText(ok, "ANLADIM", 54f, Color.white, Vector2.zero, new Vector2(620f, 130f), TextAlignmentOptions.Center, FontStyles.Bold);
        }

        private void BuildRow(RectTransform panel, Entry e, float y)
        {
            RectTransform swatch = MakeImage(panel, "Swatch", e.color, new Vector2(-360f, y), new Vector2(120f, 120f));
            Round(swatch);
            swatch.GetComponent<Image>().raycastTarget = false;

            MakeText(panel, e.name, 42f, Color.white, new Vector2(70f, y + 34f), new Vector2(620f, 52f), TextAlignmentOptions.Left, FontStyles.Bold);
            MakeText(panel, e.desc, 30f, new Color(0.8f, 0.82f, 0.88f), new Vector2(70f, y - 30f), new Vector2(640f, 70f), TextAlignmentOptions.Left);
        }

        // ---------- helpers ----------

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

        private TMP_Text MakeText(Transform parent, string text, float size, Color color, Vector2 pos, Vector2 rectSize, TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
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
            tmp.alignment = align;
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
