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
        
        // Dynamic Movement Variables
        private bool isMoving;
        private float moveSpeed;
        private float moveDistance;
        private Vector3 initialPosition;
        private bool alternateRowMovement;
        
        // Physics Fix
        private struct CubeData
        {
            public Cube cube;
            public Vector3 baseLocalPos;
            public int rowIndex;
        }
        private System.Collections.Generic.List<CubeData> activeCubes = new System.Collections.Generic.List<CubeData>();
        
        // Repulsive Breathing Variables
        private bool isBreathing;
        private float breathingPushForce;

        public void Build(int columns, int rows, LevelData data = null)
        {
            if (cubePrefab == null)
            {
                Debug.LogError("[CubeWall] No cube prefab assigned.");
                return;
            }

            Clear();

            int spawnedBombs = 0; 
            
            GridShape shape = data != null ? data.shape : GridShape.Square;
            alternateRowMovement = data != null ? data.alternateRowMovement : false;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    // Şekle göre filtreleme
                    bool shouldSpawn = true;
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
                    
                    if (!shouldSpawn) continue;

                    
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = r * spacing;
                    Vector3 localPos = new Vector3(x, groundHeight, z);
                    Vector3 pos = transform.position + localPos;

                    Cube cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    
                    CubeType type = CubeType.Normal;
                    int health = 1;
                    
                    if (data != null)
                    {
                        health = data.cubeHealth;
                        float rand = Random.value;
                        
                        float tBomb = data.bombCubeChance;
                        float tSteel = tBomb + data.steelCubeChance;
                        float tIce = tSteel + data.iceCubeChance;
                        float tShield = tIce + data.shieldCubeChance;
                        float tSplit = tShield + data.splitCubeChance;
                        
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
                    }
                    
                    cube.Init(type, health);

                    // Renk ataması
                    if (type == CubeType.Bomb)
                        cube.SetColor(Color.red);
                    else if (type == CubeType.Steel)
                        cube.SetColor(Color.black);
                    else if (type == CubeType.Normal)
                        cube.SetColor(ColorFor(c, r, columns, rows));

                    if (type != CubeType.Steel)
                    {
                        cube.Smashed += OnCubeSmashed;
                        remaining++;
                    }
                    
                    activeCubes.Add(new CubeData { cube = cube, baseLocalPos = localPos, rowIndex = r });
                }
            }

            // Spawn Vortex Hazards outside the grid
            if (data != null && data.vortexCount > 0)
            {
                for (int i = 0; i < data.vortexCount; i++)
                {
                    GameObject vObj = new GameObject("VortexHazard");
                    vObj.transform.SetParent(transform);
                    
                    // Rastgele sol veya sağ tarafı seç
                    float side = (Random.value > 0.5f) ? 1f : -1f;
                    // Grid'in genişliğine göre dışarıda bir x pozisyonu
                    float x = side * (columns * spacing * 0.5f + Random.Range(2f, 4f));
                    // Grid'in uzunluğuna (Z ekseni) denk gelen rastgele bir z pozisyonu
                    float z = Random.Range(0f, rows * spacing);
                    
                    vObj.transform.position = transform.position + new Vector3(x, groundHeight, z);
                    vObj.AddComponent<VortexHazard>();
                }
            }

            if (SpinForward.CameraControl.CameraController.Instance != null)
            {
                SpinForward.CameraControl.CameraController.Instance.FrameWall(columns, rows, spacing);
            }
            
            if (data != null)
            {
                isMoving = data.isMoving;
                moveSpeed = data.moveSpeed;
                moveDistance = data.moveDistance;
                
                isBreathing = data.isBreathing;
                breathingPushForce = data.breathingPushForce;
            }
            else
            {
                isMoving = false;
                isBreathing = false;
            }
            
            initialPosition = transform.position;
            transform.localScale = Vector3.one;
        }
        
        private void Update()
        {
            if (remaining <= 0) return;
            if (isBreathing)
            {
                float sineValue = Mathf.Sin(Time.time * moveSpeed);
                float scalePulse = 1f + sineValue * 0.3f;
                transform.localScale = Vector3.one * scalePulse;
            }
        }
        
        private void FixedUpdate()
        {
            if (remaining <= 0) return;
            
            if (isMoving || isBreathing)
            {
                float sineVal = Mathf.Sin(Time.time * moveSpeed);
                Vector3 currentScale = transform.localScale;
                
                for (int i = 0; i < activeCubes.Count; i++)
                {
                    CubeData cd = activeCubes[i];
                    if (cd.cube == null || cd.cube.IsSmashed) continue;
                    
                    float xOffset = 0f;
                    if (isMoving)
                    {
                        float direction = 1f;
                        if (alternateRowMovement) direction = (cd.rowIndex % 2 == 0) ? 1f : -1f;
                        xOffset = sineVal * moveDistance * direction;
                    }
                    
                    Vector3 scaledLocalPos = new Vector3(cd.baseLocalPos.x * currentScale.x, cd.baseLocalPos.y * currentScale.y, cd.baseLocalPos.z * currentScale.z);
                    Vector3 offsetVec = new Vector3(xOffset * currentScale.x, 0, 0);
                    
                    Vector3 targetPos = initialPosition + scaledLocalPos + offsetVec;
                    cd.cube.MoveTo(targetPos);
                }
            }
            
            if (isBreathing)
            {
                if (Mathf.Cos(Time.time * moveSpeed) > 0.3f)
                {
                    if (SpinForward.Player.SpinnerController.Instance != null)
                    {
                        Rigidbody spinnerRb = SpinForward.Player.SpinnerController.Instance.GetComponent<Rigidbody>();
                        if (spinnerRb != null)
                        {
                            Vector3 toSpinner = spinnerRb.position - transform.position;
                            if (toSpinner.magnitude < (5f * transform.localScale.x))
                            {
                                Vector3 pushDir = toSpinner.normalized;
                                spinnerRb.AddForce(pushDir * (breathingPushForce * 10f) * Time.fixedDeltaTime, ForceMode.Force);
                            }
                        }
                    }
                }
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
