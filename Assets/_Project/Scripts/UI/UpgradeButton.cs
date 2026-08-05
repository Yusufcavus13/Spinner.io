using SpinForward.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.UI
{
    /// <summary>
    /// A single shop button for one upgrade. Shows "Title Lv.N" and its price,
    /// buys on click, and greys out when the player can't afford the next level.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private UpgradeKind kind;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private Button button;

        private Upgrade upgrade;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void Start()
        {
            if (UpgradeSystem.Instance == null)
            {
                Debug.LogError("[UpgradeButton] No UpgradeSystem in scene.");
                return;
            }

            upgrade = UpgradeSystem.Instance.Get(kind);
            upgrade.Changed += Refresh;

            if (Wallet.Instance != null)
                Wallet.Instance.BalanceChanged += OnBalanceChanged;

            button.onClick.AddListener(OnClick);
            Refresh();
        }

        private void OnDestroy()
        {
            if (upgrade != null)
                upgrade.Changed -= Refresh;
            if (Wallet.Instance != null)
                Wallet.Instance.BalanceChanged -= OnBalanceChanged;
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            UpgradeSystem.Instance.Buy(kind);
        }

        // Wallet fires an int; we don't need it, we just re-check affordability.
        private void OnBalanceChanged(int _) => UpdateInteractable();

        private void Refresh()
        {
            if (titleLabel != null)
                titleLabel.text = $"{upgrade.Title} Lv.{upgrade.Level}";
            if (costLabel != null)
                costLabel.text = $"${upgrade.Cost}";
            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            if (button == null || upgrade == null)
                return;
            int balance = Wallet.Instance != null ? Wallet.Instance.Balance : 0;
            button.interactable = balance >= upgrade.Cost;
        }
    }
}
