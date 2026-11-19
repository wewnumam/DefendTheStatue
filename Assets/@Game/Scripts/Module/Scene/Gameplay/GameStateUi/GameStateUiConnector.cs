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
        }

        protected override void Disconnect()
        {
            Unsubscribe<GameStateMessage>(_gameStateUi.OnGameState);
            Unsubscribe<PauseToggledMessage>(_gameStateUi.OnPauseToggled);
            Unsubscribe<SkillUpgradePanelToggledMessage>(_gameStateUi.OnSkillUpgradePanelToggled);
        }
    }
}
