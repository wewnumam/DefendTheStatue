using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Boss
{
    /// <summary>
    /// The boss body. Three phases are one tint and one scale step each, the attack pose is a sprite
    /// swap held for a beat, and the projectile fan is cosmetic tracers that fly out and fade. The
    /// controller owns damage; this view only shows what happened.
    /// </summary>
    public class BossView : BaseView
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private SpriteRenderer _projectilePrefab;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite _attackSprite;
        [SerializeField] private Sprite _hitSprite;

        [Header("Phases")]
        [SerializeField] private Color _phase1Color = Color.white;
        [SerializeField] private Color _phase2Color = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color _phase3Color = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float _phase2Scale = 1.1f;
        [SerializeField] private float _phase3Scale = 1.25f;

        [Header("Poses")]
        [SerializeField] private float _poseHoldSeconds = 0.25f;

        [Header("Projectile fan")]
        [SerializeField] private float _projectileSpeed = 120f;
        [SerializeField] private float _projectileLifetimeSeconds = 0.5f;

        private struct Projectile
        {
            public Transform Transform;
            public Vector2 Velocity;
            public float Remaining;
        }

        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private UnityAction<float> _onTick;
        private float _poseRemaining;
        private bool _showingPose;
        private Color _phaseColor = Color.white;

        public Vector2 Position => transform.position;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        private void Awake()
        {
            EnsureRenderer();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            _onTick?.Invoke(delta);
            TickProjectiles(delta);
            TickPose(delta);
        }

        public void Spawn(Vector2 position)
        {
            gameObject.SetActive(true);
            SetPosition(position);
            SetPhase(1);
            SetSprite(_idleSprite);
            _showingPose = false;
            _poseRemaining = 0f;
            ClearProjectiles();
        }

        public void Hide()
        {
            ClearProjectiles();
            gameObject.SetActive(false);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }

        /// <summary>Phase 1 is plain, 2 is a warm tint, 3 is red and larger. Cheap, no animator states.</summary>
        public void SetPhase(int phase)
        {
            EnsureRenderer();
            _phaseColor = phase >= 3 ? _phase3Color : phase == 2 ? _phase2Color : _phase1Color;
            float scale = phase >= 3 ? _phase3Scale : phase == 2 ? _phase2Scale : 1f;

            if (_renderer != null)
            {
                _renderer.color = _phaseColor;
            }

            transform.localScale = Vector3.one * scale;
        }

        public void PlayAttack()
        {
            SetSprite(_attackSprite);
            _showingPose = true;
            _poseRemaining = _poseHoldSeconds;
        }

        public void PlayHit()
        {
            SetSprite(_hitSprite);
            _showingPose = true;
            _poseRemaining = _poseHoldSeconds;
        }

        /// <summary>Fan of tracers aimed at the target; the controller already applied the damage.</summary>
        public void SpawnProjectileFan(Vector2 target, int count, float spreadDegrees)
        {
            if (_projectilePrefab == null || count <= 0)
            {
                return;
            }

            Vector2 origin = Position;
            Vector2 direction = target - origin;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float step = count > 1 ? spreadDegrees / (count - 1) : 0f;
            float start = baseAngle - spreadDegrees * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = start + step * i;
                float radians = angle * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * _projectileSpeed;

                SpriteRenderer tracer = Instantiate(_projectilePrefab, origin, Quaternion.Euler(0f, 0f, angle), transform.parent);
                tracer.transform.position = new Vector3(origin.x, origin.y, 0f);
                tracer.color = _phaseColor;

                Collider2D collider = tracer.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                _projectiles.Add(new Projectile
                {
                    Transform = tracer.transform,
                    Velocity = velocity,
                    Remaining = _projectileLifetimeSeconds
                });
            }
        }

        private void TickProjectiles(float delta)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _projectiles[i];
                projectile.Remaining -= delta;

                if (projectile.Transform == null || projectile.Remaining <= 0f)
                {
                    if (projectile.Transform != null)
                    {
                        Destroy(projectile.Transform.gameObject);
                    }

                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Transform.position += (Vector3)(projectile.Velocity * delta);
                _projectiles[i] = projectile;
            }
        }

        private void TickPose(float delta)
        {
            if (!_showingPose)
            {
                return;
            }

            _poseRemaining -= delta;
            if (_poseRemaining > 0f)
            {
                return;
            }

            _showingPose = false;
            SetSprite(_idleSprite);
        }

        private void ClearProjectiles()
        {
            foreach (Projectile projectile in _projectiles)
            {
                if (projectile.Transform != null)
                {
                    Destroy(projectile.Transform.gameObject);
                }
            }

            _projectiles.Clear();
        }

        private void SetSprite(Sprite sprite)
        {
            EnsureRenderer();
            if (_renderer != null && sprite != null)
            {
                _renderer.sprite = sprite;
            }
        }

        private void EnsureRenderer()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }
        }
    }
}
