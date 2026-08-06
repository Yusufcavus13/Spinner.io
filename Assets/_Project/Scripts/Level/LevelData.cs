using UnityEngine;

namespace SpinForward.Level
{
    
    [CreateAssetMenu(fileName = "Level_", menuName = "Spin Forward/Level")]
    public class LevelData : ScriptableObject
    {
        [Header("Grid Size")]
        [Min(1)] public int columns = 5;
        [Min(1)] public int rows = 5;
        
        [Header("Time")]
        [Tooltip("Seconds the player has to clear this level.")]
        [Min(1f)] public float attemptDuration = 20f;

        [Header("Difficulty & Obstacles")]
        [Tooltip("How many hits a normal cube takes to shatter.")]
        [Min(1)] public int cubeHealth = 1;
        
        [Tooltip("Chance (0 to 1) for a cube to be an explosive Bomb Cube.")]
        [Range(0f, 1f)] public float bombCubeChance = 0f;
        
        [Tooltip("Maximum number of bombs allowed in this level.")]
        public int maxBombs = 1;
        
        [Tooltip("Chance (0 to 1) for a cube to be an unbreakable Steel Cube.")]
        [Range(0f, 1f)] public float steelCubeChance = 0f;
    }
}
