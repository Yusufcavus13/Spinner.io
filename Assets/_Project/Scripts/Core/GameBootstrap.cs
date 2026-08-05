using UnityEngine;

namespace SpinForward.Core
{
    /// <summary>
    /// Runtime defaults applied once before the first scene loads.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
