using UnityEngine;
using UnityEngine.UI;
using SurvivalShooter.Audio;
using SurvivalShooter.Pooling;
using SurvivalShooter.Enemies;
using SurvivalShooter.Player;
using SurvivalShooter.AR;
using SurvivalShooter.Leaderboard;
using SurvivalShooter.UI;
using SurvivalShooter.Utils;

namespace SurvivalShooter.Core
{
    /// <summary>
    /// Runtime bootstrapper that self-configures any missing subsystems at startup.
    /// Guarantees that whether tested in Unity Editor or on a mobile AR device,
    /// all components, object pools, audio clips, and UI elements are fully operational.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class RuntimeGameBootstrapper : MonoBehaviour
    {
        [SerializeField] private string studentName = "Inema Amanda Leslie";

        private void Awake()
        {
            EnsureCoreSystems();
        }

        public void EnsureCoreSystems()
        {
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
#endif

            // 1. Audio Manager
            if (AudioManager.Instance == null)
            {
                GameObject audioGo = new GameObject("[AudioManager]");
                var audioMgr = audioGo.AddComponent<AudioManager>();

                // Synthesize procedural audio clips immediately
                audioMgr.SetClips(
                    ProceduralAudioGenerator.CreatePlayerShootClip(),
                    ProceduralAudioGenerator.CreatePlayerDeathClip(),
                    ProceduralAudioGenerator.CreateEnemySpawnClip(),
                    ProceduralAudioGenerator.CreateEnemyShootClip(),
                    ProceduralAudioGenerator.CreateEnemyDamageClip(),
                    ProceduralAudioGenerator.CreateEnemyDeathClip(),
                    ProceduralAudioGenerator.CreateUIClickClip(),
                    ProceduralAudioGenerator.CreateGameOverClip(false),
                    ProceduralAudioGenerator.CreateGameOverClip(true)
                );
            }

            // 2. Object Pool Manager (Mandatory)
            if (ObjectPoolManager.Instance == null)
            {
                GameObject poolGo = new GameObject("[ObjectPoolManager]");
                var poolMgr = poolGo.AddComponent<ObjectPoolManager>();

                // Register pools
                GameObject playerBullet = ProceduralModelBuilder.BuildProjectilePrefab(true);
                playerBullet.SetActive(false);
                poolMgr.RegisterPool(ObjectPoolManager.TAG_PLAYER_PROJECTILE, playerBullet, 35, true);

                GameObject enemyBullet = ProceduralModelBuilder.BuildProjectilePrefab(false);
                enemyBullet.SetActive(false);
                poolMgr.RegisterPool(ObjectPoolManager.TAG_ENEMY_PROJECTILE, enemyBullet, 35, true);

                // Hit effect pool
                GameObject hitEffect = new GameObject("HitEffect");
                var ps = hitEffect.AddComponent<ParticleSystem>();
                var pooledEff = hitEffect.AddComponent<PooledEffect>();
                pooledEff.SetTag(ObjectPoolManager.TAG_HIT_EFFECT);
                hitEffect.SetActive(false);
                poolMgr.RegisterPool(ObjectPoolManager.TAG_HIT_EFFECT, hitEffect, 25, true);

                // Death effect pool
                GameObject deathEffect = new GameObject("EnemyDeathEffect");
                var deathPs = deathEffect.AddComponent<ParticleSystem>();
                var deathEff = deathEffect.AddComponent<PooledEffect>();
                deathEff.SetTag(ObjectPoolManager.TAG_ENEMY_DEATH_EFFECT);
                deathEffect.SetActive(false);
                poolMgr.RegisterPool(ObjectPoolManager.TAG_ENEMY_DEATH_EFFECT, deathEffect, 20, true);
            }

            // 3. Leaderboard Manager
            if (LeaderboardManager.Instance == null)
            {
                GameObject lbGo = new GameObject("[LeaderboardManager]");
                lbGo.AddComponent<LeaderboardManager>();
            }

            // AR Session
            if (FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>() == null)
            {
                GameObject sessionGo = new GameObject("[ARSession]");
                sessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
                sessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARInputManager>();
            }

            // 4. Player Components (FPS perspective on Main Camera)
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0, 0, 0, 0);
                camGo.AddComponent<AudioListener>();
            }
            else
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0, 0, 0, 0);
            }

            if (mainCam.GetComponent<PlayerHealth>() == null) mainCam.gameObject.AddComponent<PlayerHealth>();
            if (mainCam.GetComponent<PlayerShooter>() == null) mainCam.gameObject.AddComponent<PlayerShooter>();

            if (mainCam.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>() == null)
            {
                mainCam.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            }
            if (mainCam.GetComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>() == null)
            {
                mainCam.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();
            }
            if (mainCam.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>() == null)
            {
                var tpd = mainCam.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                tpd.SetPoseSource(UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice, UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.ColorCamera);
            }

            // 5. Enemy Spawner
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
            if (spawner == null)
            {
                GameObject spawnerGo = new GameObject("[EnemySpawner]");
                spawner = spawnerGo.AddComponent<EnemySpawner>();

                GameObject meleePrefab = ProceduralModelBuilder.BuildMeleeEnemyPrefab();
                meleePrefab.SetActive(false);
                GameObject shooterPrefab = ProceduralModelBuilder.BuildShooterEnemyPrefab();
                shooterPrefab.SetActive(false);

                var meleeField = typeof(EnemySpawner).GetField("meleePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                meleeField?.SetValue(spawner, meleePrefab);

                var shooterField = typeof(EnemySpawner).GetField("shooterPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                shooterField?.SetValue(spawner, shooterPrefab);
            }

            // 6. AR Placement Manager & Custom Plane Tracker
            ARPlacementManager arManager = FindAnyObjectByType<ARPlacementManager>();
            if (arManager == null)
            {
                GameObject arGo = new GameObject("[ARPlacementManager]");
                arManager = arGo.AddComponent<ARPlacementManager>();

                GameObject customTracker = ProceduralModelBuilder.BuildCustomPlaneTrackerPrefab(studentName);
                customTracker.SetActive(false);

                var trackerField = typeof(ARPlacementManager).GetField("placementReticlePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                trackerField?.SetValue(arManager, customTracker);
            }

            // 7. Core GameManager
            if (GameManager.Instance == null)
            {
                GameObject gameMgrGo = new GameObject("[GameManager]");
                var gameMgr = gameMgrGo.AddComponent<GameManager>();

                var spawnerField = typeof(GameManager).GetField("enemySpawner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                spawnerField?.SetValue(gameMgr, spawner);

                var healthField = typeof(GameManager).GetField("playerHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                healthField?.SetValue(gameMgr, mainCam.GetComponent<PlayerHealth>());

                var shooterField = typeof(GameManager).GetField("playerShooter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                shooterField?.SetValue(gameMgr, mainCam.GetComponent<PlayerShooter>());
            }

            // 8. Runtime UI Canvas
            BuildRuntimeUI();
        }

        private void BuildRuntimeUI()
        {
            if (FindAnyObjectByType<UIManager>() != null) return;

            // Root Canvas
            GameObject canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var uiMgr = canvasGo.AddComponent<UIManager>();
            var lbUI = canvasGo.AddComponent<LeaderboardUI>();

            UIBuilder.Build(canvasGo, uiMgr, lbUI);
        }
    }
}
