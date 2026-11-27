using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Statue;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectTA.Module.GameStateUi
{
    /// <summary>
    /// Owns the five game state screens. Every panel is authored in the scene and wired here; the
    /// view only toggles them. PreGame, GameOver and GameWin are exclusive, while pause and the skill
    /// upgrade overlay can sit on top of a running match.
    /// </summary>
    public class GameStateUiView : BaseView
    {
        [Header("Screens")]
        [SerializeField] private GameObject _preGamePanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _upgradeSkillPanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _gameWinPanel;

        private UnityAction _onRestart;
        private TMP_Text _gameOverStats;
        private TMP_Text _gameWinStats;

        private UnityAction _onUpgradeStatue;
        private UnityAction _onCloseUpgradePanel;
        private TMP_Text _upgradeBody;
        private Button _upgradeButton;
        private TMP_Text _upgradeButtonLabel;

        public void SetRestartCallback(UnityAction onRestart)
        {
            _onRestart = onRestart;
        }

        /// <summary>
        /// GDD §15: the upgrade overlay needs its own buttons. The scene only ships the title and a
        /// close button, so the body and the upgrade button are built here.
        /// </summary>
        public void SetUpgradeCallbacks(UnityAction onUpgradeStatue, UnityAction onCloseUpgradePanel)
        {
            _onUpgradeStatue = onUpgradeStatue;
            _onCloseUpgradePanel = onCloseUpgradePanel;
        }

        /// <summary>
        /// Fills the overlay with the eight skill rows, the statue upgrade price, and the next level's
        /// gates with live progress (GDD §5).
        /// </summary>
        public void ShowUpgradePanel(
            PlayerSkillConstants skills,
            int statueLevel,
            int essence,
            int nextLevel,
            bool gateReady,
            string[] gateLines)
        {
            EnsureUpgradeContent();
            if (_upgradeBody == null)
            {
                return;
            }

            string body = skills != null
                ? string.Join("\n", skills.BuildSkillLines())
                : string.Empty;

            if (gateLines != null && gateLines.Length > 0)
            {
                body += "\n\nUpgrade to level " + nextLevel + ":";
                for (int i = 0; i < gateLines.Length; i++)
                {
                    body += "\n" + gateLines[i];
                }

                if (!gateReady)
                {
                    body += "\n(requirements not met)";
                }
            }

            _upgradeBody.text = body;

            bool maxed = statueLevel >= StatueModel.MaxLevel;
            int cost = StatueModel.GetUpgradeCost(statueLevel);
            _upgradeButtonLabel.text = maxed
                ? "Statue MAX"
                : gateReady
                    ? $"Upgrade Statue — {cost} essence"
                    : $"Upgrade Statue — {cost} essence (locked)";
            _upgradeButton.interactable = !maxed && gateReady && essence >= cost;
        }

        /// <summary>Scene button hook: both results panels route their Restart button here.</summary>
        public void RestartRequested()
        {
            _onRestart?.Invoke();
        }

        /// <summary>Fills both results panels; only the active one is visible.</summary>
        public void ShowResults(RunStats stats)
        {
            _gameOverStats = EnsureStats(_gameOverPanel, _gameOverStats);
            _gameWinStats = EnsureStats(_gameWinPanel, _gameWinStats);

            string text = stats.Format();
            if (_gameOverStats != null)
            {
                _gameOverStats.text = text;
            }

            if (_gameWinStats != null)
            {
                _gameWinStats.text = text;
            }
        }

        private void Awake()
        {
            HideAll();
        }

        public void HideAll()
        {
            SetActive(_preGamePanel, false);
            SetActive(_pausePanel, false);
            SetActive(_upgradeSkillPanel, false);
            SetActive(_gameOverPanel, false);
            SetActive(_gameWinPanel, false);
        }

        public void ShowPreGame()
        {
            HideAll();
            SetActive(_preGamePanel, true);
        }

        public void ShowGameOver()
        {
            HideAll();
            SetActive(_gameOverPanel, true);
        }

        public void ShowGameWin()
        {
            HideAll();
            SetActive(_gameWinPanel, true);
        }

        /// <summary>Overlay on a running match, so it never hides the other screens.</summary>
        public void SetPause(bool isPaused)
        {
            SetActive(_pausePanel, isPaused);
        }

        /// <summary>Overlay on a running match: the upgrade interface dims the game, it does not replace it.</summary>
        public void SetUpgradeSkill(bool isOpen)
        {
            SetActive(_upgradeSkillPanel, isOpen);
        }

        /// <summary>Runtime text under a results panel; created once, reused on every run.</summary>
        private static TMP_Text EnsureStats(GameObject panel, TMP_Text existing)
        {
            if (panel == null)
            {
                return existing;
            }

            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject("ResultsStats", typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(panel.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -150f);
            rect.sizeDelta = new Vector2(620f, 260f);

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22f;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>
        /// Builds the overlay body and its upgrade button once. The scene's close button hides the
        /// panel directly, which would leave the slow motion on, so its click is re-routed here too.
        /// </summary>
        private void EnsureUpgradeContent()
        {
            if (_upgradeBody != null || _upgradeSkillPanel == null)
            {
                return;
            }

            Transform close = _upgradeSkillPanel.transform.Find("CloseButton");
            if (close != null && close.TryGetComponent(out Button closeButton))
            {
                closeButton.onClick.AddListener(() => _onCloseUpgradePanel?.Invoke());
            }

            _upgradeBody = CreatePanelText(
                "UpgradeBody", _upgradeSkillPanel.transform, new Vector2(0f, 1f), new Vector2(20f, -110f),
                new Vector2(472f, 340f), TextAlignmentOptions.TopLeft);
            _upgradeBody.rectTransform.pivot = new Vector2(0f, 1f);

            GameObject buttonObject = new GameObject(
                "UpgradeStatueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(_upgradeSkillPanel.transform, false);
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0f, 20f);
            buttonRect.sizeDelta = new Vector2(440f, 44f);

            Image background = buttonObject.GetComponent<Image>();
            background.color = new Color(0.16f, 0.16f, 0.16f, 0.95f);

            _upgradeButton = buttonObject.GetComponent<Button>();
            _upgradeButton.targetGraphic = background;
            _upgradeButton.onClick.AddListener(() => _onUpgradeStatue?.Invoke());

            _upgradeButtonLabel = CreatePanelText(
                "Label", buttonRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 40f),
                TextAlignmentOptions.Center);
            _upgradeButtonLabel.text = "Upgrade Statue";
        }

        private static TMP_Text CreatePanelText(
            string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 15f;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static void SetActive(GameObject panel, bool isActive)
        {
            if (panel != null && panel.activeSelf != isActive)
            {
                panel.SetActive(isActive);
            }
        }
    }
}
