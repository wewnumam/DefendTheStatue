using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Teleport
{
    public class TeleportConnector : BaseConnector
    {
        private readonly TeleportController _teleport = new();

        protected override void Connect()
        {
            Subscribe<PlayerStatsChangedMessage>(_teleport.OnPlayerStatsChanged);
            Subscribe<StatueUpgradedMessage>(_teleport.OnStatueUpgraded);
            Subscribe<GameStateMessage>(_teleport.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<PlayerStatsChangedMessage>(_teleport.OnPlayerStatsChanged);
            Unsubscribe<StatueUpgradedMessage>(_teleport.OnStatueUpgraded);
            Unsubscribe<GameStateMessage>(_teleport.OnGameState);
        }
    }
}
