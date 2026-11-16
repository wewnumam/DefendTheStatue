using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Tower
{
    /// <summary>
    /// One tower sprite per built tower, plus a tracer that fades after each shot. The tracer is
    /// cosmetic: damage is applied when the tower fires, not when an arrow arrives.
    /// ponytail: add a real enemy-arrow pool when tower shots have to miss or be dodged.
    /// </summary>
    public class TowerView : BaseView
    {
        [SerializeField] private SpriteRenderer _towerPrefab;
        [SerializeField] private SpriteRenderer _tracerPrefab;
        [SerializeField] private float _tracerSeconds = 0.12f;

        private readonly List<SpriteRenderer> _tracers = new List<SpriteRenderer>();
        private readonly List<float> _tracerTimers = new List<float>();
        private UnityAction<float> _onTick;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void AddTower(Vector2 position, float size)
        {
            if (_towerPrefab == null)
            {
                return;
            }

            SpriteRenderer tower = Instantiate(_towerPrefab, transform);
            tower.transform.localPosition = new Vector3(position.x, position.y, 0f);
            tower.transform.localScale = new Vector3(size, size, 1f);
        }

        /// <summary>Draws the shot as a thin bar from the tower towards the target.</summary>
        public void ShowShot(Vector2 from, Vector2 to)
        {
            if (_tracerPrefab == null)
            {
                return;
            }

            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                length = 0.01f;
            }

            SpriteRenderer tracer = Instantiate(_tracerPrefab, transform);
            tracer.transform.localPosition = new Vector3(from.x, from.y, 0f);
            tracer.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            tracer.transform.localScale = new Vector3(length, 2f, 1f);

            _tracers.Add(tracer);
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
