using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// One monster on screen. The prefab carries the body, collider, hitbox and sprite; this view only
    /// reads and writes them, so nothing is added at runtime.
    /// </summary>
    public class MonsterView : BaseView
    {
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private MonsterHitbox _hitbox;

        private Color _baseColor;

        public Vector2 Position => _body != null ? _body.position : (Vector2)transform.position;

        private void Awake()
        {
            if (_renderer != null)
            {
                _baseColor = _renderer.color;
            }
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
