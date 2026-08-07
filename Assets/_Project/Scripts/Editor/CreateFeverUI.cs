using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpinForward.Editor
{
    public class CreateFeverUI
    {
        [MenuItem("Tools/SpinForward/3 Kombo Arayüzünü (Fever UI) Oluştur")]
        public static void GenerateUI()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SpinForward] Sahnede Canvas bulunamadı!");
                return;
            }

            // Create Combo Parent
            GameObject comboParent = new GameObject("ComboUI", typeof(RectTransform));
            comboParent.transform.SetParent(canvas.transform, false);
            RectTransform parentRt = comboParent.GetComponent<RectTransform>();
            parentRt.anchorMin = new Vector2(0.5f, 0.85f);
            parentRt.anchorMax = new Vector2(0.5f, 0.85f);
            parentRt.anchoredPosition = Vector2.zero;

            // Create Slider
            GameObject sliderObj = new GameObject("ComboSlider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(comboParent.transform, false);
            RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
            sliderRt.sizeDelta = new Vector2(400, 30);
            sliderRt.anchoredPosition = new Vector2(0, -30); // Below text

            GameObject backgroundObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = backgroundObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
            backgroundObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRt = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one; 
            fillAreaRt.sizeDelta = new Vector2(-10, -10);

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImage = fillObj.GetComponent<Image>();
            fillImage.color = new Color(1f, 0.8f, 0f); // Yellow

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fillRt;
            slider.value = 0f;

            // Create Fever Text
            GameObject textObj = new GameObject("FeverText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(comboParent.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(400, 100);
            textRt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = "FEVER MODE!";
            tmp.fontSize = 70;
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.2f, 0f, 1f); // Red/Orange
            
            // Add custom outline
            tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = Color.black;

            // Attach Script
            SpinForward.UI.ComboUI comboUIScript = comboParent.AddComponent<SpinForward.UI.ComboUI>();
            
            // Link references via reflection or just use serialized object to force them
            SerializedObject so = new SerializedObject(comboUIScript);
            so.FindProperty("comboSlider").objectReferenceValue = slider;
            so.FindProperty("feverText").objectReferenceValue = tmp;
            so.FindProperty("fillImage").objectReferenceValue = fillImage;
            so.ApplyModifiedProperties();

            Debug.Log("[SpinForward] Kombo Arayüzü (Fever UI) başarıyla oluşturuldu!");
            Selection.activeGameObject = comboParent;
        }
    }
}
