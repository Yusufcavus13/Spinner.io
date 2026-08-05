using SpinForward.Level;
using UnityEngine;

namespace SpinForward.Economy
{
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

            // Income upgrade multiplies the base reward.
            int reward = rewardPerCube;
            if (UpgradeSystem.Instance != null)
                reward = Mathf.Max(1, Mathf.RoundToInt(rewardPerCube * UpgradeSystem.Instance.Income.Value));

            MoneyOrb orb = Instantiate(orbPrefab, position, Quaternion.identity);
            orb.Launch(target, reward);
        }
    }
}
