using System.Collections.Generic;
using SpinForward.Economy;
using SpinForward.Level;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// A code-built SHOP shown before the game starts: pick or buy a spinner skin with
    /// your money, then hit PLAY. Fully self-contained - drop this on ONE GameObject,
    /// no prefabs or wiring. Reopen anytime with the floating SHOP button.
    /// </summary>
    public class UIShop : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }

        [Header("Colors")]
        [SerializeField] private Color panelColor = new Color(0.10f, 0.11f, 0.16f, 0.99f);
        [SerializeField] private Color cardColor = new Color(0.17f, 0.19f, 0.27f, 1f);
        [SerializeField] private Color buyColor = new Color(0.16f, 0.78f, 0.4f);
        [SerializeField] private Color equipColor = new Color(0.2f, 0.55f, 1f);
        [SerializeField] private Color equippedColor = new Color(1f, 0.72f, 0.16f);
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.42f, 0.5f);

        private Sprite rounded;
        private Sprite circle;
        private Sprite[] shapeSprites; // indexed by SpinnerShape
        private GameObject shopRoot;
        private GameObject openButton;
        private TMP_Text moneyLabel;
        private TMP_Text playLabel;
        private readonly List<Card> cards = new List<Card>();

        private class Card
        {
            public int index;
            public Image preview;
            public Image accentDot;
            public TMP_Text nameLabel;
            public TMP_Text bonusLabel;
            public Image statusBg;
            public TMP_Text statusLabel;
        }

        // The floating SHOP button is only useful in the menu - hide it during play.
        private void Update()
        {
            if (openButton == null)
                return;
            bool show = !IsOpen && LevelManager.Instance != null && !LevelManager.Instance.IsPlaying;
            if (openButton.activeSelf != show)
                openButton.SetActive(show);
        }

        private void Start()
        {
            HideLeftoverMarketButton();
            EnsureSkinManager();
            EnsureEventSystem();
            rounded = MakeRoundedSprite(24);
            circle = MakeCircleSprite(96);
            shapeSprites = new[]
            {
                circle,                                  // Disc
                MakeToothedSprite(96, 16, 0.78f, true),  // Saw
                MakeToothedSprite(96, 5, 0.42f, true),   // Star
                MakeToothedSprite(96, 10, 0.72f, false)  // Gear
            };

            Transform canvas = BuildCanvas();
            BuildOpenButton(canvas);
            BuildShop(canvas);

            Show(); // the shop is the pre-game screen
        }

        private void EnsureSkinManager()
        {
            if (SkinManager.Instance == null)
                new GameObject("SkinManager").AddComponent<SkinManager>();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Disables any leftover "MARKET" text/button from an earlier manual shop attempt.
        private void HideLeftoverMarketButton()
        {
            foreach (TMP_Text t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                DisableIfMarket(t.text, t.gameObject);
            foreach (Text t in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                DisableIfMarket(t.text, t.gameObject);
        }

        private static void DisableIfMarket(string text, GameObject go)
        {
            if (string.IsNullOrEmpty(text) || text.Trim().ToUpperInvariant() != "MARKET")
                return;
            Button btn = go.GetComponentInParent<Button>();
            (btn != null ? btn.gameObject : go).SetActive(false);
        }

        private Transform BuildCanvas()
        {
            var go = new GameObject("Shop_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // above the HUD
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        // Small floating button that reopens the shop after earning money.
        private void BuildOpenButton(Transform canvas)
        {
            RectTransform rt = MakeImage(canvas, "ShopOpenButton", equipColor, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(320f, 110f), true);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = rt.GetComponent<Image>();
            btn.onClick.AddListener(Open);
            MakeText(rt, "SHOP", 52f, Color.white, Vector2.zero, new Vector2(320f, 110f), FontStyles.Bold);
            openButton = rt.gameObject;
        }

        private void BuildShop(Transform canvas)
        {
            shopRoot = new GameObject("ShopRoot", typeof(RectTransform));
            var rootRt = (RectTransform)shopRoot.transform;
            rootRt.SetParent(canvas, false);
            Stretch(rootRt);

            // Full-screen dim backdrop that blocks clicks to the game behind.
            var backdrop = MakeImage(rootRt, "Backdrop", new Color(0f, 0f, 0f, 0.75f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);
            Stretch(backdrop);

            // Center panel.
            RectTransform panel = MakeImage(rootRt, "Panel", panelColor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 1660f), false);

            MakeText(panel, "SHOP", 96f, Color.white, new Vector2(0f, 730f), new Vector2(900f, 120f), FontStyles.Bold);
            moneyLabel = MakeText(panel, "$0", 56f, equippedColor, new Vector2(0f, 625f), new Vector2(900f, 80f), FontStyles.Bold);

            // Close (X) button top-right.
            RectTransform closeRt = MakeImage(panel, "Close", new Color(0.8f, 0.25f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(430f, 740f), new Vector2(84f, 84f), true);
            var closeBtn = closeRt.gameObject.AddComponent<Button>();
            closeBtn.targetGraphic = closeRt.GetComponent<Image>();
            closeBtn.onClick.AddListener(Close);
            MakeText(closeRt, "X", 54f, Color.white, Vector2.zero, new Vector2(84f, 84f), FontStyles.Bold);

            // Skin cards (2 columns x 3 rows).
            var skins = SkinManager.Instance.availableSkins;
            for (int i = 0; i < skins.Count && i < 6; i++)
            {
                float x = (i % 2 == 0) ? -235f : 235f;
                float y = 360f - (i / 2) * 400f;
                cards.Add(BuildCard(panel, i, new Vector2(x, y)));
            }

            // PLAY / RESUME button.
            RectTransform playRt = MakeImage(panel, "Play", buyColor, new Vector2(0.5f, 0.5f), new Vector2(0f, -730f), new Vector2(760f, 130f), true);
            var playBtn = playRt.gameObject.AddComponent<Button>();
            playBtn.targetGraphic = playRt.GetComponent<Image>();
            playBtn.onClick.AddListener(OnPlay);
            playLabel = MakeText(playRt, "OYNA", 60f, Color.white, Vector2.zero, new Vector2(760f, 130f), FontStyles.Bold);
        }

        private Card BuildCard(RectTransform panel, int index, Vector2 pos)
        {
            RectTransform card = MakeImage(panel, "Card" + index, cardColor, new Vector2(0.5f, 0.5f), pos, new Vector2(450f, 380f), true);
            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card.GetComponent<Image>();
            int captured = index;
            btn.onClick.AddListener(() => OnCardClicked(captured));

            var c = new Card { index = index };
            c.preview = MakeImage(card, "Preview", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 85f), new Vector2(160f, 160f), false).GetComponent<Image>();
            c.preview.sprite = shapeSprites[(int)SkinManager.Instance.availableSkins[index].shape];
            c.preview.raycastTarget = false; // let clicks reach the card button
            c.accentDot = MakeImage(card, "Accent", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 85f), new Vector2(56f, 56f), false).GetComponent<Image>();
            c.accentDot.sprite = circle;
            c.accentDot.raycastTarget = false;
            c.nameLabel = MakeText(card, "Skin", 40f, Color.white, new Vector2(0f, -25f), new Vector2(420f, 52f), FontStyles.Bold);
            c.bonusLabel = MakeText(card, "", 28f, new Color(0.55f, 1f, 0.7f), new Vector2(0f, -68f), new Vector2(420f, 36f), FontStyles.Bold);

            RectTransform statusRt = MakeImage(card, "Status", buyColor, new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(390f, 84f), false);
            c.statusBg = statusRt.GetComponent<Image>();
            c.statusBg.raycastTarget = false;
            c.statusLabel = MakeText(statusRt, "", 38f, Color.white, Vector2.zero, new Vector2(390f, 84f), FontStyles.Bold);
            return c;
        }

        // ---------- State ----------

        private void Open()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.IsPlaying)
                Time.timeScale = 0f; // pause the game while shopping mid-run
            Show();
        }

        private void Show()
        {
            IsOpen = true;
            shopRoot.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            IsOpen = false;
            shopRoot.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnPlay()
        {
            bool preGame = LevelManager.Instance != null && LevelManager.Instance.IsWaitingToStart;
            Close();
            if (preGame)
                LevelManager.Instance.BeginPlaying();
        }

        private void OnCardClicked(int index)
        {
            SkinManager sm = SkinManager.Instance;
            if (sm.IsSkinUnlocked(index))
            {
                sm.EquipSkin(index);
            }
            else
            {
                SkinData skin = sm.availableSkins[index];
                if (Wallet.Instance != null && Wallet.Instance.TrySpend(skin.cost))
                {
                    sm.UnlockSkin(index);
                    sm.EquipSkin(index);
                }
            }
            Refresh();
        }

        private void Refresh()
        {
            SkinManager sm = SkinManager.Instance;
            int balance = Wallet.Instance != null ? Wallet.Instance.Balance : 0;
            if (moneyLabel != null)
                moneyLabel.text = "$" + balance;

            if (playLabel != null && LevelManager.Instance != null)
                playLabel.text = LevelManager.Instance.IsWaitingToStart ? "OYNA" : "DEVAM";

            foreach (Card c in cards)
            {
                SkinData skin = sm.availableSkins[c.index];
                c.preview.color = skin.bodyColor;
                c.accentDot.color = skin.accentColor;
                c.nameLabel.text = skin.skinName;
                c.bonusLabel.text = skin.bonusDamage > 0 ? "+" + skin.bonusDamage + " güç" : "";

                if (sm.CurrentSkinIndex == c.index)
                {
                    c.statusBg.color = equippedColor;
                    c.statusLabel.text = "SEÇİLİ";
                }
                else if (sm.IsSkinUnlocked(c.index))
                {
                    c.statusBg.color = equipColor;
                    c.statusLabel.text = "SEÇ";
                }
                else
                {
                    bool canAfford = balance >= skin.cost;
                    c.statusBg.color = canAfford ? buyColor : lockedColor;
                    c.statusLabel.text = "$" + skin.cost;
                }
            }
        }

        // ---------- UI builders ----------

        private RectTransform MakeImage(Transform parent, string goName, Color color, Vector2 anchor, Vector2 pos, Vector2 size, bool rounded9)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            if (rounded9)
            {
                img.sprite = rounded;
                img.type = Image.Type.Sliced;
            }
            return rt;
        }

        private TMP_Text MakeText(Transform parent, string text, float size, Color color, Vector2 pos, Vector2 rectSize, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
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

        private static Sprite MakeCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float r = size * 0.5f - 1f, c = size * 0.5f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d + 0.5f) * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // A toothed disc: sharp teeth for saw/star (pointy), flat teeth for gear. Used as
        // the shop card icon so each spinner's actual shape is visible before you buy.
        private static Sprite MakeToothedSprite(int size, int teeth, float innerFrac, bool pointy)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float c = size * 0.5f;
            float maxR = size * 0.5f - 1f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int hits = 0;
                    for (int sy = 0; sy < 2; sy++)
                        for (int sx = 0; sx < 2; sx++)
                        {
                            float dx = x + (sx + 0.5f) / 2f - c;
                            float dy = y + (sy + 0.5f) / 2f - c;
                            float r = Mathf.Sqrt(dx * dx + dy * dy) / maxR;
                            float tt = Mathf.Atan2(dy, dx) / (Mathf.PI * 2f) * teeth;
                            tt -= Mathf.Floor(tt);
                            float profile = pointy
                                ? Mathf.Lerp(innerFrac, 1f, 1f - Mathf.Abs(tt - 0.5f) * 2f)
                                : ((tt > 0.25f && tt < 0.75f) ? 1f : innerFrac);
                            if (r <= profile) hits++;
                        }
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(hits / 4f * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
