using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// One monster on screen. Placeholder art: a square quad built at runtime, no prefab needed, so
    /// the core slice runs without any Editor authoring. Swap in a sprite later by assigning
    /// MonsterPoolView's prefab — the pool prefers it when it is set.
    /// </summary>
    public class MonsterView : BaseView
    {
        private SpriteRenderer _renderer;
        private Color _baseColor;

        public void Init(SpriteRenderer renderer, Color baseColor)
        {
            _renderer = renderer;
            _baseColor = baseColor;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }

        /// <summary>
        /// Every monster needs something an arrow can physically stop on and read an id from. The
        /// placeholder square gets it from the pool; a prefab has to carry it already.
        /// </summary>
        public MonsterHitbox EnsureHitbox()
        {
            MonsterHitbox hitbox = GetComponent<MonsterHitbox>();
            if (hitbox == null)
            {
                hitbox = gameObject.AddComponent<MonsterHitbox>();
            }

            if (GetComponent<Collider2D>() == null && GetComponentInChildren<Collider2D>() == null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }

            return hitbox;
        }

        public void SetMonsterId(int monsterId)
        {
            EnsureHitbox().MonsterId = monsterId;
        }

        public void PlayHitFlash()
        {
            if (_renderer != null)
            {
                _renderer.color = Color.white;
            }
        }

        public void EndHitFlash()
        {
            if (_renderer != null)
            {
                _renderer.color = _baseColor;
            }
        }
    }
}
