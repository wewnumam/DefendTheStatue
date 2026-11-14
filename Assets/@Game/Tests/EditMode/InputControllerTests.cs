using NUnit.Framework;
using ProjectTA.Message;
using ProjectTA.Module.Input;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// The input gate. The actions are enabled during Initialize, but the views are only wired
    /// during LaunchScene, so anything the player does in between (a pointer move, a key press)
    /// must never reach a controller that has no view yet.
    /// </summary>
    public class InputControllerTests
    {
        private class AimContext : IInputActionCallbackContext
        {
            public Vector2 ReadValue()
            {
                return new Vector2(1f, 0f);
            }
        }

        [Test]
        public void Aim_BeforeTheSceneIsReady_PublishesNothing()
        {
            var input = new InputController();

            input.StartAim(new AimContext());

            Assert.That(input.PublishedMessages, Is.Empty);
        }

        [Test]
        public void Aim_AfterSetReady_PublishesRotate()
        {
            var input = new InputController();
            input.SetReady();

            input.StartAim(new AimContext());

            Assert.That(input.PublishedMessages, Has.Exactly(1).InstanceOf<RotatePlayerCharacterMessage>());
        }
    }
}
