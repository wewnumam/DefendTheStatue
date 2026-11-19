using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.PlayerSkill
{
    /// <summary>
    /// Turns a key press into a cast. Gating and cooldowns live in the model; mana is owned by the
    /// player module, so the cost leaves as a message instead of being spent here.
    /// </summary>
    public class PlayerSkillController : ObjectController<PlayerSkillController, PlayerSkillModel, PlayerSkillView>
    {
        private const int WeaponCount = 2;
        private const int SlotCount = 3;

        /// <summary>Mirrors of the player module's state: read from messages, never reached for directly.</summary>
        private EnumManager.WeaponKind _weapon = EnumManager.WeaponKind.Sword;
        private float _mana;
        private int _statueLevel = 1;

        private readonly bool[] _effectActive = new bool[WeaponCount * SlotCount];

        /// <summary>Owned here so E over the god statue and Tab cannot disagree about the panel state.</summary>
        private bool _isUpgradePanelOpen;

        private Transform _player;

        /// <summary>The sword time slash is anchored on the caster, so the launcher hands the transform over.</summary>
        public void SetPlayerTransform(Transform player)
        {
            _player = player;
        }

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            if (gameConstants.GameConstants.SkillConstants == null)
            {
                Debug.LogError("SKILLCONSTANTS IS NULL");
                return;
            }

            _model.SetSkillConstants(gameConstants.GameConstants.SkillConstants);
            _model.SetStatueLevel(_statueLevel);
        }

        public override void SetView(PlayerSkillView view)
        {
            base.SetView(view);
            _model.Reset();
            _weapon = EnumManager.WeaponKind.Sword;
            _mana = 0f;
            ClearEffects();
            view.SetCallbacks(OnTick, OnCastRequested, OnUpgradePanelToggleRequested);
        }

        /// <summary>
        /// The panel's open flag lives here, so every key that toggles it stays in sync. Opening needs
        /// the player inside the statue radius; closing always works, so walking away never traps the
        /// player in a panel he cannot dismiss.
        /// </summary>
        private void OnUpgradePanelToggleRequested()
        {
            if (!_isUpgradePanelOpen && !IsWithinUpgradeRadius())
            {
                return;
            }

            _isUpgradePanelOpen = !_isUpgradePanelOpen;
            Publish(new SkillUpgradePanelToggledMessage(_isUpgradePanelOpen));
        }

        private bool IsWithinUpgradeRadius()
        {
            return _view != null && _player != null
                && _view.IsWithinUpgradeRadius(_player.position);
        }

        public void OnRequestToggleSkillUpgradePanel(RequestToggleSkillUpgradePanelMessage message)
        {
            OnUpgradePanelToggleRequested();
        }

        private void OnTick(float deltaTime)
        {
            _model.Tick(deltaTime);
            PublishEffectEnds();
        }

        private void OnCastRequested(EnumManager.SkillSlot slot)
        {
            if (!_model.CanCast(_weapon, slot, _mana, false))
            {
                return;
            }

            int manaCost = Mathf.RoundToInt(_model.GetManaCost(slot));
            if (!_model.Cast(_weapon, slot, false))
            {
                return;
            }

            Publish(new SkillCastMessage(slot, _weapon, manaCost));
            if (_model.IsEffectActive(_weapon, slot))
            {
                _effectActive[Index(_weapon, slot)] = true;
                Publish(new SkillEffectToggledMessage(slot, _weapon, true));
            }

            // Berserker and clone mode change speed, damage and fire rate the moment they start.
            PublishMultipliers();

            // Sword 2 is the time slash: one slow disc on the caster. Its expiry drives the model timer.
            if (_weapon == EnumManager.WeaponKind.Sword
                && slot == EnumManager.SkillSlot.Skill2
                && _player != null)
            {
                PlayerSkillConstants constants = _model.SkillConstants;
                Publish(new SlowFieldAddedMessage(
                    _player.position,
                    constants.TimeSlashRadius,
                    constants.TimeSlashSlowFactor,
                    constants.TimeSlashDurationSeconds));
            }
        }

        /// <summary>Berserker and clone mode switch off on their own timer, so the end is reported here.</summary>
        private void PublishEffectEnds()
        {
            for (int weapon = 0; weapon < WeaponCount; weapon++)
            {
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    int index = weapon * SlotCount + slot;
                    if (!_effectActive[index])
                    {
                        continue;
                    }

                    var weaponKind = (EnumManager.WeaponKind)weapon;
                    var skillSlot = (EnumManager.SkillSlot)(slot + 1);
                    if (_model.IsEffectActive(weaponKind, skillSlot))
                    {
                        continue;
                    }

                    _effectActive[index] = false;
                    Publish(new SkillEffectToggledMessage(skillSlot, weaponKind, false));
                    PublishMultipliers();
                }
            }
        }

        /// <summary>
        /// The reference polls getSpeedMult / getDmgMult / getAtkCdMult every frame; here they are
        /// pushed when they can change, so the player, sword and bullet pool never hold the skill model.
        /// </summary>
        private void PublishMultipliers()
        {
            if (_model.SkillConstants == null)
            {
                return;
            }

            Publish(new SkillMultipliersMessage(
                _model.GetSpeedMultiplier(_weapon),
                _model.GetDamageMultiplier(_weapon),
                _model.GetAttackCooldownMultiplier(_weapon)));
        }

        public void OnRequestCastSkill(RequestCastSkillMessage message)
        {
            OnCastRequested(message.Slot);
        }

        /// <summary>
        /// Weapon switches are resolved here and only here: clone mode pins the bow, so a refused scroll
        /// must not reach the sword, the bullet pool or the pawn's art. Everyone else follows
        /// WeaponSwitchedMessage.
        /// </summary>
        public void OnSwitchWeapon(SwitchWeaponMessage message)
        {
            if (_model.IsWeaponLocked(EnumManager.WeaponKind.Bow))
            {
                return;
            }

            if (message.Scroll > 0f)
            {
                _weapon = EnumManager.WeaponKind.Bow;
            }
            else if (message.Scroll < 0f)
            {
                _weapon = EnumManager.WeaponKind.Sword;
            }
            else
            {
                return;
            }

            Publish(new WeaponSwitchedMessage(_weapon));

            // Which weapon is in hand decides whether a running skill3 means x3 melee or x2 fire rate.
            PublishMultipliers();
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _mana = message.Mana;
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _statueLevel = message.Level;
            _model.SetStatueLevel(_statueLevel);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();
            ClearEffects();
            _weapon = EnumManager.WeaponKind.Sword;
            Publish(new WeaponSwitchedMessage(_weapon));
            PublishMultipliers();
            _isUpgradePanelOpen = false;
        }

        private void ClearEffects()
        {
            for (int i = 0; i < _effectActive.Length; i++)
            {
                _effectActive[i] = false;
            }
        }

        private static int Index(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            return (int)weapon * SlotCount + ((int)slot - 1);
        }
    }
}