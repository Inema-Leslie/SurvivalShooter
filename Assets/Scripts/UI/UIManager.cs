using System;
using UnityEngine;
using UnityEngine.UI;
using SurvivalShooter.Core;
using SurvivalShooter.Player;
using SurvivalShooter.Audio;

namespace SurvivalShooter.UI
{
    /// <summary>
    /// Master UI Manager implementing the Observer pattern.
    /// Manages state-based view transitions, in-game HUD telemetry,
    /// damage vignetting, difficulty selection, and end-game summaries.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject placementGuidePanel;
        [SerializeField] private GameObject inGameHUDPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject leaderboardPanel;

        [Header("Start Menu Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button normalDiffButton;
        [SerializeField] private Button hardDiffButton;
        [SerializeField] private Button nightmareDiffButton;
        [SerializeField] private Text selectedDiffText;

        [Header("In-Game HUD Elements")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text difficultyBadgeText;
        [SerializeField] private Button onScreenShootButton;
        [SerializeField] private Image damageFlashImage;

        [Header("End Game Summary Elements")]
        [SerializeField] private Text gameOverTitleText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text enemiesDefeatedText;
        [SerializeField] private Text timeSurvivedText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button returnMainMenuButton;

        [Header("Placement Guide Elements")]
        [SerializeField] private Text placementInstructionText;

        [Header("Damage Vignette Effect")]
        [SerializeField] private float flashSpeed = 5f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.45f);

        private bool isTakingDamage = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            UIBuilder.Build(gameObject, this, GetComponent<LeaderboardUI>());
        }

        public void BindButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(() => { PlayClick(); GameManager.Instance.StartPlaneScanning(); });
            }

            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.RemoveAllListeners();
                leaderboardButton.onClick.AddListener(() => { PlayClick(); GameManager.Instance.ShowLeaderboard(); });
            }

            if (normalDiffButton != null)
            {
                normalDiffButton.onClick.RemoveAllListeners();
                normalDiffButton.onClick.AddListener(() => { PlayClick(); SelectDifficulty(DifficultyLevel.Normal); });
            }

            if (hardDiffButton != null)
            {
                hardDiffButton.onClick.RemoveAllListeners();
                hardDiffButton.onClick.AddListener(() => { PlayClick(); SelectDifficulty(DifficultyLevel.Hard); });
            }

            if (nightmareDiffButton != null)
            {
                nightmareDiffButton.onClick.RemoveAllListeners();
                nightmareDiffButton.onClick.AddListener(() => { PlayClick(); SelectDifficulty(DifficultyLevel.Nightmare); });
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => { PlayClick(); GameManager.Instance.RestartGame(); });
            }

            if (returnMainMenuButton != null)
            {
                returnMainMenuButton.onClick.RemoveAllListeners();
                returnMainMenuButton.onClick.AddListener(() => { PlayClick(); GameManager.Instance.ReturnToMainMenu(); });
            }

            if (onScreenShootButton != null)
            {
                onScreenShootButton.onClick.RemoveAllListeners();
                var shooter = FindAnyObjectByType<PlayerShooter>();
                if (shooter != null)
                {
                    onScreenShootButton.onClick.AddListener(() => shooter.TriggerShootFromUI());
                }
            }
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            GameManager.OnScoreChanged += UpdateScoreDisplay;
            GameManager.OnTimerTick += UpdateTimerDisplay;
            PlayerHealth.OnHealthChanged += UpdateHealthDisplay;
            PlayerHealth.OnPlayerDamaged += TriggerDamageFlash;
            GameManager.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.OnScoreChanged -= UpdateScoreDisplay;
            GameManager.OnTimerTick -= UpdateTimerDisplay;
            PlayerHealth.OnHealthChanged -= UpdateHealthDisplay;
            PlayerHealth.OnPlayerDamaged -= TriggerDamageFlash;
            GameManager.OnGameOver -= HandleGameOver;
        }

        private void Start()
        {
            SelectDifficulty(DifficultyLevel.Normal);
            UpdateHealthDisplay(100f, 100f);
            UpdateScoreDisplay(0);
        }

        private void Update()
        {
            // Smoothly fade out red damage vignette
            if (damageFlashImage != null)
            {
                if (isTakingDamage)
                {
                    damageFlashImage.color = damageFlashColor;
                    isTakingDamage = false;
                }
                else
                {
                    damageFlashImage.color = Color.Lerp(damageFlashImage.color, Color.clear, flashSpeed * Time.deltaTime);
                }
            }
        }

        private void SelectDifficulty(DifficultyLevel difficulty)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetDifficulty(difficulty);
            }

            if (selectedDiffText != null)
            {
                selectedDiffText.text = $"MODE: {difficulty.ToString().ToUpper()}";
            }

            if (difficultyBadgeText != null)
            {
                difficultyBadgeText.text = difficulty.ToString().ToUpper();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case GameState.MainMenu:
                    if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                    break;

                case GameState.PlaneScanning:
                    if (placementGuidePanel != null)
                    {
                        placementGuidePanel.SetActive(true);
                        if (placementInstructionText != null)
                            placementInstructionText.text = "SLOWLY MOVE PHONE TO SCAN FLOOR FOR PLANES...";
                    }
                    break;

                case GameState.PlacementReady:
                    if (placementGuidePanel != null)
                    {
                        placementGuidePanel.SetActive(true);
                        if (placementInstructionText != null)
                            placementInstructionText.text = "PLANE DETECTED!\nTAP SCREEN TO START SURVIVAL COMBAT";
                    }
                    break;

                case GameState.Playing:
                    if (inGameHUDPanel != null) inGameHUDPanel.SetActive(true);
                    break;

                case GameState.GameOver:
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;

                case GameState.Leaderboard:
                    if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (placementGuidePanel != null) placementGuidePanel.SetActive(false);
            if (inGameHUDPanel != null) inGameHUDPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        }

        private void UpdateHealthDisplay(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"HP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score:N0}";
            }
        }

        private void UpdateTimerDisplay(float seconds)
        {
            if (timerText != null)
            {
                int mins = Mathf.FloorToInt(seconds / 60f);
                int secs = Mathf.FloorToInt(seconds % 60f);
                timerText.text = $"SURVIVE: {mins:00}:{secs:00}";
            }
        }

        private void TriggerDamageFlash()
        {
            isTakingDamage = true;
        }

        private void HandleGameOver(bool isVictory)
        {
            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = isVictory ? "SURVIVED! VICTORY" : "GAME OVER - DEFEATED";
                gameOverTitleText.color = isVictory ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.25f, 0.25f);
            }

            if (finalScoreText != null && GameManager.Instance != null)
            {
                finalScoreText.text = $"FINAL SCORE: {GameManager.Instance.CurrentScore:N0}";
            }

            if (enemiesDefeatedText != null && GameManager.Instance != null)
            {
                enemiesDefeatedText.text = $"ENEMIES DEFEATED: {GameManager.Instance.EnemiesDefeated}";
            }

            if (timeSurvivedText != null && GameManager.Instance != null)
            {
                int mins = Mathf.FloorToInt(GameManager.Instance.TimeSurvived / 60f);
                int secs = Mathf.FloorToInt(GameManager.Instance.TimeSurvived % 60f);
                timeSurvivedText.text = $"TIME SURVIVED: {mins:00}:{secs:00}";
            }
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
        }
    }
}
