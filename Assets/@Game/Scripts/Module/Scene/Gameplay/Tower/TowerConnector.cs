using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Tower
{
    public class TowerConnector : BaseConnector
    {
        private readonly TowerController _tower = new TowerController();

        protected override void Connect()
        {
            Subscribe<TowerBuiltMessage>(_tower.OnTowerBuilt);
            Subscribe<MonsterSpawnedMessage>(_tower.OnMonsterSpawned);
            Subscribe<MonsterKilledMessage>(_tower.OnMonsterKilled);
            Subscribe<GameStateMessage>(_tower.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<TowerBuiltMessage>(_tower.OnTowerBuilt);
            Unsubscribe<MonsterSpawnedMessage>(_tower.OnMonsterSpawned);
            Unsubscribe<MonsterKilledMessage>(_tower.OnMonsterKilled);
            Unsubscribe<GameStateMessage>(_tower.OnGameState);
        }
    }
}
