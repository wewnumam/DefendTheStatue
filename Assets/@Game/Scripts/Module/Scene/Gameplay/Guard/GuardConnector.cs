using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Guard
{
    /// <summary>Feeds the guard controller: statue upgrades, recruit builds and monster mirrors.</summary>
    public class GuardConnector : BaseConnector
    {
        private readonly GuardController _guard = new GuardController();

        protected override void Connect()
        {
            Subscribe<StatueUpgradedMessage>(_guard.OnStatueUpgraded);
            Subscribe<GuardBuiltMessage>(_guard.OnGuardBuilt);
            Subscribe<GuardEmpowerMessage>(_guard.OnGuardEmpower);
            Subscribe<MonsterSpawnedMessage>(_guard.OnMonsterSpawned);
            Subscribe<MonsterDamagedMessage>(_guard.OnMonsterDamaged);
            Subscribe<MonsterKilledMessage>(_guard.OnMonsterKilled);
            Subscribe<HealZoneTaintMessage>(_guard.OnHealZoneTaint);
            Subscribe<GuardDamagedMessage>(_guard.OnGuardDamaged);
            Subscribe<GameStateMessage>(_guard.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<StatueUpgradedMessage>(_guard.OnStatueUpgraded);
            Unsubscribe<GuardBuiltMessage>(_guard.OnGuardBuilt);
            Unsubscribe<GuardEmpowerMessage>(_guard.OnGuardEmpower);
            Unsubscribe<MonsterSpawnedMessage>(_guard.OnMonsterSpawned);
            Unsubscribe<MonsterDamagedMessage>(_guard.OnMonsterDamaged);
            Unsubscribe<MonsterKilledMessage>(_guard.OnMonsterKilled);
            Unsubscribe<HealZoneTaintMessage>(_guard.OnHealZoneTaint);
            Unsubscribe<GuardDamagedMessage>(_guard.OnGuardDamaged);
            Unsubscribe<GameStateMessage>(_guard.OnGameState);
        }
    }
}
