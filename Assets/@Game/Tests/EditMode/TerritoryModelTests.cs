using NUnit.Framework;
using ProjectTA.Module.Territory;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// The three concentric territories: sizes, unlock state, four towers per ring and eight wall
    /// segments per ring, exactly as the reference game defines them.
    /// </summary>
    public class TerritoryModelTests
    {
        private TerritoryModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new TerritoryModel();
            _model.Reset();
        }

        [Test]
        public void Reset_CreatesTheThreeReferenceRings()
        {
            Assert.That(_model.Count, Is.EqualTo(3));

            Assert.That(_model.Get(0).Name, Is.EqualTo("Wilayah 1"));
            Assert.That(_model.Get(0).Width, Is.EqualTo(800f));
            Assert.That(_model.Get(0).Height, Is.EqualTo(600f));

            Assert.That(_model.Get(1).Width, Is.EqualTo(1600f));
            Assert.That(_model.Get(1).Height, Is.EqualTo(1200f));

            Assert.That(_model.Get(2).Width, Is.EqualTo(3200f));
            Assert.That(_model.Get(2).Height, Is.EqualTo(2400f));

            Assert.That(_model.Get(0).HalfWidth, Is.EqualTo(400f));
            Assert.That(_model.Get(2).HalfHeight, Is.EqualTo(1200f));
        }

        [Test]
        public void Reset_OnlyTheInnerRingIsUnlocked()
        {
            Assert.That(_model.Get(0).Unlocked, Is.True);
            Assert.That(_model.Get(1).Unlocked, Is.False);
            Assert.That(_model.Get(2).Unlocked, Is.False);
            Assert.That(_model.HighestUnlockedIndex, Is.EqualTo(0));
        }

        [Test]
        public void Unlock_RaisesTheHighestUnlockedIndex()
        {
            _model.Unlock(2);

            Assert.That(_model.Get(2).Unlocked, Is.True);
            Assert.That(_model.HighestUnlockedIndex, Is.EqualTo(2));
        }

        [Test]
        public void UnknownIndexesAreIgnored()
        {
            _model.Unlock(7);
            _model.MarkTowerBuilt(-1, 0);
            _model.MarkWallSegmentBuilt(9, 0);

            Assert.That(_model.Get(7), Is.Null);
            Assert.That(_model.HighestUnlockedIndex, Is.EqualTo(0));
        }

        [Test]
        public void TowersBuilt_CountsUpToFourAndThenReportsComplete()
        {
            for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
            {
                Assert.That(_model.Get(0).AllTowersBuilt, Is.False);
                _model.MarkTowerBuilt(0, corner);
            }

            Assert.That(_model.Get(0).TowerBuiltCount, Is.EqualTo(4));
            Assert.That(_model.Get(0).AllTowersBuilt, Is.True);
        }

        [Test]
        public void MarkingTheSameTowerTwiceKeepsTheCount()
        {
            _model.MarkTowerBuilt(0, 1);
            _model.MarkTowerBuilt(0, 1);

            Assert.That(_model.Get(0).TowerBuiltCount, Is.EqualTo(1));
        }

        [Test]
        public void OutOfRangeCornerIsIgnored()
        {
            _model.MarkTowerBuilt(0, 9);
            _model.MarkWallSegmentBuilt(0, 9);

            Assert.That(_model.Get(0).TowerBuiltCount, Is.EqualTo(0));
            Assert.That(_model.Get(0).WallBuiltCount, Is.EqualTo(0));
        }

        [Test]
        public void WallsBuilt_CountsUpToEightAndThenReportsComplete()
        {
            for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
            {
                _model.MarkWallSegmentBuilt(0, segment);
            }

            Assert.That(_model.Get(0).WallBuiltCount, Is.EqualTo(8));
            Assert.That(_model.Get(0).AllWallBuilt, Is.True);
        }

        [Test]
        public void OutermostWallIndex_FindsTheOuterRingOnlyOnceItHasWalls()
        {
            Assert.That(_model.OutermostWallIndex, Is.EqualTo(-1));

            _model.MarkWallSegmentBuilt(0, 0);
            Assert.That(_model.OutermostWallIndex, Is.EqualTo(0));

            _model.MarkWallSegmentBuilt(2, 5);
            Assert.That(_model.OutermostWallIndex, Is.EqualTo(2));
        }
    }
}
