using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>
    /// The statue the run is played around. Owns nothing but its own placeholder look and its
    /// position; health lives in the model.
    /// </summary>
    public class StatueView : BaseView
    {
        [SerializeField] private Transform _statue;
        [SerializeField] private float _radius = 96f;

        private SpriteRenderer _renderer;

        public Vector2 Position => _statue != null ? (Vector2)_statue.position : (Vector2)transform.position;

        public void EnsureVisual()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
                if (_renderer == null)
                {
                    _renderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            _renderer.sprite = PlaceholderSprite.Circle;
            _renderer.color = new Color(0.85f, 0.85f, 0.9f);
            _renderer.sortingOrder = -1;
            transform.position = new Vector3(Position.x, Position.y, 0f);
            SetRadius(_radius);
        }

        /// <summary>The statue grows as it levels, so the heal radius reads at a glance.</summary>
        public void SetLevel(int level)
        {
            SetRadius(_radius + (level - 1) * 16f);
        }

        private void SetRadius(float diameter)
        {
            transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }
}
