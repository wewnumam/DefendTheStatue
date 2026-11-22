using NUnit.Framework;
using ProjectTA.Module.Teleport;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// GDD §11 teleport network: level-8 unlock, 20 essence to build, 10 per trip, source then
    /// destination selection.
    /// </summary>
    public class TeleportModelTests
    {
        private TeleportModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new TeleportModel();
            _model.Reset();

            for (int i = 0; i < 3; i++)
            {
                _model.AddPod(i, new Vector2(0f, 100f * (i + 1)));
            }
        }

        [Test]
        public void Pods_CannotBeBuiltBeforeStatueLevelEight()
        {
            _model.SetStatueLevel(7);
            Assert.That(_model.IsUnlocked, Is.False);
            Assert.That(_model.CanBuild(0), Is.False);
            Assert.That(_model.Build(0), Is.False);

            _model.SetStatueLevel(8);
            Assert.That(_model.IsUnlocked, Is.True);
            Assert.That(_model.CanBuild(0), Is.True);
            Assert.That(_model.Build(0), Is.True);
            Assert.That(_model.Build(0), Is.False, "already built");
        }

        [Test]
        public void Teleport_NeedsTwoBuiltPodsAndDistinctIndexes()
        {
            _model.SetStatueLevel(8);
            _model.Build(0);

            Assert.That(_model.CanTeleport(0, 1), Is.False, "destination is still a site");
            Assert.That(_model.CanTeleport(0, 0), Is.False, "same pod");

            _model.Build(1);
            Assert.That(_model.CanTeleport(0, 1), Is.True);
            Assert.That(_model.CanTeleport(1, 0), Is.True);
        }

        [Test]
        public void SelectMode_RemembersTheSourceAndCanBeCancelled()
        {
            _model.SetStatueLevel(8);
            _model.Build(0);
            _model.Build(2);

            _model.BeginSelect(0);
            Assert.That(_model.PendingFromIndex, Is.EqualTo(0));

            _model.CancelSelect();
            Assert.That(_model.PendingFromIndex, Is.EqualTo(-1));
            Assert.That(_model.CanTeleport(0, 2), Is.True, "cancelling does not unbuild anything");
        }

        [Test]
        public void Reset_DropsTheNetworkAndTheLevelGate()
        {
            _model.SetStatueLevel(8);
            _model.Build(0);

            _model.Reset();

            Assert.That(_model.Count, Is.EqualTo(0));
            Assert.That(_model.IsUnlocked, Is.False);
            Assert.That(_model.PendingFromIndex, Is.EqualTo(-1));
        }
    }
}
