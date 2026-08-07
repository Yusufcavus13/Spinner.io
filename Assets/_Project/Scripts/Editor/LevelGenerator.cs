using UnityEngine;
using UnityEditor;
using SpinForward.Level;

namespace SpinForward.Editor
{
    public class LevelGenerator
    {
        [MenuItem("Tools/SpinForward/5 Bolumleri (Levels) Uret")]
        public static void GenerateLevels()
        {
            string path = "Assets/_Project/Data/Levels";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Levels");
            }
            
            for(int i = 1; i <= 20; i++)
            {
                string assetPath = $"{path}/Level_{i}.asset";
                
                // Eğer daha önceden varsa üzerine yazmak yerine var olanı güncelleyelim (ya da sıfırdan yapalım)
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                bool isNew = false;
                if (level == null)
                {
                    level = ScriptableObject.CreateInstance<LevelData>();
                    isNew = true;
                }
                
                // Zorluk Eğrisi (Difficulty scaling)
                level.columns = 5 + (i / 4);
                level.rows = 5 + (i / 4);
                level.attemptDuration = 20f + (i * 2f);
                level.cubeHealth = 1 + (i / 10);
                
                level.maxBombs = 1 + (i / 4);
                level.bombCubeChance = Mathf.Min(0.25f, i * 0.015f);
                level.steelCubeChance = Mathf.Min(0.15f, (i > 5 ? (i - 5) * 0.01f : 0f));
                
                // Yeni Küpler
                level.iceCubeChance = Mathf.Min(0.1f, (i > 2 ? 0.05f : 0f)); // Buz küpü 3. leveldan itibaren
                level.shieldCubeChance = Mathf.Min(0.15f, (i > 4 ? 0.08f : 0f)); // Kalkanlı küp 5. leveldan itibaren
                level.splitCubeChance = Mathf.Min(0.05f, (i > 3 ? 0.03f : 0f)); // Klonlanma nadir bir ödül
                level.vortexCount = (i > 5) ? 1 + (i / 10) : 0; // Girdap 6. leveldan itibaren 1 tane, sonra artar
                
                // Şekiller ve Hareketler
                // 1. Level Kare, 2. Yuvarlak, 3. Üçgen, 4. Elmas şeklinde döngüye girer
                level.shape = (GridShape)(i % 4); 
                
                level.isMoving = (i % 2 == 0); // Çift leveller hareketli
                level.alternateRowMovement = (i % 4 == 0); // Her 4 levelda bir Zıt satır kayması (Testere gibi)
                level.moveSpeed = 1.5f + (i * 0.1f);
                level.moveDistance = 2f + (i * 0.2f);
                
                level.isBreathing = (i % 3 == 0); // Her 3 levelda bir duvarlar nefes alıp iter
                level.breathingPushForce = 15f + (i * 1.5f); // İtme gücü artar
                
                if (isNew)
                {
                    AssetDatabase.CreateAsset(level, assetPath);
                }
                else
                {
                    EditorUtility.SetDirty(level);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SpinForward] 20 adet muhtesem bolum başarıyla uretildi/guncellendi!");
        }
    }
}
