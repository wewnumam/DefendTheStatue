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

        [Header("God statue")]
        [SerializeField] private Transform _godStatue;
        [SerializeField] private float _statuePickRadius = 150f;

        private UnityAction<Vector2> _onBuild;
        private UnityAction _onUpgrade;

        /// <summary>
        /// E is one context key: over the god statue it opens the skill upgrade interface, anywhere
        /// else it builds. The radius is authored here because the art scale is not a game constant.
        /// </summary>
        public bool IsOverStatue(Vector2 worldPosition)
        {
            if (_godStatue == null)
            {
                return false;
            }

            return ((Vector2)_godStatue.position - worldPosition).sqrMagnitude
                <= _statuePickRadius * _statuePickRadius;
        }

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
            // Build and upgrade land instantly, so they must not fire while the world is frozen.
            if (Time.timeScale <= 0f || Keyboard.current == null)
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
