using UnityEngine;

namespace SpinForward.Level
{
    /// <summary>
    /// A single level's design, stored as a reusable asset in the Project (not on
    /// a GameObject). Create one via Assets > Create > Spin Forward > Level, then
    /// drop it into the LevelManager's Levels list. Designers can tune levels
    /// without touching code or the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_", menuName = "Spin Forward/Level")]
    public class LevelData : ScriptableObject
    {
        [Min(1)] public int columns = 5;
        [Min(1)] public int rows = 5;
        [Tooltip("Seconds the player has to clear this level.")]
        [Min(1f)] public float attemptDuration = 20f;
    }
}
