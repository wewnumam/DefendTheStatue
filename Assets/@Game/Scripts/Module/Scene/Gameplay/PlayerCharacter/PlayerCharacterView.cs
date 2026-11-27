using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;
using ProjectTA.Module.GameConstants;
using UnityEngine.InputSystem;
using NaughtyAttributes;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterView : BaseView
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Camera _mainCamera;

        /// <summary>
        /// The Pawn controller owns all four clips (sword idle/attack, bow empty/loaded), so every
        /// sprite comes from the Animator. The names stay serialized: renaming a state is an Inspector
        /// edit, not a code edit.
        /// </summary>
        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _swordIdleState = "Pawn_SwordIdle";
        [SerializeField] private string _swordAttackState = "Pawn_SwordAttack";
        [SerializeField] private string _bowEmptyState = "Pawn_BowEmptyArrow";
        [SerializeField] private string _bowLoadedState = "Pawn_BowLoadedArrow";

        /// <summary>
        /// The art is drawn facing down, so the rig's forward is local -Y. Aiming at +X therefore needs
        /// a quarter turn: 0 rotation = looking down, 90 = looking right. The bullet spawn point and the
        /// sword hitbox are authored on -Y, so they stay in front of the player with this offset.
        /// </summary>
        [SerializeField] private float _aimRotationOffset = 90f;

        private UnityAction<float> _onTick;
        private UnityAction<bool> _onBlockChanged;

        private Vector2 _direction;
        private Vector2 _aimScreenPosition;
        private Vector2 _smoothedAim;
        private float _speedMultiplier = 1f;

        /// <summary>GDD §4 skill 1: a dash is an impulse along the cursor, not a timed walk.</summary>
        private Vector2 _dashVelocity;
        private float _dashRemaining;

        public bool IsDashing => _dashRemaining > 0f;

        /// <summary>Where the cursor points this frame; the dash uses it as its heading.</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.down;

        private int _swordIdleStateHash;
        private int _swordAttackStateHash;
        private int _bowEmptyStateHash;
        private int _bowLoadedStateHash;
        private int _currentStateHash;

        private bool _isSwordEquipped = true;
        private bool _hasArrows = true;
        private bool _isAttacking;
        private bool _attackStateEntered;

        [field: SerializeField]
        public PlayerConstants PlayerConstants { get; set; }

        private void Awake()
        {
            _swordIdleStateHash = Animator.StringToHash(_swordIdleState);
            _swordAttackStateHash = Animator.StringToHash(_swordAttackState);
            _bowEmptyStateHash = Animator.StringToHash(_bowEmptyState);
            _bowLoadedStateHash = Animator.StringToHash(_bowLoadedState);

            PlayIdleAnimation();
        }

        public void SetTickCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        /// <summary>
        /// Right mouse is the sword's block or the bow's charge. ponytail: raw mouse read, the same way
        /// BuildView reads E — the input asset has no Block action, so add one and regenerate the
        /// wrapper to route this through InputController.
        /// </summary>
        public void SetBlockCallback(UnityAction<bool> onBlockChanged)
        {
            _onBlockChanged = onBlockChanged;
        }

        private void Update()
        {
            if (_onBlockChanged == null || Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _onBlockChanged(true);
            }
            else if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                _onBlockChanged(false);
            }
        }

        /// <summary>Scroll up arms the bow, so the player character shows a bow state — the same rule Sword/BulletPool follow.</summary>
        public void SetWeaponVisual(float scrollY)
        {
            if (scrollY == 0f)
            {
                return;
            }

            _isSwordEquipped = scrollY < 0f;
            _isAttacking = false;

            PlayIdleAnimation();
        }

        /// <summary>Bow art has two states: a nocked arrow, or an empty bow at zero arrows.</summary>
        public void SetArrowState(bool hasArrows)
        {
            if (_hasArrows == hasArrows)
            {
                return;
            }

            _hasArrows = hasArrows;

            if (!_isSwordEquipped && !_isAttacking)
            {
                PlayIdleAnimation();
            }
        }

        /// <summary>
        /// Sword swing: the attack clip runs once, then the view parks itself back on idle.
        /// The bow has no attack clip, so a shot changes nothing here.
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (_animator == null || !_isSwordEquipped || _isAttacking)
            {
                return;
            }

            _isAttacking = true;
            _attackStateEntered = false;
            _currentStateHash = _swordAttackStateHash;
            _animator.Play(_swordAttackStateHash, 0, 0f);
        }

        /// <summary>Holds the idle clip for the weapon in hand; only restarts when the state actually changes.</summary>
        private void PlayIdleAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            _isAttacking = false;

            int hash = _isSwordEquipped
                ? _swordIdleStateHash
                : (_hasArrows ? _bowLoadedStateHash : _bowEmptyStateHash);

            if (_currentStateHash == hash)
            {
                return;
            }

            _currentStateHash = hash;
            _animator.Play(hash, 0, 0f);
        }

        /// <summary>A one-shot clip has no exit transition wired here, so the view owns the return to idle.</summary>
        private void TickAttackAnimation()
        {
            if (!_isAttacking || _animator == null)
            {
                return;
            }

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (!_attackStateEntered)
            {
                if (state.shortNameHash != _swordAttackStateHash)
                {
                    return;
                }

                _attackStateEntered = true;
                return;
            }

            if (state.shortNameHash == _swordAttackStateHash && state.normalizedTime < 1f)
            {
                return;
            }

            PlayIdleAnimation();
        }

        private void FixedUpdate()
        {
            if (PlayerConstants == null || _rb == null)
            {
                return;
            }

            MovePlayerCharacter();
            HandleRotation();
            TickAttackAnimation();
            _onTick?.Invoke(Time.fixedDeltaTime);
        }

        private void MovePlayerCharacter()
        {
            if (_dashRemaining > 0f)
            {
                _dashRemaining = Mathf.Max(0f, _dashRemaining - Time.fixedDeltaTime);
                _rb.linearVelocity = _dashVelocity;
                return;
            }

            // Apply both horizontal and vertical velocity for 2D top-down movement
            Vector2 targetVelocity = _direction * (PlayerConstants.MovementSpeed * _speedMultiplier);
            _rb.linearVelocity = targetVelocity;
        }

        /// <summary>GDD §4 skill 1 (sword): dash toward the cursor for DashDurationSeconds.</summary>
        public void StartDash(Vector2 direction)
        {
            if (PlayerConstants == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _dashVelocity = direction.normalized * PlayerConstants.DashSpeed;
            _dashRemaining = PlayerConstants.DashDurationSeconds;
            _rb.linearVelocity = _dashVelocity;
        }

        /// <summary>Berserker's x3 walk speed. One, i.e. no effect, whenever no skill is running.</summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier > 0f ? multiplier : 1f;
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
                AimDirection = aimDirection;
                RotatePlayerCharacter(aimDirection);
            }
        }

        private void RotatePlayerCharacter(Vector2 targetDirection)
        {
            // Interpolate vector for smooth rotation
            _smoothedAim = Vector2.Lerp(_smoothedAim, targetDirection, Time.fixedDeltaTime * PlayerConstants.RotationSpeed);

            if (_smoothedAim.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(_smoothedAim.y, _smoothedAim.x) * Mathf.Rad2Deg
                    + _aimRotationOffset;

                // The rig's Rigidbody2D carries FreezeRotation, and MoveRotation honours constraints, so
                // it silently did nothing: the pawn never turned and every arrow left on the same angle.
                // Writing the transform pose turns the body regardless; the frozen constraint still keeps
                // physics from spinning it.
                transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;
        }

        /// <summary>Respawn: snap the body (and its collider) to the statue and kill any carried speed.</summary>
        public void Teleport(Vector2 position)
        {
            if (_rb != null)
            {
                _rb.position = position;
                _rb.linearVelocity = Vector2.zero;
            }

            transform.position = position;
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
