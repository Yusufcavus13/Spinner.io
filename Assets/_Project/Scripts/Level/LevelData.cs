using UnityEngine;

namespace SpinForward.Level
{
    
    [CreateAssetMenu(fileName = "Level_", menuName = "Spin Forward/Level")]
    public class LevelData : ScriptableObject
    {
        [Min(1)] public int columns = 5;
        [Min(1)] public int rows = 5;
        [Tooltip("Seconds the player has to clear this level.")]
        [Min(1f)] public float attemptDuration = 20f;
    }
}
