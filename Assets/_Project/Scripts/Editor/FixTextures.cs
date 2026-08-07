using UnityEditor;
using UnityEngine;

namespace SpinForward.Editor
{
    public class FixTextures
    {
        [MenuItem("Tools/SpinForward/1 Tek Tıkla Resimleri Düzelt")]
        public static void Fix()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    // Buton, Testere ve Fever arayüz resimleri için Sprite ayarı
                    if (path.Contains("buzzsaw") || path.Contains("ui_button") || path.Contains("fever"))
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.SaveAndReimport();
                        Debug.Log("[SpinForward] Başarıyla Sprite yapıldı: " + path);
                    }
                    else if (path.Contains("arena_floor")) // Zemin için Default ayarı
                    {
                        importer.textureType = TextureImporterType.Default;
                        importer.SaveAndReimport();
                        Debug.Log("[SpinForward] Başarıyla Default Doku yapıldı: " + path);
                    }
                }
            }
            Debug.Log("[SpinForward] BÜTÜN RESİMLER DÜZELTİLDİ! Artık Unity onları tanıyor.");
        }
    }
}
