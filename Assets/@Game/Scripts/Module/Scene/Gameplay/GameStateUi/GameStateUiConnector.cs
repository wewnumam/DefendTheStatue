using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.GameStateUi
{
    /// <summary>Feeds the state screens: run state, pause toggle and the skill upgrade overlay.</summary>
    public class GameStateUiConnector : BaseConnector
    {
        private readonly GameStateUiController _gameStateUi = new();

        protected override void Connect()
        {
            Subscribe<GameStateMessage>(_gameStateUi.OnGameState);
            Subscribe<PauseToggledMessage>(_gameStateUi.OnPauseToggled);
            Subscribe<SkillUpgradePanelToggledMessage>(_gameStateUi.OnSkillUpgradePanelToggled);
            Subscribe<RunClockMessage>(_gameStateUi.OnRunClock);
            Subscribe<MonsterKilledMessage>(_gameStateUi.OnMonsterKilled);
            Subscribe<PlayerDiedMessage>(_gameStateUi.OnPlayerDied);
            Subscribe<SkillCastMessage>(_gameStateUi.OnSkillCast);
            Subscribe<TrapTriggeredMessage>(_gameStateUi.OnTrapTriggered);
            Subscribe<TreeChoppedMessage>(_gameStateUi.OnTreeChopped);
            Subscribe<PlayerStatsChangedMessage>(_gameStateUi.OnPlayerStatsChanged);
            Subscribe<StatueUpgradedMessage>(_gameStateUi.OnStatueUpgraded);
            Subscribe<StatueGateStatusMessage>(_gameStateUi.OnStatueGateStatus);
        }

        protected override void Disconnect()
        {
            Unsubscribe<GameStateMessage>(_gameStateUi.OnGameState);
            Unsubscribe<PauseToggledMessage>(_gameStateUi.OnPauseToggled);
            Unsubscribe<SkillUpgradePanelToggledMessage>(_gameStateUi.OnSkillUpgradePanelToggled);
            Unsubscribe<RunClockMessage>(_gameStateUi.OnRunClock);
            Unsubscribe<MonsterKilledMessage>(_gameStateUi.OnMonsterKilled);
            Unsubscribe<PlayerDiedMessage>(_gameStateUi.OnPlayerDied);
            Unsubscribe<SkillCastMessage>(_gameStateUi.OnSkillCast);
            Unsubscribe<TrapTriggeredMessage>(_gameStateUi.OnTrapTriggered);
            Unsubscribe<TreeChoppedMessage>(_gameStateUi.OnTreeChopped);
            Unsubscribe<PlayerStatsChangedMessage>(_gameStateUi.OnPlayerStatsChanged);
            Unsubscribe<StatueUpgradedMessage>(_gameStateUi.OnStatueUpgraded);
            Unsubscribe<StatueGateStatusMessage>(_gameStateUi.OnStatueGateStatus);
        }
    }
}
