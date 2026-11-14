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
            Subscribe<PlayerStatsChangedMessage>(_statue.OnPlayerStatsChanged);
            Subscribe<GameStateMessage>(_statue.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RequestUpgradeStatueMessage>(_statue.OnRequestUpgrade);
            Unsubscribe<RequestStatueDamageMessage>(_statue.OnRequestDamage);
            Unsubscribe<WallSegmentBuiltMessage>(_statue.OnWallSegmentBuilt);
            Unsubscribe<MonsterKilledMessage>(_statue.OnMonsterKilled);
            Unsubscribe<PlayerStatsChangedMessage>(_statue.OnPlayerStatsChanged);
            Unsubscribe<GameStateMessage>(_statue.OnGameState);
        }
    }
}
