using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Sword
{
    public class SwordConnector : BaseConnector
    {
        private readonly SwordController _sword = new();

        protected override void Connect()
        {
            Subscribe<SwitchWeaponMessage>(_sword.OnSwitchWeapon);
            Subscribe<PlayerCharacterShootStartMessage>(_sword.OnShootStart);
            Subscribe<PlayerCharacterShootEndMessage>(_sword.OnShootEnd);
        }

        protected override void Disconnect()
        {
            Unsubscribe<SwitchWeaponMessage>(_sword.OnSwitchWeapon);
            Unsubscribe<PlayerCharacterShootStartMessage>(_sword.OnShootStart);
            Unsubscribe<PlayerCharacterShootEndMessage>(_sword.OnShootEnd);
        }
    }
}
