using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SurvivalShooter.Core;
using SurvivalShooter.Leaderboard;
using SurvivalShooter.Audio;

namespace SurvivalShooter.UI
{
    /// <summary>
    /// Constructs and wires the complete UI system.
    /// Strictly adheres to design specifications:
    /// - Main Menu panel background set to user's uploaded custom image.
    /// - Main Menu contains strictly: Game Title ("Survival shooter"), Start Button, and Leaderboard Button.
    /// - All buttons are guaranteed 100% clickable using targetGraphic, raycast filtering, and UIButtonFixer.
    /// </summary>
    public static class UIBuilder
    {
        public static void Build(GameObject canvasGo, UIManager uiMgr, LeaderboardUI lbUI)
        {
            // Clear previous UI elements to rebuild cleanly
            for (int i = canvasGo.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasGo.transform.GetChild(i).gameObject);
            }

            // 1. Ensure EventSystem exists and is properly configured
            EnsureEventSystem();

            // 2. Configure Canvas & Scaler
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = canvasGo.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite menuBgSprite = LoadMenuBackgroundSprite();

            // 3. Screen Damage Flash Vignette (raycastTarget = false to never block clicks)
            GameObject flashGo = CreateRect("DamageFlash", canvasGo.transform);
            StretchFull(flashGo.GetComponent<RectTransform>());
            var flashImg = flashGo.AddComponent<Image>();
            flashImg.color = Color.clear;
            flashImg.raycastTarget = false;

            // ==========================================
            // 4. MAIN MENU PANEL
            // STRICT USER REQUIREMENTS:
            // - Background: User uploaded picture
            // - Title: "Survival shooter"
            // - Start Button
            // - Leaderboard Button
            // ==========================================
            GameObject mainMenu = CreateRect("MainMenuPanel", canvasGo.transform);
            StretchFull(mainMenu.GetComponent<RectTransform>());
            var menuBg = mainMenu.AddComponent<Image>();
            if (menuBgSprite != null)
            {
                menuBg.sprite = menuBgSprite;
                menuBg.color = Color.white;
            }
            else
            {
                menuBg.color = new Color(0.08f, 0.1f, 0.16f, 0.95f);
            }
            menuBg.raycastTarget = false; // Do not block button clicks

            // Game Title: "Survival shooter"
            var title = CreateText("GameTitle", mainMenu.transform, "Survival shooter", defaultFont, 68, Color.white, new Vector2(0, 520), new Vector2(950, 140));
            title.fontStyle = FontStyle.Bold;
            var titleShadow = title.gameObject.AddComponent<Outline>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            titleShadow.effectDistance = new Vector2(3f, -3f);

            // Start Button
            var startBtn = CreateButton("StartButton", mainMenu.transform, "START", defaultFont, 38, new Vector2(0, -80), new Vector2(480, 110), new Color(0.12f, 0.72f, 0.95f));

            // Leaderboard Button
            var lbBtn = CreateButton("LeaderboardButton", mainMenu.transform, "LEADERBOARD", defaultFont, 32, new Vector2(0, -220), new Vector2(480, 95), new Color(0.18f, 0.42f, 0.65f));

            // ==========================================
            // 5. PLACEMENT GUIDE PANEL
            // ==========================================
            GameObject placementPanel = CreateRect("PlacementGuidePanel", canvasGo.transform);
            StretchFull(placementPanel.GetComponent<RectTransform>());
            var placeBg = placementPanel.AddComponent<Image>();
            placeBg.color = Color.clear;
            placeBg.raycastTarget = false;

            GameObject banner = CreateRect("TopBanner", placementPanel.transform);
            var bannerRect = banner.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 1);
            bannerRect.anchorMax = new Vector2(1, 1);
            bannerRect.pivot = new Vector2(0.5f, 1);
            bannerRect.sizeDelta = new Vector2(0, 180);
            bannerRect.anchoredPosition = Vector2.zero;
            var bannerImg = banner.AddComponent<Image>();
            bannerImg.color = new Color(0.04f, 0.07f, 0.14f, 0.88f);
            bannerImg.raycastTarget = false;

            var instructText = CreateText("InstructionText", banner.transform, "SLOWLY MOVE PHONE TO SCAN FLOOR FOR PLANES...", defaultFont, 32, Color.cyan, Vector2.zero, new Vector2(1000, 120));
            instructText.fontStyle = FontStyle.Bold;
            placementPanel.SetActive(false);

            // ==========================================
            // 6. IN-GAME COMBAT HUD
            // ==========================================
            GameObject inGameHud = CreateRect("InGameHUDPanel", canvasGo.transform);
            StretchFull(inGameHud.GetComponent<RectTransform>());

