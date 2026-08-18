using SpinForward.Economy;
using SpinForward.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpinForward.Editor
{
    /// <summary>
    /// One-click upgrade-button styling: assigns the button PNGs from Assets/Buttons/PNG
    /// (green/red/blue/yellow per upgrade kind), sets them up as 9-sliced sprites, and
    /// configures nice pressed/disabled states. No MCP or manual dragging needed.
    /// Run via: Tools > SpinForward > Beautify Upgrade Buttons.
    /// </summary>
    public static class ButtonBeautifier
    {
        private const string Green = "Assets/Buttons/PNG/22Button_Midl_Green.png";
        private const string Red = "Assets/Buttons/PNG/23Button_Midl_Red.png";
        private const string Blue = "Assets/Buttons/PNG/24Button_Midl_Blue.png";
        private const string Yellow = "Assets/Buttons/PNG/10Button_Midl_Yellow.png";

        [MenuItem("Tools/SpinForward/Beautify Upgrade Buttons")]
        public static void Beautify()
        {
            var buttons = Object.FindObjectsByType<UpgradeButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (buttons.Length == 0)
            {
                EditorUtility.DisplayDialog("Beautify Buttons",
                    "Sahnede UpgradeButton bulunamadı. Doğru sahne açık mı?", "Tamam");
                return;
            }

            int done = 0;
            foreach (var ub in buttons)
            {
                var so = new SerializedObject(ub);
                var kindProp = so.FindProperty("kind");
                UpgradeKind kind = kindProp != null ? (UpgradeKind)kindProp.enumValueIndex : UpgradeKind.Power;

                string path = kind switch
                {
                    UpgradeKind.Power => Red,
                    UpgradeKind.Income => Blue,
                    UpgradeKind.Energy => Green,
                    _ => Yellow // Rotate / diğer
                };

                Sprite sprite = LoadAsSlicedSprite(path);
                if (sprite == null)
                    continue;

                Button btn = ub.GetComponent<Button>();
                Image img = (btn != null && btn.image != null) ? btn.image : ub.GetComponent<Image>();
                if (img == null)
                    continue;

                Undo.RecordObject(img, "Beautify Button");
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                img.pixelsPerUnitMultiplier = 1f;
                EditorUtility.SetDirty(img);

                // Sprite is opaque now: split labels into top/bottom halves so they
                // can never overlap, and make them white & readable on top.
                StyleLabel(so, "titleLabel", topHalf: true);
                StyleLabel(so, "costLabel", topHalf: false);

                if (btn != null)
                {
                    Undo.RecordObject(btn, "Beautify Button");
                    btn.transition = Selectable.Transition.ColorTint;
                    var cb = btn.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
                    cb.pressedColor = new Color(0.78f, 0.78f, 0.78f);
                    cb.selectedColor = Color.white;
                    cb.disabledColor = new Color(0.72f, 0.72f, 0.72f, 1f); // dimmer but still readable
                    cb.fadeDuration = 0.08f;
                    btn.colors = cb;
                    EditorUtility.SetDirty(btn);
                }
                done++;
            }

            // Also style the win/lose panel buttons (Next Level = green, Retry = red),
            // found by the method they call on click or by their GameObject name.
            foreach (Button btn in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string path = null;
                if (ButtonCalls(btn, "NextLevel")) path = Green;
                else if (ButtonCalls(btn, "Retry")) path = Red;
                else
                {
                    string n = btn.gameObject.name.ToLowerInvariant();
                    if (n.Contains("next")) path = Green;
                    else if (n.Contains("retry") || n.Contains("tekrar") || n.Contains("again")) path = Red;
                }
                if (path == null)
                    continue;

                Sprite sprite = LoadAsSlicedSprite(path);
                Image img = btn.image != null ? btn.image : btn.GetComponent<Image>();
                if (sprite == null || img == null)
                    continue;

                Undo.RecordObject(img, "Beautify Button");
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                img.pixelsPerUnitMultiplier = 1f;
                EditorUtility.SetDirty(img);

                Undo.RecordObject(btn, "Beautify Button");
                btn.transition = Selectable.Transition.ColorTint;
                var cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
                cb.pressedColor = new Color(0.78f, 0.78f, 0.78f);
                cb.disabledColor = new Color(0.72f, 0.72f, 0.72f, 1f);
                cb.fadeDuration = 0.08f;
                btn.colors = cb;
                EditorUtility.SetDirty(btn);

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    StyleSingleLabel(label);

                done++;
            }

            if (done > 0)
            {
                Scene scene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.DisplayDialog("Beautify Buttons",
                $"{done} buton güzelleştirildi.\n\nSahneyi kaydetmeyi unutma: Cmd+S", "Süper");
        }

        // Pins a label to the top or bottom half of its button so the two labels can
        // never overlap, centers it, makes it white and auto-sizes it to fill that half.
        private static void StyleLabel(SerializedObject so, string field, bool topHalf)
        {
            var p = so.FindProperty(field);
            if (p == null || !(p.objectReferenceValue is TMP_Text tmp))
                return;

            RectTransform rt = tmp.rectTransform;
            Undo.RecordObject(tmp, "Style Label");
            Undo.RecordObject(rt, "Style Label");

            rt.SetAsLastSibling(); // draw above the sprite
            rt.anchorMin = new Vector2(0.06f, topHalf ? 0.5f : 0.08f);
            rt.anchorMax = new Vector2(0.94f, topHalf ? 0.92f : 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;   // fills its own half; can't overlap the other label
            tmp.fontSizeMin = 10f;
            tmp.fontSizeMax = 48f;

            EditorUtility.SetDirty(tmp);
            EditorUtility.SetDirty(rt);
        }

        // True if the button's OnClick has a persistent listener calling the given method.
        private static bool ButtonCalls(Button btn, string methodName)
        {
            if (btn == null)
                return false;
            for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                if (btn.onClick.GetPersistentMethodName(i) == methodName)
                    return true;
            return false;
        }

        // Styles a single button label (Next/Retry) to fill the button, white and bold.
        private static void StyleSingleLabel(TMP_Text tmp)
        {
            RectTransform rt = tmp.rectTransform;
            Undo.RecordObject(tmp, "Style Label");
            Undo.RecordObject(rt, "Style Label");
            rt.SetAsLastSibling();
            rt.anchorMin = new Vector2(0.1f, 0.18f);
            rt.anchorMax = new Vector2(0.9f, 0.82f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle |= FontStyles.Bold;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 12f;
            tmp.fontSizeMax = 60f;
            EditorUtility.SetDirty(tmp);
            EditorUtility.SetDirty(rt);
        }

        // Ensures the PNG is imported as a 9-sliced Sprite, then returns the Sprite.
        private static Sprite LoadAsSlicedSprite(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter ti))
            {
                Debug.LogWarning($"[ButtonBeautifier] Sprite bulunamadı: {path}");
                return null;
            }

            bool changed = false;
            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
            if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (changed)
                ti.SaveAndReimport();

            // 9-slice border from the texture size (keeps corners crisp when stretched).
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                int b = Mathf.RoundToInt(Mathf.Min(tex.width, tex.height) * 0.33f);
                var border = new Vector4(b, b, b, b);
                if (ti.spriteBorder != border)
                {
                    ti.spriteBorder = border;
                    ti.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
