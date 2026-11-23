using NUnit.Framework;
using ProjectTA.Module.Trap;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Spike trap rules. Reference game: one trap per wall gap, armed by the player, one durability
    /// per trigger, a 45-frame cooldown between hits, and the trap goes dark at zero durability.
    /// </summary>
    public class TrapModelTests
    {
        private TrapModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new TrapModel();
        }

        [Test]
        public void Constants_MatchTheReferenceTrap()
        {
            Assert.That(_model.Constants.Cost, Is.EqualTo(5));
            Assert.That(_model.Constants.Damage, Is.EqualTo(1));
            Assert.That(_model.Constants.MaxDurability, Is.EqualTo(10));
            Assert.That(_model.Constants.CooldownSeconds, Is.EqualTo(0.75f));
        }

        [Test]
        public void AddFoundation_RejectsADuplicateGap()
        {
            int first = _model.AddFoundation(0, 0, Vector2.zero);
            int second = _model.AddFoundation(0, 0, Vector2.zero);

            Assert.That(first, Is.EqualTo(0));
            Assert.That(second, Is.EqualTo(-1));
            Assert.That(_model.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryBuild_ArmsTheFoundationOnce()
        {
            int index = _model.AddFoundation(0, 0, Vector2.zero);

            Assert.That(_model.TryBuild(index), Is.True);
            Assert.That(_model.TryBuild(index), Is.False);

            TrapState trap = _model.Get(index);
            Assert.That(trap.IsArmed, Is.True);
            Assert.That(trap.Durability, Is.EqualTo(_model.Constants.MaxDurability));
        }

        [Test]
        public void TryTrigger_DecrementsDurabilityOncePerTrigger()
        {
            int index = _model.AddFoundation(0, 0, Vector2.zero);
            _model.TryBuild(index);

            Assert.That(_model.TryTrigger(index), Is.True);
            Assert.That(_model.Get(index).Durability, Is.EqualTo(_model.Constants.MaxDurability - 1));
        }

        [Test]
        public void TryTrigger_IsBlockedByCooldown_UntilItTicksDown()
        {
            int index = _model.AddFoundation(0, 0, Vector2.zero);
            _model.TryBuild(index);

            _model.TryTrigger(index);
            Assert.That(_model.TryTrigger(index), Is.False);

            _model.Tick(_model.Constants.CooldownSeconds);
            Assert.That(_model.Get(index).CooldownRemaining, Is.EqualTo(0f));
            Assert.That(_model.TryTrigger(index), Is.True);
        }

        [Test]
        public void TryTrigger_DeactivatesAtZeroDurability()
        {
            int index = _model.AddFoundation(0, 0, Vector2.zero);
            _model.TryBuild(index);

            for (int i = 0; i < _model.Constants.MaxDurability; i++)
            {
                _model.Tick(_model.Constants.CooldownSeconds);
                Assert.That(_model.TryTrigger(index), Is.True);
            }

            TrapState trap = _model.Get(index);
            Assert.That(trap.Durability, Is.EqualTo(0));
            Assert.That(trap.IsArmed, Is.False);
            Assert.That(_model.TryTrigger(index), Is.False);
        }

        [Test]
        public void TryTrigger_RejectsAnUnbuiltFoundation()
        {
            int index = _model.AddFoundation(0, 0, Vector2.zero);

            Assert.That(_model.TryTrigger(index), Is.False);
        }

        [Test]
        public void Clear_RemovesEveryTrap()
        {
            _model.AddFoundation(0, 0, Vector2.zero);
            _model.AddFoundation(0, 1, Vector2.zero);

            _model.Clear();

            Assert.That(_model.Count, Is.EqualTo(0));
        }
    }
}
