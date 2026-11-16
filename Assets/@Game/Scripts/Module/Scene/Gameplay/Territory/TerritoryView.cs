using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Territory
{
    /// <summary>
    /// Draws each ring as four thin bars, dimmed until the ring is unlocked. Geometry only: what is
    /// built on a ring is the tower and build modules' business.
    /// </summary>
    public class TerritoryView : BaseView
    {
        private const float BarThickness = 6f;

        [SerializeField] private SpriteRenderer _barPrefab;
        [SerializeField] private GameObject _wallPrefab;

        private readonly List<Transform> _rings = new List<Transform>();
        private readonly List<GameObject> _walls = new List<GameObject>();

        public void Refresh(TerritoryModel model)
        {
            for (int i = 0; i < model.Count; i++)
            {
                Transform ring = EnsureRing(i, model.Get(i));
                if (ring == null)
                {
                    continue;
                }

                ring.gameObject.SetActive(true);
                bool unlocked = model.Get(i).Unlocked;
                foreach (SpriteRenderer bar in ring.GetComponentsInChildren<SpriteRenderer>())
                {
                    bar.color = unlocked ? new Color(0.35f, 0.45f, 0.5f) : new Color(0.2f, 0.2f, 0.22f, 0.35f);
                }
            }
        }

        /// <summary>A wall was paid for: draw the segment between its two ends.</summary>
        public void AddWall(Vector2 start, Vector2 end, float thickness)
        {
            if (_wallPrefab == null)
            {
                return;
            }

            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject wall = Instantiate(_wallPrefab, transform);
            wall.transform.localPosition = new Vector3((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f, 0f);
            wall.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            wall.transform.localScale = new Vector3(length, thickness, 1f);

            _walls.Add(wall);
        }

        /// <summary>Walls belong to one run; a restart must not leave the old ring standing.</summary>
        public void ClearWalls()
        {
            for (int i = 0; i < _walls.Count; i++)
            {
                if (_walls[i] != null)
                {
                    Destroy(_walls[i]);
                }
            }

            _walls.Clear();
        }

        private Transform EnsureRing(int index, TerritoryState territory)
        {
            if (index < _rings.Count && _rings[index] != null)
            {
                return _rings[index];
            }

            if (_barPrefab == null)
            {
                return null;
            }

            var host = new GameObject($"Wilayah {index + 1}");
            host.transform.SetParent(transform, false);
            _rings.Add(host.transform);

            float halfWidth = territory.HalfWidth;
            float halfHeight = territory.HalfHeight;

            CreateBar(host.transform, "Top", new Vector2(0f, halfHeight), new Vector2(territory.Width, BarThickness));
            CreateBar(host.transform, "Bottom", new Vector2(0f, -halfHeight), new Vector2(territory.Width, BarThickness));
            CreateBar(host.transform, "Left", new Vector2(-halfWidth, 0f), new Vector2(BarThickness, territory.Height));
            CreateBar(host.transform, "Right", new Vector2(halfWidth, 0f), new Vector2(BarThickness, territory.Height));

            return host.transform;
        }

        private void CreateBar(Transform parent, string name, Vector2 offset, Vector2 size)
        {
            SpriteRenderer bar = Instantiate(_barPrefab, parent);
            bar.name = name;
            bar.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            bar.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
