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
