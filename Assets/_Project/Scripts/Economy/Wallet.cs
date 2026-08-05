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

        /// <summary>Tries to spend money. Returns false (and changes nothing) if
        /// the player can't afford it, so callers can just check the bool.</summary>
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Balance < amount)
                return false;

            Balance -= amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
