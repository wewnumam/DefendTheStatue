using NUnit.Framework;
using ProjectTA.Module.Tower;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Tower cadence and range. Reference game: range 300, 50 frames between shots at 60 fps,
    /// 15 damage per hit.
    /// </summary>
    public class TowerModelTests
    {
        private TowerModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new TowerModel();
        }

        [Test]
        public void Constants_MatchTheReferenceTower()
        {
            Assert.That(TowerState.Size, Is.EqualTo(32f));
            Assert.That(TowerState.AttackRange, Is.EqualTo(300f));
            Assert.That(TowerState.AttackRateFrames, Is.EqualTo(50));
            Assert.That(TowerState.Damage, Is.EqualTo(15f));
            Assert.That(GameTime.FramesToSeconds(50), Is.EqualTo(50f / 60f).Within(1e-5f));
        }

        [Test]
        public void FreshTower_FiresImmediatelyForReferenceDamage()
        {
            _model.Add(Vector2.zero, 0, 0);

            Assert.That(_model.TryFire(0, new Vector2(100f, 0f)), Is.EqualTo(15f));
        }

        [Test]
        public void AfterFiring_TheTowerWaitsFiftyFrames()
        {
            _model.Add(Vector2.zero, 0, 0);
            _model.TryFire(0, new Vector2(100f, 0f));

            float cadence = GameTime.FramesToSeconds(TowerState.AttackRateFrames);
            Assert.That(_model.Get(0).CooldownRemaining, Is.EqualTo(cadence).Within(1e-5f));
            Assert.That(_model.TryFire(0, new Vector2(100f, 0f)), Is.EqualTo(0f), "still cooling down");

            _model.TickCooldowns(cadence);
            Assert.That(_model.Get(0).IsReadyToFire, Is.True);
            Assert.That(_model.TryFire(0, new Vector2(100f, 0f)), Is.EqualTo(15f));
        }

        [Test]
        public void Cooldown_CountsDownWithoutGoingNegative()
        {
            _model.Add(Vector2.zero, 0, 0);
            _model.TryFire(0, new Vector2(100f, 0f));

            _model.TickCooldowns(100f);
            Assert.That(_model.Get(0).CooldownRemaining, Is.EqualTo(0f));
        }

        [Test]
        public void OutOfRangeTarget_IsNotFiredAtAndLeavesTheCooldownAlone()
        {
            _model.Add(Vector2.zero, 0, 0);

            Assert.That(_model.TryFire(0, new Vector2(301f, 0f)), Is.EqualTo(0f));
            Assert.That(_model.Get(0).IsReadyToFire, Is.True, "a refused shot does not start the cooldown");
        }

        [Test]
        public void Firing_AimsAtTheTarget()
        {
            _model.Add(Vector2.zero, 0, 0);
            _model.TryFire(0, new Vector2(0f, 100f));

            Assert.That(_model.Get(0).AimAngle, Is.EqualTo(Mathf.PI / 2f).Within(1e-4f));
        }

        [Test]
        public void Firing_IgnoresUnknownIndexes()
        {
            Assert.That(_model.TryFire(3, Vector2.zero), Is.EqualTo(0f));
        }

        [Test]
        public void FindShooterFor_SkipsTowersOnCooldownAndOutOfRange()
        {
            _model.Add(Vector2.zero, 0, 0);
            _model.Add(new Vector2(200f, 0f), 0, 1);

            Assert.That(_model.FindShooterFor(new Vector2(100f, 0f)), Is.EqualTo(0));

            _model.TryFire(0, new Vector2(100f, 0f));
            Assert.That(_model.FindShooterFor(new Vector2(100f, 0f)), Is.EqualTo(1));

            Assert.That(_model.FindShooterFor(new Vector2(5000f, 0f)), Is.EqualTo(-1));
        }

        [Test]
        public void FindShooterFor_IsMinusOneWithoutTowers()
        {
            Assert.That(_model.FindShooterFor(Vector2.zero), Is.EqualTo(-1));
        }

        [Test]
        public void Clear_DropsEveryTower()
        {
            _model.Add(Vector2.zero, 0, 0);
            _model.Clear();

            Assert.That(_model.Count, Is.EqualTo(0));
            Assert.That(_model.Get(0), Is.Null);
        }
    }
}
