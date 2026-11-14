using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Read-only view: reports where the cursor is and when the player asked to build or upgrade.
    /// ponytail: raw keyboard reads, because there is no build menu yet. Route E and F through
    /// InputController once the menu lands.
    /// </summary>
    public class BuildView : BaseView
    {
        [SerializeField] private Camera _camera;

        private UnityAction<Vector2> _onBuild;
        private UnityAction _onUpgrade;

        public Vector2 CursorPosition
        {
            get
            {
                Camera camera = _camera != null ? _camera : Camera.main;
                if (camera == null || Mouse.current == null)
                {
                    return Vector2.zero;
                }

                return camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
        }

        public void SetCallbacks(UnityAction<Vector2> onBuild, UnityAction onUpgrade)
        {
            _onBuild = onBuild;
            _onUpgrade = onUpgrade;
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                _onBuild?.Invoke(CursorPosition);
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                _onUpgrade?.Invoke();
            }
        }
    }
}
