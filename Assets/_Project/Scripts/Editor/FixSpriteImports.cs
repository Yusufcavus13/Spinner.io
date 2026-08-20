using UnityEditor;
using UnityEngine;

public static class FixSpriteImports
{
    [MenuItem("Tools/Fix Sprite Imports")]
    public static void Fix()
    {
        string[] paths = {
            "Assets/_Project/Art/UI/Circle96.png",
            "Assets/_Project/Art/UI/Rounded24.png",
            "Assets/_Project/Art/UI/Rounded20.png",
            "Assets/_Project/Art/UI/Saw.png",
            "Assets/_Project/Art/UI/Star.png",
            "Assets/_Project/Art/UI/Gear.png"
        };

        foreach (string p in paths)
        {
            TextureImporter imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) { Debug.LogWarning("Bulunamadı: " + p); continue; }

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            if (p.Contains("Rounded24")) imp.spriteBorder = new Vector4(24, 24, 24, 24);
            else if (p.Contains("Rounded20")) imp.spriteBorder = new Vector4(20, 20, 20, 20);
            imp.SaveAndReimport();
            Debug.Log("Sprite olarak ayarlandı: " + p);
        }
        Debug.Log("Tüm sprite'lar düzeltildi!");
    }
}
