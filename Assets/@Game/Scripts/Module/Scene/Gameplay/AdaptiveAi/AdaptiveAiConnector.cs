using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.AdaptiveAi
{
    /// <summary>The threat dial's feed: clock, health, monsters, guard fire and run state.</summary>
    public class AdaptiveAiConnector : BaseConnector
    {
        private readonly AdaptiveAiController _adaptiveAi = new AdaptiveAiController();

        protected override void Connect()
        {
            Subscribe<RunClockMessage>(_adaptiveAi.OnRunClock);
            Subscribe<PlayerStatsChangedMessage>(_adaptiveAi.OnPlayerStatsChanged);
            Subscribe<StatueDamagedMessage>(_adaptiveAi.OnStatueDamaged);
            Subscribe<MonsterSpawnedMessage>(_adaptiveAi.OnMonsterSpawned);
            Subscribe<MonsterKilledMessage>(_adaptiveAi.OnMonsterKilled);
            Subscribe<MonsterKilledMessage>(_adaptiveAi.OnMonsterKilledByPlayer);
            Subscribe<GuardFiredMessage>(_adaptiveAi.OnGuardFired);
            Subscribe<GameStateMessage>(_adaptiveAi.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RunClockMessage>(_adaptiveAi.OnRunClock);
            Unsubscribe<PlayerStatsChangedMessage>(_adaptiveAi.OnPlayerStatsChanged);
            Unsubscribe<StatueDamagedMessage>(_adaptiveAi.OnStatueDamaged);
            Unsubscribe<MonsterSpawnedMessage>(_adaptiveAi.OnMonsterSpawned);
            Unsubscribe<MonsterKilledMessage>(_adaptiveAi.OnMonsterKilled);
            Unsubscribe<MonsterKilledMessage>(_adaptiveAi.OnMonsterKilledByPlayer);
            Unsubscribe<GuardFiredMessage>(_adaptiveAi.OnGuardFired);
            Unsubscribe<GameStateMessage>(_adaptiveAi.OnGameState);
        }
    }
}
