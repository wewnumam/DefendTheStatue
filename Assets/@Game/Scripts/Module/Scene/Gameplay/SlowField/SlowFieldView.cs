using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.SlowField
{
    /// <summary>
    /// Draws one disc per active slow zone and drives the model's expiry clock. Radius is in world
    /// units, so the disc is scaled to twice it. The discs live at the scene root: a zone is
    /// world-anchored, while this view sits on the player character, whose transform rotates with the aim.
    /// </summary>
    public class SlowFieldView : BaseView
    {
        [SerializeField] private SpriteRenderer _fieldPrefab;

        private readonly List<GameObject> _discs = new List<GameObject>();

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

            // Unparented on purpose: the prefab is a child of this view, so a parented clone would be
            // dragged along with the player and spun by the aim rotation.
            SpriteRenderer disc = Instantiate(_fieldPrefab);
            disc.gameObject.SetActive(true);
            disc.transform.SetParent(null);
            disc.transform.position = new Vector3(field.Center.x, field.Center.y, 0f);

            // The disc art is one unit on its longest side, so twice the radius spans the zone.
            float diameter = field.Radius * 2f;
            disc.transform.localScale = new Vector3(diameter, diameter, 1f);

            Destroy(disc.gameObject, field.ExpiryTime - Time.time);
            _discs.Add(disc.gameObject);
        }

        /// <summary>Drops only the discs this view spawned — never the view's own children.</summary>
        public void Clear()
        {
            for (int i = _discs.Count - 1; i >= 0; i--)
            {
                if (_discs[i] != null)
                {
                    Destroy(_discs[i]);
                }
            }

            _discs.Clear();
        }

        private void Update()
        {
            _onTick?.Invoke(Time.time);
        }
    }
}
