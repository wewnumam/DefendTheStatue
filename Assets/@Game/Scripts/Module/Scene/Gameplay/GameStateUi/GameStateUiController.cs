using Agate.MVC.Base;
using ProjectTA.Boot;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
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
        private PlayerSkillConstants _skills;
        private bool _upgradePanelOpen;

        /// <summary>Latest gate report from the statue: requirement rows and whether the level is open.</summary>
        private bool _gateReady;
        private int _gateNextLevel;
        private string[] _gateLines = System.Array.Empty<string>();

        /// <summary>Handed over by the launcher so the overlay can print the eight skill rows.</summary>
        public void SetSkillConstants(PlayerSkillConstants skills)
        {
            _skills = skills;
        }

        public override void SetView(GameStateUiView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetRestartCallback(OnRestart);
            view.SetUpgradeCallbacks(OnUpgradeStatue, OnCloseUpgradePanel);
        }

        /// <summary>GDD §5: the only thing the upgrade overlay can buy is the next statue level.</summary>
        private void OnUpgradeStatue()
        {
            Publish(new RequestUpgradeStatueMessage());
        }

        private void OnCloseUpgradePanel()
        {
            Publish(new RequestToggleSkillUpgradePanelMessage());
        }

        private void RefreshUpgradePanel()
        {
            _view.ShowUpgradePanel(_skills, _model.StatueLevel, _model.Essence, _gateNextLevel, _gateReady, _gateLines);
        }

        /// <summary>GDD §5: the panel shows the next level's gates and live progress.</summary>
        public void OnStatueGateStatus(StatueGateStatusMessage message)
        {
            _gateNextLevel = message.NextLevel;
            _gateReady = message.Ready;
            _gateLines = message.Lines ?? System.Array.Empty<string>();

            if (_upgradePanelOpen)
            {
                RefreshUpgradePanel();
            }
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

            if (_upgradePanelOpen)
            {
                RefreshUpgradePanel();
            }
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _model.SetStatueLevel(message.Level);

            if (_upgradePanelOpen)
            {
                RefreshUpgradePanel();
            }
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
                    _upgradePanelOpen = false;
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

            _upgradePanelOpen = message.IsOpen;
            _view.SetUpgradeSkill(message.IsOpen);

            if (message.IsOpen)
            {
                Publish(new RequestStatueGateStatusMessage());
                RefreshUpgradePanel();
            }
        }
    }
}
