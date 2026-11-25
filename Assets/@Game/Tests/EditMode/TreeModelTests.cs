using System.Collections.Generic;
using NUnit.Framework;
using ProjectTA.Module.Tree;
using UnityEngine;
using ProjectTA.Module.GameConstants;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Tree placement, growth clock and clearing. Reference game: 20 trees per ring, one new tree
    /// every 3600 frames, a built tower or wall flattens what stands on it.
    /// </summary>
    public class TreeModelTests
    {
        private TreeModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new TreeModel();
        }

        [Test]
        public void Constants_MatchTheReferenceTree()
        {
            TreeKindStats red = _model.Constants.GetKind(Utility.EnumManager.TreeKind.Red);
            Assert.That(red.CountPerTerritory, Is.EqualTo(15));
            Assert.That(red.Health, Is.EqualTo(5f));
            Assert.That(_model.Constants.GrowthIntervalSeconds, Is.EqualTo(60f));
        }

        [Test]
        public void ClearAt_RemovesOnlyTreesInsideTheRadius()
        {
            _model.Add(Vector2.zero, 0);
            _model.Add(new Vector2(2f, 0f), 0);
            _model.Add(new Vector2(10f, 0f), 0);

            List<TreeState> cleared = new List<TreeState>();
            int count = _model.ClearAt(Vector2.zero, 3f, cleared);

            Assert.That(count, Is.EqualTo(2));
            Assert.That(cleared.Count, Is.EqualTo(2));
            Assert.That(_model.Count, Is.EqualTo(1));
            Assert.That(_model.Trees[0].Position, Is.EqualTo(new Vector2(10f, 0f)));
        }

        [Test]
        public void ClearAlongSegment_RemovesTreesOnTheWallLine()
        {
            _model.Add(new Vector2(0f, 0f), 0);
            _model.Add(new Vector2(0f, 4f), 0);
            _model.Add(new Vector2(20f, 0f), 0);

            List<TreeState> cleared = new List<TreeState>();
            int count = _model.ClearAlongSegment(new Vector2(-10f, 0f), new Vector2(10f, 0f), 2f, cleared);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(_model.Count, Is.EqualTo(2));
        }

        [Test]
        public void AdvanceGrowth_StaysStillWhileNoTerritoryIsUnlocked()
        {
            Assert.That(_model.AdvanceGrowth(60f, false), Is.EqualTo(0));
            Assert.That(_model.AdvanceGrowth(120f, true), Is.EqualTo(2));
            Assert.That(_model.AdvanceGrowth(30f, true), Is.EqualTo(0), "the remainder carries over");
            Assert.That(_model.AdvanceGrowth(30f, true), Is.EqualTo(1));
        }

        [Test]
        public void Damage_ChopsTheTreeAndStartsItsRespawnTimer()
        {
            TreeModel model = new TreeModel();
            model.Add(Vector2.zero, 0);
            int id = model.Trees[0].Id;

            Assert.That(model.Damage(id, 1f), Is.False, "1 of 5 health is not a chop");
            Assert.That(model.Trees[0].Health, Is.EqualTo(4f));
            Assert.That(model.Damage(id, 4f), Is.True);
            Assert.That(model.Trees[0].IsDestroyed, Is.True);
            Assert.That(model.Trees[0].CanHeal, Is.False);
        }

        [Test]
        public void DamageArea_ReportsChippedTreesTheHealthBarNeeds()
        {
            TreeModel model = new TreeModel();
            model.Add(Vector2.zero, 0);
            model.Add(new Vector2(50f, 0f), 0);
            var damaged = new System.Collections.Generic.List<TreeState>();

            int chopped = model.DamageArea(Vector2.zero, 5f, 1f, null, damaged);

            Assert.That(chopped, Is.EqualTo(0));
            Assert.That(damaged.Count, Is.EqualTo(1), "only the tree inside the radius took the hit");
            Assert.That(damaged[0].Health, Is.LessThan(damaged[0].MaxHealth), "the bar reads the ratio");
        }

        [Test]
        public void TickRespawn_RevivesAChoppedTreeAfterItsCooldown()
        {
            TreeModel model = new TreeModel();
            model.Add(Vector2.zero, 0);
            int id = model.Trees[0].Id;
            model.Damage(id, 999f);

            Assert.That(model.TickRespawn(1f), Is.EqualTo(0), "one second is not enough");
            float redRespawn = model.Constants.GetKind(Utility.EnumManager.TreeKind.Red).RespawnSeconds;
            Assert.That(model.TickRespawn(redRespawn), Is.EqualTo(1));
            Assert.That(model.Trees[0].IsDestroyed, Is.False);
            Assert.That(model.Trees[0].Health, Is.EqualTo(model.Trees[0].MaxHealth));
        }

        [Test]
        public void FindNearestHealTree_SkipsChoppedTrees()
        {
            TreeModel model = new TreeModel();
            model.Add(new Vector2(10f, 0f), 0);
            model.Add(new Vector2(200f, 0f), 0);
            int near = model.Trees[0].Id;
            model.Damage(near, 100f);

            TreeState found = model.FindNearestHealTree(Vector2.zero, 500f);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Id, Is.EqualTo(model.Trees[1].Id));
        }

        [Test]
        public void Clear_DropsEveryTreeAndResetsTheGrowthClock()
        {
            _model.Add(Vector2.zero, 0);
            _model.AdvanceGrowth(60f, true);
            _model.Clear();

            Assert.That(_model.Count, Is.EqualTo(0));
            Assert.That(_model.AdvanceGrowth(59f, true), Is.EqualTo(0));
        }
        [Test]
        public void Kinds_CarryTheirOwnHealthDropAndRespawn()
        {
            int red = _model.Add(Vector2.zero, 0, Utility.EnumManager.TreeKind.Red);
            int purple = _model.Add(new Vector2(10f, 0f), 0, Utility.EnumManager.TreeKind.Purple);
            int magic = _model.Add(new Vector2(20f, 0f), 0, Utility.EnumManager.TreeKind.Magic);
            int arcana = _model.Add(new Vector2(30f, 0f), 0, Utility.EnumManager.TreeKind.Arcana);

            Assert.That(_model.Get(red).MaxHealth, Is.EqualTo(5f));
            Assert.That(_model.Get(purple).MaxHealth, Is.EqualTo(5f));
            Assert.That(_model.Get(magic).MaxHealth, Is.EqualTo(9f));
            Assert.That(_model.Get(arcana).MaxHealth, Is.EqualTo(12f));

            Assert.That(_model.Get(purple).Stats.ManaDrop, Is.EqualTo(3f));
            Assert.That(_model.Get(red).Stats.ManaDrop, Is.EqualTo(0f), "a red tree pays no mana");
            Assert.That(_model.Get(magic).Stats.RespawnSeconds, Is.EqualTo(60f));
            Assert.That(_model.Get(arcana).Stats.RespawnSeconds, Is.EqualTo(120f));
        }

        [Test]
        public void Monsters_NeverPickAPurpleTreeToHeal()
        {
            int purple = _model.Add(Vector2.zero, 0, Utility.EnumManager.TreeKind.Purple);

            Assert.That(_model.Get(purple).IsEdibleByMonsters, Is.False);
            Assert.That(_model.FindNearestHealTree(Vector2.zero, 500f), Is.Null);
        }
    }
}
