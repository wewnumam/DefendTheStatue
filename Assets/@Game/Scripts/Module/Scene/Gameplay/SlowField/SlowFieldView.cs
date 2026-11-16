using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.SlowField
{
    /// <summary>
    /// Draws one disc per active slow zone and drives the model's expiry clock. Radius is in world
    /// units, so the disc is scaled to twice it.
    /// </summary>
    public class SlowFieldView : BaseView
    {
        [SerializeField] private SpriteRenderer _fieldPrefab;

        private UnityAction<float> _onTick;

        public void SetTickCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void Draw(SlowField field)
        {
            if (_fieldPrefab == null)
            {
                return;
            }

            SpriteRenderer renderer = Instantiate(_fieldPrefab, transform);
            renderer.transform.position = new Vector3(field.Center.x, field.Center.y, 0f);

            // The disc art is one unit on its longest side, so twice the radius spans the zone.
            float diameter = field.Radius * 2f;
            renderer.transform.localScale = new Vector3(diameter, diameter, 1f);

            Destroy(renderer.gameObject, field.ExpiryTime - Time.time);
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void Update()
        {
            _onTick?.Invoke(Time.time);
        }
    }
}
