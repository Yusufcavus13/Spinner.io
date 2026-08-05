using UnityEngine;

namespace SpinForward.Economy
{
    public enum UpgradeKind { Rotate, Power, Income }

    /// <summary>
    /// One upgradable stat (Rotate / Power / Income). Holds its level and the rules
    /// for how its value and price grow. Not a MonoBehaviour - it lives inside
    /// <see cref="UpgradeSystem"/> as serialized data, editable in the Inspector.
    /// </summary>
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

        /// <summary>Fires whenever this upgrade is purchased (level changed).</summary>
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

        /// <summary>The stat's current effect value, e.g. spin speed or a multiplier.</summary>
        public float Value => baseValue + valuePerLevel * level;

        /// <summary>What the NEXT level-up costs. Grows exponentially with level.</summary>
        public int Cost => Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level));

        /// <summary>Spends from the wallet and levels up if affordable.</summary>
        public bool TryPurchase(Wallet wallet)
        {
            if (wallet == null || !wallet.TrySpend(Cost))
                return false;

            level++;
            Changed?.Invoke();
            return true;
        }
    }
}
