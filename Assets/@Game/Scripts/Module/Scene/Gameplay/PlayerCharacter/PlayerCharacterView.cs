using Agate.MVC.Base;
using UnityEngine;
using ProjectTA.Utility;
using ProjectTA.Module.GameConstants;
using UnityEngine.InputSystem;
using NaughtyAttributes;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterView : BaseView
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Camera _mainCamera;

        private Vector2 _direction;
        private Vector2 _aimScreenPosition;
        private Vector2 _smoothedAim;

        [field: SerializeField]
        public PlayerConstants PlayerConstants { get; set; }

        private void Awake()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void FixedUpdate()
        {
            if (PlayerConstants == null || _rb == null)
            {
                return;
            }

            MovePlayerCharacter();
            HandleRotation();
        }

        private void MovePlayerCharacter()
        {
            // Apply both horizontal and vertical velocity for 2D top-down movement
            Vector2 targetVelocity = _direction * PlayerConstants.MovementSpeed;
            _rb.linearVelocity = targetVelocity;
        }

        private void HandleRotation()
        {
            if (_mainCamera == null) return;

            // 1. Prepare screen position with correct Z distance relative to camera
            Vector3 screenPos = _aimScreenPosition;
            screenPos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);

            // 2. Convert mouse screen coordinates to 2D world coordinates
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            // 3. Calculate direction vector pointing from player to mouse world position
            Vector2 aimDirection = ((Vector2)mouseWorldPos - _rb.position).normalized;

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                RotatePlayerCharacter(aimDirection);
            }
        }

        private void RotatePlayerCharacter(Vector2 targetDirection)
        {
            // Interpolate vector for smooth rotation
            _smoothedAim = Vector2.Lerp(_smoothedAim, targetDirection, Time.fixedDeltaTime * PlayerConstants.RotationSpeed);

            if (_smoothedAim.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(_smoothedAim.y, _smoothedAim.x) * Mathf.Rad2Deg;
                _rb.MoveRotation(targetAngle);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;
        }

        /// <summary>
        /// Accepts raw screen coordinates from Input System (e.g. Mouse Position action)
        /// </summary>
        public void SetAim(Vector2 mouseScreenPosition)
        {
            _aimScreenPosition = mouseScreenPosition;
        }
    }
}
