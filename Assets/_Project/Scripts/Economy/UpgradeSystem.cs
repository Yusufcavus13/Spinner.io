using UnityEngine;

namespace SpinForward.Economy
{
    /// <summary>
    /// Holds the three upgrades and is the single point other systems read from
    /// (spinner reads Rotate, cubes read Power, money reads Income). Singleton so
    /// they can reach it without wiring a reference into every object.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        [SerializeField] private Wallet wallet;
        [SerializeField] private Upgrade rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
        [SerializeField] private Upgrade power = new Upgrade("Power", 1f, 0.5f, 15, 1.7f);
        [SerializeField] private Upgrade income = new Upgrade("Income", 1f, 1f, 20, 1.8f);

        public Upgrade Rotate => rotate;
        public Upgrade Power => power;
        public Upgrade Income => income;

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
                default: return income;
            }
        }

        /// <summary>Buys a level of the given upgrade. Returns false if unaffordable.</summary>
        public bool Buy(UpgradeKind kind)
        {
            return Get(kind).TryPurchase(wallet);
        }

        // Sensible defaults when the component is first added in the editor.
        private void Reset()
        {
            rotate = new Upgrade("Rotate", 720f, 180f, 10, 1.6f);
            power = new Upgrade("Power", 1f, 0.5f, 15, 1.7f);
            income = new Upgrade("Income", 1f, 1f, 20, 1.8f);
        }
    }
}
