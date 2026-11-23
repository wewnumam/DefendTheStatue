using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Hud
{
    public class HudConnector : BaseConnector
    {
        private readonly HudController _hud = new HudController();

        protected override void Connect()
        {
            Subscribe<PlayerStatsChangedMessage>(_hud.OnPlayerStatsChanged);
            Subscribe<WeaponSwitchedMessage>(_hud.OnWeaponSwitched);
            Subscribe<StatueDamagedMessage>(_hud.OnStatueDamaged);
            Subscribe<HealZoneTaintMessage>(_hud.OnHealZoneTaint);
            Subscribe<StatueUpgradedMessage>(_hud.OnStatueUpgraded);
            Subscribe<RunClockMessage>(_hud.OnRunClock);
            Subscribe<SkillStateMessage>(_hud.OnSkillState);
            Subscribe<BossSpawnedMessage>(_hud.OnBossSpawned);
            Subscribe<BossDamagedMessage>(_hud.OnBossDamaged);
            Subscribe<BossPhaseChangedMessage>(_hud.OnBossPhaseChanged);
            Subscribe<BossDefeatedMessage>(_hud.OnBossDefeated);
            Subscribe<GameStateMessage>(_hud.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<PlayerStatsChangedMessage>(_hud.OnPlayerStatsChanged);
            Unsubscribe<WeaponSwitchedMessage>(_hud.OnWeaponSwitched);
            Unsubscribe<StatueDamagedMessage>(_hud.OnStatueDamaged);
            Unsubscribe<HealZoneTaintMessage>(_hud.OnHealZoneTaint);
            Unsubscribe<StatueUpgradedMessage>(_hud.OnStatueUpgraded);
            Unsubscribe<RunClockMessage>(_hud.OnRunClock);
            Unsubscribe<SkillStateMessage>(_hud.OnSkillState);
            Unsubscribe<BossSpawnedMessage>(_hud.OnBossSpawned);
            Unsubscribe<BossDamagedMessage>(_hud.OnBossDamaged);
            Unsubscribe<BossPhaseChangedMessage>(_hud.OnBossPhaseChanged);
            Unsubscribe<BossDefeatedMessage>(_hud.OnBossDefeated);
            Unsubscribe<GameStateMessage>(_hud.OnGameState);
        }
    }
}