using NUnit.Framework;
using ProjectTA.Module.GameStateUi;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>Results numbers and their exact panel text.</summary>
    public class GameStateUiModelTests
    {
        [Test]
        public void Snapshot_StartsAtZeroAndLevelOne()
        {
            GameStateUiModel model = new GameStateUiModel();
            model.Reset();

            RunStats stats = model.Snapshot();

            Assert.That(stats.Seconds, Is.EqualTo(0f));
            Assert.That(stats.Kills, Is.EqualTo(0));
            Assert.That(stats.StatueLevel, Is.EqualTo(1));
        }

        [Test]
        public void Format_PrintsOneStatPerLine()
        {
            RunStats stats = new RunStats(92f, 12, 3, 4, 7, 1500, 5, 9, 2, 1, 2, 14f);

            Assert.That(
                stats.Format(),
                Is.EqualTo(
                    "Time 01:32\n"
                    + "Kills 12 (Stacker 9 / Waltroy 2 / Deasor 1)\n"
                    + "Deaths 2\n"
                    + "Max mana 14\n"
                    + "Skill 3 used 3\n"
                    + "Traps triggered 4\n"
                    + "Trees chopped 7\n"
                    + "Essence 1500\n"
                    + "Statue level 5"));
        }

        [Test]
        public void Counters_AccumulateAndReset()
        {
            GameStateUiModel model = new GameStateUiModel();
            model.Reset();
            model.SetClock(61);
            model.RegisterKill();
            model.RegisterKill(ProjectTA.Utility.EnumManager.MonsterKind.Deasor);
            model.RegisterDeath();
            model.SetMaxMana(7f);
            model.RegisterSkill3();
            model.RegisterTrapTriggered();
            model.RegisterTreeChopped();
            model.SetEssence(700);
            model.SetStatueLevel(4);

            RunStats stats = model.Snapshot();
            Assert.That(stats.Seconds, Is.EqualTo(61f));
            Assert.That(stats.Kills, Is.EqualTo(2));
            Assert.That(stats.StackerKills, Is.EqualTo(1));
            Assert.That(stats.DeasorKills, Is.EqualTo(1));
            Assert.That(stats.Deaths, Is.EqualTo(1));
            Assert.That(stats.MaxMana, Is.EqualTo(7f));
            Assert.That(stats.Skill3Uses, Is.EqualTo(1));
            Assert.That(stats.TrapsTriggered, Is.EqualTo(1));
            Assert.That(stats.TreesChopped, Is.EqualTo(1));
            Assert.That(stats.Essence, Is.EqualTo(700));
            Assert.That(stats.StatueLevel, Is.EqualTo(4));

            model.Reset();
            stats = model.Snapshot();
            Assert.That(stats.Kills, Is.EqualTo(0));
            Assert.That(stats.Essence, Is.EqualTo(0));
            Assert.That(stats.StatueLevel, Is.EqualTo(1));
        }
    }
}
