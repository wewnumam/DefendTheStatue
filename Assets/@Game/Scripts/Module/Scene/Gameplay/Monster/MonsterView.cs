using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// GDD §7 art for one monster kind. The poses are optional: a kind with no clip of its own still
    /// falls back to its idle frame when it attacks or takes a hit.
    /// </summary>
    [System.Serializable]
    public class MonsterKindSprites
    {
        public Utility.EnumManager.MonsterKind Kind;
        public Sprite Idle;
        public Sprite Attack;
        public Sprite Hit;

        /// <summary>GDD §21 "😇 Mind Control": the ally body shown while sword skill 4 holds.</summary>
        public Sprite MindControlled;
    }

    /// <summary>
    /// One monster on screen. The prefab carries the body, collider, hitbox, sprite and Animator;
    /// this view only reads and writes them.
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
        /// GDD §7: the three roles must read as three different creatures. Only this kind has clips in
        /// the Animator, so every other kind drives its sprite from <see cref="_kindSprites"/> instead.
        /// </summary>
        [SerializeField] private Utility.EnumManager.MonsterKind _animatedKind =
            Utility.EnumManager.MonsterKind.Deasor;

        [Header("Per-kind art (GDD §7)")]
        [SerializeField] private MonsterKindSprites[] _kindSprites = new MonsterKindSprites[0];

        /// <summary>
        /// The clips are one frame each, so a pose would vanish instantly. Hold floor for the attack and
        /// get-hit poses. ponytail: drop it once the clips carry real timing — the clip length drives the
        /// hold then.
        /// </summary>
        [SerializeField] private float _minOneShotSeconds = 0.15f;

        /// <summary>
        /// How long the hit tint holds. TickAnimation drives it, so the flash needs no coroutine and no
        /// second Update on a pooled object.
        /// </summary>
        [SerializeField] private float _hitFlashSeconds = 0.1f;

        /// <summary>Shown only after the first hit: a fresh monster carries no bar.</summary>
        [Header("Health bar")]
        [SerializeField] private Utility.HealthBarView _healthBarPrefab;

        private Utility.HealthBarView _healthBar;

        private Color _baseColor;
        private MonsterKindSprites _kindArt;
        private bool _artFromAnimator = true;
        private bool _oneShotSpriteActive;
        private bool _isMindControlled;

        private int _idleStateHash;
        private int _attackStateHash;
        private int _hitStateHash;
        private int _oneShotStateHash;
        private float _oneShotElapsed;
        private float _hitFlashRemaining;

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
            // A recycled monster must not inherit the previous life's hit tint.
            EndHitFlash();
            PlayIdleAnimation();
        }

        /// <summary>
        /// Per-spawn and per-hit. The bar is built on the first call because the pool scales the body
        /// after <see cref="Awake"/>; passing full health hides it again for a recycled monster.
        /// </summary>
        public void SetHealth(float health, float maxHealth)
        {
            if (_healthBar == null)
            {
                if (_healthBarPrefab == null)
                {
                    return;
                }

                _healthBar = Instantiate(_healthBarPrefab, transform);
                _healthBar.Init(transform, transform.localScale.x);
            }

            _healthBar.Show(maxHealth > 0f ? health / maxHealth : 0f);
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

        /// <summary>
        /// GDD §7: give a pooled monster its kind's body. Called before the object is shown, so the
        /// Animator never spends a frame playing the previous tenant's clips on the wrong creature.
        /// </summary>
        public void SetKind(Utility.EnumManager.MonsterKind kind)
        {
            _kindArt = FindKindArt(kind);

            _artFromAnimator = _animator != null && kind == _animatedKind;
            if (_animator != null)
            {
                _animator.enabled = _artFromAnimator;
            }

            _oneShotSpriteActive = false;
            _oneShotStateHash = 0;
            _isMindControlled = false;
            ApplySprite(_kindArt != null ? _kindArt.Idle : null);
        }

        /// <summary>
        /// GDD §21 "😇 Mind Control": the ally body replaces the monster's own while sword skill 4 holds.
        /// The Animator would repaint its own frame every tick, so it is parked for the duration; a kind
        /// with no ally art keeps its normal body and the effect stays invisible rather than glitching.
        /// </summary>
        public void SetMindControlled(bool mindControlled)
        {
            if (_isMindControlled == mindControlled)
            {
                return;
            }

            _isMindControlled = mindControlled;

            if (_animator != null && _artFromAnimator)
            {
                _animator.enabled = !mindControlled;
            }

            if (mindControlled)
            {
                _oneShotSpriteActive = false;
                _oneShotStateHash = 0;
                ApplySprite(_kindArt != null && _kindArt.MindControlled != null
                    ? _kindArt.MindControlled
                    : _kindArt != null ? _kindArt.Idle : null);
                return;
            }

            PlayIdleAnimation();
        }

        private MonsterKindSprites FindKindArt(Utility.EnumManager.MonsterKind kind)
        {
            if (_kindSprites == null)
            {
                return null;
            }

            for (int i = 0; i < _kindSprites.Length; i++)
            {
                if (_kindSprites[i] != null && _kindSprites[i].Kind == kind)
                {
                    return _kindSprites[i];
                }
            }

            return null;
        }

        private void ApplySprite(Sprite sprite)
        {
            if (_renderer != null && sprite != null)
            {
                _renderer.sprite = sprite;
            }
        }

        /// <summary>Bite pose. The pool's tick polls it back to idle.</summary>
        public void PlayAttackAnimation()
        {
            if (_artFromAnimator)
            {
                PlayOneShot(_attackStateHash);
                return;
            }

            PlayOneShotSprite(_kindArt != null ? _kindArt.Attack : null);
        }

        /// <summary>Damage pose. Also polled back to idle by the pool's tick.</summary>
        public void PlayHitAnimation()
        {
            if (_artFromAnimator)
            {
                PlayOneShot(_hitStateHash);
                return;
            }

            PlayOneShotSprite(_kindArt != null ? _kindArt.Hit : null);
        }

        /// <summary>
        /// Sprite-only kinds have no clip to time, so the view holds the pose for the same floor the
        /// Animator path uses. A null pose leaves the idle frame up.
        /// </summary>
        private void PlayOneShotSprite(Sprite sprite)
        {
            if (sprite == null || _isMindControlled)
            {
                return;
            }

            ApplySprite(sprite);
            _oneShotSpriteActive = true;
            _oneShotElapsed = 0f;
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
            if (_hitFlashRemaining > 0f)
            {
                _hitFlashRemaining -= deltaTime;
                if (_hitFlashRemaining <= 0f)
                {
                    EndHitFlash();
                }
            }

            if (!_artFromAnimator)
            {
                if (_oneShotSpriteActive)
                {
                    _oneShotElapsed += deltaTime;
                    if (_oneShotElapsed >= _minOneShotSeconds)
                    {
                        _oneShotSpriteActive = false;
                        ApplySprite(_kindArt != null ? _kindArt.Idle : null);
                    }
                }

                return;
            }

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
            _oneShotSpriteActive = false;

            // The ally body owns the frame while the effect holds; an idle repaint would undo it.
            if (_isMindControlled)
            {
                return;
            }

            if (!_artFromAnimator)
            {
                ApplySprite(_kindArt != null ? _kindArt.Idle : null);
                return;
            }

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
                _hitFlashRemaining = _hitFlashSeconds;
            }
        }

        public void EndHitFlash()
        {
            _hitFlashRemaining = 0f;

            if (_renderer != null)
            {
                _renderer.color = _baseColor;
            }
        }

        private static readonly Color HitFlashColor = new Color(1f, 0.35f, 0.35f);
    }
}
