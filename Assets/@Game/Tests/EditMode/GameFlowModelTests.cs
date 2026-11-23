using NUnit.Framework;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.GameFlow;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Run clock and spawn cadence. The reference game starts at 180 frames between spawns and
    /// shaves 14 frames per elapsed minute, floored at 40.
    /// </summary>
    public class GameFlowModelTests
    {
        private GameFlowModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new GameFlowModel();
            _model.Reset();
        }

        [Test]
        public void Reset_StartsACleanRun()
        {
            Assert.That(_model.GameSeconds, Is.EqualTo(0f));
            Assert.That(_model.MonstersKilled, Is.EqualTo(0));
            Assert.That(_model.TreesChopped, Is.EqualTo(0));
            Assert.That(_model.IsDefeat, Is.False);
            Assert.That(_model.IsVictory, Is.False);
        }

        [Test]
        public void SpawnInterval_IsThreeSecondsAtTheStartAndFloorsAtTwoThirdsOfASecond()
        {
            Assert.That(_model.CurrentSpawnIntervalSeconds, Is.EqualTo(3f).Within(1e-5f));

            _model.Advance(600f);
            Assert.That(_model.CurrentSpawnIntervalSeconds, Is.EqualTo(40f / 60f).Within(1e-5f));
        }

        [Test]
        public void Advance_DoesNotSpawnBeforeTheIntervalElapses()
        {
            Assert.That(_model.Advance(1f), Is.EqualTo(0));
            Assert.That(_model.Advance(1f), Is.EqualTo(0));
            Assert.That(_model.GameSeconds, Is.EqualTo(2f).Within(1e-5f));
        }

        [Test]
        public void Advance_SpawnsOnceTheIntervalHasPassed()
        {
            Assert.That(_model.Advance(3f), Is.EqualTo(1));
        }

        [Test]
        public void Advance_CarriesTheRemainderInsteadOfLosingIt()
        {
            Assert.That(_model.Advance(4f), Is.EqualTo(1), "3s interval, 1s carried");

            // The carried second counts towards the next spawn.
            Assert.That(_model.Advance(2f), Is.EqualTo(1));
        }

        [Test]
        public void Advance_CatchesUpAfterALongHitch()
        {
            int spawns = _model.Advance(30f);

            Assert.That(spawns, Is.EqualTo(10), "30s at a 3s cadence");
        }

        [Test]
        public void Advance_IgnoresNonPositiveDeltasAndAFinishedRun()
        {
            Assert.That(_model.Advance(0f), Is.EqualTo(0));
            Assert.That(_model.Advance(-5f), Is.EqualTo(0));

            _model.MarkDefeat();
            Assert.That(_model.Advance(60f), Is.EqualTo(0));
            Assert.That(_model.GameSeconds, Is.EqualTo(0f), "a dead run does not tick");
        }

        [Test]
        public void KillsAndTrees_Accumulate()
        {
            _model.RegisterMonsterKilled(5);
            _model.RegisterMonsterKilled(15);
            _model.RegisterTreeChopped();

            Assert.That(_model.MonstersKilled, Is.EqualTo(2));
            Assert.That(_model.TreesChopped, Is.EqualTo(1));
        }

        [Test]
        public void MarkDefeat_IsSticky()
        {
            _model.MarkDefeat();
            _model.MarkDefeat();

            Assert.That(_model.IsDefeat, Is.True);
            Assert.That(_model.IsRunOver, Is.True);
        }

        [Test]
        public void MarkDefeatIfDead_OnlyFiresOnTheZeroCrossing()
        {
            Assert.That(_model.MarkDefeatIfDead(10f), Is.False);
            Assert.That(_model.IsDefeat, Is.False);

            Assert.That(_model.MarkDefeatIfDead(0f), Is.True);
            Assert.That(_model.IsDefeat, Is.True);

            Assert.That(_model.MarkDefeatIfDead(0f), Is.False, "already dead");
        }

        [Test]
        public void MarkVictory_IsFinalAndBlockedByDefeat()
        {
            Assert.That(_model.MarkVictory(), Is.True);
            Assert.That(_model.MarkVictory(), Is.False, "already won");
            Assert.That(_model.IsVictory, Is.True);

            GameFlowModel lost = new GameFlowModel();
            lost.Reset();
            lost.MarkDefeat();
            Assert.That(lost.MarkVictory(), Is.False, "a defeat cannot become a win");
            Assert.That(lost.IsVictory, Is.False);
        }

        [Test]
        public void BossTrigger_FiresExactlyOnceAtTenMinutes()
        {
            _model.Advance(599f);
            Assert.That(_model.TryTriggerBoss(), Is.False, "still 599s");

            _model.Advance(1f);
            Assert.That(_model.TryTriggerBoss(), Is.True);
            Assert.That(_model.HasBossSpawned, Is.True);

            _model.Advance(600f);
            Assert.That(_model.TryTriggerBoss(), Is.False, "the boss only arrives once");
        }

        [Test]
        public void BossTrigger_DoesNotFireAfterTheRunEnds()
        {
            _model.MarkDefeat();
            _model.Advance(600f);
            Assert.That(_model.TryTriggerBoss(), Is.False);
            Assert.That(_model.HasBossSpawned, Is.False);
        }

        [Test]
        public void Reset_ClearsTheBossFlag()
        {
            _model.Advance(600f);
            _model.TryTriggerBoss();
            _model.Reset();

            Assert.That(_model.HasBossSpawned, Is.False);
            Assert.That(_model.IsRunOver, Is.False);
        }

        [Test]
        public void RingSpawns_SitJustOutsideTheRingOnAllFourSides()
        {
            Vector2 statue = new Vector2(100f, 50f);
            const float Width = 800f;
            const float Height = 600f;

            // random01 = 0.5 puts the spawn on the ring's centre line.
            float offset = new MonsterConstants().SpawnOffset;
            Vector2 bottom = GameFlowModel.GetRingSpawnPosition(0, statue, Width, Height, 0.5f, offset);
            Vector2 right = GameFlowModel.GetRingSpawnPosition(1, statue, Width, Height, 0.5f, offset);
            Vector2 top = GameFlowModel.GetRingSpawnPosition(2, statue, Width, Height, 0.5f, offset);
            Vector2 left = GameFlowModel.GetRingSpawnPosition(3, statue, Width, Height, 0.5f, offset);

            Assert.That(bottom, Is.EqualTo(new Vector2(100f, -350f)));
            Assert.That(right, Is.EqualTo(new Vector2(600f, 50f)));
            Assert.That(top, Is.EqualTo(new Vector2(100f, 450f)));
            Assert.That(left, Is.EqualTo(new Vector2(-400f, 50f)));
        }

        [Test]
        public void RingSpawns_NeverReachTheCorner_AllTheWayOutToTheEdges()
        {
            Vector2 statue = Vector2.zero;

            float offset = new MonsterConstants().SpawnOffset;
            Vector2 atZero = GameFlowModel.GetRingSpawnPosition(0, statue, 800f, 600f, 0f, offset);
            Vector2 atOne = GameFlowModel.GetRingSpawnPosition(0, statue, 800f, 600f, 1f, offset);

            Assert.That(atZero.x, Is.EqualTo(-350f).Within(1e-5f), "half of (800 - 100)");
            Assert.That(atOne.x, Is.EqualTo(350f).Within(1e-5f));
        }

        [Test]
        public void CameraSpawns_AreFiveHundredAwayFromTheCameraCentre()
        {
            Vector2 camera = new Vector2(10f, 20f);
            float distance = new MonsterConstants().CameraSpawnDistance;

            Assert.That(GameFlowModel.GetCameraSpawnPosition(0, camera, 400f, 300f, 0.5f, distance), Is.EqualTo(new Vector2(10f, -480f)));
            Assert.That(GameFlowModel.GetCameraSpawnPosition(1, camera, 400f, 300f, 0.5f, distance), Is.EqualTo(new Vector2(510f, 20f)));
            Assert.That(GameFlowModel.GetCameraSpawnPosition(2, camera, 400f, 300f, 0.5f, distance), Is.EqualTo(new Vector2(10f, 520f)));
            Assert.That(GameFlowModel.GetCameraSpawnPosition(3, camera, 400f, 300f, 0.5f, distance), Is.EqualTo(new Vector2(-490f, 20f)));
        }

        [Test]
        public void FramesToSeconds_MatchesTheSixtyFpsReference()
        {
            Assert.That(GameTime.FramesToSeconds(60), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(GameTime.SecondsToFrames(0.5f), Is.EqualTo(30f).Within(1e-5f));
        }
    }
}
