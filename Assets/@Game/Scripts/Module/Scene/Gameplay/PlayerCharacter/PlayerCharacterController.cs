using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using UnityEngine;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.SaveSystem;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterController : ObjectController<PlayerCharacterController, PlayerCharacterModel, PlayerCharacterView>
    {
        /// <summary>Statue heal radius, in the same world units the statue view is drawn in.</summary>
        private const float HealRadius = 120f;

        /// <summary>Reference: the bow's charged shot costs 15 stamina, one arrow and lands 50 flat.</summary>
        private const float ChargedShotStaminaCost = 15f;
        private const float ChargedShotDamage = 50f;

        /// <summary>Weapon in hand, mirrored from the skill module's resolved switch.</summary>
        private EnumManager.WeaponKind _weapon = EnumManager.WeaponKind.Sword;

        private bool _isBlocking;
        private bool _isCharging;

        public void InitModel(IGameConstantsModel gameConstants, IGameSettingsModel gameSettings)
        {
            if (!ValidateGameConstants(gameConstants) || !ValidateGameSettings(gameSettings))
                return;

            _model.SetPlayerConstants(gameConstants.GameConstants.PlayerConstants);
            _model.ResetStats();
            PublishStatsChanged();
        }

        public override void SetView(PlayerCharacterView view)
        {
            base.SetView(view);
            view.PlayerConstants = _model.PlayerConstants;
            view.SetTickCallback(OnTick);
            view.SetBlockCallback(OnBlockChanged);
        }

        /// <summary>
        /// Runs the reference's per-frame player block: the heal aura, the block drain, the regen lock
        /// while blocking/charging/attacking, and the arrow respawn clock.
        /// </summary>
        private void OnTick(float deltaTime)
        {
            bool isInHealRadius = _view.transform.position.sqrMagnitude <= HealRadius * HealRadius;
            _model.Tick(deltaTime, _isBlocking, _isCharging, false, isInHealRadius);

            // Reference: falling to zero stamina drops the block by itself.
            if (_isBlocking && _model.Stamina <= 0f)
            {
                _isBlocking = false;
            }

            _view.SetArrowState(_model.IsInfiniteArrows || _model.Arrows > 0);

            PublishStatsChanged();
        }

        /// <summary>Right mouse: sword blocks, bow charges. Release fires the charged shot.</summary>
        private void OnBlockChanged(bool isActive)
        {
            if (isActive)
            {
                BeginHold();
            }
            else
            {
                ReleaseHold();
            }
        }

        private void BeginHold()
        {
            if (_weapon == EnumManager.WeaponKind.Sword)
            {
                if (_model.Stamina > 0f)
                {
                    _isBlocking = true;
                }

                return;
            }

            if (_model.Stamina >= ChargedShotStaminaCost && CanShootArrow())
            {
                _isCharging = true;
            }
        }

        private void ReleaseHold()
        {
            if (_weapon == EnumManager.WeaponKind.Sword)
            {
                _isBlocking = false;
                return;
            }

            if (!_isCharging)
            {
                return;
            }

            _isCharging = false;

            if (_model.Stamina < ChargedShotStaminaCost || !CanShootArrow())
            {
                return;
            }

            _model.TrySpendStamina(ChargedShotStaminaCost);
            _model.TryConsumeArrows(1, false);
            PublishStatsChanged();

            // Firing is the bullet pool's job; it stamps this flat damage on the next arrow it spawns.
            Publish(new RequestHeavyShotMessage(ChargedShotDamage));
        }

        private bool CanShootArrow()
        {
            return _model.IsInfiniteArrows || _model.Arrows > 0;
        }

         #region PRIVATE METHOD

        private bool ValidateGameConstants(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null)
                return LogError("GAMECONSTANTS IS NULL");

            if (gameConstants.GameConstants == null)
                return LogError("SOGAMECONSTANTS IS NULL");

            if (gameConstants.GameConstants.PlayerConstants == null)
                return LogError("PLAYERCONSTANTS IS NULL");

            return true;
        }

        private bool ValidateGameSettings(IGameSettingsModel gameSettings)
        {
            if (gameSettings == null)
                return LogError("GAMESETTINGS IS NULL");

            if (gameSettings.SavedSettingsData == null)
                return LogError("SAVEDSETTINGSDATA IS NULL");

            return true;
        }

        private bool LogError(string message)
        {
            Debug.LogError(message);
            return false;
        }

        #endregion

        public void OnMove(MovePlayerCharacterMessage message)
        {
            _view.SetDirection(message.Direction);
        }

        public void OnRotate(RotatePlayerCharacterMessage message)
        {
            _view.SetAim(message.Aim);
        }

        /// <summary>
        /// The skill module resolves the switch, because clone mode refuses it, so this only mirrors the
        /// result. The reference also drops the block and the charge on a weapon change.
        /// </summary>
        public void OnWeaponSwitched(WeaponSwitchedMessage message)
        {
            _weapon = message.Weapon;
            _isBlocking = false;
            _isCharging = false;

            if (_view != null)
            {
                _view.SetWeaponVisual(message.Weapon == EnumManager.WeaponKind.Bow ? 1f : -1f);
            }
        }

        /// <summary>Stamina bill from the sword or the bow; the pool is owned here.</summary>
        public void OnRequestSpendStamina(RequestSpendStaminaMessage message)
        {
            if (_model.TrySpendStamina(message.Amount))
            {
                PublishStatsChanged();
            }
        }

        /// <summary>Sword swing animation; the view ignores it while the bow is in hand.</summary>
        public void OnAttackStart(PlayerCharacterShootStartMessage message)
        {
            if (_view == null)
                return;

            _view.PlayAttackAnimation();
        }

        /// <summary>Mana is owned here, so the skill module reports the cost and this pays it.</summary>
        public void OnSkillCast(SkillCastMessage message)
        {
            if (message.ManaSpent <= 0 || _model.TrySpendMana(message.ManaSpent))
            {
                PublishStatsChanged();
            }
        }

        /// <summary>Gold has one owner: builders and the statue post a bill, this pays it.</summary>
        public void OnRequestSpendGold(RequestSpendGoldMessage message)
        {
            if (_model.TrySpendGold(message.Amount))
            {
                PublishStatsChanged();
            }
        }

        /// <summary>Bow ammo. The pool asks per shot; clone mode spends nothing, matching the reference.</summary>
        public void OnRequestSpendArrows(RequestSpendArrowsMessage message)
        {
            if (_model.TryConsumeArrows(message.Count, false))
            {
                PublishStatsChanged();
            }
        }

        /// <summary>
        /// Berserker's x3 is the only thing that touches walk speed. The reference applies it to plain
        /// movement only — the dash keeps its own speed — so the view, not the model, holds it.
        /// </summary>
        public void OnSkillMultipliers(SkillMultipliersMessage message)
        {
            if (_view != null)
            {
                _view.SetSpeedMultiplier(message.SpeedMultiplier);
            }
        }

        /// <summary>
        /// A kill pays out here rather than in the monster module, so gold keeps a single owner.
        /// The model already has the gold field; this only routes the message into it.
        /// </summary>
        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.AddGold(message.GoldValue);
            PublishStatsChanged();
        }

        private void PublishStatsChanged()
        {
            Publish(new PlayerStatsChangedMessage(
                _model.Health,
                _model.MaxHealth,
                _model.Stamina,
                _model.MaxStamina,
                _model.Mana,
                _model.MaxMana,
                _model.Gold,
                _model.Arrows));
        }
    }
}