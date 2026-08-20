using System.Collections.Generic;
using SpinForward.Economy;
using SpinForward.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor tool: Sahnede Shop ve Guide Canvas'larını oluşturur,
/// UIShop/UICubeGuide bileşenlerini ekler ve TÜM referansları bağlar.
/// Tek tıklama ile çalışır. Oyun kodlarına dokunmaz.
/// </summary>
public static class BuildSceneUI
{
    // ─────────────────────────────────────────────
    //  Renk Paleti (UIShop ile aynı değerler)
    // ─────────────────────────────────────────────
    static readonly Color PanelColor   = new Color(0.10f, 0.11f, 0.16f, 0.99f);
    static readonly Color CardColor    = new Color(0.17f, 0.19f, 0.27f, 1f);
    static readonly Color BuyColor     = new Color(0.16f, 0.78f, 0.4f);
    static readonly Color EquipColor   = new Color(0.2f, 0.55f, 1f);
    static readonly Color EquippedColor= new Color(1f, 0.72f, 0.16f);
    static readonly Color Accent       = new Color(0.2f, 0.85f, 1f);
    static readonly Color GuidePanelColor = new Color(0.10f, 0.11f, 0.16f, 0.99f);

    // Küp rehberi verileri
    struct GuideEntry { public Color color; public string name, desc; }
    static readonly GuideEntry[] GuideEntries = {
        new GuideEntry { color = Color.red,                          name = "Bomba",          desc = "Patlar, çevreyi temizler — yakınındaysan enerji kaybedersin!" },
        new GuideEntry { color = new Color(0.35f, 0.85f, 0.1f),     name = "Tuzak (yeşil)",  desc = "Parlayan zehirli yeşil. Vurunca enerjini emer — kaçın!" },
        new GuideEntry { color = new Color(1f, 0.55f, 0.05f),       name = "Zaman Bombası",  desc = "Yaklaşınca sayar; 6 sn'de kırmazsan patlar!" },
        new GuideEntry { color = Color.cyan,                         name = "Laser (avantaj)", desc = "Parlayan camgöbeği. Satır + sütunu komple temizler!" },
        new GuideEntry { color = new Color(0.32f, 0.35f, 0.42f),    name = "Çelik",          desc = "Sert (çok vuruş). Kırmak zorunlu değil, yolu tıkar." },
        new GuideEntry { color = new Color(0.5f, 0.8f, 1f),         name = "Buz",            desc = "Vurunca spinner bir süre yavaşlar." },
        new GuideEntry { color = Color.grey,                         name = "Kalkan",         desc = "2 vuruş ister: önce kalkanı, sonra çekirdeği kır." },
    };

    // Skin verileri (SkinManager.BuildDefaultSkins ile birebir aynı)
    struct SkinInfo { public string name; public int cost, bonus; public int shapeIdx; public Color body, accent; }
    static readonly SkinInfo[] Skins = {
        new SkinInfo { name="Klasik",  cost=0,    bonus=0,  shapeIdx=0, body=C(0.11f,0.42f,0.95f), accent=C(0.2f,0.95f,1f) },
        new SkinInfo { name="Testere", cost=0,    bonus=2,  shapeIdx=1, body=C(0.85f,0.86f,0.9f),  accent=C(1f,0.35f,0.1f) },
        new SkinInfo { name="Yıldız", cost=400,  bonus=3,  shapeIdx=2, body=C(0.10f,0.72f,0.38f), accent=C(0.6f,1f,0.55f) },
        new SkinInfo { name="Dişli",   cost=900,  bonus=5,  shapeIdx=3, body=C(1f,0.72f,0.12f),    accent=C(1f,0.96f,0.65f) },
        new SkinInfo { name="Ametist", cost=2000, bonus=8,  shapeIdx=2, body=C(0.52f,0.20f,0.85f), accent=C(0.85f,0.6f,1f) },
        new SkinInfo { name="Gölge",  cost=5000, bonus=14, shapeIdx=1, body=C(0.12f,0.12f,0.16f), accent=C(0.95f,0.1f,0.2f) },
    };
    static Color C(float r, float g, float b) => new Color(r, g, b);

