using NUnit.Framework;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.PlayerSkill;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Skill gating, cooldowns and effect state. Gates are statue level 3 / 5 / 7 and cooldowns are
    /// tracked per weapon, matching the reference game's per-weapon cooldown fields.
    /// </summary>
    public class PlayerSkillModelTests
    {
        private const EnumManager.WeaponKind Sword = EnumManager.WeaponKind.Sword;
        private const EnumManager.WeaponKind Bow = EnumManager.WeaponKind.Bow;
        private const EnumManager.SkillSlot Skill1 = EnumManager.SkillSlot.Skill1;
        private const EnumManager.SkillSlot Skill2 = EnumManager.SkillSlot.Skill2;
        private const EnumManager.SkillSlot Skill3 = EnumManager.SkillSlot.Skill3;

        private SOGameConstants _constants;
        private PlayerSkillModel _model;

        [SetUp]
        public void SetUp()
        {
            _constants = Resources.Load<SOGameConstants>("GameConstants");
            Assert.That(_constants, Is.Not.Null, "GameConstants asset missing from Resources.");

            _model = new PlayerSkillModel();
            _model.SetSkillConstants(_constants.SkillConstants);
        }

        [Test]
        public void Skills_UnlockAtStatueLevelsThreeFiveSeven()
        {
            _model.SetStatueLevel(2);
            Assert.That(_model.IsUnlocked(Skill1), Is.False);
            Assert.That(_model.IsUnlocked(Skill2), Is.False);
            Assert.That(_model.IsUnlocked(Skill3), Is.False);

            _model.SetStatueLevel(3);
            Assert.That(_model.IsUnlocked(Skill1), Is.True);
            Assert.That(_model.IsUnlocked(Skill2), Is.False);

            _model.SetStatueLevel(5);
            Assert.That(_model.IsUnlocked(Skill2), Is.True);
            Assert.That(_model.IsUnlocked(Skill3), Is.False);

            _model.SetStatueLevel(7);
            Assert.That(_model.IsUnlocked(Skill3), Is.True);
        }

        [Test]
        public void ManaCosts_MatchReferenceValues()
        {
            Assert.That(_model.GetManaCost(Skill1), Is.EqualTo(5f));
            Assert.That(_model.GetManaCost(Skill2), Is.EqualTo(20f));
            Assert.That(_model.GetManaCost(Skill3), Is.EqualTo(20f));
        }

        [Test]
        public void CooldownDurations_AreReferenceFramesInSeconds()
        {
            Assert.That(_model.GetCooldownDuration(Skill1), Is.EqualTo(6f), "360 frames");
            Assert.That(_model.GetCooldownDuration(Skill2), Is.EqualTo(6f), "360 frames");
            Assert.That(_model.GetCooldownDuration(Skill3), Is.EqualTo(8f), "480 frames");
        }

        [Test]
        public void CanCast_FalseWhileLocked()
        {
            _model.SetStatueLevel(2);

            Assert.That(_model.CanCast(Sword, Skill1, 99f, false), Is.False);
        }

        [Test]
        public void CanCast_FalseWithoutEnoughMana()
        {
            _model.SetStatueLevel(3);

            Assert.That(_model.CanCast(Sword, Skill1, 4.99f, false), Is.False);
            Assert.That(_model.CanCast(Sword, Skill1, 5f, false), Is.True);
        }

        [Test]
        public void Cast_StartsCooldownThatTicksDown()
        {
            _model.SetStatueLevel(3);

            Assert.That(_model.Cast(Sword, Skill1, false), Is.True);
            Assert.That(_model.IsOnCooldown(Sword, Skill1), Is.True);
            Assert.That(_model.GetCooldownRemaining(Sword, Skill1), Is.EqualTo(6f).Within(0.001f));

            _model.Tick(6f);

            Assert.That(_model.IsOnCooldown(Sword, Skill1), Is.False);
            Assert.That(_model.GetCooldownRemaining(Sword, Skill1), Is.EqualTo(0f));
        }

        [Test]
        public void Cooldowns_AreTrackedPerWeapon()
        {
            _model.SetStatueLevel(3);
            _model.Cast(Sword, Skill1, false);

            Assert.That(_model.IsOnCooldown(Sword, Skill1), Is.True);
            Assert.That(_model.IsOnCooldown(Bow, Skill1), Is.False);
        }

        [Test]
        public void NoCooldownCheat_BypassesCooldownAndMana()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill1, false);

            Assert.That(_model.IsOnCooldown(Sword, Skill1), Is.True);
            Assert.That(_model.CanCast(Sword, Skill1, 0f, false), Is.False, "cooldown and mana still gate");
            Assert.That(_model.CanCast(Sword, Skill1, 0f, true), Is.True);
        }

        [Test]
        public void Skill3_RefusesARecastEvenWhileTheCheatIsOn()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill3, true);

            // Reference useSkill3 checks isSkill3Active before the noCooldown branch.
            Assert.That(_model.CanCast(Sword, Skill3, 0f, true), Is.False);
        }

        [Test]
        public void BowSkill2_BlocksARecastOnlyOutsideTheCheat()
        {
            _model.SetStatueLevel(5);
            _model.Cast(Bow, Skill2, false);

            Assert.That(_model.CanCast(Bow, Skill2, 99f, false), Is.False, "lock shoot is still running");
            Assert.That(_model.CanCast(Bow, Skill2, 0f, true), Is.True, "cheat path skips the effect check too");
        }

        [Test]
        public void SwordSkill2_RecastsOverAnActiveTimeSlash()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill2, false);

            Assert.That(_model.Cast(Sword, Skill2, false), Is.True, "reference pushes a second slash");
        }

        [Test]
        public void Cast_FalseWhenTheEffectIsAlreadyRunning()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill3, true);

            Assert.That(_model.CanCast(Sword, Skill3, 99f, true), Is.False);
            Assert.That(_model.Cast(Sword, Skill3, true), Is.False);
        }

        [Test]
        public void SwordSkill3_TriplesSpeedAndDamageForFiveSeconds()
        {
            _model.SetStatueLevel(7);

            Assert.That(_model.GetSpeedMultiplier(Sword), Is.EqualTo(1f));

            _model.Cast(Sword, Skill3, false);

            Assert.That(_model.GetSpeedMultiplier(Sword), Is.EqualTo(3f));
            Assert.That(_model.GetDamageMultiplier(Sword), Is.EqualTo(3f));
            Assert.That(_model.GetEffectRemaining(Sword, Skill3), Is.EqualTo(5f).Within(0.001f));

            _model.Tick(5f);

            Assert.That(_model.GetSpeedMultiplier(Sword), Is.EqualTo(1f));
            Assert.That(_model.GetDamageMultiplier(Sword), Is.EqualTo(1f));
        }

        [Test]
        public void BowSkill3_LocksWeaponAndHalvesAttackCooldown()
        {
            _model.SetStatueLevel(7);

            Assert.That(_model.IsWeaponLocked(Bow), Is.False);
            Assert.That(_model.GetAttackCooldownMultiplier(Bow), Is.EqualTo(1f));

            _model.Cast(Bow, Skill3, false);

            Assert.That(_model.IsWeaponLocked(Bow), Is.True);
            Assert.That(_model.GetAttackCooldownMultiplier(Bow), Is.EqualTo(0.5f));
            Assert.That(_model.IsWeaponLocked(Sword), Is.False, "clone mode is bow-only");
        }

        [Test]
        public void BerserkerMultipliers_DoNotLeakToTheBow()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill3, false);

            Assert.That(_model.GetSpeedMultiplier(Bow), Is.EqualTo(1f));
            Assert.That(_model.GetDamageMultiplier(Bow), Is.EqualTo(1f));
        }

        [Test]
        public void Skill2Effects_DifferPerWeapon()
        {
            _model.SetStatueLevel(5);

            _model.Cast(Sword, Skill2, false);
            Assert.That(_model.IsTimeSlashActive(Sword), Is.True);
            Assert.That(_model.GetEffectRemaining(Sword, Skill2), Is.EqualTo(6f).Within(0.001f), "360 frames");

            _model.Cast(Bow, Skill2, false);
            Assert.That(_model.IsTimeSlashActive(Bow), Is.False, "bow skill 2 is lock shoot, not a slash");
            Assert.That(_model.GetEffectRemaining(Bow, Skill2), Is.EqualTo(8f).Within(0.001f), "480 frames");
        }

        [Test]
        public void Reset_ClearsCooldownsAndEffects()
        {
            _model.SetStatueLevel(7);
            _model.Cast(Sword, Skill3, false);

            _model.Reset();

            Assert.That(_model.IsEffectActive(Sword, Skill3), Is.False);
            Assert.That(_model.IsOnCooldown(Sword, Skill3), Is.False);
        }
    }
}
