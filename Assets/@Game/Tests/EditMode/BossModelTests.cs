using NUnit.Framework;
using ProjectTA.Module.Boss;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// The Destroyer from GDD §8: 20 HP, no projectile, no enrage, phase boundaries at 60% and 30%,
    /// a summon bundle per phase and a doubled attack rate in phase 3.
    /// </summary>
    public class BossModelTests
    {
        private BossModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new BossModel();
            _model.SetConstants(new BossConstants());
            _model.Spawn();
        }

        [Test]
        public void Spawn_ArrivesWithTwentyHealthAndItsPhaseOneMinions()
        {
            Assert.That(_model.IsActive, Is.True);
            Assert.That(_model.MaxHealth, Is.EqualTo(20f), "GDD §8");
            Assert.That(_model.Health, Is.EqualTo(20f));
            Assert.That(_model.Phase, Is.EqualTo(1));
            Assert.That(_model.PendingSummonCount, Is.EqualTo(2), "two Stackers");
        }

        [Test]
        public void PhaseForFraction_SplitsAtSixtyAndThirtyPercent()
        {
            Assert.That(BossModel.PhaseForFraction(1f), Is.EqualTo(1));
            Assert.That(BossModel.PhaseForFraction(0.61f), Is.EqualTo(1));
            Assert.That(BossModel.PhaseForFraction(0.6f), Is.EqualTo(2), "exactly 60% is phase 2");
            Assert.That(BossModel.PhaseForFraction(0.31f), Is.EqualTo(2));
            Assert.That(BossModel.PhaseForFraction(0.3f), Is.EqualTo(3), "exactly 30% is phase 3");
            Assert.That(BossModel.PhaseForFraction(0f), Is.EqualTo(3));
        }

        [Test]
        public void ApplyDamage_CrossesIntoPhaseTwoAndOwesItsBundle()
        {
            bool changed = _model.ApplyDamage(_model.MaxHealth * 0.41f);

            Assert.That(changed, Is.True);
            Assert.That(_model.Phase, Is.EqualTo(2));
            Assert.That(_model.PendingSummonCount, Is.EqualTo(4), "phase 1's two plus phase 2's two");
        }

        [Test]
        public void PhaseThree_SummonsAWaltroyAndADeasor()
        {
            _model.ApplyDamage(_model.MaxHealth * 0.41f);
            _model.TakeSummons();

            bool changed = _model.ApplyDamage(_model.MaxHealth * 0.31f);
            var summons = _model.TakeSummons();

            Assert.That(changed, Is.True);
            Assert.That(_model.Phase, Is.EqualTo(3));
            Assert.That(summons.Count, Is.EqualTo(2));
            Assert.That(summons[0], Is.EqualTo(EnumManager.MonsterKind.Waltroy));
            Assert.That(summons[1], Is.EqualTo(EnumManager.MonsterKind.Deasor));
        }

        [Test]
        public void Escort_IsFourStackersTwoWaltroysTwoDeasors()
        {
            var escort = _model.Constants.Escort;
            int stackers = 0;
            int waltroys = 0;
            int deasors = 0;

            for (int i = 0; i < escort.Count; i++)
            {
                if (escort[i] == EnumManager.MonsterKind.Stacker)
                {
                    stackers++;
                }
                else if (escort[i] == EnumManager.MonsterKind.Waltroy)
                {
                    waltroys++;
                }
                else
                {
                    deasors++;
                }
            }

            Assert.That(escort.Count, Is.EqualTo(8));
            Assert.That(stackers, Is.EqualTo(4));
            Assert.That(waltroys, Is.EqualTo(2));
            Assert.That(deasors, Is.EqualTo(2));
        }

        [Test]
        public void ApplyDamage_DoesNotRepeatAPhaseChange()
        {
            _model.ApplyDamage(_model.MaxHealth * 0.41f);

            Assert.That(_model.ApplyDamage(1f), Is.False);
        }

        [Test]
        public void TakeSummons_DrainsTheOwedBundleOnce()
        {
            Assert.That(_model.TakeSummons().Count, Is.EqualTo(2));
            Assert.That(_model.TakeSummons(), Is.Empty);
        }

        [Test]
        public void AttackRate_DoublesOnlyInPhaseThree()
        {
            Assert.That(_model.AttackIntervalSeconds, Is.EqualTo(1f).Within(0.001f));

            _model.ApplyDamage(_model.MaxHealth * 0.41f);
            Assert.That(_model.AttackIntervalSeconds, Is.EqualTo(1f).Within(0.001f));

            _model.ApplyDamage(_model.MaxHealth * 0.31f);
            Assert.That(_model.AttackIntervalSeconds, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void TryAttack_RespectsTheCooldown()
        {
            Assert.That(_model.TryAttack(), Is.True);
            Assert.That(_model.TryAttack(), Is.False);

            _model.Tick(_model.AttackIntervalSeconds);

            Assert.That(_model.TryAttack(), Is.True);
        }

        [Test]
        public void ApplyDamage_ToZeroHealthKillsWithoutAPhaseChange()
        {
            bool changed = _model.ApplyDamage(_model.MaxHealth);

            Assert.That(changed, Is.False);
            Assert.That(_model.IsAlive, Is.False);
            Assert.That(_model.Health, Is.EqualTo(0f));
        }

        [Test]
        public void GodMode_IsRememberedAndClearedByReset()
        {
            _model.SetGodMode(true);
            Assert.That(_model.IsGodMode, Is.True);

            _model.Reset();
            Assert.That(_model.IsGodMode, Is.False);
        }

        [Test]
        public void Reset_ClearsTheBoss()
        {
            _model.ApplyDamage(5f);
            _model.Reset();

            Assert.That(_model.IsActive, Is.False);
            Assert.That(_model.IsAlive, Is.False);
            Assert.That(_model.Phase, Is.EqualTo(0));
            Assert.That(_model.PendingSummonCount, Is.EqualTo(0));
        }
    }
}
