#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;
using SurvivalShooter.Pooling;
using SurvivalShooter.Enemies;
using SurvivalShooter.Player;
using SurvivalShooter.AR;
using SurvivalShooter.Leaderboard;
using SurvivalShooter.UI;
using SurvivalShooter.Utils;

namespace SurvivalShooter.Editor
{
    /// <summary>
    /// Unity Editor Tool to automate full scene generation, audio generation,
    /// prefab creation, and component wiring for the AR Survival Shooter game.
    /// Accessible via top menu: [Survival Shooter -> Setup Complete Game Scene].
    /// </summary>
    public class GameSceneSetupWindow : EditorWindow
    {
        [MenuItem("Survival Shooter/Setup Complete Game Scene")]
        public static void ShowWindow()
        {
            SetupCompleteProject();
        }

        [MenuItem("Survival Shooter/Apply App Icon and Game Name")]
        public static void ApplyAppIcon()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/AppIcon.png");
            if (icon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { icon });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new Texture2D[] { icon });
                PlayerSettings.productName = "Survival shooter";
                AssetDatabase.SaveAssets();
                Debug.Log("[SurvivalShooter] Game Icon set to user's picture and Product Name updated to 'Survival shooter'!");
            }
        }

        [MenuItem("Survival Shooter/Generate Audio Assets (.WAV)")]
        public static void GenerateAudioAssets()
        {
            string audioDir = "Assets/Audio";
            if (!Directory.Exists(audioDir)) Directory.CreateDirectory(audioDir);

            SaveWav("PlayerShoot.wav", ProceduralAudioGenerator.CreatePlayerShootClip());
            SaveWav("PlayerDeath.wav", ProceduralAudioGenerator.CreatePlayerDeathClip());
            SaveWav("EnemySpawn.wav", ProceduralAudioGenerator.CreateEnemySpawnClip());
            SaveWav("EnemyShoot.wav", ProceduralAudioGenerator.CreateEnemyShootClip());
            SaveWav("EnemyDamage.wav", ProceduralAudioGenerator.CreateEnemyDamageClip());
            SaveWav("EnemyDeath.wav", ProceduralAudioGenerator.CreateEnemyDeathClip());
            SaveWav("UIClick.wav", ProceduralAudioGenerator.CreateUIClickClip());
            SaveWav("GameOverDefeat.wav", ProceduralAudioGenerator.CreateGameOverClip(false));
            SaveWav("GameOverVictory.wav", ProceduralAudioGenerator.CreateGameOverClip(true));

            AssetDatabase.Refresh();
            Debug.Log("[SurvivalShooter] All 9 procedural WAV audio clips generated in Assets/Audio/!");
        }

        private static void SaveWav(string filename, AudioClip clip)
        {
            string path = Path.Combine("Assets/Audio", filename);
            byte[] bytes = ProceduralAudioGenerator.EncodeToWav(clip);
            File.WriteAllBytes(path, bytes);
        }

        [MenuItem("Survival Shooter/Generate All Prefabs")]
        public static void GeneratePrefabs()
        {
            string prefabDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // 1. Melee Enemy Prefab
            GameObject melee = ProceduralModelBuilder.BuildMeleeEnemyPrefab();
            PrefabUtility.SaveAsPrefabAsset(melee, "Assets/Prefabs/MeleeEnemy.prefab");
            DestroyImmediate(melee);

            // 2. Shooter Enemy Prefab
            GameObject shooter = ProceduralModelBuilder.BuildShooterEnemyPrefab();
            PrefabUtility.SaveAsPrefabAsset(shooter, "Assets/Prefabs/ShooterEnemy.prefab");
            DestroyImmediate(shooter);

            // 3. Player Projectile Prefab
            GameObject pBullet = ProceduralModelBuilder.BuildProjectilePrefab(true);
            PrefabUtility.SaveAsPrefabAsset(pBullet, "Assets/Prefabs/PlayerBullet.prefab");
            DestroyImmediate(pBullet);

            // 4. Enemy Projectile Prefab
            GameObject eBullet = ProceduralModelBuilder.BuildProjectilePrefab(false);
            PrefabUtility.SaveAsPrefabAsset(eBullet, "Assets/Prefabs/EnemyBullet.prefab");
            DestroyImmediate(eBullet);

            // 5. Custom Plane Tracker Prefab (Inema Amanda Leslie)
            GameObject tracker = ProceduralModelBuilder.BuildCustomPlaneTrackerPrefab("Inema Amanda Leslie");
            PrefabUtility.SaveAsPrefabAsset(tracker, "Assets/Prefabs/CustomPlaneTracker.prefab");
            DestroyImmediate(tracker);

            // 6. Hit Spark Effect Prefab
            GameObject hitSpark = new GameObject("HitEffect");
            var ps = hitSpark.AddComponent<ParticleSystem>();
            hitSpark.GetComponent<ParticleSystemRenderer>().sharedMaterial = ProceduralModelBuilder.CreateMaterial(new Color(1f, 0.9f, 0.3f), new Color(1f, 0.9f, 0.3f) * 2f);
            var pooledEff = hitSpark.AddComponent<PooledEffect>();
            pooledEff.SetTag(ObjectPoolManager.TAG_HIT_EFFECT);
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 4f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.9f, 0.3f);
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });
            PrefabUtility.SaveAsPrefabAsset(hitSpark, "Assets/Prefabs/HitEffect.prefab");
            DestroyImmediate(hitSpark);

            AssetDatabase.Refresh();
            Debug.Log("[SurvivalShooter] All game prefabs generated in Assets/Prefabs/!");
        }

        public static void SetupCompleteProject()
        {
            GenerateAudioAssets();
            GeneratePrefabs();
            ApplyAppIcon();

            string scenePath = "Assets/Scenes/MainARSurvivalShooter.unity";
            string sceneDir = "Assets/Scenes";
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            GameObject lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. AR Rig / Camera Setup
            GameObject arSessionGo = new GameObject("AR Session");
            arSessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
            arSessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARInputManager>();

            GameObject xrOriginGo = new GameObject("XR Origin");
            var xrOrigin = xrOriginGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            var arRaycastMgr = xrOriginGo.AddComponent<UnityEngine.XR.ARFoundation.ARRaycastManager>();
            var arPlaneMgr = xrOriginGo.AddComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>();

            GameObject cameraOffsetGo = new GameObject("Camera Offset");
            cameraOffsetGo.transform.SetParent(xrOriginGo.transform, false);
            xrOrigin.CameraFloorOffsetObject = cameraOffsetGo;

            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(cameraOffsetGo.transform, false);
            cameraGo.transform.localPosition = new Vector3(0, 1.2f, 0);
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cameraGo.AddComponent<AudioListener>();

            // AR Camera Components for mobile video passthrough and motion tracking
            cameraGo.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            cameraGo.AddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();
            var poseDriver = cameraGo.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            poseDriver.SetPoseSource(UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice, UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.ColorCamera);

            // Connect camera to XROrigin
            xrOrigin.Camera = cam;

            // Player Components on Camera (FPS perspective)
            var pHealth = cameraGo.AddComponent<PlayerHealth>();
            var pShooter = cameraGo.AddComponent<PlayerShooter>();

            // 3. Audio Manager
            GameObject audioMgrGo = new GameObject("AudioManager");
            var audioMgr = audioMgrGo.AddComponent<AudioManager>();
            audioMgr.SetClips(
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/PlayerShoot.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/PlayerDeath.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/EnemySpawn.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/EnemyShoot.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/EnemyDamage.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/EnemyDeath.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/UIClick.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/GameOverDefeat.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/GameOverVictory.wav")
            );

            // 4. Object Pool Manager (Mandatory)
            GameObject poolMgrGo = new GameObject("ObjectPoolManager");
            var poolMgr = poolMgrGo.AddComponent<ObjectPoolManager>();
            var playerBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");
            var enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
            var hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HitEffect.prefab");

            if (playerBulletPrefab != null) poolMgr.RegisterPool(ObjectPoolManager.TAG_PLAYER_PROJECTILE, playerBulletPrefab, 30, true);
            if (enemyBulletPrefab != null) poolMgr.RegisterPool(ObjectPoolManager.TAG_ENEMY_PROJECTILE, enemyBulletPrefab, 30, true);
            if (hitEffectPrefab != null) poolMgr.RegisterPool(ObjectPoolManager.TAG_HIT_EFFECT, hitEffectPrefab, 20, true);

            // 5. Enemy Spawner
            GameObject spawnerGo = new GameObject("EnemySpawner");
            var spawner = spawnerGo.AddComponent<EnemySpawner>();
            var meleePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeleeEnemy.prefab");
            var shooterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ShooterEnemy.prefab");

            var meleeField = typeof(EnemySpawner).GetField("meleePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            meleeField?.SetValue(spawner, meleePrefab);
            var shooterField = typeof(EnemySpawner).GetField("shooterPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shooterField?.SetValue(spawner, shooterPrefab);

            // 6. AR Placement Manager & Custom Plane Tracker
            GameObject arPlacementGo = new GameObject("ARPlacementManager");
            var arPlacement = arPlacementGo.AddComponent<ARPlacementManager>();
            var planeTrackerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CustomPlaneTracker.prefab");
            var trackerField = typeof(ARPlacementManager).GetField("placementReticlePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            trackerField?.SetValue(arPlacement, planeTrackerPrefab);
            var raycastField = typeof(ARPlacementManager).GetField("arRaycastManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            raycastField?.SetValue(arPlacement, arRaycastMgr);
            var planeField = typeof(ARPlacementManager).GetField("arPlaneManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            planeField?.SetValue(arPlacement, arPlaneMgr);

            // 7. Leaderboard Manager
            GameObject leaderboardMgrGo = new GameObject("LeaderboardManager");
            leaderboardMgrGo.AddComponent<LeaderboardManager>();

            // 8. Core Game Manager
            GameObject gameMgrGo = new GameObject("GameManager");
            var gameMgr = gameMgrGo.AddComponent<GameManager>();
            var spawnerProp = typeof(GameManager).GetField("enemySpawner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spawnerProp?.SetValue(gameMgr, spawner);
            var healthProp = typeof(GameManager).GetField("playerHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthProp?.SetValue(gameMgr, pHealth);
            var shooterProp = typeof(GameManager).GetField("playerShooter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shooterProp?.SetValue(gameMgr, pShooter);

            // 9. Full UI Canvas Setup
            SetupCanvasUI();

            // 10. Editor Simulation Environment (prevents black screen on PC)
            GameObject simGo = new GameObject("EditorSimulationEnvironment");
            simGo.AddComponent<EditorSimulationEnvironment>();

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(scenePath, true)
            };

            Debug.Log($"[SurvivalShooter] Scene '{scenePath}' created and configured successfully!");
        }

        private static void SetupCanvasUI()
        {
            GameObject canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Master UI Manager & Leaderboard UI
            var uiMgr = canvasGo.AddComponent<UIManager>();
            var lbUI = canvasGo.AddComponent<LeaderboardUI>();

            // Build full panel hierarchy and bind serialized fields
            UIBuilder.Build(canvasGo, uiMgr, lbUI);
        }

        private static void AddOptionalComponent(GameObject target, string typeQualifiedName)
        {
            var type = System.Type.GetType(typeQualifiedName);
            if (type != null)
            {
                target.AddComponent(type);
            }
        }
    }
}
#endif
