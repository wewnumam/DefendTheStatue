using NUnit.Framework;
using ProjectTA.Module.Statue;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Statue health and the level 1-7 upgrade ladder, matching the reference game's upgrade
    /// section: 200 base HP, +100 at level 6, +200 at level 7, cost 500 x level.
    /// </summary>
    public class StatueModelTests
    {
        private StatueModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new StatueModel();
            _model.Reset();
        }

        [Test]
        public void Reset_StartsAtLevelOneWithReferenceHealth()
        {
            Assert.That(_model.Level, Is.EqualTo(1));
            Assert.That(_model.MaxHealth, Is.EqualTo(200f));
            Assert.That(_model.Health, Is.EqualTo(200f));
            Assert.That(_model.IsAlive, Is.True);
        }

        [Test]
        public void UpgradeCost_ScalesWithCurrentLevel()
        {
            Assert.That(_model.UpgradeCost, Is.EqualTo(500));

            _model.Upgrade();
            Assert.That(_model.UpgradeCost, Is.EqualTo(1000));

            _model.Upgrade();
            Assert.That(_model.UpgradeCost, Is.EqualTo(1500));
        }

        [Test]
        public void CanUpgrade_NeedsEnoughGoldAndARemainingLevel()
        {
            Assert.That(_model.CanUpgrade(499), Is.False);
            Assert.That(_model.CanUpgrade(500), Is.True);

            for (int level = 1; level < StatueModel.MaxLevel; level++)
            {
                _model.Upgrade();
            }

            Assert.That(_model.Level, Is.EqualTo(StatueModel.MaxLevel));
            Assert.That(_model.CanUpgrade(int.MaxValue), Is.False);
        }

        [Test]
        public void Upgrade_AppliesLevelEffectsInOrder()
        {
            StatueUpgradeEffect territory = _model.Upgrade();
            Assert.That(territory.UnlocksTerritory, Is.True);
            Assert.That(territory.UnlockedTerritoryIndex, Is.EqualTo(1));

            StatueUpgradeEffect skill1 = _model.Upgrade();
            Assert.That(skill1.SpawnsGuardFoundations, Is.True);
            Assert.That(skill1.GuardFoundationLevel, Is.EqualTo(3));
            Assert.That(skill1.UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill1));

            StatueUpgradeEffect territory2 = _model.Upgrade();
            Assert.That(territory2.UnlockedTerritoryIndex, Is.EqualTo(2));

            StatueUpgradeEffect skill2 = _model.Upgrade();
            Assert.That(skill2.GuardFoundationLevel, Is.EqualTo(5));
            Assert.That(skill2.UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill2));

            StatueUpgradeEffect bump = _model.Upgrade();
            Assert.That(bump.MaxHealthDelta, Is.EqualTo(100f));
            Assert.That(_model.MaxHealth, Is.EqualTo(300f));
            Assert.That(_model.Health, Is.EqualTo(300f));

            StatueUpgradeEffect final = _model.Upgrade();
            Assert.That(final.MaxHealthDelta, Is.EqualTo(200f));
            Assert.That(final.UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill3));
            Assert.That(_model.MaxHealth, Is.EqualTo(500f));
            Assert.That(_model.Health, Is.EqualTo(500f));
        }

        [Test]
        public void Upgrade_AtMaxLevelIsANoOp()
        {
            for (int level = 1; level < StatueModel.MaxLevel; level++)
            {
                _model.Upgrade();
            }

            float maxHealthAtCap = _model.MaxHealth;
            _model.Upgrade();

            Assert.That(_model.Level, Is.EqualTo(StatueModel.MaxLevel));
            Assert.That(_model.MaxHealth, Is.EqualTo(maxHealthAtCap));
        }

        [Test]
        public void Effects_AreReadableWithoutUpgrading()
        {
            Assert.That(StatueModel.GetEffect(1).UnlocksTerritory, Is.False);
            Assert.That(StatueModel.GetEffect(2).UnlockedTerritoryIndex, Is.EqualTo(1));
            Assert.That(StatueModel.GetEffect(4).UnlockedTerritoryIndex, Is.EqualTo(2));
            Assert.That(StatueModel.GetEffect(6).MaxHealthDelta, Is.EqualTo(100f));
            Assert.That(StatueModel.GetEffect(7).MaxHealthDelta, Is.EqualTo(200f));

            // Clamped, so an out-of-range query still answers something sane.
            Assert.That(StatueModel.GetEffect(0).Level, Is.EqualTo(1));
            Assert.That(StatueModel.GetEffect(99).Level, Is.EqualTo(StatueModel.MaxLevel));
        }

        [Test]
        public void ApplyDamage_ClampsAtZeroAndReportsFurtherHits()
        {
            Assert.That(_model.ApplyDamage(50f), Is.True);
            Assert.That(_model.Health, Is.EqualTo(150f));

            Assert.That(_model.ApplyDamage(1000f), Is.True);
            Assert.That(_model.Health, Is.EqualTo(0f));
            Assert.That(_model.IsAlive, Is.False);

            Assert.That(_model.ApplyDamage(10f), Is.False, "destroyed statue ignores more damage");
        }

        [Test]
        public void Heal_ClampsAtMaximum()
        {
            _model.ApplyDamage(100f);
            _model.Heal(30f);
            Assert.That(_model.Health, Is.EqualTo(130f));

            _model.Heal(1000f);
            Assert.That(_model.Health, Is.EqualTo(_model.MaxHealth));
        }
    }
}
