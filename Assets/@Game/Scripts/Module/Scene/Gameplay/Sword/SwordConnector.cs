using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Sword
{
    public class SwordConnector : BaseConnector
    {
        private readonly SwordController _sword = new();

        protected override void Connect()
        {
            Subscribe<WeaponSwitchedMessage>(_sword.OnWeaponSwitched);
            Subscribe<PlayerCharacterShootStartMessage>(_sword.OnShootStart);
            Subscribe<PlayerCharacterShootEndMessage>(_sword.OnShootEnd);
            Subscribe<SkillMultipliersMessage>(_sword.OnSkillMultipliers);
            Subscribe<PlayerStatsChangedMessage>(_sword.OnPlayerStatsChanged);
        }

        protected override void Disconnect()
        {
            Unsubscribe<WeaponSwitchedMessage>(_sword.OnWeaponSwitched);
            Unsubscribe<PlayerCharacterShootStartMessage>(_sword.OnShootStart);
            Unsubscribe<PlayerCharacterShootEndMessage>(_sword.OnShootEnd);
            Unsubscribe<SkillMultipliersMessage>(_sword.OnSkillMultipliers);
            Unsubscribe<PlayerStatsChangedMessage>(_sword.OnPlayerStatsChanged);
        }
    }
}
