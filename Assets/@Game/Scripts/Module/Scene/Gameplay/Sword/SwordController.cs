using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.PlayerWeapon;
using UnityEngine;

namespace ProjectTA.Module.Sword
{
    public class SwordController : ObjectController<SwordController, SwordView>
    {
        /// <summary>Berserker's x3, mirrored off the bus. One whenever no skill is running.</summary>
        private float _damageMultiplier = 1f;

        /// <summary>Reference swing cost. Without enough stamina the swing does not land at all.</summary>
        private const float SwingStaminaCost = 8f;

        /// <summary>Mirrored from the player module, which owns the pool.</summary>
        private float _stamina;

        public void OnWeaponSwitched(WeaponSwitchedMessage message)
        {
            // The launcher publishes Playing (and the skill module republishes the switch) before
            // views are wired; the view's own default is the scene's sword.
            if (_view == null)
            {
                return;
            }

            _view.SetWeapon(message.Weapon == ProjectTA.Utility.EnumManager.WeaponKind.Bow ? 1f : -1f);
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _stamina = message.Stamina;
        }

        public void OnSkillMultipliers(SkillMultipliersMessage message)
        {
            _damageMultiplier = message.DamageMultiplier;
        }

        /// <summary>
        /// The swing lands the moment the player attacks, the way the reference applies melee damage
        /// up front. Damage leaves as a message; the monster pool owns who is inside the radius.
        /// </summary>
        public void OnShootStart(PlayerCharacterShootStartMessage message)
        {
            // A stale container can hand this controller a view from a previous Play session (domain
            // reload is off), so the destroyed-object check is real, not defensive noise.
            if (_view == null)
            {
                return;
            }

            _view.SetHitboxActive(true);

            if (!_view.IsEquipped || _stamina < SwingStaminaCost)
            {
                return;
            }

            Vector2 origin = _view.transform.position;
            float radius = PlayerWeaponModel.SwordRadius;

            // The authored collider wins: a swing only reaches what the sword actually covers. The
            // constant is only the fallback for a prefab/scene without a wired hitbox.
            if (_view.TryGetHitArea(out Vector2 hitCenter, out float hitRadius))
            {
                origin = hitCenter;
                radius = hitRadius;
            }

            float damage = PlayerWeaponModel.SwordDamage * _damageMultiplier;
            Publish(new RequestAreaDamageMessage(origin, radius, damage, origin));
            Publish(new RequestSpendStaminaMessage(SwingStaminaCost));
        }

        public void OnShootEnd(PlayerCharacterShootEndMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetHitboxActive(false);
        }
    }
}
