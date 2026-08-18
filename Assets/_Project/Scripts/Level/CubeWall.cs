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
        public int TotalCubes => totalCubes;

        private int remaining;
        private int totalCubes;
        
        
        // Physics Fix
        private struct CubeData
        {
            public Cube cube;
            public Vector3 baseLocalPos;
            public int rowIndex;
        }
        private System.Collections.Generic.List<CubeData> activeCubes = new System.Collections.Generic.List<CubeData>();
        

        public void Build(int columns, int rows, LevelData data = null)
        {
            if (cubePrefab == null)
            {
                Debug.LogError("[CubeWall] No cube prefab assigned.");
                return;
            }

            Clear();
            int spawnedBombs = 0; 
            
            Texture2D sprite = data != null ? data.levelSprite : null;
            Color[] pixels = null;
            if (sprite != null)
            {
                try
                {
                    // Sample the sprite DOWN to a capped grid instead of one cube per
                    // pixel. A 96x96 photo at maxResolution 40 becomes 40x40 (~1600
                    // cubes) instead of ~9200 - the difference between smooth and dead.
                    // Use the level's own resolution (full detail); completability comes
                    // from the per-cube energy refund (LevelManager), not a small cube count.
                    int cap = Mathf.Max(8, data.maxResolution);
                    columns = Mathf.Min(sprite.width, cap);
                    rows = Mathf.Min(sprite.height, cap);

                    pixels = new Color[columns * rows];
                    for (int yy = 0; yy < rows; yy++)
                    {
                        for (int xx = 0; xx < columns; xx++)
                        {
                            float u = (xx + 0.5f) / columns;
                            float v = (yy + 0.5f) / rows;
                            pixels[yy * columns + xx] = Quantize(sprite.GetPixelBilinear(u, v), data.colorSteps);
                        }
                    }
                }
                catch (UnityException e)
                {
                    Debug.LogWarning("[CubeWall] Cannot read texture: " + sprite.name + ". Make sure 'Read/Write Enabled' is checked in import settings! " + e.Message);
                    sprite = null;
                    pixels = null;
                }
            }

            GridShape shape = data != null ? data.shape : GridShape.Square;

            // Distance Transform (Depth calculation for health)
            int[,] distanceMap = new int[columns, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    bool isSolid = true;
                    if (sprite != null && pixels != null)
                        isSolid = pixels[r * columns + c].a >= 0.1f;
                    
                    if (isSolid)
                    {
                        // Kenarlardakiler 1 uzaklıktadır
                        if (c == 0 || c == columns - 1 || r == 0 || r == rows - 1)
                            distanceMap[c, r] = 1;
                        else
                            distanceMap[c, r] = 9999;
                    }
                    else
                    {
                        distanceMap[c, r] = 0; // Boşluk
                    }
                }
            }

            // Forward pass
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (distanceMap[c, r] > 1)
                    {
                        int min = distanceMap[c, r];
                        if (c > 0) min = Mathf.Min(min, distanceMap[c - 1, r] + 1);
                        if (r > 0) min = Mathf.Min(min, distanceMap[c, r - 1] + 1);
                        distanceMap[c, r] = min;
                    }
                }
            }
            // Backward pass
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = columns - 1; c >= 0; c--)
                {
                    if (distanceMap[c, r] > 1)
                    {
                        int min = distanceMap[c, r];
                        if (c < columns - 1) min = Mathf.Min(min, distanceMap[c + 1, r] + 1);
                        if (r < rows - 1) min = Mathf.Min(min, distanceMap[c, r + 1] + 1);
                        distanceMap[c, r] = min;
                    }
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    bool shouldSpawn = true;
                    Color pixelColor = Color.white;
                    
                    if (sprite != null && pixels != null)
                    {
                        // Resimdeki pikselleri oku
                        pixelColor = pixels[r * columns + c];
                        if (pixelColor.a < 0.1f) shouldSpawn = false; // Şeffaf pikselleri atla
                    }
                    else
                    {
                        // Eski şekle göre filtreleme
                        if (shape == GridShape.Circle)
                        {
                            float centerCol = (columns - 1) / 2f;
                            float centerRow = (rows - 1) / 2f;
                            float radius = Mathf.Min(columns, rows) / 2f;
                            float dist = Vector2.Distance(new Vector2(c, r), new Vector2(centerCol, centerRow));
                            if (dist > radius) shouldSpawn = false;
                        }
                        else if (shape == GridShape.Triangle)
                        {
                            float centerCol = (columns - 1) / 2f;
                            float widthAtRow = columns * (1f - (float)r / rows);
                            if (Mathf.Abs(c - centerCol) > widthAtRow / 2f) shouldSpawn = false;
                        }
                        else if (shape == GridShape.Diamond)
                        {
                            float centerCol = (columns - 1) / 2f;
                            float centerRow = (rows - 1) / 2f;
                            float normX = Mathf.Abs(c - centerCol) / (columns / 2.2f);
                            float normY = Mathf.Abs(r - centerRow) / (rows / 2.2f);
                            if (normX + normY > 1f) shouldSpawn = false;
                        }
                    }
                    
                    if (!shouldSpawn) continue;
                    
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = r * spacing + 5f; // Başlangıç pozisyonunu biraz ileri (Z ekseninde) kaydır
                    Vector3 localPos = new Vector3(x, groundHeight, z);
                    Vector3 pos = transform.position + localPos;

                    Cube cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    
                    CubeType type = CubeType.Normal;
                    int health = (data != null) ? data.cubeHealth : 1;

                    if (data != null)
                    {
                        float rand = Random.value;

                        float tBomb = data.bombCubeChance;
                        float tSteel = tBomb + data.steelCubeChance;
                        float tIce = tSteel + data.iceCubeChance;
                        float tShield = tIce + data.shieldCubeChance;
                        float tSplit = tShield + data.splitCubeChance;

                        float tFrenzy = tSplit + (data != null ? data.frenzyCubeChance : 0f);

                        float tLaser = tFrenzy + (data != null ? Mathf.Max(data.laserCubeChance, 0.015f) : 0f); // haç temizleyici hep biraz cikar
                        float tGold = tLaser + (data != null ? data.goldCubeChance : 0f);
                        float tDrain = tGold + (data != null ? data.drainCubeChance : 0f);
                        float tTimeBomb = tDrain + (data != null ? data.timeBombCubeChance : 0f);

                        if (rand < tBomb && spawnedBombs < data.maxBombs)
                        {
                            type = CubeType.Bomb;
                            health *= 3;
                            spawnedBombs++;
                        }
                        else if (rand < tSteel) type = CubeType.Steel;
                        else if (rand < tIce) type = CubeType.Ice;
                        else if (rand < tShield) type = CubeType.Shield;
                        else if (rand < tSplit) type = CubeType.Split;
                        else if (rand < tFrenzy) type = CubeType.Frenzy;
                        else if (rand < tLaser) type = CubeType.Laser;
                        else if (rand < tGold) type = CubeType.Gold;
                        else if (rand < tDrain) type = CubeType.Drain;
                        else if (rand < tTimeBomb) type = CubeType.TimeBomb;
                    }

                    // Resmin iç tarafında kalan siyah veya koyu renkli küplerin aşırı
                    // canlanıp kırılamaz hale gelmesini önlemek için extraHealth mekanizması KALDIRILDI.
                        
                    cube.Init(type, health);

                    // Renk ataması
                    if (type == CubeType.Bomb)
                        cube.SetColor(Color.red);
                    else if (type == CubeType.Steel)
                        cube.SetColor(new Color(0.32f, 0.35f, 0.42f)); // metalik gri
                    else if (type == CubeType.Laser)
                        cube.SetGlowColor(Color.cyan); // parlayan camgöbeği - avantaj (haç temizleyici) küpü
                    else if (type == CubeType.Gold)
                        cube.SetColor(new Color(1f, 0.84f, 0f)); // Gold (Altın sarısı)
                    else if (type == CubeType.Normal)
                    {
                        if (sprite != null)
                            cube.SetColor(pixelColor);
                        else
                            cube.SetColor(ColorFor(c, r, columns, rows));
                    }

                    // Steel and Drain (trap) cubes are optional - not required to clear the level.
                    if (type != CubeType.Steel && type != CubeType.Drain)
                    {
                        cube.Smashed += OnCubeSmashed;
                        remaining++;
                    }
                    
                    activeCubes.Add(new CubeData { cube = cube, baseLocalPos = localPos, rowIndex = r });
                }
            }
            
            totalCubes = remaining;

            if (SpinForward.CameraControl.CameraController.Instance != null)
            {
                SpinForward.CameraControl.CameraController.Instance.FrameWall(columns, rows, spacing);
            }
            
            transform.localScale = Vector3.one;
        }

        // Snaps a color to a coarse grid so many pixels share the exact same color
        // (and therefore the same pooled material -> far fewer draw calls).
        private static Color Quantize(Color c, int steps)
        {
            if (steps <= 1)
                return c;
            float s = steps - 1;
            return new Color(
                Mathf.Round(c.r * s) / s,
                Mathf.Round(c.g * s) / s,
                Mathf.Round(c.b * s) / s,
                c.a);
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
            totalCubes = 0;
            activeCubes.Clear();
            
            if (envRoot != null) Destroy(envRoot.gameObject);
        }

        private Transform envRoot;

        private void OnCubeSmashed(Cube cube)
        {
            cube.Smashed -= OnCubeSmashed;
            remaining--;
            if (remaining <= 0)
                Cleared?.Invoke();
        }
    }
}
