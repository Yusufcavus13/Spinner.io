using System.Collections.Generic;
using SpinForward.Player;
using UnityEngine;

namespace SpinForward.Economy
{
    [System.Serializable]
    public class SkinData
    {
        public string skinName;
        public int cost;
        public SpinnerShape shape = SpinnerShape.Disc;
        [Tooltip("Flat damage added to every hit while this skin is equipped.")]
        public int bonusDamage = 0;
        public Color bodyColor = new Color(0.11f, 0.42f, 0.95f);
        public Color accentColor = new Color(0.2f, 0.95f, 1f);
        public Color pointerColor = new Color(1f, 0.3f, 0.35f);
    }

    /// <summary>
    /// Owns the shop's spinner skins (defined in code), remembers which are bought and
    /// which is equipped (PlayerPrefs), and recolors the procedural spinner to match.
    /// </summary>
    public class SkinManager : MonoBehaviour
    {
        public static SkinManager Instance { get; private set; }

        public List<SkinData> availableSkins = new List<SkinData>();
        public int CurrentSkinIndex { get; private set; }
        public event System.Action<int> OnSkinChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Always define skins in code so it's deterministic (ignores stale scene data).
            availableSkins.Clear();
            BuildDefaultSkins();

            CurrentSkinIndex = Mathf.Clamp(PlayerPrefs.GetInt("SelectedSkin", 0), 0, availableSkins.Count - 1);
            UnlockSkin(0); // first skin is always free/owned
        }

        private void Start() => ApplyToSpinner();

        private void BuildDefaultSkins()
        {
            availableSkins.Add(new SkinData { skinName = "Klasik",  cost = 0,    shape = SpinnerShape.Disc, bonusDamage = 0,  bodyColor = C(0.11f, 0.42f, 0.95f), accentColor = C(0.2f, 0.95f, 1f),  pointerColor = C(1f, 0.30f, 0.35f) });
            availableSkins.Add(new SkinData { skinName = "Testere",  cost = 150,  shape = SpinnerShape.Saw,  bonusDamage = 2,  bodyColor = C(0.85f, 0.86f, 0.9f),  accentColor = C(1f, 0.35f, 0.1f),  pointerColor = C(1f, 0.9f, 0.3f) });
            availableSkins.Add(new SkinData { skinName = "Yıldız",   cost = 400,  shape = SpinnerShape.Star, bonusDamage = 3,  bodyColor = C(0.10f, 0.72f, 0.38f), accentColor = C(0.6f, 1f, 0.55f),  pointerColor = C(1f, 1f, 0.7f) });
            availableSkins.Add(new SkinData { skinName = "Dişli",    cost = 900,  shape = SpinnerShape.Gear, bonusDamage = 5,  bodyColor = C(1f, 0.72f, 0.12f),    accentColor = C(1f, 0.96f, 0.65f), pointerColor = C(0.55f, 0.35f, 0f) });
            availableSkins.Add(new SkinData { skinName = "Ametist",  cost = 2000, shape = SpinnerShape.Star, bonusDamage = 8,  bodyColor = C(0.52f, 0.20f, 0.85f), accentColor = C(0.85f, 0.6f, 1f),  pointerColor = C(1f, 0.9f, 1f) });
            availableSkins.Add(new SkinData { skinName = "Gölge",    cost = 5000, shape = SpinnerShape.Saw,  bonusDamage = 14, bodyColor = C(0.12f, 0.12f, 0.16f), accentColor = C(0.95f, 0.1f, 0.2f), pointerColor = C(1f, 0.35f, 0.4f) });
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b);

        public bool IsSkinUnlocked(int index)
        {
            if (index == 0) return true;
            return PlayerPrefs.GetInt($"SkinUnlocked_{index}", 0) == 1;
        }

        public void UnlockSkin(int index)
        {
            PlayerPrefs.SetInt($"SkinUnlocked_{index}", 1);
            PlayerPrefs.Save();
        }

        public void EquipSkin(int index)
        {
            if (index < 0 || index >= availableSkins.Count || !IsSkinUnlocked(index))
                return;

            CurrentSkinIndex = index;
            PlayerPrefs.SetInt("SelectedSkin", index);
            PlayerPrefs.Save();

            ApplyToSpinner();
            OnSkinChanged?.Invoke(index);
        }

        public SkinData GetCurrentSkin()
        {
            if (availableSkins.Count == 0)
                return null;
            return availableSkins[Mathf.Clamp(CurrentSkinIndex, 0, availableSkins.Count - 1)];
        }

        // Flat damage the equipped skin adds to every hit.
        public int CurrentBonusDamage
        {
            get { SkinData s = GetCurrentSkin(); return s != null ? s.bonusDamage : 0; }
        }

        // Recolors the procedural spinner to the equipped skin.
        public void ApplyToSpinner()
        {
            SkinData skin = GetCurrentSkin();
            if (skin == null)
                return;

            ProceduralSpinner spinner = FindFirstObjectByType<ProceduralSpinner>();
            if (spinner != null)
                spinner.ApplySkin(skin.shape, skin.bodyColor, skin.accentColor, skin.pointerColor);

            // Re-fit the collider to the new shape (saw teeth etc. change the footprint).
            if (SpinnerController.Instance != null)
                SpinnerController.Instance.MatchColliderToVisual();
        }
    }
}
