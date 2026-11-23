using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>
    /// The statue the run is played around. Owns nothing but its heal radius and its position; the
    /// sprite and renderer are authored in the scene, health lives in the model.
    /// </summary>
    public class StatueView : BaseView
    {
        [SerializeField] private Transform _statue;
        [SerializeField] private float _radius = 96f;

        /// <summary>GDD §21: the heal zone blinks yellow/red while a monster stands inside it.</summary>
        [SerializeField] private SpriteRenderer _zoneRenderer;
        [SerializeField] private Color _zoneTaintedA = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color _zoneTaintedB = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private float _zoneBlinkSeconds = 0.45f;

        private Color _zoneBaseColor = Color.white;
        private bool _zoneTainted;
        private float _zoneBlinkElapsed;

        private void Awake()
        {
            if (_zoneRenderer != null)
            {
                _zoneBaseColor = _zoneRenderer.color;
            }
        }

        public void SetZoneTainted(bool tainted)
        {
            if (_zoneTainted == tainted)
            {
                return;
            }

            _zoneTainted = tainted;
            _zoneBlinkElapsed = 0f;

            if (!tainted && _zoneRenderer != null)
            {
                _zoneRenderer.color = _zoneBaseColor;
            }
        }

        /// <summary>A two-state blink, not a fade: the GDD asks for yellow/red, not a gradient.</summary>
        private void Update()
        {
            if (!_zoneTainted || _zoneRenderer == null)
            {
                return;
            }

            _zoneBlinkElapsed += Time.deltaTime;
            bool second = Mathf.Repeat(_zoneBlinkElapsed, _zoneBlinkSeconds * 2f) >= _zoneBlinkSeconds;
            _zoneRenderer.color = second ? _zoneTaintedB : _zoneTaintedA;
        }

        public Vector2 Position => _statue != null ? (Vector2)_statue.position : (Vector2)transform.position;

        public void EnsureVisual()
        {
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
