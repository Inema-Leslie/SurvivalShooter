using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SurvivalShooter.Core;

namespace SurvivalShooter.Leaderboard
{
    [Serializable]
    public class SessionData
    {
        public int score;
        public int enemiesDefeated;
        public float timeSurvived;
        public string difficulty;
        public bool isVictory;
        public string timestamp;
    }

    [Serializable]
    public class LeaderboardContainer
    {
        public List<SessionData> sessions = new List<SessionData>();
    }

    /// <summary>
    /// Singleton Local Leaderboard Manager.
    /// Persists session scores locally using JSON and PlayerPrefs.
    /// Strict requirement: Shows only the latest 5 sessions.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private const string PREFS_KEY = "SurvivalShooter_Leaderboard";
        private const int MAX_DISPLAYED_SESSIONS = 5;

        private LeaderboardContainer container = new LeaderboardContainer();
        private string filePath;

        public event Action OnLeaderboardUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            filePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
            LoadLeaderboard();
        }

        public void SaveSession(int score, int enemiesDefeated, float timeSurvived, DifficultyLevel difficulty, bool isVictory)
        {
            var session = new SessionData
            {
                score = score,
                enemiesDefeated = enemiesDefeated,
                timeSurvived = timeSurvived,
                difficulty = difficulty.ToString(),
                isVictory = isVictory,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            // Insert at beginning so index 0 is most recent
            container.sessions.Insert(0, session);

            // Cap stored sessions to prevent unbounded growth while keeping latest 5
            if (container.sessions.Count > 20)
            {
                container.sessions.RemoveRange(20, container.sessions.Count - 20);
            }

            SaveToDisk();
            OnLeaderboardUpdated?.Invoke();
        }

        /// <summary>
        /// Retrieves strictly the latest 5 sessions as mandated by requirements.
        /// </summary>
        public List<SessionData> GetLatestSessions()
        {
            int count = Mathf.Min(container.sessions.Count, MAX_DISPLAYED_SESSIONS);
            return container.sessions.GetRange(0, count);
        }

        private void SaveToDisk()
        {
            try
            {
                string json = JsonUtility.ToJson(container, true);
                File.WriteAllText(filePath, json);
                PlayerPrefs.SetString(PREFS_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeaderboardManager] Failed to save leaderboard: {ex.Message}");
            }
        }

        private void LoadLeaderboard()
        {
            try
            {
                string json = string.Empty;

                if (File.Exists(filePath))
                {
                    json = File.ReadAllText(filePath);
                }
                else if (PlayerPrefs.HasKey(PREFS_KEY))
                {
                    json = PlayerPrefs.GetString(PREFS_KEY);
                }

                if (!string.IsNullOrEmpty(json))
                {
                    container = JsonUtility.FromJson<LeaderboardContainer>(json);
                }
                else
                {
                    container = new LeaderboardContainer();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LeaderboardManager] Could not parse leaderboard: {ex.Message}. Starting fresh.");
                container = new LeaderboardContainer();
            }
        }

        public void ClearLeaderboard()
        {
            container.sessions.Clear();
            SaveToDisk();
            OnLeaderboardUpdated?.Invoke();
        }
    }
}
