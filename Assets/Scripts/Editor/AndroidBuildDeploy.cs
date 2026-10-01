#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurvivalShooter.Editor
{
    /// <summary>
    /// Automated Android Build and Deployment Pipeline.
    /// Configures all Android PlayerSettings (ARM64, IL2CPP, API 24+, Package ID),
    /// generates all procedural assets and prefabs, generates the AR scene,
    /// compiles the APK, and provides entry points for batchmode deployment.
    /// </summary>
    public static class AndroidBuildDeploy
    {
        public const string PackageIdentifier = "com.InemaLeslie.SurvivalShooter";
        public const string ApplicationName = "Survival shooter";
        public const string Company = "Inema Leslie";
        public const string ScenePath = "Assets/Scenes/MainARSurvivalShooter.unity";
        public const string ApkDirectory = "Builds/Android";
        public const string ApkPath = "Builds/Android/SurvivalShooter.apk";

        [MenuItem("Survival Shooter/Build Android APK")]
        public static void BuildApkMenu()
        {
            BuildAndroidApk();
        }

        public static void BuildAndroidApk()
        {
            Debug.Log("[AndroidBuildDeploy] Starting Android APK build pipeline...");

            // 1. Configure Android NDK / SDK
            ConfigureAndroidToolchain();

            // 2. Setup Complete Game Scene
            GameSceneSetupWindow.SetupCompleteProject();

            // 3. Switch active platform to Android if needed
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[AndroidBuildDeploy] Switching platform to Android...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // 4. Configure Android PlayerSettings
            ConfigurePlayerSettings();

            // 5. Ensure Build directory exists
            if (!Directory.Exists(ApkDirectory))
            {
                Directory.CreateDirectory(ApkDirectory);
            }

            // 6. Build the Player
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[AndroidBuildDeploy] Executing BuildPipeline.BuildPlayer -> {ApkPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidBuildDeploy] SUCCESS! APK generated at {ApkPath} (Size: {summary.totalSize / (1024 * 1024)} MB)");
            }
            else
            {
                Debug.LogError($"[AndroidBuildDeploy] BUILD FAILED: {summary.result}, Errors: {summary.totalErrors}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[AndroidBuildDeploy Error] {msg.content}");
                        }
                    }
                }
                throw new Exception($"Android Build Failed: {summary.result}");
            }
        }

        private static void ConfigureAndroidToolchain()
        {
            string ndkPath = "C:/Android/NDK/android-ndk-r27c";
            if (Directory.Exists(ndkPath))
            {
                EditorPrefs.SetString("AndroidNdkRootR27C", ndkPath);
                EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
                EditorPrefs.SetBool("NdkUseEmbedded", false);
                Debug.Log($"[AndroidBuildDeploy] Set NDK to {ndkPath}");
            }
            else
            {
                // Fallback to local appdata if present
                string localNdk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Android\Sdk\ndk\28.2.13676358");
                if (Directory.Exists(localNdk))
                {
                    EditorPrefs.SetString("AndroidNdkRoot", localNdk);
                    EditorPrefs.SetBool("NdkUseEmbedded", false);
                    Debug.Log($"[AndroidBuildDeploy] Set NDK to {localNdk}");
                }
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageIdentifier);
            PlayerSettings.productName = ApplicationName;
            PlayerSettings.companyName = Company;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Android API levels (ARCore with Vulkan requires at least API 29 / Android 10)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Scripting Backend & Architectures (ARCore requires 64-bit ARM64)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Use OpenGLES3 for rock-solid stability with ARCore in Built-in Render Pipeline
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            // Apply App Icon
            GameSceneSetupWindow.ApplyAppIcon();

            AssetDatabase.SaveAssets();
            Debug.Log("[AndroidBuildDeploy] PlayerSettings configured successfully for Android ARM64 IL2CPP!");
        }
    }
}
#endif
