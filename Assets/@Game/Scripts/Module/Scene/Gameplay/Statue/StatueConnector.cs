using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Statue
{
    public class StatueConnector : BaseConnector
    {
        private readonly StatueController _statue = new StatueController();

        protected override void Connect()
        {
            Subscribe<RequestUpgradeStatueMessage>(_statue.OnRequestUpgrade);
            Subscribe<RequestStatueDamageMessage>(_statue.OnRequestDamage);
            Subscribe<WallSegmentBuiltMessage>(_statue.OnWallSegmentBuilt);
            Subscribe<MonsterKilledMessage>(_statue.OnMonsterKilled);
            Subscribe<GuardBuiltMessage>(_statue.OnGuardBuilt);
            Subscribe<BossDefeatedMessage>(_statue.OnBossDefeated);
            Subscribe<PlayerStatsChangedMessage>(_statue.OnPlayerStatsChanged);
            Subscribe<CheatToggledMessage>(_statue.OnCheatToggled);
            Subscribe<DevHealStatueMessage>(_statue.OnDevHeal);
            Subscribe<DevMaxLevelMessage>(_statue.OnDevMaxLevel);
            Subscribe<GameStateMessage>(_statue.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RequestUpgradeStatueMessage>(_statue.OnRequestUpgrade);
            Unsubscribe<RequestStatueDamageMessage>(_statue.OnRequestDamage);
            Unsubscribe<WallSegmentBuiltMessage>(_statue.OnWallSegmentBuilt);
            Unsubscribe<MonsterKilledMessage>(_statue.OnMonsterKilled);
            Unsubscribe<GuardBuiltMessage>(_statue.OnGuardBuilt);
            Unsubscribe<BossDefeatedMessage>(_statue.OnBossDefeated);
            Unsubscribe<PlayerStatsChangedMessage>(_statue.OnPlayerStatsChanged);
            Unsubscribe<CheatToggledMessage>(_statue.OnCheatToggled);
            Unsubscribe<DevHealStatueMessage>(_statue.OnDevHeal);
            Unsubscribe<DevMaxLevelMessage>(_statue.OnDevMaxLevel);
            Unsubscribe<GameStateMessage>(_statue.OnGameState);
        }
    }
}
