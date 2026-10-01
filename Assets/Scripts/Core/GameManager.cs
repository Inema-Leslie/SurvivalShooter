using System;
using UnityEngine;
using SurvivalShooter.Enemies;
using SurvivalShooter.Pooling;
using SurvivalShooter.Audio;
using SurvivalShooter.Leaderboard;
using SurvivalShooter.Player;

namespace SurvivalShooter.Core
{
    /// <summary>
    /// Core Game Manager singleton.
    /// Controls overall game state, survival countdown timer, score accumulation,
    /// difficulty configurations, and coordinates subsystem lifecycles.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Subsystem References")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerShooter playerShooter;

        [Header("Survival Settings")]
        [SerializeField] private float normalRoundTime = 60f;
        [SerializeField] private float hardRoundTime = 75f;
        [SerializeField] private float nightmareRoundTime = 90f;

        [Header("Current Runtime State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

        private float timeRemaining;
        private float timeSurvived;
        private int currentScore;
        private int enemiesDefeated;
        private float roundDuration;

        public GameState CurrentState => currentState;
        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public float TimeRemaining => timeRemaining;
        public float TimeSurvived => timeSurvived;
        public int CurrentScore => currentScore;
        public int EnemiesDefeated => enemiesDefeated;
        public float RoundDuration => roundDuration;

        // Observer Pattern Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<int> OnScoreChanged;
        public static event Action<float> OnTimerTick;
        public static event Action<int> OnEnemiesDefeatedChanged;
        public static event Action<bool> OnGameOver; // true = victory, false = defeat

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ensure Editor Simulation Arena exists if testing on PC without AR hardware
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (FindAnyObjectByType<SurvivalShooter.Utils.EditorSimulationEnvironment>() == null)
                {
                    GameObject simGo = new GameObject("[EditorSimulationEnvironment]");
                    simGo.AddComponent<SurvivalShooter.Utils.EditorSimulationEnvironment>();
                }

                if (Camera.main != null && Camera.main.GetComponent<SurvivalShooter.Utils.EditorCameraFreelook>() == null)
                {
                    Camera.main.gameObject.AddComponent<SurvivalShooter.Utils.EditorCameraFreelook>();
                }
            }
#endif
        }

        private void Start()
        {
            SetState(GameState.MainMenu);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                timeRemaining -= Time.deltaTime;
                timeSurvived += Time.deltaTime;

                OnTimerTick?.Invoke(Mathf.Max(0f, timeRemaining));

                if (timeRemaining <= 0f)
                {
                    EndGame(true); // Survived round time -> Victory!
                }
            }
        }

        public void SetDifficulty(DifficultyLevel difficulty)
        {
            currentDifficulty = difficulty;
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Transition from Main Menu to scanning AR environment for horizontal planes.
        /// </summary>
        public void StartPlaneScanning()
        {
            SetState(GameState.PlaneScanning);
        }

        /// <summary>
        /// Called when an AR plane is detected and ready for tap placement.
        /// </summary>
        public void SetPlacementReady()
        {
            if (currentState == GameState.PlaneScanning)
            {
                SetState(GameState.PlacementReady);
            }
        }

        /// <summary>
        /// Triggered when player taps to place the game world / anchor on the detected plane.
        /// </summary>
        public void StartGame(Transform anchorTransform = null)
        {
            // Configure round duration based on difficulty
            switch (currentDifficulty)
            {
                case DifficultyLevel.Normal:
                    roundDuration = normalRoundTime;
                    break;
                case DifficultyLevel.Hard:
                    roundDuration = hardRoundTime;
                    break;
                case DifficultyLevel.Nightmare:
                    roundDuration = nightmareRoundTime;
                    break;
            }

            timeRemaining = roundDuration;
            timeSurvived = 0f;
            currentScore = 0;
            enemiesDefeated = 0;

            OnScoreChanged?.Invoke(currentScore);
            OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
            OnTimerTick?.Invoke(timeRemaining);

            // Reset player
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            // Start spawner
            if (enemySpawner != null)
            {
                if (anchorTransform != null) enemySpawner.SetAnchor(anchorTransform);
                enemySpawner.StartSpawning(currentDifficulty);
            }

            SetState(GameState.Playing);
        }

        public void AddScore(int amount)
        {
            if (currentState != GameState.Playing) return;
            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void RegisterEnemyDefeated()
        {
            if (currentState != GameState.Playing) return;
            enemiesDefeated++;
            OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
        }

        /// <summary>
        /// Concludes the game session, cleans up active objects, and saves to leaderboard.
        /// </summary>
        public void EndGame(bool isVictory)
        {
            if (currentState != GameState.Playing) return;

            SetState(GameState.GameOver);

            // Stop spawning and wipe all active enemies cleanly
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.WipeAllEnemies();
            }

            // Recycle all active pooled projectiles
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.RecycleAllActive();
            }

            // Save to Local Leaderboard (tracks latest 5 sessions)
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SaveSession(
                    currentScore,
                    enemiesDefeated,
                    timeSurvived,
                    currentDifficulty,
                    isVictory
                );
            }

            // Play audio feedback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver(isVictory);
            }

            OnGameOver?.Invoke(isVictory);
        }

        /// <summary>
        /// Restarts combat immediately on the current plane.
        /// </summary>
        public void RestartGame()
        {
            StartGame();
        }

        /// <summary>
        /// Returns to Main Menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.WipeAllEnemies();
            }

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.RecycleAllActive();
            }

            SetState(GameState.MainMenu);
        }

        public void ShowLeaderboard()
        {
            SetState(GameState.Leaderboard);
        }

        public void CloseLeaderboard()
        {
            SetState(GameState.MainMenu);
        }
    }
}
