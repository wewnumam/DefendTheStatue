using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.PlayerWeapon;
using UnityEngine;

namespace ProjectTA.Module.Sword
{
    public class SwordController : ObjectController<SwordController, SwordView>
    {
        public void OnSwitchWeapon(SwitchWeaponMessage message)
        {
            _view.SetWeapon(message.Scroll);
        }

        /// <summary>
        /// The swing lands the moment the player attacks, the way the reference applies melee damage
        /// up front. Damage leaves as a message; the monster pool owns who is inside the radius.
        /// </summary>
        public void OnShootStart(PlayerCharacterShootStartMessage message)
        {
            _view.SetHitboxActive(true);

            if (!_view.IsEquipped)
            {
                return;
            }

            Vector2 origin = _view.transform.position;
            Publish(new RequestAreaDamageMessage(origin, PlayerWeaponModel.SwordRadius, PlayerWeaponModel.SwordDamage, origin));
        }

        public void OnShootEnd(PlayerCharacterShootEndMessage message)
        {
            _view.SetHitboxActive(false);
        }
    }
}
