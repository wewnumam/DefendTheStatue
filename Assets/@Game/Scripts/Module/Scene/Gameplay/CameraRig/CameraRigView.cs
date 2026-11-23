using Agate.MVC.Base;
using Unity.Cinemachine;
using UnityEngine;

namespace ProjectTA.Module.CameraRig
{
    /// <summary>
    /// Camera follow and zoom. Cinemachine already does the follow when a CinemachineCamera is wired,
    /// so the rig only pushes its lens size; with no CinemachineCamera it falls back to a plain
    /// position lerp so the rig still behaves if the package is removed.
    /// </summary>
    public class CameraRigView : BaseView
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private float _followLerp = 0.1f;
        [SerializeField] private float _zoomLerp = 0.1f;

        private float _targetSize;

        public float CurrentSize => _camera != null ? _camera.orthographicSize : 0f;

        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
        }

        public void SetZoomTarget(float size)
        {
            _targetSize = size;
        }

        private void LateUpdate()
        {
            if (_targetSize <= 0f)
            {
                return;
            }

            if (_cinemachineCamera != null)
            {
                LensSettings lens = _cinemachineCamera.Lens;
                lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, _targetSize, _zoomLerp);
                _cinemachineCamera.Lens = lens;
                return;
            }

            if (_camera == null)
            {
                return;
            }

            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetSize, _zoomLerp);

            if (_followTarget != null)
            {
                Vector3 position = _camera.transform.position;
                Vector3 target = _followTarget.position;
                target.z = position.z;
                _camera.transform.position = Vector3.Lerp(position, target, _followLerp);
            }
        }
    }
}
