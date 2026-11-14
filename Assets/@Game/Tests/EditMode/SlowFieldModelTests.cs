using NUnit.Framework;
using ProjectTA.Module.SlowField;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Slow zones shared by the sword time slash and the spike traps. Anything outside, or inside an
    /// expired zone, keeps full speed; overlapping zones multiply.
    /// </summary>
    public class SlowFieldModelTests
    {
        private SlowFieldModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new SlowFieldModel();
        }

        [Test]
        public void WithoutFields_EverythingMovesAtFullSpeed()
        {
            Assert.That(_model.GetSpeedMultiplierAt(Vector2.zero, 0f), Is.EqualTo(1f));
            Assert.That(_model.IsInsideAny(Vector2.zero, 0f), Is.False);
        }

        [Test]
        public void InsideField_AppliesSlowFactor_SwordTimeSlashIsTwentyPercent()
        {
            _model.Add(Vector2.zero, 360f, 0.2f, 0f, 6f);

            Assert.That(_model.GetSpeedMultiplierAt(new Vector2(100f, 0f), 1f), Is.EqualTo(0.2f));
            Assert.That(_model.IsInsideAny(new Vector2(100f, 0f), 1f), Is.True);
        }

        [Test]
        public void OutsideFieldRadius_IsUnaffected()
        {
            _model.Add(Vector2.zero, 35f, 0.4f, 0f, 2f);

            Assert.That(_model.GetSpeedMultiplierAt(new Vector2(36f, 0f), 1f), Is.EqualTo(1f));
            Assert.That(_model.GetSpeedMultiplierAt(new Vector2(35f, 0f), 1f), Is.EqualTo(0.4f), "edge is inside");
        }

        [Test]
        public void ExpiredField_IsIgnoredAndThenRemoved()
        {
            _model.Add(Vector2.zero, 360f, 0.2f, 0f, 6f);

            Assert.That(_model.GetSpeedMultiplierAt(Vector2.zero, 6f), Is.EqualTo(1f));
            Assert.That(_model.RemoveExpired(6f), Is.EqualTo(1));
            Assert.That(_model.Fields.Count, Is.EqualTo(0));
        }

        [Test]
        public void OverlappingFields_Multiply()
        {
            _model.Add(Vector2.zero, 360f, 0.2f, 0f, 6f);
            _model.Add(Vector2.zero, 35f, 0.4f, 0f, 6f);

            Assert.That(_model.GetSpeedMultiplierAt(Vector2.zero, 1f), Is.EqualTo(0.08f).Within(0.0001f));
            Assert.That(_model.GetSpeedMultiplierAt(new Vector2(200f, 0f), 1f), Is.EqualTo(0.2f));
        }

        [Test]
        public void Clear_DropsEveryField()
        {
            _model.Add(Vector2.zero, 360f, 0.2f, 0f, 6f);
            _model.Add(new Vector2(500f, 0f), 35f, 0.4f, 0f, 2f);

            _model.Clear();

            Assert.That(_model.Fields.Count, Is.EqualTo(0));
            Assert.That(_model.GetSpeedMultiplierAt(Vector2.zero, 1f), Is.EqualTo(1f));
        }
    }
}
