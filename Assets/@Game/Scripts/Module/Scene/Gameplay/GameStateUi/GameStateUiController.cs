using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.GameStateUi
{
    /// <summary>
    /// Maps the run's messages onto the state screens. It never publishes: the launcher starts a run
    /// with GameStateMessage(Playing), the statue reports GameOver, and GameFlow owns both pause and
    /// the slow motion upgrade overlay.
    /// </summary>
    public class GameStateUiController : ObjectController<GameStateUiController, GameStateUiView>
    {
        public void OnGameState(GameStateMessage message)
        {
            if (_view == null)
            {
                return;
            }

            switch (message.GameState)
            {
                case EnumManager.GameState.PreGame:
                    _view.ShowPreGame();
                    break;

                case EnumManager.GameState.GameOver:
                    _view.ShowGameOver();
                    break;

                case EnumManager.GameState.GameWin:
                    _view.ShowGameWin();
                    break;

                case EnumManager.GameState.Playing:
                    _view.HideAll();
                    break;

                // Pause arrives as PauseToggledMessage: resuming must not republish Playing.
                default:
                    break;
            }
        }

        public void OnPauseToggled(PauseToggledMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetPause(message.IsPaused);
        }

        public void OnSkillUpgradePanelToggled(SkillUpgradePanelToggledMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetUpgradeSkill(message.IsOpen);
        }
    }
}
