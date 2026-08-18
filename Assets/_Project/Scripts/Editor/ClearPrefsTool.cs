#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SpinForward.EditorScripts
{
    public class ClearPrefsTool
    {
        [MenuItem("Tools/Clear All Save Data (PlayerPrefs)")]
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All PlayerPrefs (Money, Unlocked Skins, Selected Skin) have been cleared!");
        }
    }
}
#endif
