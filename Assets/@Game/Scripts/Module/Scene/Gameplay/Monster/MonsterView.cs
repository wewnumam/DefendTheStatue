using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// One monster on screen. The prefab carries the body, collider, hitbox, sprite and Animator;
    /// this view only reads and writes them, so nothing is added at runtime.
    /// </summary>
    public class MonsterView : BaseView
    {
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private MonsterHitbox _hitbox;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _idleState = "Deasor_Idle";
        [SerializeField] private string _attackState = "Deasor_Attack";
        [SerializeField] private string _hitState = "Deasor_GetHit";

        /// <summary>
        /// The clips are one frame each, so a pose would vanish instantly. Hold floor for the attack and
        /// get-hit poses. ponytail: drop it once the clips carry real timing — the clip length drives the
        /// hold then.
        /// </summary>
        [SerializeField] private float _minOneShotSeconds = 0.15f;

        private Color _baseColor;

        private int _idleStateHash;
        private int _attackStateHash;
        private int _hitStateHash;
        private int _oneShotStateHash;
        private float _oneShotElapsed;

        public Vector2 Position => _body != null ? _body.position : (Vector2)transform.position;

        private void Awake()
        {
            if (_renderer != null)
            {
                _baseColor = _renderer.color;
            }

            _idleStateHash = Animator.StringToHash(_idleState);
            _attackStateHash = Animator.StringToHash(_attackState);
            _hitStateHash = Animator.StringToHash(_hitState);
        }

        /// <summary>Pooled object: a previous life may have been recycled mid-attack.</summary>
        private void OnEnable()
        {
            PlayIdleAnimation();
        }

        /// <summary>Spawn and recycle use this: it teleports the body, so no velocity survives the move.</summary>
        public void SetPosition(Vector2 position)
        {
            if (_body != null)
            {
                _body.position = position;
                _body.linearVelocity = Vector2.zero;
            }

            transform.position = new Vector3(position.x, position.y, 0f);
        }

        /// <summary>Pool-driven walk. Zero parks the monster on the statue clearance.</summary>
        public void SetVelocity(Vector2 velocity)
        {
            if (_body == null)
            {
                return;
            }

            _body.linearVelocity = velocity;
        }

        public void SetMonsterId(int monsterId)
        {
            if (_hitbox != null)
            {
                _hitbox.MonsterId = monsterId;
            }
        }

        /// <summary>Bite pose. The pool's tick polls it back to idle.</summary>
        public void PlayAttackAnimation()
        {
            PlayOneShot(_attackStateHash);
        }

        /// <summary>Damage pose. Also polled back to idle by the pool's tick.</summary>
        public void PlayHitAnimation()
        {
            PlayOneShot(_hitStateHash);
        }

        private void PlayOneShot(int stateHash)
        {
            if (_animator == null)
            {
                return;
            }

            _oneShotStateHash = stateHash;
            _oneShotElapsed = 0f;
            _animator.Play(stateHash, 0, 0f);
        }

        /// <summary>
        /// One-shot clips have no exit transition wired here, so the view owns the return to idle. Waits
        /// for the state to actually enter before timing it, otherwise the previous state's length lies.
        /// </summary>
        public void TickAnimation(float deltaTime)
        {
            if (_animator == null || _oneShotStateHash == 0)
            {
                return;
            }

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash != _oneShotStateHash)
            {
                return;
            }

            _oneShotElapsed += deltaTime;
            if (_oneShotElapsed < Mathf.Max(state.length, _minOneShotSeconds))
            {
                return;
            }

            PlayIdleAnimation();
        }

        private void PlayIdleAnimation()
        {
            _oneShotStateHash = 0;
            _oneShotElapsed = 0f;

            if (_animator != null)
            {
                _animator.Play(_idleStateHash, 0, 0f);
            }
        }

        public void PlayHitFlash()
        {
            if (_renderer != null)
            {
                // Art has no tint of its own, so a white flash would be invisible: use a red one instead.
                _renderer.color = HitFlashColor;
            }
        }

        public void EndHitFlash()
        {
            if (_renderer != null)
            {
                _renderer.color = _baseColor;
            }
        }

        private static readonly Color HitFlashColor = new Color(1f, 0.35f, 0.35f);
    }
}
