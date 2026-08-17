using UnityEngine;

namespace SpinForward.Economy
{
    
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        [SerializeField] private Wallet wallet;
        [SerializeField] private Upgrade rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
        [SerializeField] private Upgrade power = new Upgrade("Power", 2f, 1f, 15, 1.7f); // Start with 2 power to break easier
        [SerializeField] private Upgrade income = new Upgrade("Income", 2f, 1f, 20, 1.8f); // Start with 2 income
        [SerializeField] private Upgrade energy = new Upgrade("Energy", 150f, 25f, 15, 1.6f); // More base energy

        public Upgrade Rotate => rotate;
        public Upgrade Power => power;
        public Upgrade Income => income;
        public Upgrade Energy => energy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Ensure upgrades are initialized if added to an existing prefab
            if (rotate == null) rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
            if (power == null) power = new Upgrade("Power", 2f, 1f, 15, 1.7f);
            if (income == null) income = new Upgrade("Income", 2f, 1f, 20, 1.8f);
            if (energy == null) energy = new Upgrade("Energy", 150f, 25f, 15, 1.6f);
        }

        private void Start()
        {
            if (wallet == null)
                wallet = Wallet.Instance;
        }

        public Upgrade Get(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Rotate: return rotate;
                case UpgradeKind.Power: return power;
                case UpgradeKind.Energy: return energy;
                default: return income;
            }
        }

        public bool Buy(UpgradeKind kind)
        {
            return Get(kind).TryPurchase(wallet);
        }

        // Retry penalty: drop every upgrade by a fraction of its level (which also brings
        // its cost back down, so re-buying after a fail stays affordable).
        public void ApplyRetryPenalty(float levelFraction)
        {
            Drop(rotate, levelFraction);
            Drop(power, levelFraction);
            Drop(income, levelFraction);
            Drop(energy, levelFraction);
        }

        private static void Drop(Upgrade u, float fraction)
        {
            u.ReduceLevel(Mathf.CeilToInt(u.Level * fraction));
        }

        private void Reset()
        {
            rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
            power = new Upgrade("Power", 2f, 1f, 15, 1.7f);
            income = new Upgrade("Income", 2f, 1f, 20, 1.8f);
            energy = new Upgrade("Energy", 150f, 25f, 15, 1.6f);
        }
    }
}