    // ─────────────────────────────────────────────
    //  SPRITE YÜKLEME
    // ─────────────────────────────────────────────
    static Sprite LoadSprite(string name)
    {
        string path = "Assets/_Project/Art/UI/" + name + ".png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ─────────────────────────────────────────────
    //  ANA MENÜ KOMUTU
    // ─────────────────────────────────────────────
    [MenuItem("Tools/Build Scene UI (Safe)")]
    public static void Run()
    {
        // Eski kalıntıları temizle
        DestroyIfExists("Shop_Canvas");
        DestroyIfExists("Guide_Canvas");

        Sprite rounded24 = LoadSprite("Rounded24");
        Sprite rounded20 = LoadSprite("Rounded20");
        Sprite circle    = LoadSprite("Circle96");

        if (rounded24 == null || circle == null || rounded20 == null)
        {
            Debug.LogError("Sprite'lar bulunamadı! Önce Assets/_Project/Art/UI/ klasöründe Circle96.png, Rounded24.png, Rounded20.png olmalı.");
            return;
        }

        BuildShopCanvas(rounded24, circle);
        BuildGuideCanvas(rounded20);

        // Sahneyi kaydet
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("✅ Shop ve Guide Canvas başarıyla oluşturuldu ve bağlandı!");
    }

    // ═════════════════════════════════════════════
    //  SHOP CANVAS
    // ═════════════════════════════════════════════
    static void BuildShopCanvas(Sprite rounded, Sprite circle)
    {
        // Canvas
        var canvasGO = new GameObject("Shop_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        // UIShop bileşenini ekle
        var shop = canvasGO.AddComponent<UIShop>();

        // Open Button
        RectTransform openBtnRT = MakeImg(canvasGO.transform, "ShopOpenButton", EquipColor, rounded,
            new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(320f, 110f));
        var openBtn = openBtnRT.gameObject.AddComponent<Button>();
        openBtn.targetGraphic = openBtnRT.GetComponent<Image>();
        MakeTxt(openBtnRT, "ShopOpenLabel", "SHOP", 52f, Color.white, Vector2.zero, new Vector2(320f, 110f), FontStyles.Bold);

        // ShopRoot
        var shopRootGO = new GameObject("ShopRoot", typeof(RectTransform));
        var shopRootRT = (RectTransform)shopRootGO.transform;
        shopRootRT.SetParent(canvasGO.transform, false);
        Stretch(shopRootRT);

        // Backdrop
        var backdrop = MakeImg(shopRootRT, "Backdrop", new Color(0, 0, 0, 0.75f), null,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Stretch(backdrop);

        // Panel
        var panel = MakeImg(shopRootRT, "Panel", PanelColor, null,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 1660f));

        // Title
        MakeTxt(panel, "Title", "SHOP", 96f, Color.white, new Vector2(0, 730f), new Vector2(900f, 120f), FontStyles.Bold);

        // Money Label
        var moneyLabel = MakeTxt(panel, "MoneyLabel", "$0", 56f, EquippedColor,
            new Vector2(0, 625f), new Vector2(900f, 80f), FontStyles.Bold);

        // Close Button
        var closeRT = MakeImg(panel, "Close", new Color(0.8f, 0.25f, 0.25f), rounded,
            new Vector2(0.5f, 0.5f), new Vector2(430f, 740f), new Vector2(84f, 84f));
        var closeBtn = closeRT.gameObject.AddComponent<Button>();
        closeBtn.targetGraphic = closeRT.GetComponent<Image>();
        MakeTxt(closeRT, "CloseLabel", "X", 54f, Color.white, Vector2.zero, new Vector2(84f, 84f), FontStyles.Bold);

        // Cards
        var cardList = new List<UIShop.Card>();
        for (int i = 0; i < 6; i++)
        {
            float x = (i % 2 == 0) ? -235f : 235f;
            float y = 360f - (i / 2) * 400f;
            var card = BuildShopCard(panel, i, new Vector2(x, y), rounded, circle);
            cardList.Add(card);
        }

        // Play Button
        var playRT = MakeImg(panel, "Play", BuyColor, rounded,
            new Vector2(0.5f, 0.5f), new Vector2(0, -730f), new Vector2(760f, 130f));
        var playBtn = playRT.gameObject.AddComponent<Button>();
        playBtn.targetGraphic = playRT.GetComponent<Image>();
        var playLabel = MakeTxt(playRT, "PlayLabel", "OYNA", 60f, Color.white, Vector2.zero, new Vector2(760f, 130f), FontStyles.Bold);

        // ── SerializedObject ile tüm referansları bağla ──
        var so = new SerializedObject(shop);
        so.FindProperty("shopRoot").objectReferenceValue = shopRootGO;
        so.FindProperty("openButton").objectReferenceValue = openBtnRT.gameObject;
        so.FindProperty("moneyLabel").objectReferenceValue = moneyLabel;
        so.FindProperty("playLabel").objectReferenceValue = playLabel;
        so.FindProperty("playButton").objectReferenceValue = playBtn;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("rounded").objectReferenceValue = rounded;
        so.FindProperty("circle").objectReferenceValue = circle;

        // shapeSprites (runtime'da oluşturulacak, burada null bırakabiliriz)
        // Ama Circle sprite'ı atayabiliriz
        var spritesProp = so.FindProperty("shapeSprites");
        spritesProp.arraySize = 0; // runtime'da doldurulacak

        // Cards
        var cardsProp = so.FindProperty("cards");
        cardsProp.arraySize = cardList.Count;
        for (int i = 0; i < cardList.Count; i++)
        {
            var elem = cardsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("index").intValue = cardList[i].index;
            elem.FindPropertyRelative("preview").objectReferenceValue = cardList[i].preview;
            elem.FindPropertyRelative("accentDot").objectReferenceValue = cardList[i].accentDot;
            elem.FindPropertyRelative("nameLabel").objectReferenceValue = cardList[i].nameLabel;
            elem.FindPropertyRelative("bonusLabel").objectReferenceValue = cardList[i].bonusLabel;
            elem.FindPropertyRelative("statusBg").objectReferenceValue = cardList[i].statusBg;
            elem.FindPropertyRelative("statusLabel").objectReferenceValue = cardList[i].statusLabel;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("Shop Canvas oluşturuldu ve UIShop'a bağlandı.");
    }

    static UIShop.Card BuildShopCard(RectTransform panel, int index, Vector2 pos, Sprite rounded, Sprite circle)
    {
        var cardRT = MakeImg(panel, "Card" + index, CardColor, rounded,
            new Vector2(0.5f, 0.5f), pos, new Vector2(450f, 380f));
        var cardBtn = cardRT.gameObject.AddComponent<Button>();
        cardBtn.targetGraphic = cardRT.GetComponent<Image>();

        var c = new UIShop.Card { index = index };

        // Preview (spinner shape icon)
        var previewRT = MakeImg(cardRT, "Preview", Skins[index].body, null,
            new Vector2(0.5f, 0.5f), new Vector2(0, 85f), new Vector2(160f, 160f));
        previewRT.GetComponent<Image>().raycastTarget = false;
        c.preview = previewRT.GetComponent<Image>();

        // Accent dot
        var accentRT = MakeImg(cardRT, "Accent", Skins[index].accent, circle,
            new Vector2(0.5f, 0.5f), new Vector2(0, 85f), new Vector2(56f, 56f));
        accentRT.GetComponent<Image>().raycastTarget = false;
        c.accentDot = accentRT.GetComponent<Image>();

        // Name
        c.nameLabel = MakeTxt(cardRT, "NameLabel", Skins[index].name, 40f, Color.white,
            new Vector2(0, -25f), new Vector2(420f, 52f), FontStyles.Bold);

        // Bonus
        string bonusText = Skins[index].bonus > 0 ? "+" + Skins[index].bonus + " güç" : "";
        c.bonusLabel = MakeTxt(cardRT, "BonusLabel", bonusText, 28f, new Color(0.55f, 1f, 0.7f),
            new Vector2(0, -68f), new Vector2(420f, 36f), FontStyles.Bold);

        // Status
        var statusRT = MakeImg(cardRT, "Status", BuyColor, null,
            new Vector2(0.5f, 0.5f), new Vector2(0, -140f), new Vector2(390f, 84f));
        statusRT.GetComponent<Image>().raycastTarget = false;
        c.statusBg = statusRT.GetComponent<Image>();
        c.statusLabel = MakeTxt(statusRT, "StatusLabel", "", 38f, Color.white,
            Vector2.zero, new Vector2(390f, 84f), FontStyles.Bold);

        return c;
    }

    // ═════════════════════════════════════════════
    //  GUIDE CANVAS
    // ═════════════════════════════════════════════
    static void BuildGuideCanvas(Sprite rounded)
    {
        var canvasGO = new GameObject("Guide_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1100;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var guide = canvasGO.AddComponent<UICubeGuide>();

        // Open Button (?)
        var openRT = MakeImg(canvasGO.transform, "GuideButton", Accent, rounded,
            new Vector2(0.5f, 0.5f), new Vector2(100f, -250f), new Vector2(110f, 110f));
        openRT.anchorMin = new Vector2(0f, 1f);
        openRT.anchorMax = new Vector2(0f, 1f);
        var openBtn = openRT.gameObject.AddComponent<Button>();
        openBtn.targetGraphic = openRT.GetComponent<Image>();
        MakeTxt(openRT, "GuideOpenLabel", "?", 70f, Color.white, Vector2.zero, new Vector2(110f, 110f), FontStyles.Bold);

        // GuideRoot
        var guideRootGO = new GameObject("GuideRoot", typeof(RectTransform));
        var guideRootRT = (RectTransform)guideRootGO.transform;
        guideRootRT.SetParent(canvasGO.transform, false);
        Stretch(guideRootRT);

        // Backdrop
        var backdrop = MakeImg(guideRootRT, "Backdrop", new Color(0, 0, 0, 0.78f), null,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Stretch(backdrop);

        // Panel
        var panelRT = MakeImg(guideRootRT, "Panel", GuidePanelColor, rounded,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(940f, 1560f));

        // Title
        MakeTxt(panelRT, "GuideTitle", "NASIL OYNANIR", 74f, Accent,
            new Vector2(0, 680f), new Vector2(880f, 110f), FontStyles.Bold);

        // Rows
        for (int i = 0; i < GuideEntries.Length; i++)
        {
            float y = 540f - i * 168f;
            var swatchRT = MakeImg(panelRT, "Swatch" + i, GuideEntries[i].color, rounded,
                new Vector2(0.5f, 0.5f), new Vector2(-360f, y), new Vector2(120f, 120f));
            swatchRT.GetComponent<Image>().raycastTarget = false;

            var nameTxt = MakeTxt(panelRT, "RowName" + i, GuideEntries[i].name, 42f, Color.white,
                new Vector2(70f, y + 34f), new Vector2(620f, 52f), FontStyles.Bold, TextAlignmentOptions.Left);
            var descTxt = MakeTxt(panelRT, "RowDesc" + i, GuideEntries[i].desc, 30f, new Color(0.8f, 0.82f, 0.88f),
                new Vector2(70f, y - 30f), new Vector2(640f, 70f), FontStyles.Normal, TextAlignmentOptions.Left);
        }

        // OK Button
        var okRT = MakeImg(panelRT, "OK", Accent, rounded,
            new Vector2(0.5f, 0.5f), new Vector2(0, -690f), new Vector2(620f, 130f));
        var okBtn = okRT.gameObject.AddComponent<Button>();
        okBtn.targetGraphic = okRT.GetComponent<Image>();
        MakeTxt(okRT, "OKLabel", "ANLADIM", 54f, Color.white, Vector2.zero, new Vector2(620f, 130f), FontStyles.Bold);

        // ── Referansları bağla ──
        var so = new SerializedObject(guide);
        so.FindProperty("root").objectReferenceValue = guideRootGO;
        so.FindProperty("openButton").objectReferenceValue = openRT.gameObject;
        so.FindProperty("closeButton").objectReferenceValue = okBtn;
        so.FindProperty("rounded").objectReferenceValue = rounded;
        so.ApplyModifiedPropertiesWithoutUndo();

        guideRootGO.SetActive(false); // Başlangıçta kapalı

        Debug.Log("Guide Canvas oluşturuldu ve UICubeGuide'a bağlandı.");
    }

    // ═════════════════════════════════════════════
    //  YARDIMCI METODLAR
    // ═════════════════════════════════════════════
    static RectTransform MakeImg(Transform parent, string name, Color color, Sprite sprite,
        Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = color;
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        return rt;
    }

    static TMP_Text MakeTxt(Transform parent, string name, string text, float size, Color color,
        Vector2 pos, Vector2 rectSize, FontStyles style, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = rectSize;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.fontStyle = style; tmp.alignment = align; tmp.raycastTarget = false;
        return tmp;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) { Object.DestroyImmediate(go); Debug.Log("Eski " + name + " silindi."); }
    }
}
