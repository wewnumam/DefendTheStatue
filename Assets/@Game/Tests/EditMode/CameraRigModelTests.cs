using NUnit.Framework;
using ProjectTA.Module.CameraRig;

namespace ProjectTA.Tests.EditMode
{
    public class CameraRigModelTests
    {
        [Test]
        public void DefaultSize_IsTheTargetWhenNotZoomed()
        {
            var model = new CameraRigModel();

            model.SetDefaultSize(12f);

            Assert.AreEqual(12f, model.DefaultSize, 0.0001f);
            Assert.AreEqual(12f, model.TargetSize, 0.0001f);
            Assert.IsFalse(model.IsZoomed);
        }

        [Test]
        public void Zooming_PullsBackToTheZoomFraction()
        {
            var model = new CameraRigModel();
            model.SetDefaultSize(12f);

            model.SetZoomed(true);

            Assert.IsTrue(model.IsZoomed);
            Assert.AreEqual(12f * CameraRigModel.ZoomFraction, model.TargetSize, 0.0001f);
        }

        [Test]
        public void Unzooming_ReturnsToTheDefaultSize()
        {
            var model = new CameraRigModel();
            model.SetDefaultSize(12f);
            model.SetZoomed(true);

            model.SetZoomed(false);

            Assert.IsFalse(model.IsZoomed);
            Assert.AreEqual(12f, model.TargetSize, 0.0001f);
        }

        [Test]
        public void ChangingDefaultSize_KeepsTheZoomState()
        {
            var model = new CameraRigModel();
            model.SetDefaultSize(12f);
            model.SetZoomed(true);

            model.SetDefaultSize(20f);

            Assert.AreEqual(20f, model.DefaultSize, 0.0001f);
            Assert.AreEqual(20f * CameraRigModel.ZoomFraction, model.TargetSize, 0.0001f);
        }
    }
}
