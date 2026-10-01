using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SurvivalShooter.Leaderboard;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;

namespace SurvivalShooter.UI
{
    /// <summary>
    /// UI Controller for the Leaderboard Modal.
    /// Strictly displays only the latest 5 gameplay sessions stored in local persistence.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI Table Components")]
        [SerializeField] private Transform tableContainer;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private Text emptyStateText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button clearHistoryButton;

        private readonly List<GameObject> activeRows = new List<GameObject>();

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
                    if (GameManager.Instance != null) GameManager.Instance.CloseLeaderboard();
                });
            }

            if (clearHistoryButton != null)
            {
                clearHistoryButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
                    if (LeaderboardManager.Instance != null) LeaderboardManager.Instance.ClearLeaderboard();
                });
            }
        }

        private void OnEnable()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardUpdated += RefreshLeaderboardDisplay;
            }
            RefreshLeaderboardDisplay();
        }

        private void OnDisable()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardUpdated -= RefreshLeaderboardDisplay;
            }
        }

        public void RefreshLeaderboardDisplay()
        {
            // Clear existing rows
            foreach (var row in activeRows)
            {
                if (row != null) Destroy(row);
            }
            activeRows.Clear();

            if (LeaderboardManager.Instance == null) return;

            // Strict requirement: show latest 5 sessions
            List<SessionData> sessions = LeaderboardManager.Instance.GetLatestSessions();

            if (sessions == null || sessions.Count == 0)
            {
                if (emptyStateText != null) emptyStateText.gameObject.SetActive(true);
                return;
            }

            if (emptyStateText != null) emptyStateText.gameObject.SetActive(false);

            for (int i = 0; i < sessions.Count; i++)
            {
                var s = sessions[i];
                GameObject rowObj = null;

                if (rowPrefab != null && tableContainer != null)
                {
                    rowObj = Instantiate(rowPrefab, tableContainer);
                }
                else
                {
                    // Fallback runtime row generator
                    rowObj = CreateDefaultRow(tableContainer);
                }

                if (rowObj != null)
                {
                    Text rowText = rowObj.GetComponentInChildren<Text>();
                    if (rowText != null)
                    {
                        int mins = Mathf.FloorToInt(s.timeSurvived / 60f);
                        int secs = Mathf.FloorToInt(s.timeSurvived % 60f);
                        string outcome = s.isVictory ? "<color=#4AFF4A>WIN</color>" : "<color=#FF4A4A>LOSS</color>";
                        rowText.text = $"#{i + 1} | {s.timestamp} | {s.difficulty} | {outcome} | SCORE: {s.score:N0} | KILLS: {s.enemiesDefeated} | TIME: {mins:00}:{secs:00}";
                    }
                    activeRows.Add(rowObj);
                }
            }
        }

        private GameObject CreateDefaultRow(Transform parent)
        {
            GameObject row = new GameObject("LeaderboardRow");
            if (parent != null) row.transform.SetParent(parent, false);

            var rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 36);

            var text = row.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 15;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            return row;
        }
    }
}
