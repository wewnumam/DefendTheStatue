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
            // Apply both horizontal and vertical velocity for 2D top-down movement
            Vector2 targetVelocity = _direction * (PlayerConstants.MovementSpeed * _speedMultiplier);
            _rb.linearVelocity = targetVelocity;
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
