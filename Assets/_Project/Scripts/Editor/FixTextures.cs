using UnityEditor;
using UnityEngine;

namespace SpinForward.Editor
{
    public class FixTextures
    {
        [MenuItem("Tools/SpinForward/1 Tek Tıkla Resimleri Düzelt")]
        public static void Fix()
        {
            string[] paths = new string[] {
                "Assets/_Project/Art/buzzsaw_texture_1786021637446.jpg",
                "Assets/_Project/Art/ui_button_shiny_1786021668223.jpg",
                "Assets/_Project/Art/arena_floor_texture_1786021608817.jpg"
            };
            
            foreach (var path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    // Buton ve Testere için Sprite ayarı
                    if (path.Contains("buzzsaw") || path.Contains("ui_button"))
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.SaveAndReimport();
                        Debug.Log("[SpinForward] Başarıyla Sprite yapıldı: " + path);
                    }
                    else // Arena zemini için Default ayarı
                    {
                        importer.textureType = TextureImporterType.Default;
                        importer.SaveAndReimport();
                    }
                }
            }
            Debug.Log("[SpinForward] BÜTÜN RESİMLER DÜZELTİLDİ! Artık Unity onları tanıyor.");
        }
    }
}
