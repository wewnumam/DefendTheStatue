using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;

namespace ProjectTA.Module.Hud
{
    public class HudController : ObjectController<HudController, HudModel, HudView>
    {
        private PlayerSkillConstants _skills;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants != null && gameConstants.GameConstants != null)
            {
                _skills = gameConstants.GameConstants.SkillConstants;
            }
        }

        public override void SetView(HudView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetStatueHealth(_model.StatueHealth, _model.StatueMaxHealth);
            view.SetWeapon(EnumManager.WeaponKind.Sword);
            view.SetSkillGrid(BuildSkillLines());
        }

        /// <summary>GDD §15: 4 sword + 4 bow boxes with their unlock level and mana cost.</summary>
        private string[] BuildSkillLines()
        {
            return _skills != null ? _skills.BuildSkillLines() : new string[8];
        }

        public void OnHealZoneTaint(HealZoneTaintMessage message)
        {
            if (_view != null)
            {
                _view.SetStatueTainted(message.Tainted);
            }
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

        /// <summary>GDD §15: the eight skill boxes, refreshed on the skill module's slow tick.</summary>
        public void OnSkillState(SkillStateMessage message)
        {
            if (_view != null)
            {
                _view.SetSkillState(message);
            }
        }

        public void OnRunClock(RunClockMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetRunClock(message.TotalSeconds);
        }

        public void OnBossSpawned(BossSpawnedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetBossVisible(true);
        }

        public void OnBossDamaged(BossDamagedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetBossHealth(message.Health, message.MaxHealth, message.Phase);
        }

        public void OnBossPhaseChanged(BossPhaseChangedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetBossPhase(message.Phase);
        }

        public void OnBossDefeated(BossDefeatedMessage message)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetBossVisible(false);
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
            if (message.GameState == EnumManager.GameState.GameOver
                || message.GameState == EnumManager.GameState.GameWin)
            {
                if (_view != null)
                {
                    _view.SetBossVisible(false);
                }

                return;
            }

            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();

            if (_view == null)
            {
                return;
            }

            _view.ResetRunUi();
            _view.SetStatueHealth(_model.StatueHealth, _model.StatueMaxHealth);
        }
    }
}