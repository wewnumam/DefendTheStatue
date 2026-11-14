using NUnit.Framework;
using ProjectTA.Module.Build;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// The build and upgrade gates. Every branch of the reference game's canBuildTowerAt,
    /// canBuildWallSegmentAt and canUpgradeToLevel is covered here.
    /// </summary>
    public class BuildRulesTests
    {
        [Test]
        public void Towers_InRingOneOpenAtStatueLevelTwo()
        {
            Assert.That(BuildRules.CanBuildTowerAt(0, 1), Is.False);
            Assert.That(BuildRules.CanBuildTowerAt(0, 2), Is.True);
            Assert.That(BuildRules.CanBuildTowerAt(0, StatueModel.MaxLevel), Is.True);
        }

        [Test]
        public void Towers_InRingTwoOpenAtStatueLevelFour()
        {
            Assert.That(BuildRules.CanBuildTowerAt(1, 3), Is.False);
            Assert.That(BuildRules.CanBuildTowerAt(1, 4), Is.True);
        }

        [Test]
        public void Towers_TheOutermostRingNeverTakesTowers()
        {
            Assert.That(BuildRules.CanBuildTowerAt(2, StatueModel.MaxLevel), Is.False);
            Assert.That(BuildRules.CanBuildTowerAt(9, StatueModel.MaxLevel), Is.False);
        }

        [Test]
        public void Walls_NeedAnUnlockedRingWithEveryTowerUp()
        {
            var territory = new TerritoryState(0, "Wilayah 1", 800f, 600f, false);
            Assert.That(BuildRules.CanBuildWallSegmentAt(territory, true), Is.False, "locked ring");

            territory = new TerritoryState(0, "Wilayah 1", 800f, 600f, true);
            Assert.That(BuildRules.CanBuildWallSegmentAt(territory, true), Is.False, "towers missing");

            for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
            {
                territory.MarkTowerBuilt(corner);
            }

            Assert.That(BuildRules.CanBuildWallSegmentAt(territory, true), Is.True);
            Assert.That(BuildRules.CanBuildWallSegmentAt(null, true), Is.False);
        }

        [Test]
        public void Walls_OnTheSecondRingWaitForTheInnerRing()
        {
            var inner = new TerritoryState(0, "Wilayah 1", 800f, 600f, true);
            var second = new TerritoryState(1, "Wilayah 2", 1600f, 1200f, true);

            for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
            {
                inner.MarkTowerBuilt(corner);
                second.MarkTowerBuilt(corner);
            }

            Assert.That(BuildRules.CanBuildWallSegmentAt(second, false), Is.False);
            Assert.That(BuildRules.CanBuildWallSegmentAt(second, true), Is.True);
            Assert.That(BuildRules.CanBuildWallSegmentAt(inner, false), Is.True, "inner ring has no such gate");
        }

        [Test]
        public void UpgradeLadder_LevelTwoIsAlwaysAllowed()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(2, false, 0, false, 0, 0), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelThreeNeedsTheInnerWalls()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(3, false, 99, false, 99, 9999), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(3, true, 0, false, 0, 0), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelFourNeedsThreeInnerGuards()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(4, true, 2, false, 0, 0), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(4, true, 3, false, 0, 0), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelFiveNeedsTheSecondRingWalls()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(5, true, 3, false, 0, 0), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(5, true, 3, true, 0, 0), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelSixNeedsSixOuterGuards()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(6, true, 3, true, 5, 0), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(6, true, 3, true, 6, 0), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelSevenNeedsAHundredKills()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(7, true, 3, true, 6, 99), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(7, true, 3, true, 6, 100), Is.True);
        }

        [Test]
        public void UpgradeLadder_LevelsOutsideOneThroughSevenAreRefused()
        {
            Assert.That(BuildRules.CanUpgradeToLevel(1, true, 3, true, 6, 100), Is.False);
            Assert.That(BuildRules.CanUpgradeToLevel(8, true, 3, true, 6, 100), Is.False);
        }

        [Test]
        public void SpawnInterval_ShrinksFourteenFramesPerMinuteAndFloorsAtForty()
        {
            // max(40, 180 - minutes * 14) frames at 60 fps.
            Assert.That(BuildRules.GetSpawnIntervalFrames(0f), Is.EqualTo(180));
            Assert.That(BuildRules.GetSpawnIntervalFrames(60f), Is.EqualTo(166));
            Assert.That(BuildRules.GetSpawnIntervalFrames(300f), Is.EqualTo(110));
            Assert.That(BuildRules.GetSpawnIntervalFrames(600f), Is.EqualTo(40));
            Assert.That(BuildRules.GetSpawnIntervalFrames(3600f), Is.EqualTo(40), "floored");
        }
    }
}
