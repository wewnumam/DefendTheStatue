using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Tower
{
    /// <summary>
    /// One placeholder square per built tower, plus a tracer that fades after each shot. The tracer
    /// is cosmetic: damage is applied when the tower fires, not when an arrow arrives.
    /// ponytail: add a real enemy-arrow pool when tower shots have to miss or be dodged.
    /// </summary>
    public class TowerView : BaseView
    {
        [SerializeField] private float _tracerSeconds = 0.12f;

        private readonly List<SpriteRenderer> _tracers = new List<SpriteRenderer>();
        private readonly List<float> _tracerTimers = new List<float>();
        private UnityAction<float> _onTick;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void AddTower(Vector2 position)
        {
            var tower = new GameObject("Tower");
            tower.transform.SetParent(transform, false);
            tower.transform.localPosition = new Vector3(position.x, position.y, 0f);
            tower.transform.localScale = new Vector3(TowerState.Size, TowerState.Size, 1f);

            var renderer = tower.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprite.Square;
            renderer.color = new Color(0.6f, 0.5f, 0.3f);
        }

        /// <summary>Draws the shot as a thin bar from the tower towards the target.</summary>
        public void ShowShot(Vector2 from, Vector2 to)
        {
            var tracer = new GameObject("Tracer");
            tracer.transform.SetParent(transform, false);

            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                length = 0.01f;
            }

            tracer.transform.localPosition = new Vector3(from.x, from.y, 0f);
            tracer.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            tracer.transform.localScale = new Vector3(length, 2f, 1f);

            var renderer = tracer.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprite.Square;
            renderer.color = new Color(0.95f, 0.9f, 0.6f);
            renderer.sortingOrder = 1;

            _tracers.Add(renderer);
            _tracerTimers.Add(_tracerSeconds);
        }

        public void ClearTowers()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _tracers.Clear();
            _tracerTimers.Clear();
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);

            for (int i = _tracerTimers.Count - 1; i >= 0; i--)
            {
                _tracerTimers[i] -= Time.deltaTime;
                if (_tracerTimers[i] > 0f)
                {
                    continue;
                }

                if (_tracers[i] != null)
                {
                    Destroy(_tracers[i].gameObject);
                }

                _tracers.RemoveAt(i);
                _tracerTimers.RemoveAt(i);
            }
        }
    }
}
