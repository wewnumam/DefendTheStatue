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
            Subscribe<StatueUpgradedMessage>(_hud.OnStatueUpgraded);
            Subscribe<GameStateMessage>(_hud.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<PlayerStatsChangedMessage>(_hud.OnPlayerStatsChanged);
            Unsubscribe<WeaponSwitchedMessage>(_hud.OnWeaponSwitched);
            Unsubscribe<StatueDamagedMessage>(_hud.OnStatueDamaged);
            Unsubscribe<StatueUpgradedMessage>(_hud.OnStatueUpgraded);
            Unsubscribe<GameStateMessage>(_hud.OnGameState);
        }
    }
}