using UnityEditor;
using UnityEngine;
using TMPro;
using SpinForward.UI;

namespace SpinForward.Editor
{
    public class CreateDamageTextPrefab
    {
        [MenuItem("Tools/SpinForward/Create Damage Text Prefab")]
        public static void CreatePrefab()
        {
            // Create empty game object
            GameObject obj = new GameObject("DamageTextPrefab");

            // Add TextMeshPro
            TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = "10";
            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            // Set RectTransform size
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(5, 5);

            // Add FloatingText
            obj.AddComponent<FloatingText>();

            // Save as Prefab
            string path = "Assets/_Project/Prefabs/DamageTextPrefab.prefab";
            
            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            GameObject.DestroyImmediate(obj);

            Debug.Log($"[SpinForward] Damage Text Prefab başarıyla oluşturuldu: {path}");
        }
    }
}
