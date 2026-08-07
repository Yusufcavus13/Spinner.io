using UnityEngine;

namespace SpinForward.Economy
{
    
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        [SerializeField] private Wallet wallet;
        [SerializeField] private Upgrade rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
        [SerializeField] private Upgrade power = new Upgrade("Power", 1f, 0.5f, 15, 1.7f);
        [SerializeField] private Upgrade income = new Upgrade("Income", 1f, 1f, 20, 1.8f);
        [SerializeField] private Upgrade energy = new Upgrade("Energy", 100f, 20f, 15, 1.6f);

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

        private void Reset()
        {
            rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
            power = new Upgrade("Power", 1f, 0.5f, 15, 1.7f);
            income = new Upgrade("Income", 1f, 1f, 20, 1.8f);
            energy = new Upgrade("Energy", 100f, 20f, 15, 1.6f);
        }
    }
}
