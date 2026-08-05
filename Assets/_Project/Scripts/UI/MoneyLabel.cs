using TMPro;
using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.UI
{

    [RequireComponent(typeof(TMP_Text))]
    public class MoneyLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private string prefix = "$";

        private void Awake()
        {
            if (label == null)
                label = GetComponent<TMP_Text>();
        }

        // Start (not Awake) so the Wallet's Awake has definitely run first.
        private void Start()
        {
            if (Wallet.Instance == null)
                return;

            Wallet.Instance.BalanceChanged += Refresh;
            Refresh(Wallet.Instance.Balance);
        }

        private void OnDestroy()
        {
            if (Wallet.Instance != null)
                Wallet.Instance.BalanceChanged -= Refresh;
        }

        private void Refresh(int balance)
        {
            label.text = prefix + balance;
        }
    }
}
