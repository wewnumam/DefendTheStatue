using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Scene view for the run. Owns the per-frame tick, the same way BulletPoolView does, and holds
    /// the statue transform the spawn ring is measured from.
    /// </summary>
    public class GameFlowView : BaseView
    {
        [SerializeField] private Transform _statue;
        [SerializeField] private Camera _camera;

        private UnityAction<float> _onTick;

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
        }

        /// <summary>Half extents of what the camera sees, used by the pre-wall spawn fallback.</summary>
        public void GetHalfViewSize(out float halfWidth, out float halfHeight)
        {
            Camera camera = Camera;
            if (camera == null)
            {
                halfWidth = GameFlowModel.CameraSpawnDistance * 0.5f;
                halfHeight = halfWidth;
                return;
            }

            halfHeight = camera.orthographicSize;
            halfWidth = halfHeight * camera.aspect;
        }
    }
}
