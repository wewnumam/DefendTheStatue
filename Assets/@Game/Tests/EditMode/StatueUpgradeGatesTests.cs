using NUnit.Framework;
using ProjectTA.Module.Statue;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// GDD §5 upgrade gates as the panel prints them: one row per requirement per target level, with
    /// the run's progress. Pure rules, so the headless harness can run them.
    /// </summary>
    public class StatueUpgradeGatesTests
    {
        private static bool Ready(int next, int walls0, int guards3, int walls1, int guards5, int kills, bool boss)
        {
            return StatueUpgradeGates.IsReady(next, walls0, guards3, walls1, guards5, kills, boss);
        }

        [Test]
        public void Level2_HasNoGate()
        {
            Assert.That(Ready(2, 0, 0, 0, 0, 0, false), Is.True);
            Assert.That(StatueUpgradeGates.Describe(2, 0, 0, 0, 0, 0, false).Length, Is.EqualTo(0));
        }

        [Test]
        public void Level3_WaitsForAllInnerWalls()
        {
            Assert.That(Ready(3, 7, 0, 0, 0, 0, false), Is.False);
            Assert.That(Ready(3, 8, 0, 0, 0, 0, false), Is.True);

            StatueGateLine row = StatueUpgradeGates.Describe(3, 7, 0, 0, 0, 0, false)[0];
            Assert.That(row.Label, Is.EqualTo("Walls W1"));
            Assert.That(row.Current, Is.EqualTo(7));
            Assert.That(row.Target, Is.EqualTo(8));
            Assert.That(row.Met, Is.False);
            Assert.That(row.Format(), Does.Contain("7/8"));
        }

        [Test]
        public void Level4_WantsThreeInnerGuards()
        {
            Assert.That(Ready(4, 8, 2, 0, 0, 0, false), Is.False);
            Assert.That(Ready(4, 8, 3, 0, 0, 0, false), Is.True);
        }

        [Test]
        public void Level5_WaitsForAllSecondRingWalls()
        {
            Assert.That(Ready(5, 8, 3, 7, 0, 0, false), Is.False);
            Assert.That(Ready(5, 8, 3, 8, 0, 0, false), Is.True);
        }

        [Test]
        public void Level6_WantsSixOuterGuards()
        {
            Assert.That(Ready(6, 8, 3, 8, 5, 0, false), Is.False);
            Assert.That(Ready(6, 8, 3, 8, 6, 0, false), Is.True);
        }

        [Test]
        public void Level7_WantsOneHundredKills()
        {
            Assert.That(Ready(7, 0, 0, 0, 0, 99, false), Is.False);
            Assert.That(Ready(7, 0, 0, 0, 0, 100, false), Is.True);
        }

        [Test]
        public void Level8And9_AcceptTheBossKillInsteadOfKills()
        {
            Assert.That(Ready(8, 0, 0, 0, 0, 149, false), Is.False);
            Assert.That(Ready(8, 0, 0, 0, 0, 150, false), Is.True);
            Assert.That(Ready(8, 0, 0, 0, 0, 0, true), Is.True, "boss kills count for level 8");

            Assert.That(Ready(9, 0, 0, 0, 0, 199, false), Is.False);
            Assert.That(Ready(9, 0, 0, 0, 0, 0, true), Is.True);

            StatueGateLine bossRow = StatueUpgradeGates.Describe(8, 0, 0, 0, 0, 0, true)[0];
            Assert.That(bossRow.Met, Is.True);
        }

        [Test]
        public void Level10_DemandsBossAndTwoHundredFiftyKills()
        {
            Assert.That(Ready(10, 0, 0, 0, 0, 250, false), Is.False, "boss still alive");
            Assert.That(Ready(10, 0, 0, 0, 0, 249, true), Is.False, "kills short");
            Assert.That(Ready(10, 0, 0, 0, 0, 250, true), Is.True);

            StatueGateLine[] rows = StatueUpgradeGates.Describe(10, 0, 0, 0, 0, 249, false);
            Assert.That(rows.Length, Is.EqualTo(2));
            Assert.That(rows[0].Met, Is.False);
            Assert.That(rows[1].Current, Is.EqualTo(249));
        }
    }
}
