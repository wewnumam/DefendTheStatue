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

        /// <summary>GDD §4: the bow's charged shot costs 2 stamina, one arrow and lands 2.</summary>
        private const float ChargedShotStaminaCost = 2f;
        private const float ChargedShotDamage = 2f;

        /// <summary>GDD §4 skill 1 (sword): the dash body-checks for 2, once per monster per dash.</summary>
        public const float DashDamage = 2f;
        private const float DashHitRadius = 25f;

        /// <summary>Weapon in hand, mirrored from the skill module's resolved switch.</summary>
        private EnumManager.WeaponKind _weapon = EnumManager.WeaponKind.Sword;

        private bool _isBlocking;
        private bool _isCharging;

        /// <summary>GDD §5: an enemy inside the heal radius switches the statue's regeneration off.</summary>
        private bool _healZoneTainted;

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
            _model.Tick(deltaTime, _isBlocking, _isCharging, false, isInHealRadius && !_healZoneTainted);

            // Reference: falling to zero stamina drops the block by itself.
            if (_isBlocking && _model.Stamina <= 0f)
            {
                _isBlocking = false;
            }

            _view.SetArrowState(_model.IsInfiniteArrows || _model.Arrows > 0);

            // Dash contact damage: the pool hits each monster once per dash (see the reset on cast).
            if (_view.IsDashing)
            {
                Vector2 position = _view.transform.position;
                Publish(new RequestAreaDamageMessage(position, DashHitRadius, DashDamage, position, true));
            }

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

        /// <summary>Dev cheats: only the three that change player stats are consumed here.</summary>
        public void OnCheatToggled(CheatToggledMessage message)
        {
            switch (message.Cheat)
            {
                case EnumManager.CheatType.GodMode:
                    _model.SetGodMode(message.Enabled);
                    break;
                case EnumManager.CheatType.InfiniteStamina:
                    _model.SetInfiniteStamina(message.Enabled);
                    break;
                case EnumManager.CheatType.InfiniteArrows:
                    _model.SetInfiniteArrows(message.Enabled);
                    break;
                case EnumManager.CheatType.InfiniteEssence:
                    _model.SetInfiniteEssence(message.Enabled);
                    break;
            }

            PublishStatsChanged();
        }

        public void OnRequestManaGain(RequestManaGainMessage message)
        {
            _model.AddMana(message.Amount);
            PublishStatsChanged();
        }

        public void OnDevAddEssence(DevAddEssenceMessage message)
        {
            _model.AddEssence(message.Amount);
            PublishStatsChanged();
        }

        public void OnDevAddMana(DevAddManaMessage message)
        {
            _model.AddMana(message.Amount);
            PublishStatsChanged();
        }

        public void OnDevHealPlayer(DevHealPlayerMessage message)
        {
            _model.Heal(_model.MaxHealth);
            PublishStatsChanged();
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

            if (message.Weapon == EnumManager.WeaponKind.Sword
                && message.Slot == EnumManager.SkillSlot.Skill1
                && _view != null)
            {
                _view.StartDash(_view.AimDirection);
                Publish(new RequestResetDashHitsMessage());
            }
        }

        /// <summary>Essence has one owner: builders and the statue post a bill, this pays it.</summary>
        public void OnRequestSpendEssence(RequestSpendEssenceMessage message)
        {
            if (_model.TrySpendEssence(message.Amount))
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
        /// A kill pays out here rather than in the monster module, so essence keeps a single owner.
        /// The model already has the essence field; this only routes the message into it.
        /// </summary>
        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.AddEssence(message.EssenceValue);
            _model.RegisterKill();
            PublishStatsChanged();
        }

        /// <summary>GDD §11: a teleport moves the pawn instantly; walls and monsters never block it.</summary>
        public void OnTeleportPerformed(TeleportPerformedMessage message)
        {
            _view?.Teleport(message.Destination);
        }

        public void OnHealZoneTaint(HealZoneTaintMessage message)
        {
            _healZoneTainted = message.Tainted;
        }

        /// <summary>
        /// GDD §4: dying is not a game over. It costs the statue 30 HP and puts the player back at the
        /// statue with 3 s of invincibility, so a death spiral still ends the run but a mistake does not.
        /// </summary>
        public void OnRequestPlayerDamage(RequestPlayerDamageMessage message)
        {
            if (!_model.ApplyDamage(message.Amount) || _model.IsAlive)
            {
                PublishStatsChanged();
                return;
            }

            if (_view != null)
            {
                _view.Teleport(Vector2.zero);
            }

            Publish(new PlayerDiedMessage());
            Publish(new RequestStatueDamageMessage(PlayerCharacterModel.DeathStatuePenalty));
            _model.Respawn();
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
                _model.Essence,
                _model.Arrows));
        }
    }
}