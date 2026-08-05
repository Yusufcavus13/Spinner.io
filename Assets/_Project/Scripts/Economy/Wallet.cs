using UnityEngine;

namespace SpinForward.Economy
{
    public class Wallet : MonoBehaviour
    {
        public static Wallet Instance { get; private set; }

        public int Balance { get; private set; }

        public event System.Action<int> BalanceChanged;

        private void Awake()
        {
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
