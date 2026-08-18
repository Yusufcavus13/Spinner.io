using UnityEngine;

namespace SpinForward.Level
{
    
    public enum GridShape { Square, Circle, Triangle, Diamond }
    
    [CreateAssetMenu(fileName = "Level_", menuName = "Spin Forward/Level")]
    public class LevelData : ScriptableObject
    {
        [Header("Pixel Art Data")]
        [Tooltip("The 2D sprite to convert into 3D voxels. Must have Read/Write Enabled in Import Settings!")]
        public Texture2D levelSprite;

        [Tooltip("Sprite is sampled down to at most this many cubes PER SIDE. Lower = fewer cubes = way faster. 96 is ~9000 cubes (heavy!), 40 is ~1600.")]
        [Range(8, 96)] public int maxResolution = 40;

        [Tooltip("Snap sprite colors to this many steps per channel: fewer distinct colors = fewer materials = fewer draw calls. 6 = up to 216 colors.")]
        [Range(2, 16)] public int colorSteps = 6;
        
        [Header("Grid Size & Shape (Fallback if no sprite)")]
        [Min(1)] public int columns = 5;
        [Min(1)] public int rows = 5;
        public GridShape shape = GridShape.Square;
        
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
        
        [Tooltip("Chance (0 to 1) for a cube to be a slowing Ice Cube.")]
        [Range(0f, 1f)] public float iceCubeChance = 0f;
        
        [Tooltip("Chance (0 to 1) for a cube to be a 2-hit Shield Cube.")]
        [Range(0f, 1f)] public float shieldCubeChance = 0f;
        
        [Tooltip("Chance (0 to 1) for a cube to be a Split/Clone Powerup Cube.")]
        [Range(0f, 1f)] public float splitCubeChance = 0f;
        
        [Tooltip("Chance (0 to 1) for a cube to be a Frenzy Powerup Cube.")]
        [Range(0f, 1f)] public float frenzyCubeChance = 0f;

        [Tooltip("Chance (0 to 1) for a cube to be a glowing Drain (energy trap) cube.")]
        [Range(0f, 1f)] public float drainCubeChance = 0.03f;

        [Tooltip("Chance (0 to 1) for a cube to be a pulsing Time-Bomb cube (detonates if not cleared in time).")]
        [Range(0f, 1f)] public float timeBombCubeChance = 0.02f;

        [Tooltip("Chance (0 to 1) for a cube to be a Laser Cube.")]
        [Range(0f, 1f)] public float laserCubeChance = 0f;

        [Tooltip("Chance (0 to 1) for a cube to be a Gold (Bonus) Cube.")]
        [Range(0f, 1f)] public float goldCubeChance = 0f;
        
        [Header("Environmental Hazards")]
        [Tooltip("Number of Vortex (Black Hole) hazards to spawn in this level.")]
        [Min(0)] public int vortexCount = 0;
        
        [Header("Dynamic Movement")]
        [Tooltip("If true, the entire wall will move left and right.")]
        public bool isMoving = false;
        
        [Tooltip("If true, even rows move right and odd rows move left. Creates a shearing effect!")]
        public bool alternateRowMovement = false;
        
        [Tooltip("How fast the wall moves left and right.")]
        [Min(0f)] public float moveSpeed = 2f;
        
        [Tooltip("How far the wall moves from its center.")]
        [Min(0f)] public float moveDistance = 3f;
        
        [Header("Repulsive & Breathing Options")]
        [Tooltip("If true, the wall will scale up and down (breathe).")]
        public bool isBreathing = false;
        
        [Tooltip("Force applied to push the player away when breathing out.")]
        public float breathingPushForce = 15f;
    }
}
