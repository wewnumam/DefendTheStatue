using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Tree
{
    /// <summary>Feeds the tree controller: ring unlocks, builds that flatten trees, and run resets.</summary>
    public class TreeConnector : BaseConnector
    {
        private readonly TreeController _tree = new TreeController();

        protected override void Connect()
        {
            Subscribe<TerritoryUnlockedMessage>(_tree.OnTerritoryUnlocked);
            Subscribe<TowerBuiltMessage>(_tree.OnTowerBuilt);
            Subscribe<WallSegmentBuiltMessage>(_tree.OnWallSegmentBuilt);
            Subscribe<RequestAreaDamageMessage>(_tree.OnAreaDamage);
            Subscribe<RequestTreeDamageMessage>(_tree.OnTreeDamage);
            Subscribe<GameStateMessage>(_tree.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<TerritoryUnlockedMessage>(_tree.OnTerritoryUnlocked);
            Unsubscribe<TowerBuiltMessage>(_tree.OnTowerBuilt);
            Unsubscribe<WallSegmentBuiltMessage>(_tree.OnWallSegmentBuilt);
            Unsubscribe<RequestAreaDamageMessage>(_tree.OnAreaDamage);
            Unsubscribe<RequestTreeDamageMessage>(_tree.OnTreeDamage);
            Unsubscribe<GameStateMessage>(_tree.OnGameState);
        }
    }
}
