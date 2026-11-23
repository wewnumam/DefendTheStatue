using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.BalanceLog
{
    /// <summary>The balance CSV's feed: clock, spawns, kills, statue health, boss, run end.</summary>
    public class BalanceLogConnector : BaseConnector
    {
        private readonly BalanceLogController _balanceLog = new BalanceLogController();

        protected override void Connect()
        {
            Subscribe<RunClockMessage>(_balanceLog.OnRunClock);
            Subscribe<MonsterSpawnedMessage>(_balanceLog.OnMonsterSpawned);
            Subscribe<MonsterKilledMessage>(_balanceLog.OnMonsterKilled);
            Subscribe<StatueDamagedMessage>(_balanceLog.OnStatueDamaged);
            Subscribe<BossSpawnedMessage>(_balanceLog.OnBossSpawned);
            Subscribe<BossDefeatedMessage>(_balanceLog.OnBossDefeated);
            Subscribe<GameStateMessage>(_balanceLog.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RunClockMessage>(_balanceLog.OnRunClock);
            Unsubscribe<MonsterSpawnedMessage>(_balanceLog.OnMonsterSpawned);
            Unsubscribe<MonsterKilledMessage>(_balanceLog.OnMonsterKilled);
            Unsubscribe<StatueDamagedMessage>(_balanceLog.OnStatueDamaged);
            Unsubscribe<BossSpawnedMessage>(_balanceLog.OnBossSpawned);
            Unsubscribe<BossDefeatedMessage>(_balanceLog.OnBossDefeated);
            Unsubscribe<GameStateMessage>(_balanceLog.OnGameState);
        }
    }
}
