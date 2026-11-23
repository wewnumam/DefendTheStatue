using NUnit.Framework;
using ProjectTA.Module.BalanceLog;

namespace ProjectTA.Tests.EditMode
{
    public class BalanceLogModelTests
    {
        private static BalanceLogModel EnabledModel()
        {
            var model = new BalanceLogModel();
            model.SetEnabled(true);
            return model;
        }

        [Test]
        public void DisabledModel_TakesNoSamples()
        {
            var model = new BalanceLogModel();

            Assert.IsFalse(model.TryTakeSample(30, out _));
            Assert.AreEqual(0, model.Samples.Count);
        }

        [Test]
        public void SamplesOnlyOnTheInterval_AndOnlyOncePerSecond()
        {
            var model = EnabledModel();

            Assert.IsFalse(model.TryTakeSample(29, out _));
            Assert.IsTrue(model.TryTakeSample(30, out BalanceLogModel.Sample sample));
            Assert.AreEqual(30, sample.Seconds);
            Assert.IsFalse(model.TryTakeSample(30, out _));
            Assert.IsTrue(model.TryTakeSample(60, out _));
            Assert.AreEqual(2, model.Samples.Count);
        }

        [Test]
        public void SampleCarriesHealthMonstersKillsAndBoss()
        {
            var model = EnabledModel();
            model.OnSpawned();
            model.OnSpawned();
            model.OnSpawned();
            model.OnKilled();
            model.OnStatueDamaged(0.75f);
            model.OnBossSpawned(90);

            model.TryTakeSample(30, out BalanceLogModel.Sample sample);

            Assert.AreEqual(0.75f, sample.HealthFraction, 0.0001f);
            Assert.AreEqual(2, sample.Monsters);
            Assert.AreEqual(1, sample.Kills);
            Assert.IsTrue(sample.BossPresent);
        }

        [Test]
        public void BossArrival_KeepsTheFirstSpawnTime()
        {
            var model = EnabledModel();

            model.OnBossSpawned(600);
            model.OnBossSpawned(610);

            Assert.AreEqual(600, model.BossArrivalSeconds);
        }

        [Test]
        public void Format_WritesTheCsvRow()
        {
            var sample = new BalanceLogModel.Sample
            {
                Seconds = 120,
                HealthFraction = 0.5f,
                Monsters = 7,
                Kills = 11,
                BossPresent = false,
            };

            Assert.AreEqual("120,0.500,7,11,0", BalanceLogModel.Format(sample));
        }

        [Test]
        public void Summary_ReportsLastSampleBossArrivalKillsAndRows()
        {
            var model = EnabledModel();
            model.OnKilled();
            model.TryTakeSample(30, out _);
            model.TryTakeSample(60, out _);
            model.OnBossSpawned(60);

            Assert.AreEqual("summary,60,60,1,2", model.Summary());
        }
    }
}
