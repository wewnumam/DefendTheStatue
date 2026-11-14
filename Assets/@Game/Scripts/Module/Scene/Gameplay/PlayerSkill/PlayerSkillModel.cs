using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.PlayerSkill
{
    /// <summary>
    /// Skill gating and effect state for both weapons. Cooldowns are tracked per weapon, matching
    /// the reference game's swordSkillNCooldown / bowSkillNCooldown pairs. All times are seconds.
    /// </summary>
    public class PlayerSkillModel : BaseModel
    {
        public PlayerSkillConstants SkillConstants { get; private set; }
        public int StatueLevel { get; private set; } = 1;

        private const int WeaponCount = 2;
        private const int SlotCount = 3;

        private readonly float[,] _cooldowns = new float[WeaponCount, SlotCount];
        private readonly float[,] _effectTimers = new float[WeaponCount, SlotCount];

        #region SETUP

        public void SetSkillConstants(PlayerSkillConstants skillConstants)
        {
            SkillConstants = skillConstants;
            SetDataAsDirty();
        }

        public void SetStatueLevel(int level)
        {
            StatueLevel = Mathf.Clamp(level, 1, 7);
            SetDataAsDirty();
        }

        public void Reset()
        {
            for (int weapon = 0; weapon < WeaponCount; weapon++)
            {
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    _cooldowns[weapon, slot] = 0f;
                    _effectTimers[weapon, slot] = 0f;
                }
            }

            SetDataAsDirty();
        }

        #endregion

        #region TICK

        public void Tick(float deltaTime)
        {
            for (int weapon = 0; weapon < WeaponCount; weapon++)
            {
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    if (_cooldowns[weapon, slot] > 0f)
                    {
                        _cooldowns[weapon, slot] = Mathf.Max(0f, _cooldowns[weapon, slot] - deltaTime);
                    }

                    if (_effectTimers[weapon, slot] > 0f)
                    {
                        _effectTimers[weapon, slot] = Mathf.Max(0f, _effectTimers[weapon, slot] - deltaTime);
                    }
                }
            }

            SetDataAsDirty();
        }

        #endregion

        #region GATING

        public int GetRequiredStatueLevel(EnumManager.SkillSlot slot)
        {
            if (SkillConstants == null)
            {
                return int.MaxValue;
            }

            switch (slot)
            {
                case EnumManager.SkillSlot.Skill1:
                    return SkillConstants.Skill1RequiredStatueLevel;
                case EnumManager.SkillSlot.Skill2:
                    return SkillConstants.Skill2RequiredStatueLevel;
                case EnumManager.SkillSlot.Skill3:
                    return SkillConstants.Skill3RequiredStatueLevel;
                default:
                    return int.MaxValue;
            }
        }

        public bool IsUnlocked(EnumManager.SkillSlot slot)
        {
            return StatueLevel >= GetRequiredStatueLevel(slot);
        }

        public float GetManaCost(EnumManager.SkillSlot slot)
        {
            if (SkillConstants == null)
            {
                return 0f;
            }

            switch (slot)
            {
                case EnumManager.SkillSlot.Skill1:
                    return SkillConstants.Skill1ManaCost;
                case EnumManager.SkillSlot.Skill2:
                    return SkillConstants.Skill2ManaCost;
                case EnumManager.SkillSlot.Skill3:
                    return SkillConstants.Skill3ManaCost;
                default:
                    return 0f;
            }
        }

        public float GetCooldownDuration(EnumManager.SkillSlot slot)
        {
            if (SkillConstants == null)
            {
                return 0f;
            }

            switch (slot)
            {
                case EnumManager.SkillSlot.Skill1:
                    return SkillConstants.Skill1CooldownSeconds;
                case EnumManager.SkillSlot.Skill2:
                    return SkillConstants.Skill2CooldownSeconds;
                case EnumManager.SkillSlot.Skill3:
                    return SkillConstants.Skill3CooldownSeconds;
                default:
                    return 0f;
            }
        }

        public float GetCooldownRemaining(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            return _cooldowns[(int)weapon, SlotIndex(slot)];
        }

        public bool IsOnCooldown(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            return GetCooldownRemaining(weapon, slot) > 0f;
        }

        /// <summary>
        /// Pure check: unlocks are statue-level gated, and the reference game skips cooldown and
        /// mana checks entirely while the noCooldown cheat is live.
        /// </summary>
        public bool CanCast(
            EnumManager.WeaponKind weapon,
            EnumManager.SkillSlot slot,
            float availableMana,
            bool noCooldown)
        {
            if (SkillConstants == null)
            {
                return false;
            }

            if (!IsUnlocked(slot))
            {
                return false;
            }

            if (HasRunningEffectGate(weapon, slot, noCooldown) && IsEffectActive(weapon, slot))
            {
                return false;
            }

            if (!noCooldown && IsOnCooldown(weapon, slot))
            {
                return false;
            }

            return noCooldown || availableMana >= GetManaCost(slot);
        }

        /// <summary>Commits the cast: starts the cooldown and the effect timer for that weapon.</summary>
        public bool Cast(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot, bool noCooldown)
        {
            if (SkillConstants == null || !IsUnlocked(slot))
            {
                return false;
            }

            if (HasRunningEffectGate(weapon, slot, noCooldown) && IsEffectActive(weapon, slot))
            {
                return false;
            }

            if (!noCooldown)
            {
                _cooldowns[(int)weapon, SlotIndex(slot)] = GetCooldownDuration(slot);
            }

            _effectTimers[(int)weapon, SlotIndex(slot)] = GetEffectDuration(weapon, slot);

            SetDataAsDirty();
            return true;
        }

        /// <summary>
        /// Whether a still-running effect blocks a recast. Reference game: skill 3 always refuses
        /// while active, bow skill 2 refuses only outside the noCooldown cheat, sword skill 2 stacks
        /// freely, and dash has no effect timer to begin with.
        /// </summary>
        private static bool HasRunningEffectGate(
            EnumManager.WeaponKind weapon,
            EnumManager.SkillSlot slot,
            bool noCooldown)
        {
            switch (slot)
            {
                case EnumManager.SkillSlot.Skill2:
                    return weapon == EnumManager.WeaponKind.Bow && !noCooldown;
                case EnumManager.SkillSlot.Skill3:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region EFFECT STATE

        public bool IsEffectActive(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            return _effectTimers[(int)weapon, SlotIndex(slot)] > 0f;
        }

        public float GetEffectRemaining(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            return _effectTimers[(int)weapon, SlotIndex(slot)];
        }

        /// <summary>Sword skill 2 only: the time-slash slow field the caster keeps running.</summary>
        public bool IsTimeSlashActive(EnumManager.WeaponKind weapon)
        {
            return weapon == EnumManager.WeaponKind.Sword
                && IsEffectActive(weapon, EnumManager.SkillSlot.Skill2);
        }

        /// <summary>Berserker speed x3 is sword-only, per the reference game's getSpeedMult().</summary>
        public float GetSpeedMultiplier(EnumManager.WeaponKind weapon)
        {
            if (weapon != EnumManager.WeaponKind.Sword || SkillConstants == null)
            {
                return 1f;
            }

            return IsEffectActive(weapon, EnumManager.SkillSlot.Skill3)
                ? SkillConstants.BerserkerSpeedMultiplier
                : 1f;
        }

        /// <summary>Berserker damage x3, sword-only.</summary>
        public float GetDamageMultiplier(EnumManager.WeaponKind weapon)
        {
            if (weapon != EnumManager.WeaponKind.Sword || SkillConstants == null)
            {
                return 1f;
            }

            return IsEffectActive(weapon, EnumManager.SkillSlot.Skill3)
                ? SkillConstants.BerserkerDamageMultiplier
                : 1f;
        }

        /// <summary>Clone mode halves bow attack cooldown, per getAtkCdMult().</summary>
        public float GetAttackCooldownMultiplier(EnumManager.WeaponKind weapon)
        {
            if (weapon != EnumManager.WeaponKind.Bow || SkillConstants == null)
            {
                return 1f;
            }

            return IsEffectActive(weapon, EnumManager.SkillSlot.Skill3)
                ? SkillConstants.CloneModeAttackCooldownMultiplier
                : 1f;
        }

        /// <summary>Clone mode pins the bow in hand: scroll switching is refused meanwhile.</summary>
        public bool IsWeaponLocked(EnumManager.WeaponKind weapon)
        {
            return weapon == EnumManager.WeaponKind.Bow
                && IsEffectActive(weapon, EnumManager.SkillSlot.Skill3);
        }

        private float GetSkill3Multiplier(EnumManager.WeaponKind weapon)
        {
            if (weapon != EnumManager.WeaponKind.Sword || SkillConstants == null)
            {
                return 1f;
            }

            return IsEffectActive(weapon, EnumManager.SkillSlot.Skill3)
                ? SkillConstants.BerserkerSpeedMultiplier
                : 1f;
        }

        private float GetEffectDuration(EnumManager.WeaponKind weapon, EnumManager.SkillSlot slot)
        {
            switch (slot)
            {
                case EnumManager.SkillSlot.Skill1:
                    // Dash is an instantaneous impulse: the controller reads DashDurationSeconds off
                    // PlayerConstants and drives it itself, so no skill effect timer is opened here.
                    return 0f;
                case EnumManager.SkillSlot.Skill2:
                    return weapon == EnumManager.WeaponKind.Sword
                        ? SkillConstants.TimeSlashDurationSeconds
                        : SkillConstants.LockShootDurationSeconds;
                case EnumManager.SkillSlot.Skill3:
                    return SkillConstants.Skill3DurationSeconds;
                default:
                    return 0f;
            }
        }

        #endregion

        private static int SlotIndex(EnumManager.SkillSlot slot)
        {
            return (int)slot - 1;
        }
    }
}
