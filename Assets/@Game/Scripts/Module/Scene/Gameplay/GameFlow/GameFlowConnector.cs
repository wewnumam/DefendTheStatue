using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>Feeds the run controller: spawn cadence and the run clock live inside it.</summary>
    public class GameFlowConnector : BaseConnector
    {
        private readonly GameFlowController _gameFlow = new GameFlowController();

        protected override void Connect()
        {
            Subscribe<WallSegmentBuiltMessage>(_gameFlow.OnWallSegmentBuilt);
            Subscribe<TowerBuiltMessage>(_gameFlow.OnTowerBuilt);
            Subscribe<MonsterKilledMessage>(_gameFlow.OnMonsterKilled);
            Subscribe<GameStateMessage>(_gameFlow.OnGameState);
        }

        protected override void Disconnect()
        {
        }
    }
}
