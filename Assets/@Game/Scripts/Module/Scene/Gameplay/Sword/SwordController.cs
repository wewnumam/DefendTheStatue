using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.Sword
{
    public class SwordController : ObjectController<SwordController, SwordView>
    {
        public void OnSwitchWeapon(SwitchWeaponMessage message)
        {
            _view.SetWeapon(message.Scroll);
        }

        public void OnShootStart(PlayerCharacterShootStartMessage message)
        {
            _view.SetHitboxActive(true);
        }

        public void OnShootEnd(PlayerCharacterShootEndMessage message)
        {
            _view.SetHitboxActive(false);
        }
    }
}
