using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;

namespace ProjectTA.Module.Hud
{
    public class HudController : ObjectController<HudController, HudModel, HudView>
    {
        /// <summary>GDD §13 effect names, one per weapon, in slot order.</summary>
        private static readonly string[] SwordSkills = { "Dash", "Time Slash", "Berserker", "Mind Control" };
        private static readonly string[] BowSkills = { "Triple Shot", "Lock-On", "Clone Mode", "Guard Empower" };

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
            string[] lines = new string[8];

            for (int i = 0; i < 4; i++)
            {
                lines[i] = $"{i + 1} S {SwordSkills[i]} — {UnlockLevel(i)} · {ManaCost(i)} MP";
                lines[i + 4] = $"{i + 1} B {BowSkills[i]} — {UnlockLevel(i)} · {ManaCost(i)} MP";
            }

            return lines;
        }

        private int UnlockLevel(int slot)
        {
            if (_skills == null)
            {
                return 0;
            }

            switch (slot)
            {
                case 0:
                    return _skills.Skill1RequiredStatueLevel;
                case 1:
                    return _skills.Skill2RequiredStatueLevel;
                case 2:
                    return _skills.Skill3RequiredStatueLevel;
                default:
                    return _skills.Skill4RequiredStatueLevel;
            }
        }

        private int ManaCost(int slot)
        {
            if (_skills == null)
            {
                return 0;
            }

            switch (slot)
            {
                case 0:
                    return UnityEngine.Mathf.RoundToInt(_skills.Skill1ManaCost);
                case 1:
                    return UnityEngine.Mathf.RoundToInt(_skills.Skill2ManaCost);
                case 2:
                    return UnityEngine.Mathf.RoundToInt(_skills.Skill3ManaCost);
                default:
                    return UnityEngine.Mathf.RoundToInt(_skills.Skill4ManaCost);
            }
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