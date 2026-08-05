using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// Builds a flat grid of <see cref="Cube"/>s on demand and reports when the
    /// grid is fully smashed. The <see cref="LevelManager"/> drives it: it decides
    /// the size and calls <see cref="Build"/> at the start of each attempt.
    /// </summary>
    public class CubeWall : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Cube cubePrefab;
        [Tooltip("Distance between cube centers. 1.05 leaves a hair of gap for 1x1 cubes.")]
        [SerializeField] private float spacing = 1.05f;
        [Tooltip("Height of the cube centers above the ground.")]
        [SerializeField] private float groundHeight = 0.5f;

        /// <summary>Fires once, when the last cube of the current grid is smashed.</summary>
        public event System.Action Cleared;

        public int Remaining => remaining;

        private int remaining;

        /// <summary>Removes any current cubes and spawns a fresh cols x rows grid.</summary>
        public void Build(int columns, int rows)
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
                    // Centered left-right, but grows FORWARD only (+Z) from this
                    // object's position, so the near edge never creeps back onto
                    // the spinner's start point as the wall gets bigger.
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = r * spacing;
                    Vector3 pos = transform.position + new Vector3(x, groundHeight, z);

                    Cube cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    cube.Smashed += OnCubeSmashed;
                    remaining++;
                }
            }
        }

        /// <summary>Destroys every cube (smashed debris included) and resets the count.</summary>
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
