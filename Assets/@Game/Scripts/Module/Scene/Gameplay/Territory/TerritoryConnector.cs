using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Territory
{
    public class TerritoryConnector : BaseConnector
    {
        private readonly TerritoryController _territory = new TerritoryController();

        protected override void Connect()
        {
            Subscribe<TerritoryUnlockedMessage>(_territory.OnTerritoryUnlocked);
            Subscribe<TowerBuiltMessage>(_territory.OnTowerBuilt);
            Subscribe<WallSegmentBuiltMessage>(_territory.OnWallSegmentBuilt);
            Subscribe<GameStateMessage>(_territory.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<TerritoryUnlockedMessage>(_territory.OnTerritoryUnlocked);
            Unsubscribe<TowerBuiltMessage>(_territory.OnTowerBuilt);
            Unsubscribe<WallSegmentBuiltMessage>(_territory.OnWallSegmentBuilt);
            Unsubscribe<GameStateMessage>(_territory.OnGameState);
        }
    }
}
