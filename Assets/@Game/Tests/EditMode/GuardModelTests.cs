using NUnit.Framework;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Guard;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Guards: level 3 covers the inner territory, level 5 covers the outer ring, and a recruited
    /// guard only shoots on its cooldown. Reference game's guard rules, rebased to this scale.
    /// </summary>
    public class GuardModelTests
    {
        private GuardModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new GuardModel();
            _model.SetConstants(new GuardConstants(), new TerritoryConstants());
        }

        [Test]
        public void Constants_MatchTheReferenceGuard()
        {
            Assert.That(_model.Constants.Damage, Is.EqualTo(1), "GDD §6");
            Assert.That(_model.Constants.AttackRange, Is.EqualTo(250f));
            Assert.That(_model.Constants.AttackIntervalSeconds, Is.EqualTo(1f));
        }

        [Test]
        public void FoundationCount_IsThreeInnerSixOuterFourPerimeter()
        {
            Assert.That(GuardLayout.GetFoundationCount(0), Is.EqualTo(0));
            Assert.That(GuardLayout.GetFoundationCount(3), Is.EqualTo(3));
            Assert.That(GuardLayout.GetFoundationCount(5), Is.EqualTo(6), "the level-6 gate wants six");
            Assert.That(GuardLayout.GetFoundationCount(10), Is.EqualTo(4));
        }

        [Test]
        public void Health_FollowsTheRoleTable()
        {
            int inner = _model.AddFoundation(3, Vector2.zero);
            int outer = _model.AddFoundation(5, new Vector2(0f, 400f));
            int perimeter = _model.AddFoundation(10, new Vector2(0f, 900f));

            Assert.That(_model.Get(inner).MaxHealth, Is.EqualTo(100f));
            Assert.That(_model.Get(outer).MaxHealth, Is.EqualTo(150f));
            Assert.That(_model.Get(perimeter).MaxHealth, Is.EqualTo(200f));
        }

        [Test]
        public void Damage_KillsTheGuardAndItsSlotStopsShooting()
        {
            int index = _model.AddFoundation(3, Vector2.zero);
            _model.TryBuild(index);

            Assert.That(_model.ApplyDamage(index, 40f), Is.EqualTo(60f));
            Assert.That(_model.ApplyDamage(index, 60f), Is.EqualTo(0f));
            Assert.That(_model.Get(index).IsDead, Is.True);
            Assert.That(_model.TryFire(index), Is.False);
        }

        [Test]
        public void PerimeterGuards_CoverEverythingOutsideTheSecondTerritory()
        {
            Assert.That(_model.IsEligibleTarget(Vector2.zero, 10), Is.False, "inside everything");
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 350f), 10), Is.False, "still inside territory 2");
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 700f), 10), Is.True, "outside territory 2");
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 900f), 10), Is.True);
        }

        [Test]
        public void StateMachine_RetreatsHealsAndReturnsHome()
        {
            int index = _model.AddFoundation(3, new Vector2(0f, 200f));
            _model.TryBuild(index);
            _model.ApplyDamage(index, 80f); // 20/100 hp

            _model.Tick(0.01f, 90f, false, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Retreating));

            // Walk home: 200 units at 135/s needs ~1.5 s.
            _model.Tick(2f, 90f, false, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Healing));

            // 10%/s of 100 hp: five seconds takes 20 hp to 70.
            _model.Tick(5.5f, 90f, false, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Returning));

            _model.Tick(2f, 90f, false, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Patrol));
            Assert.That(_model.Get(index).Position, Is.EqualTo(new Vector2(0f, 200f)));
        }

        [Test]
        public void TaintedZone_PutsEveryGuardIntoPurging()
        {
            int index = _model.AddFoundation(3, Vector2.zero);
            _model.TryBuild(index);

            _model.Tick(0.01f, 90f, true, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Purging));
            Assert.That(_model.IsEligibleTarget(new Vector2(5000f, 0f), 3, GuardStateKind.Purging), Is.True);

            _model.Tick(0.01f, 90f, false, 60f);
            Assert.That(_model.Get(index).State, Is.EqualTo(GuardStateKind.Patrol));
        }

        [Test]
        public void AddFoundation_RejectsADuplicateSlot()
        {
            int first = _model.AddFoundation(3, Vector2.zero);
            int second = _model.AddFoundation(3, Vector2.zero);

            Assert.That(first, Is.EqualTo(0));
            Assert.That(second, Is.EqualTo(-1));
            Assert.That(_model.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryBuild_ArmsTheFoundationOnce()
        {
            int index = _model.AddFoundation(3, Vector2.zero);

            Assert.That(_model.TryBuild(index), Is.True);
            Assert.That(_model.TryBuild(index), Is.False);
            Assert.That(_model.Get(index).IsArmed, Is.True);
        }

        [Test]
        public void TryFire_RespectsTheCooldown()
        {
            int index = _model.AddFoundation(3, Vector2.zero);
            _model.TryBuild(index);

            Assert.That(_model.TryFire(index), Is.True);
            Assert.That(_model.TryFire(index), Is.False);

            _model.TickCooldowns(_model.Constants.AttackIntervalSeconds);

            Assert.That(_model.TryFire(index), Is.True);
        }

        [Test]
        public void TryFire_RejectsAnUnbuiltFoundation()
        {
            int index = _model.AddFoundation(3, Vector2.zero);

            Assert.That(_model.TryFire(index), Is.False);
        }

        [Test]
        public void Level3_IgnoresMonstersOutsideTheInnerTerritory()
        {
            Assert.That(_model.IsEligibleTarget(Vector2.zero, 3), Is.True);
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 250f), 3), Is.True);
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 350f), 3), Is.False);
            Assert.That(_model.IsEligibleTarget(new Vector2(1000f, 0f), 3), Is.False);
        }

        [Test]
        public void Level5_IgnoresMonstersInsideTheInnerTerritory()
        {
            Assert.That(_model.IsEligibleTarget(Vector2.zero, 5), Is.False);
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 250f), 5), Is.False);
            Assert.That(_model.IsEligibleTarget(new Vector2(0f, 350f), 5), Is.True);
            Assert.That(_model.IsEligibleTarget(new Vector2(1000f, 0f), 5), Is.True);
        }

        [Test]
        public void FindGuardFor_PicksTheNearestEligibleGuardInRange()
        {
            int inner = _model.AddFoundation(3, new Vector2(0f, 100f));
            _model.TryBuild(inner);

            // A level 5 guard that may not shoot a monster inside the inner territory.
            int outer = _model.AddFoundation(5, new Vector2(0f, 600f));
            _model.TryBuild(outer);

            Assert.That(_model.FindGuardFor(new Vector2(0f, 120f)), Is.EqualTo(inner));
            Assert.That(_model.FindGuardFor(new Vector2(0f, 700f)), Is.EqualTo(outer));
        }

        [Test]
        public void FindGuardFor_ReturnsMinusOneWhenNoGuardIsInRange()
        {
            int inner = _model.AddFoundation(3, new Vector2(0f, 100f));
            _model.TryBuild(inner);

            Assert.That(_model.FindGuardFor(new Vector2(350f, 0f)), Is.EqualTo(-1));
        }

        [Test]
        public void Clear_RemovesEveryGuard()
        {
            _model.AddFoundation(3, Vector2.zero);

            _model.Clear();

            Assert.That(_model.Count, Is.EqualTo(0));
        }
        [Test]
        public void Empower_QuadruplesDamageAndDoublesTheRateInsideTheRadius()
        {
            int inner = _model.AddFoundation(3, new Vector2(10f, 0f));
            int far = _model.AddFoundation(3, new Vector2(500f, 0f));
            _model.TryBuild(inner);
            _model.TryBuild(far);

            int empowered = _model.ApplyEmpower(Vector2.zero, 180f, 4f, 2f, 6f);

            Assert.That(empowered, Is.EqualTo(1), "only the guard inside 180");
            Assert.That(_model.GetDamage(inner), Is.EqualTo(4f), "1 base x4");
            Assert.That(_model.GetDamage(far), Is.EqualTo(1f), "untouched");

            Assert.That(_model.TryFire(inner), Is.True);
            Assert.That(_model.Get(inner).CooldownRemaining, Is.EqualTo(0.5f).Within(0.001f), "1 s / x2");
        }

        [Test]
        public void Empower_ExpiresWithItsTimer()
        {
            int index = _model.AddFoundation(3, Vector2.zero);
            _model.TryBuild(index);
            _model.ApplyEmpower(Vector2.zero, 180f, 4f, 2f, 6f);

            _model.TickCooldowns(5.9f);
            Assert.That(_model.GetDamage(index), Is.EqualTo(4f), "still empowered");

            _model.TickCooldowns(0.2f);
            Assert.That(_model.GetDamage(index), Is.EqualTo(1f), "back to base");
        }

        [Test]
        public void Empower_IgnoresUnrecruitedFoundations()
        {
            int index = _model.AddFoundation(3, Vector2.zero);

            Assert.That(_model.ApplyEmpower(Vector2.zero, 180f, 4f, 2f, 6f), Is.EqualTo(0));
            Assert.That(_model.GetDamage(index), Is.EqualTo(1f));
        }

        [Test]
        public void Patrol_WalksAroundTheFoundationWithNoMonsters()
        {
            Vector2 home = new Vector2(0f, 200f);
            int index = _model.AddFoundation(3, home);
            _model.TryBuild(index);

            _model.Tick(0.5f, 90f, false, 60f);

            Vector2 moved = _model.Get(index).Position;
            Assert.That(moved, Is.Not.EqualTo(home), "a patrolling guard must move");
            Assert.That((moved - home).magnitude, Is.LessThanOrEqualTo(_model.Constants.PatrolRadius + 0.01f));
        }

        [Test]
        public void Chase_ClosesToFireRangeAndStopsThere()
        {
            int index = _model.AddFoundation(3, new Vector2(0f, 200f));
            _model.TryBuild(index);

            Vector2 monster = new Vector2(0f, -100f); // 300 away: outside fire range, inside the leash
            _model.Tick(1f, 90f, false, 60f, new[] { monster });

            Vector2 guard = _model.Get(index).Position;
            float distance = (guard - monster).magnitude;
            Assert.That(guard.y, Is.LessThan(200f), "moved toward the monster");
            Assert.That(distance, Is.LessThanOrEqualTo(_model.Constants.AttackRange * 0.8f + 0.01f));

            _model.Tick(1f, 90f, false, 60f, new[] { monster });
            Assert.That((_model.Get(index).Position - monster).magnitude,
                Is.EqualTo(distance).Within(0.01f), "holds fire range instead of walking into the monster");
        }

        [Test]
        public void Chase_IgnoresMonstersOutsideTheGuardsZone()
        {
            Vector2 home = new Vector2(0f, 200f);
            int index = _model.AddFoundation(3, home);
            _model.TryBuild(index);

            Vector2 outside = new Vector2(0f, 900f); // beyond territory 1 and the leash
            _model.Tick(1f, 90f, false, 60f, new[] { outside });

            Vector2 guard = _model.Get(index).Position;
            Assert.That((guard - home).magnitude, Is.LessThanOrEqualTo(_model.Constants.PatrolRadius + 0.01f));
            Assert.That((guard - outside).magnitude, Is.GreaterThan(400f));
        }
    }
}
