using NUnit.Framework;
using ProjectTA.Module.AdaptiveAi;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// GDD §14: three scores make a threat level, the safety net halves it, and the spawn buff never
    /// passes 1.5x.
    /// </summary>
    public class AdaptiveAiModelTests
    {
        private AdaptiveAiModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new AdaptiveAiModel();
            _model.Reset();
        }

        [Test]
        public void CampingTheStatue_RaisesTheTurtleScoreOverTime()
        {
            _model.Tick(1f, true, 0, 1f, 1f);
            float first = _model.TurtleScore;
            Assert.That(first, Is.GreaterThan(0f));

            for (int i = 0; i < 20; i++)
            {
                _model.Tick(1f, true, 0, 1f, 1f);
            }

            Assert.That(_model.TurtleScore, Is.GreaterThan(first));
            Assert.That(_model.TurtleScore, Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void FightingAwayFromTheStatue_KeepsTheTurtleScoreDown()
        {
            for (int i = 0; i < 20; i++)
            {
                _model.Tick(1f, false, 0, 1f, 1f);
            }

            Assert.That(_model.TurtleScore, Is.EqualTo(0f));
        }

        [Test]
        public void GuardScore_IsTheGuardShareOfTheWork()
        {
            _model.RegisterGuardShot();
            _model.RegisterGuardShot();
            _model.RegisterGuardShot();
            _model.RegisterPlayerKill();

            _model.Tick(1f, false, 0, 1f, 1f);

            Assert.That(_model.GuardScore, Is.EqualTo(0.75f).Within(0.001f));
        }

        [Test]
        public void Neglect_GrowsAsTheStatueWeakensAndMonstersPress()
        {
            _model.Tick(1f, false, 0, 1f, 1f);
            Assert.That(_model.NeglectScore, Is.EqualTo(0f));

            _model.Tick(1f, false, 1, 0.4f, 1f);
            Assert.That(_model.NeglectScore, Is.GreaterThan(0.4f));
        }

        [Test]
        public void SafetyNet_HalvesTheThreatWhenSomeoneIsNearlyDead()
        {
            for (int i = 0; i < 30; i++)
            {
                _model.Tick(1f, true, 0, 1f, 1f);
            }

            float healthyThreat = _model.Threat;
            Assert.That(healthyThreat, Is.GreaterThan(0f));

            _model.Tick(1f, true, 0, 0.2f, 1f);
            Assert.That(_model.Threat, Is.LessThan(healthyThreat));
        }

        [Test]
        public void MonsterBuff_StaysInsideTheGddCap()
        {
            Assert.That(_model.MonsterBuff, Is.EqualTo(1f).Within(0.001f));

            for (int i = 0; i < 40; i++)
            {
                _model.Tick(1f, true, 0, 0.01f, 0.01f);
            }

            Assert.That(_model.MonsterBuff, Is.LessThanOrEqualTo(AdaptiveAiModel.MaxMonsterBuff));
        }
    }
}
