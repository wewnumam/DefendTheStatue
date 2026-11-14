using NUnit.Framework;
using ProjectTA.Module.Monster;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Monster stats and the spawn-bookkeeping rules. Values are the reference game's spawnMonster:
    /// 20 HP / 10 damage / 5 gold, doubled and tripled inside the statue heal radius.
    /// </summary>
    public class MonsterModelTests
    {
        private MonsterModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new MonsterModel();
        }

        [Test]
        public void SpawnOutsideHealRadius_UsesTheBaseReferenceStats()
        {
            int index = _model.Add(new Vector2(500f, 0f), false);
            MonsterState monster = _model.Get(index);

            Assert.That(_model.Count, Is.EqualTo(1));
            Assert.That(monster.Health, Is.EqualTo(20f));
            Assert.That(monster.MaxHealth, Is.EqualTo(20f));
            Assert.That(monster.Damage, Is.EqualTo(10f));
            Assert.That(monster.GoldValue, Is.EqualTo(5));
            Assert.That(monster.InHealRadius, Is.False);
            Assert.That(MonsterState.Size, Is.EqualTo(25f));
            Assert.That(MonsterState.Speed, Is.EqualTo(1.5f));
        }

        [Test]
        public void SpawnInsideHealRadius_IsTougherAndWorthMoreGold()
        {
            MonsterState monster = _model.Get(_model.Add(new Vector2(10f, 10f), true));

            Assert.That(monster.Health, Is.EqualTo(40f));
            Assert.That(monster.Damage, Is.EqualTo(30f));
            Assert.That(monster.GoldValue, Is.EqualTo(15));
            Assert.That(monster.InHealRadius, Is.True);
        }

        [Test]
        public void Damage_PaysGoldOnlyOnTheKillingBlow()
        {
            int index = _model.Add(Vector2.zero, false);

            Assert.That(_model.ApplyDamage(index, 15f), Is.EqualTo(0), "still alive");
            Assert.That(_model.Get(index).Health, Is.EqualTo(5f));
            Assert.That(_model.Get(index).IsDead, Is.False);

            Assert.That(_model.ApplyDamage(index, 5f), Is.EqualTo(5), "killing blow pays out");
            Assert.That(_model.Get(index).IsDead, Is.True);

            Assert.That(_model.ApplyDamage(index, 100f), Is.EqualTo(0), "a corpse pays once");
        }

        [Test]
        public void Damage_OverkillClampsHealthAtZero()
        {
            int index = _model.Add(Vector2.zero, true);
            _model.ApplyDamage(index, 999f);

            Assert.That(_model.Get(index).Health, Is.EqualTo(0f));
        }

        [Test]
        public void Damage_IgnoresUnknownIndexesAndNonPositiveAmounts()
        {
            int index = _model.Add(Vector2.zero, false);

            Assert.That(_model.ApplyDamage(42, 10f), Is.EqualTo(0));
            Assert.That(_model.ApplyDamage(index, 0f), Is.EqualTo(0));
            Assert.That(_model.Get(index).Health, Is.EqualTo(20f));
        }

        [Test]
        public void FindNearestInRange_PicksTheClosestLiveMonster()
        {
            _model.Add(new Vector2(100f, 0f), false);
            _model.Add(new Vector2(10f, 0f), false);
            _model.Add(new Vector2(900f, 0f), false);

            Assert.That(_model.FindNearestInRange(Vector2.zero, 300f), Is.EqualTo(1));
        }

        [Test]
        public void FindNearestInRange_ReturnsMinusOneWhenNothingIsClose()
        {
            _model.Add(new Vector2(1000f, 0f), false);

            Assert.That(_model.FindNearestInRange(Vector2.zero, 300f), Is.EqualTo(-1));
            Assert.That(_model.FindNearestInRange(Vector2.zero, 0f), Is.EqualTo(-1));
        }

        [Test]
        public void FindNearestInRange_SkipsTheDead()
        {
            int dying = _model.Add(new Vector2(5f, 0f), false);
            _model.Add(new Vector2(50f, 0f), false);
            _model.ApplyDamage(dying, 20f);

            Assert.That(_model.FindNearestInRange(Vector2.zero, 300f), Is.EqualTo(1));
        }

        [Test]
        public void RemoveDead_DropsCorpsesAndKeepsTheRest()
        {
            int first = _model.Add(new Vector2(5f, 0f), false);
            _model.Add(new Vector2(50f, 0f), false);
            _model.ApplyDamage(first, 20f);

            Assert.That(_model.RemoveDead(), Is.EqualTo(1));
            Assert.That(_model.Count, Is.EqualTo(1));
            Assert.That(_model.RemoveDead(), Is.EqualTo(0));
        }

        [Test]
        public void AreaDamage_HitsEverythingInsideTheRadiusOnly()
        {
            _model.Add(new Vector2(10f, 0f), false);
            _model.Add(new Vector2(59f, 0f), false);
            _model.Add(new Vector2(61f, 0f), false);
            var hit = new System.Collections.Generic.List<int>();

            _model.ApplyAreaDamage(Vector2.zero, 60f, 5f, hit);

            Assert.That(hit, Is.EqualTo(new[] { 0, 1 }), "the reference sword radius is 60");
            Assert.That(_model.Get(0).Health, Is.EqualTo(15f));
            Assert.That(_model.Get(2).Health, Is.EqualTo(20f), "outside the swing");
        }

        [Test]
        public void AreaDamage_PaysForEveryKillInTheSwing()
        {
            _model.Add(Vector2.zero, false);
            _model.Add(new Vector2(5f, 0f), true);
            var hit = new System.Collections.Generic.List<int>();

            int gold = _model.ApplyAreaDamage(Vector2.zero, 60f, 40f, hit);

            Assert.That(gold, Is.EqualTo(20), "5 for the small one, 15 for the one inside the heal radius");
            Assert.That(hit.Count, Is.EqualTo(2));
        }

        [Test]
        public void AreaDamage_OncePerMonster_OnlyHitsEachTargetOneTimePerDash()
        {
            int first = _model.Add(Vector2.zero, false);
            _model.Add(new Vector2(5f, 0f), false);
            var hit = new System.Collections.Generic.List<int>();

            Assert.That(_model.ApplyAreaDamage(Vector2.zero, 40f, 5f, hit, true), Is.EqualTo(0));
            Assert.That(hit.Count, Is.EqualTo(2));

            hit.Clear();
            Assert.That(_model.ApplyAreaDamage(Vector2.zero, 40f, 5f, hit, true), Is.EqualTo(0));
            Assert.That(hit, Is.Empty, "already hit by this dash");

            _model.ResetDashHits();
            _model.ApplyAreaDamage(Vector2.zero, 40f, 5f, hit, true);
            Assert.That(hit.Count, Is.EqualTo(2), "a new dash hits again");
            Assert.That(_model.Get(first).Health, Is.EqualTo(10f));
        }

        [Test]
        public void Advance_WalksAMonsterAtTheReferenceSpeed()
        {
            int id = _model.Add(new Vector2(0f, -400f), false);

            Assert.That(MonsterState.SpeedPerSecond, Is.EqualTo(90f), "1.5 per reference frame at 60 fps");
            Assert.That(_model.Advance(1f, Vector2.zero, MonsterState.StatueStopDistance), Is.EqualTo(1));
            Assert.That(_model.Get(id).Position.y, Is.EqualTo(-400f + 90f).Within(0.001f));
        }

        [Test]
        public void Advance_StopsOnTheStatueClearanceInsteadOfOvershooting()
        {
            int id = _model.Add(new Vector2(0f, MonsterState.StatueStopDistance + 1f), false);

            _model.Advance(10f, Vector2.zero, MonsterState.StatueStopDistance);

            Assert.That(MonsterState.StatueStopDistance, Is.EqualTo(39.5f), "(25 + 50) / 2 + 2");
            Assert.That(_model.Get(id).Position.magnitude, Is.EqualTo(MonsterState.StatueStopDistance).Within(0.01f));
        }

        [Test]
        public void Advance_LeavesSettledAndDeadMonstersWhereTheyAre()
        {
            int settled = _model.Add(new Vector2(0f, MonsterState.StatueStopDistance), false);
            int dying = _model.Add(new Vector2(0f, 500f), false);
            _model.ApplyDamage(dying, 20f);

            Assert.That(_model.Advance(1f, Vector2.zero, MonsterState.StatueStopDistance), Is.EqualTo(0));
            Assert.That(_model.Advance(0f, Vector2.zero, MonsterState.StatueStopDistance), Is.EqualTo(0));
            Assert.That(_model.Get(settled).Position.y, Is.EqualTo(MonsterState.StatueStopDistance));
            Assert.That(_model.Get(dying).Position.y, Is.EqualTo(500f));
        }

        [Test]
        public void Clear_EmptiesTheList()
        {
            _model.Add(Vector2.zero, false);
            _model.Add(Vector2.one, false);
            _model.Clear();

            Assert.That(_model.Count, Is.EqualTo(0));
            Assert.That(_model.Get(0), Is.Null);
        }
    }
}
