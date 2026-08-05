using SpinForward.Level;
using UnityEngine;

namespace SpinForward.Economy
{
    /// <summary>
    /// Listens for any cube shattering and drops a <see cref="MoneyOrb"/> at that
    /// spot, aimed at the spinner. Because it hooks the global cube event, cubes
    /// never need to know money exists. The Income upgrade will scale rewardPerCube.
    /// </summary>
    public class MoneySpawner : MonoBehaviour
    {
        [SerializeField] private MoneyOrb orbPrefab;
        [Tooltip("The spinner the coins fly toward.")]
        [SerializeField] private Transform target;
        [SerializeField] private int rewardPerCube = 1;

        private void OnEnable()
        {
            Cube.AnyCubeSmashed += SpawnOrb;
        }

        private void OnDisable()
        {
            Cube.AnyCubeSmashed -= SpawnOrb;
        }

        private void SpawnOrb(Vector3 position)
        {
            if (orbPrefab == null || target == null)
                return;

            MoneyOrb orb = Instantiate(orbPrefab, position, Quaternion.identity);
            orb.Launch(target, rewardPerCube);
        }
    }
}
