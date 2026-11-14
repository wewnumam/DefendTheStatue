using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Build
{
    public class BuildConnector : BaseConnector
    {
        private readonly BuildController _build = new BuildController();

        protected override void Connect()
        {
            Subscribe<TerritoryUnlockedMessage>(_build.OnTerritoryUnlocked);
            Subscribe<StatueUpgradedMessage>(_build.OnStatueUpgraded);
            Subscribe<PlayerStatsChangedMessage>(_build.OnPlayerStatsChanged);
            Subscribe<TowerBuiltMessage>(_build.OnTowerBuilt);
            Subscribe<WallSegmentBuiltMessage>(_build.OnWallSegmentBuilt);
            Subscribe<GameStateMessage>(_build.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<TerritoryUnlockedMessage>(_build.OnTerritoryUnlocked);
            Unsubscribe<StatueUpgradedMessage>(_build.OnStatueUpgraded);
            Unsubscribe<PlayerStatsChangedMessage>(_build.OnPlayerStatsChanged);
            Unsubscribe<TowerBuiltMessage>(_build.OnTowerBuilt);
            Unsubscribe<WallSegmentBuiltMessage>(_build.OnWallSegmentBuilt);
            Unsubscribe<GameStateMessage>(_build.OnGameState);
        }
    }
}
