using NUnit.Framework;
using ProjectTA.Module.Statue;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Statue health and the ten-level upgrade ladder from GDD §5: 50 HP base, +10 per level (140 at
    /// level 10), and the published price for each target level.
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
        public void Reset_StartsAtLevelOneWithFiftyHealth()
        {
            Assert.That(_model.Level, Is.EqualTo(1));
            Assert.That(_model.MaxHealth, Is.EqualTo(50f));
            Assert.That(_model.Health, Is.EqualTo(50f));
            Assert.That(_model.IsAlive, Is.True);
        }

        [Test]
        public void UpgradeCost_FollowsTheGddPriceList()
        {
            // 20 / 25 / 40 / 50 / 60 / 70 / 90 / 110 / 140 for target levels 2..10.
            int[] expected = { 20, 25, 40, 50, 60, 70, 90, 110, 140 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(_model.UpgradeCost, Is.EqualTo(expected[i]), $"cost to level {_model.Level + 1}");
                _model.Upgrade();
            }

            Assert.That(_model.UpgradeCost, Is.EqualTo(0), "no price at max level");
        }

        [Test]
        public void CanUpgrade_NeedsEnoughEssenceAndARemainingLevel()
        {
            Assert.That(_model.CanUpgrade(19), Is.False);
            Assert.That(_model.CanUpgrade(20), Is.True);

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

            StatueUpgradeEffect plain = _model.Upgrade();
            Assert.That(plain.MaxHealthDelta, Is.EqualTo(10f));
            Assert.That(plain.UnlocksSkill, Is.False);

            StatueUpgradeEffect skill3 = _model.Upgrade();
            Assert.That(skill3.UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill3));

            StatueUpgradeEffect teleport = _model.Upgrade();
            Assert.That(teleport.UnlocksTeleport, Is.True);

            StatueUpgradeEffect skill4 = _model.Upgrade();
            Assert.That(skill4.UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill4));

            StatueUpgradeEffect perimeter = _model.Upgrade();
            Assert.That(perimeter.GuardFoundationLevel, Is.EqualTo(10));
            Assert.That(_model.MaxHealth, Is.EqualTo(140f));
            Assert.That(_model.Health, Is.EqualTo(140f));
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
            Assert.That(StatueModel.GetEffect(8).UnlocksTeleport, Is.True);
            Assert.That(StatueModel.GetEffect(9).UnlockedSkill, Is.EqualTo(EnumManager.SkillSlot.Skill4));
            Assert.That(StatueModel.GetEffect(10).GuardFoundationLevel, Is.EqualTo(10));
            Assert.That(StatueModel.GetEffect(6).MaxHealthDelta, Is.EqualTo(10f));

            // Clamped, so an out-of-range query still answers something sane.
            Assert.That(StatueModel.GetEffect(0).Level, Is.EqualTo(1));
            Assert.That(StatueModel.GetEffect(99).Level, Is.EqualTo(StatueModel.MaxLevel));
        }

        [Test]
        public void ApplyDamage_ClampsAtZeroAndReportsFurtherHits()
        {
            Assert.That(_model.ApplyDamage(20f), Is.True);
            Assert.That(_model.Health, Is.EqualTo(30f));

            Assert.That(_model.ApplyDamage(1000f), Is.True);
            Assert.That(_model.Health, Is.EqualTo(0f));
            Assert.That(_model.IsAlive, Is.False);

            Assert.That(_model.ApplyDamage(10f), Is.False, "destroyed statue ignores more damage");
        }

        [Test]
        public void Heal_ClampsAtMaximum()
        {
            _model.ApplyDamage(20f);
            _model.Heal(5f);
            Assert.That(_model.Health, Is.EqualTo(35f));

            _model.Heal(1000f);
            Assert.That(_model.Health, Is.EqualTo(_model.MaxHealth));
        }
    }
}
