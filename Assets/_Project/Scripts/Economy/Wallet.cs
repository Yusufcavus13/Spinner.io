using UnityEngine;

namespace SpinForward.Economy
{
    /// <summary>
    /// Single source of truth for the player's money. Anyone can read it through
    /// <see cref="Instance"/>, and UI listens to <see cref="BalanceChanged"/> so
    /// it never has to poll the value every frame.
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        public static Wallet Instance { get; private set; }

        public int Balance { get; private set; }

        /// <summary>Fires whenever the balance changes, carrying the new total.</summary>
        public event System.Action<int> BalanceChanged;

        private void Awake()
        {
            // Classic singleton guard: only one wallet may exist.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Add(int amount)
        {
            if (amount == 0)
                return;

            Balance += amount;
            BalanceChanged?.Invoke(Balance);
        }
    }
}
