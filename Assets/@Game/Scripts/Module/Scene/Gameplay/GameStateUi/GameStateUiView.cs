using Agate.MVC.Base;
using UnityEngine;

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

        private static void SetActive(GameObject panel, bool isActive)
        {
            if (panel != null && panel.activeSelf != isActive)
            {
                panel.SetActive(isActive);
            }
        }
    }
}
