using UnityEngine;

namespace SpinForward.Level
{
    public class CubeWall : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Cube cubePrefab;
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 6;
        [Tooltip("Distance between cube centers. 1.05 leaves a hair of gap for 1x1 cubes.")]
        [SerializeField] private float spacing = 1.05f;
        [Tooltip("Height of the cube centers above the ground.")]
        [SerializeField] private float groundHeight = 0.5f;

        public event System.Action Cleared;

        private int remaining;

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (cubePrefab == null)
            {
                Debug.LogError("[CubeWall] No cube prefab assigned.");
                return;
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = (r - (rows - 1) / 2f) * spacing;
                    Vector3 pos = transform.position + new Vector3(x, groundHeight, z);

                    Cube cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    cube.Smashed += OnCubeSmashed;
                    remaining++;
                }
            }
        }

        private void OnCubeSmashed(Cube cube)
        {
            cube.Smashed -= OnCubeSmashed; 
            remaining--;
            if (remaining <= 0)
            {
                Debug.Log("[CubeWall] Wall cleared!");
                Cleared?.Invoke();
            }
        }
    }
}