            GameObject statusBar = CreateRect("StatusBar", inGameHud.transform);
            var statusRect = statusBar.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 1);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(0.5f, 1);
            statusRect.sizeDelta = new Vector2(0, 200);
            statusRect.anchoredPosition = Vector2.zero;
            var statusImg = statusBar.AddComponent<Image>();
            statusImg.color = new Color(0.04f, 0.07f, 0.12f, 0.82f);
            statusImg.raycastTarget = false;

            var hpText = CreateText("HPText", statusBar.transform, "HP: 100/100", defaultFont, 34, new Color(0.2f, 1f, 0.4f), new Vector2(-320, 0), new Vector2(340, 70));
            hpText.fontStyle = FontStyle.Bold;

            var timerText = CreateText("TimerText", statusBar.transform, "SURVIVE: 01:00", defaultFont, 40, Color.yellow, new Vector2(0, 0), new Vector2(380, 80));
            timerText.fontStyle = FontStyle.Bold;

            var scoreText = CreateText("ScoreText", statusBar.transform, "SCORE: 0", defaultFont, 34, Color.cyan, new Vector2(320, 0), new Vector2(340, 70));
            scoreText.fontStyle = FontStyle.Bold;

            var diffBadge = CreateText("DiffBadge", statusBar.transform, "NORMAL MODE", defaultFont, 20, Color.gray, new Vector2(0, -60), new Vector2(300, 35));

            // Crosshair
            GameObject ch = CreateRect("Crosshair", inGameHud.transform);
            var chImg = ch.AddComponent<Image>();
            chImg.color = new Color(0.1f, 0.95f, 1f, 0.9f);
            chImg.raycastTarget = false;
            var chRect = ch.GetComponent<RectTransform>();
            chRect.sizeDelta = new Vector2(14, 14);
            chRect.anchoredPosition = Vector2.zero;

            GameObject chRing = CreateRect("CrosshairRing", ch.transform);
            var ringImg = chRing.AddComponent<Image>();
            ringImg.color = new Color(0.1f, 0.9f, 1f, 0.45f);
            ringImg.raycastTarget = false;
            chRing.GetComponent<RectTransform>().sizeDelta = new Vector2(56, 56);

            // Ergonomic Touch Fire Button
            var shootBtn = CreateButton("ShootButton", inGameHud.transform, "FIRE", defaultFont, 36, new Vector2(360, -640), new Vector2(220, 220), new Color(0.95f, 0.2f, 0.2f, 0.85f));
            inGameHud.SetActive(false);

            // ==========================================
            // 7. GAME OVER SUMMARY PANEL
            // ==========================================
            GameObject gameOverPanel = CreateRect("GameOverPanel", canvasGo.transform);
            StretchFull(gameOverPanel.GetComponent<RectTransform>());
            var goBg = gameOverPanel.AddComponent<Image>();
            goBg.color = new Color(0.04f, 0.05f, 0.08f, 0.96f);
            goBg.raycastTarget = true; // Blocks clicks through to scene

            var goTitle = CreateText("GameOverTitle", gameOverPanel.transform, "MISSION COMPLETE", defaultFont, 54, Color.green, new Vector2(0, 420), new Vector2(850, 110));
            goTitle.fontStyle = FontStyle.Bold;

            var finalScore = CreateText("FinalScore", gameOverPanel.transform, "FINAL SCORE: 0", defaultFont, 36, Color.white, new Vector2(0, 260), new Vector2(700, 65));
            var killsText = CreateText("KillsText", gameOverPanel.transform, "ENEMIES DEFEATED: 0", defaultFont, 30, Color.cyan, new Vector2(0, 170), new Vector2(700, 55));
            var surviveText = CreateText("SurviveText", gameOverPanel.transform, "TIME SURVIVED: 00:00", defaultFont, 30, Color.yellow, new Vector2(0, 80), new Vector2(700, 55));

            var restartBtn = CreateButton("RestartButton", gameOverPanel.transform, "PLAY AGAIN", defaultFont, 32, new Vector2(0, -90), new Vector2(480, 90), new Color(0.15f, 0.65f, 0.3f));
            var menuBtn = CreateButton("MainMenuButton", gameOverPanel.transform, "MAIN MENU", defaultFont, 28, new Vector2(0, -210), new Vector2(420, 80), new Color(0.3f, 0.35f, 0.45f));
            gameOverPanel.SetActive(false);

            // ==========================================
            // 8. LEADERBOARD MODAL PANEL
            // ==========================================
            GameObject lbPanel = CreateRect("LeaderboardPanel", canvasGo.transform);
            StretchFull(lbPanel.GetComponent<RectTransform>());
            var lbBg = lbPanel.AddComponent<Image>();
            lbBg.color = new Color(0.03f, 0.05f, 0.09f, 0.97f);
            lbBg.raycastTarget = true;

            var lbTitle = CreateText("LBTitle", lbPanel.transform, "LOCAL LEADERBOARD (LATEST 5 SESSIONS)", defaultFont, 36, Color.cyan, new Vector2(0, 480), new Vector2(950, 80));
            lbTitle.fontStyle = FontStyle.Bold;

            GameObject tableContainer = CreateRect("TableContainer", lbPanel.transform);
            var tableRect = tableContainer.GetComponent<RectTransform>();
            tableRect.sizeDelta = new Vector2(950, 600);
            tableRect.anchoredPosition = new Vector2(0, 80);

            var vlg = tableContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;

            var emptyText = CreateText("EmptyStateText", tableContainer.transform, "NO RECORDED SESSIONS YET.\nCOMPLETE A RUN TO TRACK SCORES!", defaultFont, 26, Color.gray, Vector2.zero, new Vector2(800, 100));

            var lbCloseBtn = CreateButton("LBCloseBtn", lbPanel.transform, "RETURN TO MENU", defaultFont, 28, new Vector2(0, -360), new Vector2(400, 75), new Color(0.2f, 0.45f, 0.7f));
            var lbClearBtn = CreateButton("LBClearBtn", lbPanel.transform, "CLEAR SESSION HISTORY", defaultFont, 20, new Vector2(0, -450), new Vector2(300, 50), new Color(0.55f, 0.2f, 0.2f));
            lbPanel.SetActive(false);

            // ==========================================
            // 9. BIND TO UIManager & LeaderboardUI
            // ==========================================
            if (uiMgr != null)
            {
                SetPrivateField(uiMgr, "mainMenuPanel", mainMenu);
                SetPrivateField(uiMgr, "placementGuidePanel", placementPanel);
                SetPrivateField(uiMgr, "inGameHUDPanel", inGameHud);
                SetPrivateField(uiMgr, "gameOverPanel", gameOverPanel);
                SetPrivateField(uiMgr, "leaderboardPanel", lbPanel);

                SetPrivateField(uiMgr, "startButton", startBtn);
                SetPrivateField(uiMgr, "leaderboardButton", lbBtn);

                SetPrivateField(uiMgr, "healthText", hpText);
                SetPrivateField(uiMgr, "scoreText", scoreText);
                SetPrivateField(uiMgr, "timerText", timerText);
                SetPrivateField(uiMgr, "difficultyBadgeText", diffBadge);
                SetPrivateField(uiMgr, "onScreenShootButton", shootBtn);
                SetPrivateField(uiMgr, "damageFlashImage", flashImg);

                SetPrivateField(uiMgr, "gameOverTitleText", goTitle);
                SetPrivateField(uiMgr, "finalScoreText", finalScore);
                SetPrivateField(uiMgr, "enemiesDefeatedText", killsText);
                SetPrivateField(uiMgr, "timeSurvivedText", surviveText);
                SetPrivateField(uiMgr, "restartButton", restartBtn);
                SetPrivateField(uiMgr, "returnMainMenuButton", menuBtn);

                SetPrivateField(uiMgr, "placementInstructionText", instructText);

                uiMgr.BindButtons();
            }

            if (lbUI != null)
            {
                SetPrivateField(lbUI, "tableContainer", tableContainer.transform);
                SetPrivateField(lbUI, "emptyStateText", emptyText);
                SetPrivateField(lbUI, "closeButton", lbCloseBtn);
                SetPrivateField(lbUI, "clearHistoryButton", lbClearBtn);
            }
        }

        public static Sprite LoadMenuBackgroundSprite()
        {
#if UNITY_EDITOR
            var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/MenuBackground.png");
            if (sprite != null) return sprite;
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/MenuBackground.png");
            if (sprite != null) return sprite;
#endif

            return Resources.Load<Sprite>("MenuBackground");
        }

        private static void EnsureEventSystem()
        {
            var es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
            }
            if (es.GetComponent<BaseInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(string name, Transform parent, string content, Font font, int size, Color color, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = pos;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; // Never block raycasts
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Font font, int fontSize, Vector2 pos, Vector2 sizeDelta, Color btnColor)
        {
            GameObject go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true; // Crucial for receiving clicks

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img; // Required for Unity Button interactions

            var colors = btn.colors;
            colors.normalColor = btnColor;
            colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.35f);
            colors.pressedColor = btnColor * 0.7f;
            colors.selectedColor = btnColor;
            btn.colors = colors;

            // Child Text
            GameObject textGo = CreateRect("Label", go.transform);
            StretchFull(textGo.GetComponent<RectTransform>());
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false; // MUST be false so clicks pass directly to button

            // Attach UIButtonFixer to guarantee hardware-level click events
            go.AddComponent<UIButtonFixer>();

            return btn;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            field?.SetValue(target, value);
        }
    }
}
