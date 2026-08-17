using UnityEngine;

namespace SpinForward.Economy
{
    public enum UpgradeKind { Rotate, Power, Income, Energy }
    
    [System.Serializable]
    public class Upgrade
    {
        [SerializeField] private string title = "Upgrade";
        [Tooltip("Value at level 0.")]
        [SerializeField] private float baseValue = 1f;
        [Tooltip("How much the value grows per level.")]
        [SerializeField] private float valuePerLevel = 1f;
        [Tooltip("Price of the first level-up.")]
        [SerializeField] private int baseCost = 10;
        [Tooltip("Cost is multiplied by this each level (1.6 = +60% each time).")]
        [SerializeField] private float costGrowth = 1.6f;
        [SerializeField] private int level = 0;

        public event System.Action Changed;

        public Upgrade() { }

        public Upgrade(string title, float baseValue, float valuePerLevel, int baseCost, float costGrowth)
        {
            this.title = title;
            this.baseValue = baseValue;
            this.valuePerLevel = valuePerLevel;
            this.baseCost = baseCost;
            this.costGrowth = costGrowth;
        }

        public string Title => title;
        public int Level => level;

        public float Value => baseValue + valuePerLevel * level;

        public int Cost => Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level));

        public bool TryPurchase(Wallet wallet)
        {
            if (wallet == null || !wallet.TrySpend(Cost))
                return false;

            level++;
            Changed?.Invoke();
            return true;
        }

        // Drops the level (e.g. as a retry penalty); also brings the Cost back down.
        public void ReduceLevel(int amount)
        {
            if (amount <= 0)
                return;
            level = Mathf.Max(0, level - amount);
            Changed?.Invoke();
        }
    }
}
