using SpinForward.Economy;
using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// Swaps to a fancier spinner model as the Power upgrade climbs. Put your
    /// imported models as children of this object and drop them into Skins,
    /// plainest first. The one matching the current Power tier is shown; the rest
    /// are hidden. Event-driven, so it only swaps when Power actually changes.
    /// </summary>
    public class SpinnerSkins : MonoBehaviour
    {
        [Tooltip("Spinner models, plainest first. Each should be a child of this object so it spins with the visual.")]
        [SerializeField] private GameObject[] skins;
        [Tooltip("Power levels needed before advancing to the next skin.")]
        [SerializeField] private int powerLevelsPerSkin = 3;

        private int currentIndex = -1;

        private void Start()
        {
            if (UpgradeSystem.Instance != null)
                UpgradeSystem.Instance.Power.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (UpgradeSystem.Instance != null)
                UpgradeSystem.Instance.Power.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (skins == null || skins.Length == 0)
                return;

            int powerLevel = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.Power.Level : 0;
            int index = Mathf.Clamp(powerLevel / Mathf.Max(1, powerLevelsPerSkin), 0, skins.Length - 1);

            if (index == currentIndex)
                return;
            currentIndex = index;

            for (int i = 0; i < skins.Length; i++)
                if (skins[i] != null)
                    skins[i].SetActive(i == index);
        }
    }
}
