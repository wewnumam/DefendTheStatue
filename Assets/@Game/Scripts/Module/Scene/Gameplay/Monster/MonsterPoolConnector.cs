using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// Wires the bus to the monster pool. The pool publishes its own spawn / damage / kill messages
    /// from inside the controller, so this connector only needs the inbound half.
    /// </summary>
    public class MonsterPoolConnector : BaseConnector
    {
        private readonly MonsterPoolController _monsterPool = new MonsterPoolController();

        protected override void Connect()
        {
            Subscribe<RequestSpawnMonsterMessage>(_monsterPool.OnSpawnMonster);
            Subscribe<RequestDamageMonsterMessage>(_monsterPool.OnDamageMonster);
            Subscribe<RequestAreaDamageMessage>(_monsterPool.OnAreaDamage);
            Subscribe<RequestResetDashHitsMessage>(_monsterPool.OnResetDashHits);
            Subscribe<GameStateMessage>(_monsterPool.OnGameState);
        }

        protected override void Disconnect()
        {
        }
    }
}
