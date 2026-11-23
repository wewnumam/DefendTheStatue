using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Trap
{
    /// <summary>Feeds the trap controller: ring unlocks, finished walls and player builds.</summary>
    public class TrapConnector : BaseConnector
    {
        private readonly TrapController _trap = new TrapController();

        protected override void Connect()
        {
            Subscribe<TerritoryUnlockedMessage>(_trap.OnTerritoryUnlocked);
            Subscribe<WallSegmentBuiltMessage>(_trap.OnWallSegmentBuilt);
            Subscribe<TrapBuiltMessage>(_trap.OnTrapBuilt);
            Subscribe<GameStateMessage>(_trap.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<TerritoryUnlockedMessage>(_trap.OnTerritoryUnlocked);
            Unsubscribe<WallSegmentBuiltMessage>(_trap.OnWallSegmentBuilt);
            Unsubscribe<TrapBuiltMessage>(_trap.OnTrapBuilt);
            Unsubscribe<GameStateMessage>(_trap.OnGameState);
        }
    }
}
