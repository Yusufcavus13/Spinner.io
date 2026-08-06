using UnityEditor;
using UnityEngine;
using System.IO;

namespace SpinForward.Editor
{
    public class GenerateAssets
    {
        [MenuItem("Tools/SpinForward/2 Kusursuz Testere ve Zemin Üret")]
        public static void Generate()
        {
            CreateBuzzsawTexture();
            Debug.Log("[SpinForward] Şeffaf Testere resmi ve Zemin Materyali oluşturuldu!");
        }

        private static void CreateBuzzsawTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f - 10f;
            float innerRadius = size * 0.15f; // Middle hole

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    
                    // Testere dişleri (Sawtooth) mantığı
                    int teeth = 16;
                    float normalizedAngle = (angle + Mathf.PI) / (Mathf.PI * 2f);
                    float toothShape = (normalizedAngle * teeth) % 1.0f; // 0'dan 1'e doğru çıkar (diş şekli)
                    
                    // Dişin yarıçapı
                    float currentRadius = Mathf.Lerp(outerRadius * 0.8f, outerRadius, toothShape);
                    
                    Color pixelColor = Color.clear; // Arka plan tamamen şeffaf!
                    
                    if (dist < currentRadius && dist > innerRadius)
                    {
                        // Metalik renk geçişi (Gradient)
                        float colorVal = Mathf.Lerp(0.8f, 0.4f, dist / outerRadius);
                        
                        // Dişlerin uçlarını daha parlak yap
                        if (dist > outerRadius * 0.75f)
                        {
                            colorVal += toothShape * 0.3f; // Uca doğru parlaklık artar
                        }
                        
                        // İç kısma mavi bir neon halka ekle
                        if (dist > innerRadius + 10f && dist < innerRadius + 30f)
                        {
                            pixelColor = new Color(0f, 0.8f, 1f, 1f); // Neon Mavi
                        }
                        else
                        {
                            pixelColor = new Color(colorVal, colorVal, colorVal + 0.05f, 1f); // Çelik/Metal rengi
                        }
                    }
                    
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            tex.Apply();

            // PNG olarak kaydet (PNG şeffaflığı destekler)
            byte[] bytes = tex.EncodeToPNG();
            
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Art");
            }
            
            string path = "Assets/_Project/Art/PerfectBuzzsaw.png";
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // Texture tipini otomatik olarak Sprite yap
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }
}
