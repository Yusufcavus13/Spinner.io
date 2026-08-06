using UnityEngine;

namespace SpinForward.Level
{
    
    public class CubeWall : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Cube cubePrefab;
        [Tooltip("Distance between cube centers. 1.05 leaves a hair of gap for 1x1 cubes.")]
        [SerializeField] private float spacing = 1.05f;
        [Tooltip("Height of the cube centers above the ground.")]
        [SerializeField] private float groundHeight = 0.5f;

        [Header("Look")]
        [Tooltip("Color richness of the wall (0 = grey, 1 = vivid).")]
        [Range(0f, 1f)]
        [SerializeField] private float saturation = 0.7f;
        [Tooltip("Overall brightness of the cubes.")]
        [Range(0f, 1f)]
        [SerializeField] private float brightness = 0.95f;
        [Tooltip("How much darker the back rows get, for depth (0 = flat).")]
        [Range(0f, 0.6f)]
        [SerializeField] private float rowShade = 0.35f;

        public event System.Action Cleared;

        public int Remaining => remaining;

        private int remaining;

        public void Build(int columns, int rows, LevelData data = null)
        {
            if (cubePrefab == null)
            {
                Debug.LogError("[CubeWall] No cube prefab assigned.");
                return;
            }

            Clear();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = r * spacing;
                    Vector3 pos = transform.position + new Vector3(x, groundHeight, z);

                    Cube cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    
                    CubeType type = CubeType.Normal;
                    int health = 1;
                    
                    if (data != null)
                    {
                        health = data.cubeHealth;
                        float rand = Random.value;
                        if (rand < data.bombCubeChance)
                            type = CubeType.Bomb;
                        else if (rand < data.bombCubeChance + data.steelCubeChance)
                            type = CubeType.Steel;
                    }
                    
                    cube.Init(type, health);

                    // Renk ataması (Bombalar kırmızı, Çelikler siyah, Normaller gökkuşağı)
                    if (type == CubeType.Bomb)
                        cube.SetColor(Color.red);
                    else if (type == CubeType.Steel)
                        cube.SetColor(Color.black);
                    else
                        cube.SetColor(ColorFor(c, r, columns, rows));

                    cube.Smashed += OnCubeSmashed;
                    remaining++;
                }
            }

            // Duvar inşa edildikten sonra kamerayı geriye çek
            if (SpinForward.CameraControl.CameraController.Instance != null)
            {
                SpinForward.CameraControl.CameraController.Instance.FrameWall(columns, rows, spacing);
            }
        }

        private Color ColorFor(int col, int row, int columns, int rows)
        {
            // Normal küplerin rengi (Hue) 0 ile 1 arasındadır. 0 ve 1 Kırmızı demektir.
            // Bomba küp kırmızı olduğu için, normal küplerin kırmızı olmasını engellemeliyiz.
            // Bu yüzden Hue değerini 0.15 (Sarı/Yeşil) ile 0.85 (Mor/Pembe) arasına sıkıştırıyoruz.
            float huePercent = columns > 0 ? (float)col / columns : 0f;
            float hue = Mathf.Lerp(0.15f, 0.85f, huePercent); 
            
            Color baseColor = Color.HSVToRGB(hue, saturation, brightness);

            // Arka sıraları hafif koyulaştırıyoruz (Derinlik hissi)
            float depth = rows > 1 ? (float)row / (rows - 1) : 0f;
            return Color.Lerp(baseColor, baseColor * 0.5f, depth * rowShade);
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            remaining = 0;
        }

        private void OnCubeSmashed(Cube cube)
        {
            cube.Smashed -= OnCubeSmashed;
            remaining--;
            if (remaining <= 0)
                Cleared?.Invoke();
        }
    }
}
