using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SpinForward.EditorTools
{
    /// <summary>
    /// Applies the baseline project settings for Spin Forward.
    /// Run from the menu so Unity itself writes ProjectSettings (editing those
    /// files by hand while the editor is open gets overwritten on save).
    /// </summary>
    public static class ProjectSetup
    {
        private const string CompanyName = "Yusuf Cavus";
        private const string ProductName = "Spin Forward";
        private const string BundleId = "com.yusufcavus.spinforward";

        [MenuItem("Tools/Spin Forward/Apply Mobile Settings")]
        public static void ApplyMobileSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            // Portrait only - camera looks down at the arena, character centered.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            // Hypercasual: no splash noise, no accidental portrait/landscape flips.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.useAnimatedAutorotation = false;

            AssetDatabase.SaveAssets();
            Debug.Log("[Spin Forward] Mobile settings applied (portrait, ARM64, IL2CPP, bundle id " + BundleId + ").");
        }

        [MenuItem("Tools/Spin Forward/Switch Platform To Android")]
        public static void SwitchToAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                Debug.Log("[Spin Forward] Already on Android.");
                return;
            }

            // Triggers a full reimport - can take a few minutes on first run.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
    }
}
