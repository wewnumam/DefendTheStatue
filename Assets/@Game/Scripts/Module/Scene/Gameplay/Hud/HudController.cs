using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.Hud
{
    public class HudController : ObjectController<HudController, HudModel, HudView>
    {
        public override void SetView(HudView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetStatueHealth(_model.StatueHealth, _model.StatueMaxHealth);
            view.SetWeapon(EnumManager.WeaponKind.Sword);
        }

        /// <summary>Weapon status line. The launcher may publish the switch before views are wired.</summary>
        public void OnWeaponSwitched(WeaponSwitchedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetWeapon(message.Weapon);
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetStats(message);
        }

        public void OnStatueDamaged(StatueDamagedMessage message)
        {
            _model.SetStatue(message.Health, message.MaxHealth);

            if (_view == null)
            {
                return;
            }

            _view.SetStatueHealth(message.Health, message.MaxHealth);
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _model.ApplyUpgrade(message.Effect.MaxHealthDelta);

            if (_view == null)
            {
                return;
            }

            _view.SetStatueHealth(_model.StatueHealth, _model.StatueMaxHealth);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();

            if (_view == null)
            {
                return;
            }

            _view.SetStatueHealth(_model.StatueHealth, _model.StatueMaxHealth);
        }
    }
}