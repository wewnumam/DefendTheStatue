using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Scene view for the run. Owns the per-frame tick, the same way BulletPoolView does, and holds
    /// the statue transform the spawn ring is measured from. It also applies the run's time scale,
    /// because pausing and slow motion are a global engine setting and this is the run's own view.
    /// </summary>
    public class GameFlowView : BaseView
    {
        [SerializeField] private Transform _statue;
        [SerializeField] private Camera _camera;

        [Header("Time")]
        [SerializeField] private float _slowMotionScale = 0.25f;

        private UnityAction<float> _onTick;
        private UnityAction _onPausePressed;

        /// <summary>Everything slows to this while the skill upgrade interface is open.</summary>
        public float SlowMotionScale => _slowMotionScale;

        public void SetPauseCallback(UnityAction onPausePressed)
        {
            _onPausePressed = onPausePressed;
        }

        /// <summary>Only GameFlow writes this — 0 pauses, SlowMotionScale slows, 1 is normal.</summary>
        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Max(0f, scale);
        }

        /// <summary>Origin of the ring. Falls back to the world origin when nothing is wired yet.</summary>
        public Vector2 StatuePosition => _statue != null
            ? (Vector2)_statue.position
            : Vector2.zero;

        public Camera Camera => _camera != null ? _camera : Camera.main;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);

            // ponytail: raw Escape read, the same way BuildView reads E/F — route it through
            // InputController once the input asset grows a Pause action.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _onPausePressed?.Invoke();
            }
        }

        /// <summary>Fallback half-view when no camera is wired, fed from MonsterConstants by the controller.</summary>
        private float _fallbackHalfViewSize = 250f;

        public void SetSpawnDistance(float cameraSpawnDistance)
        {
            _fallbackHalfViewSize = cameraSpawnDistance * 0.5f;
        }

        /// <summary>Half extents of what the camera sees, used by the pre-wall spawn fallback.</summary>
        public void GetHalfViewSize(out float halfWidth, out float halfHeight)
        {
            Camera camera = Camera;
            if (camera == null)
            {
                halfWidth = _fallbackHalfViewSize;
                halfHeight = halfWidth;
                return;
            }

            halfHeight = camera.orthographicSize;
            halfWidth = halfHeight * camera.aspect;
        }
    }
}
