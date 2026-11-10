using Agate.MVC.Base;
using ProjectTA.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.Input
{
    public class InputController : BaseController<InputController>
    {
        public InputActionManager InputActions { get; private set; }

        private const int MaxPublishedMessages = 10;
        private readonly Queue<object> _publishedMessages = new Queue<object>();

        public IEnumerable<object> PublishedMessages => _publishedMessages;

        protected override void Publish<TMessage>(TMessage message)
        {
            // Add the new message to the queue
            _publishedMessages.Enqueue(message);

            // Remove the oldest message if the queue exceeds the maximum size
            if (_publishedMessages.Count > MaxPublishedMessages)
            {
                _publishedMessages.Dequeue();
            }

            base.Publish(message);
        }

        public override IEnumerator Initialize()
        {
            yield return base.Initialize();

            InputActions = new InputActionManager();
            InputActions.Character.Enable();
            InputActions.Character.Move.performed += OnStartMove;
            InputActions.Character.Move.canceled += OnEndMove;
            InputActions.Character.Shoot.started += OnShootStart;
            InputActions.Character.Shoot.canceled += OnShootEnd;
            InputActions.Character.Aim.performed += OnStartAim;
            InputActions.Character.Aim.canceled += OnEndAim;
        }

        public override IEnumerator Terminate()
        {
            InputActions.Character.Move.performed -= OnStartMove;
            InputActions.Character.Move.canceled -= OnEndMove;
            InputActions.Character.Shoot.performed -= OnShootStart;
            InputActions.Character.Shoot.canceled -= OnShootEnd;
            InputActions.Character.Aim.performed -= OnStartAim;
            InputActions.Character.Aim.canceled -= OnEndAim;
            InputActions.Character.Disable();

            yield return base.Terminate();
        }

        private void OnStartMove(InputAction.CallbackContext context)
        {
            StartMove(new InputActionCallbackContextWrapper(context));
        }

        public void StartMove(IInputActionCallbackContext context)
        {
            Publish(new MovePlayerCharacterMessage(context.ReadValue()));
        }

        public void OnEndMove(InputAction.CallbackContext context)
        {
            Publish(new MovePlayerCharacterMessage(Vector2.zero));
        }

        public void OnShootStart(InputAction.CallbackContext context)
        {
            Publish(new PlayerCharacterShootStartMessage());
        }
        public void OnShootEnd(InputAction.CallbackContext context)
        {
            Publish(new PlayerCharacterShootEndMessage());
        }

        private void OnStartAim(InputAction.CallbackContext context)
        {
            StartAim(new InputActionCallbackContextWrapper(context));
        }

        public void StartAim(IInputActionCallbackContext context)
        {
            Publish(new RotatePlayerCharacterMessage(context.ReadValue()));
        }

        public void OnEndAim(InputAction.CallbackContext context)
        {
            Publish(new RotatePlayerCharacterMessage(Vector2.zero));
        }
    }

    public interface IInputActionCallbackContext
    {
        Vector2 ReadValue();
    }

    public class InputActionCallbackContextWrapper : IInputActionCallbackContext
    {
        private readonly InputAction.CallbackContext _context;

        public InputActionCallbackContextWrapper(InputAction.CallbackContext context)
        {
            _context = context;
        }

        public Vector2 ReadValue()
        {
            return _context.ReadValue<Vector2>();
        }
    }
}
