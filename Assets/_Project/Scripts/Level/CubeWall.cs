using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// Spawns a flat grid of <see cref="Cube"/>s on the ground to form the level's
    /// "image". Counts them down as they shatter and raises <see cref="Cleared"/>
    /// when the last one is gone (that hooks up to the success panel later).
    /// </summary>
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

        /// <summary>Fires once, when every cube in the grid has been smashed.</summary>
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
                    // Center the grid on this object's position, laid flat on XZ.
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
            cube.Smashed -= OnCubeSmashed; // stop listening to a cube that's leaving
            remaining--;
            if (remaining <= 0)
            {
                Debug.Log("[CubeWall] Wall cleared!");
                Cleared?.Invoke();
            }
        }
    }
}
