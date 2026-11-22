using Agate.MVC.Base;
using ProjectTA.Boot;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.GameStateUi
{
    /// <summary>
    /// Maps the run's messages onto the state screens and mirrors the stats they print. It never
    /// publishes run state: the launcher starts a run with GameStateMessage(Playing), the statue
    /// reports GameOver, and GameFlow owns both pause and the slow motion upgrade overlay.
    /// </summary>
    public class GameStateUiController : ObjectController<GameStateUiController, GameStateUiModel, GameStateUiView>
    {
        /// <summary>Restart is a scene reload: every model resets for free and no module needs a ResetMessage.</summary>
        public override void SetView(GameStateUiView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetRestartCallback(OnRestart);
        }

        private void OnRestart()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(TagManager.SCENE_GAMEPLAY);
            }
        }

        public void OnRunClock(RunClockMessage message)
        {
            _model.SetClock(message.TotalSeconds);
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.RegisterKill(message.Kind);
        }

        public void OnPlayerDied(PlayerDiedMessage message)
        {
            _model.RegisterDeath();
        }

        public void OnSkillCast(SkillCastMessage message)
        {
            if (message.Slot == EnumManager.SkillSlot.Skill3)
            {
                _model.RegisterSkill3();
            }
        }

        public void OnTrapTriggered(TrapTriggeredMessage message)
        {
            _model.RegisterTrapTriggered();
        }

        public void OnTreeChopped(TreeChoppedMessage message)
        {
            _model.RegisterTreeChopped();
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _model.SetEssence(message.Essence);
            _model.SetMaxMana(message.MaxMana);
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _model.SetStatueLevel(message.Level);
        }

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
                    _view.ShowResults(_model.Snapshot());
                    _view.ShowGameOver();
                    break;

                case EnumManager.GameState.GameWin:
                    _view.ShowResults(_model.Snapshot());
                    _view.ShowGameWin();
                    break;

                case EnumManager.GameState.Playing:
                    _model.Reset();
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
