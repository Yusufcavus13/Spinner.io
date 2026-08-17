using SpinForward.Level;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// Builds a polished HUD entirely FROM CODE: a horizontal Progress bar at the
    /// top (with a flag badge on its right) and a vertical Energy bar down the right
    /// side (with a lightning badge on top). Rounded pills, gloss, shadow, smooth
    /// fill, procedurally-drawn icons - no sprites, fonts or inspector wiring.
    /// Drop this on ONE GameObject and press Play.
    /// </summary>
    public class AutoHUD : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);
        [SerializeField] private Vector2 progressBarSize = new Vector2(720f, 40f);
        [SerializeField] private float progressTopOffset = 175f;
        [SerializeField] private Vector2 energyBarSize = new Vector2(42f, 460f);
        [SerializeField] private float energyRightOffset = 34f;
        [SerializeField] private float badgeSize = 62f;

        [Header("Colors")]
        [SerializeField] private Color progressColor = new Color(0.16f, 0.86f, 0.44f);
        [SerializeField] private Color energyColor = new Color(1f, 0.78f, 0.18f);
        [SerializeField] private Color lowEnergyColor = new Color(0.96f, 0.26f, 0.2f);
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.06f, 0.09f, 0.72f);

        private const float Pad = 5f;

        private Sprite rounded;
        private Sprite circle;
        private Sprite boltIcon;
        private Sprite flagIcon;

        private RectTransform progressFill;
        private RectTransform energyFill;
        private Image energyFillImg;

        private void Start()
        {
            rounded = MakeRoundedSprite(16);
            circle = MakeCircleSprite(64);
            boltIcon = MakeIconSprite(72, new[] { Bolt });
            flagIcon = MakeIconSprite(72, new[] { FlagPole, FlagCloth });

            Transform canvas = BuildCanvas();
            progressFill = BuildProgressBar(canvas);
            energyFill = BuildEnergyBar(canvas);
            energyFillImg = energyFill.GetComponent<Image>();
        }

        private Transform BuildCanvas()
        {
            var go = new GameObject("AutoHUD_Canvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        // ---- Progress bar: horizontal, top-center, flag badge on the right ----
        private RectTransform BuildProgressBar(Transform parent)
        {
            RectTransform bar = MakeBarContainer(parent, "ProgressBar",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -progressTopOffset), progressBarSize);

            // Fill: left-anchored, width animates.
            RectTransform fill = MakeFill(bar, progressColor,
                new Vector2(0f, 0.5f), new Vector2(Pad, 0f),
                new Vector2(progressBarSize.x - Pad * 2f, progressBarSize.y - Pad * 2f));
            AddGloss(fill, horizontal: true);

            // Flag badge just off the right end.
            MakeBadge(bar, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), flagIcon, progressColor);
            return fill;
        }

        // ---- Energy bar: vertical, right side, lightning badge on top ----
        private RectTransform BuildEnergyBar(Transform parent)
        {
            RectTransform bar = MakeBarContainer(parent, "EnergyBar",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-energyRightOffset, -30f), energyBarSize);

            // Fill: bottom-anchored, height animates.
            RectTransform fill = MakeFill(bar, energyColor,
                new Vector2(0.5f, 0f), new Vector2(0f, Pad),
                new Vector2(energyBarSize.x - Pad * 2f, energyBarSize.y - Pad * 2f));
            AddGloss(fill, horizontal: false);

            // Lightning badge on top.
            MakeBadge(bar, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), boltIcon, energyColor);
            return fill;
        }

        private RectTransform MakeBarContainer(Transform parent, string barName, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(barName, typeof(RectTransform), typeof(Image), typeof(Shadow));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var bg = go.GetComponent<Image>();
            bg.sprite = rounded;
            bg.type = Image.Type.Sliced;
            bg.color = backgroundColor;
            bg.raycastTarget = false;

            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -5f);
            return rt;
        }

        private RectTransform MakeFill(RectTransform bar, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(bar, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        private void AddGloss(RectTransform fill, bool horizontal)
        {
            var go = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(fill, false);
            // Top half for horizontal bars, left half for vertical bars.
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = horizontal ? new Vector2(1f, 1f) : new Vector2(0.5f, 1f);
            if (horizontal) rt.anchorMin = new Vector2(0f, 0.52f);
            rt.offsetMin = new Vector2(4f, horizontal ? 0f : 4f);
            rt.offsetMax = new Vector2(horizontal ? -4f : 0f, -3f);

            var img = go.GetComponent<Image>();
            img.sprite = rounded;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.22f);
            img.raycastTarget = false;
        }

        private void MakeBadge(RectTransform bar, Vector2 anchor, Vector2 pivot, Vector2 pos, Sprite icon, Color badgeColor)
        {
            var badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(Shadow));
            var brt = (RectTransform)badgeGo.transform;
            brt.SetParent(bar, false);
            brt.anchorMin = anchor;
            brt.anchorMax = anchor;
            brt.pivot = pivot;
            brt.anchoredPosition = pos;
            brt.sizeDelta = new Vector2(badgeSize, badgeSize);

            var bimg = badgeGo.GetComponent<Image>();
            bimg.sprite = circle;
            bimg.color = badgeColor;
            bimg.raycastTarget = false;

            var sh = badgeGo.GetComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.35f);
            sh.effectDistance = new Vector2(0f, -4f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)iconGo.transform;
            irt.SetParent(badgeGo.transform, false);
            irt.anchorMin = new Vector2(0.5f, 0.5f);
            irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = Vector2.zero;
            irt.sizeDelta = new Vector2(badgeSize * 0.6f, badgeSize * 0.6f);

            var iimg = iconGo.GetComponent<Image>();
            iimg.sprite = icon;
            iimg.color = Color.white;
            iimg.raycastTarget = false;
        }

        private void Update()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            if (energyFill != null)
            {
                float max = lm.MaxEnergy;
                float t = max > 0f ? Mathf.Clamp01(lm.CurrentEnergy / max) : 0f;
                float h = Mathf.Lerp(energyFill.sizeDelta.y, t * (energyBarSize.y - Pad * 2f), Time.deltaTime * 10f);
                energyFill.sizeDelta = new Vector2(energyBarSize.x - Pad * 2f, h);
                if (energyFillImg != null)
                    energyFillImg.color = Color.Lerp(lowEnergyColor, energyColor, Mathf.Clamp01(t / 0.35f));
            }

            if (progressFill != null && lm.Wall != null)
            {
                int total = lm.Wall.TotalCubes;
                float t = total > 0 ? Mathf.Clamp01((float)(total - lm.Wall.Remaining) / total) : 0f;
                float w = Mathf.Lerp(progressFill.sizeDelta.x, t * (progressBarSize.x - Pad * 2f), Time.deltaTime * 10f);
                progressFill.sizeDelta = new Vector2(w, progressBarSize.y - Pad * 2f);
            }
        }

        // ---------- Procedural sprites ----------

        private static Sprite MakeRoundedSprite(int radius)
        {
            int size = radius * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float ny = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float d = Mathf.Sqrt((x + 0.5f - nx) * (x + 0.5f - nx) + (y + 0.5f - ny) * (y + 0.5f - ny));
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Sprite MakeCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            float r = size * 0.5f - 1f;
            float c = size * 0.5f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // Rasterizes filled polygons (normalized 0..1, y up) into a white icon sprite, 3x3 anti-aliased.
        private static Sprite MakeIconSprite(int size, Vector2[][] polys)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int hits = 0;
                    for (int sy = 0; sy < 3; sy++)
                        for (int sx = 0; sx < 3; sx++)
                        {
                            float u = (x + (sx + 0.5f) / 3f) / size;
                            float v = (y + (sy + 0.5f) / 3f) / size;
                            foreach (var poly in polys)
                                if (PointInPolygon(u, v, poly)) { hits++; break; }
                        }
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(hits / 9f * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static bool PointInPolygon(float px, float py, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                Vector2 a = poly[i], b = poly[j];
                if (((a.y > py) != (b.y > py)) &&
                    (px < (b.x - a.x) * (py - a.y) / (b.y - a.y) + a.x))
                    inside = !inside;
            }
            return inside;
        }

        private static readonly Vector2[] Bolt =
        {
            new Vector2(0.58f, 0.98f), new Vector2(0.16f, 0.50f), new Vector2(0.44f, 0.50f),
            new Vector2(0.30f, 0.02f), new Vector2(0.84f, 0.56f), new Vector2(0.54f, 0.56f)
        };

        private static readonly Vector2[] FlagPole =
        {
            new Vector2(0.28f, 0.04f), new Vector2(0.37f, 0.04f),
            new Vector2(0.37f, 0.96f), new Vector2(0.28f, 0.96f)
        };

        private static readonly Vector2[] FlagCloth =
        {
            new Vector2(0.37f, 0.94f), new Vector2(0.88f, 0.78f), new Vector2(0.37f, 0.62f)
        };
    }
}
