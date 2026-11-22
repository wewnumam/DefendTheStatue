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
            Subscribe<MindControlMessage>(_monsterPool.OnMindControl);
            Subscribe<RequestLockOnTargetMessage>(_monsterPool.OnRequestLockOnTarget);
            Subscribe<ThreatLevelMessage>(_monsterPool.OnThreatLevel);
            Subscribe<SlowFieldAddedMessage>(_monsterPool.OnSlowFieldAdded);
            Subscribe<TreeSpawnedMessage>(_monsterPool.OnTreeSpawned);
            Subscribe<TreeChoppedMessage>(_monsterPool.OnTreeChopped);
            Subscribe<GameStateMessage>(_monsterPool.OnGameState);
            Subscribe<CheatToggledMessage>(_monsterPool.OnCheatToggled);
            Subscribe<DevKillAllMessage>(_monsterPool.OnDevKillAll);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RequestSpawnMonsterMessage>(_monsterPool.OnSpawnMonster);
            Unsubscribe<RequestDamageMonsterMessage>(_monsterPool.OnDamageMonster);
            Unsubscribe<RequestAreaDamageMessage>(_monsterPool.OnAreaDamage);
            Unsubscribe<RequestResetDashHitsMessage>(_monsterPool.OnResetDashHits);
            Unsubscribe<MindControlMessage>(_monsterPool.OnMindControl);
            Unsubscribe<RequestLockOnTargetMessage>(_monsterPool.OnRequestLockOnTarget);
            Unsubscribe<ThreatLevelMessage>(_monsterPool.OnThreatLevel);
            Unsubscribe<SlowFieldAddedMessage>(_monsterPool.OnSlowFieldAdded);
            Unsubscribe<TreeSpawnedMessage>(_monsterPool.OnTreeSpawned);
            Unsubscribe<TreeChoppedMessage>(_monsterPool.OnTreeChopped);
            Unsubscribe<GameStateMessage>(_monsterPool.OnGameState);
            Unsubscribe<CheatToggledMessage>(_monsterPool.OnCheatToggled);
            Unsubscribe<DevKillAllMessage>(_monsterPool.OnDevKillAll);
        }
    }
}