using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Boss
{
    /// <summary>Feeds the boss: the ten-minute trigger, hits from arrows and swords, and the cheat flag.</summary>
    public class BossConnector : BaseConnector
    {
        private readonly BossController _boss = new BossController();

        protected override void Connect()
        {
            Subscribe<BossSpawnedMessage>(_boss.OnBossSpawned);
            Subscribe<RequestDamageBossMessage>(_boss.OnRequestDamageBoss);
            Subscribe<RequestAreaDamageMessage>(_boss.OnAreaDamage);
            Subscribe<CheatToggledMessage>(_boss.OnCheatToggled);
            Subscribe<GameStateMessage>(_boss.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<BossSpawnedMessage>(_boss.OnBossSpawned);
            Unsubscribe<RequestDamageBossMessage>(_boss.OnRequestDamageBoss);
            Unsubscribe<RequestAreaDamageMessage>(_boss.OnAreaDamage);
            Unsubscribe<CheatToggledMessage>(_boss.OnCheatToggled);
            Unsubscribe<GameStateMessage>(_boss.OnGameState);
        }
    }
}
