using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Guard
{
    /// <summary>
    /// One guard sprite per foundation. A foundation sits dim until recruited; the view only mirrors
    /// the model's built flag. Attack feedback is a short colour flash, no projectile.
    /// </summary>
    public class GuardView : BaseView
    {
        [SerializeField] private SpriteRenderer _guardPrefab;
        [SerializeField] private Color _foundationColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        [SerializeField] private Color _armedColor = Color.white;

        /// <summary>GDD §21 guard visuals: green while retreating/healing/returning, red while purging.</summary>
        [SerializeField] private Color _recoveringColor = new Color(0.35f, 0.9f, 0.4f);
        [SerializeField] private Color _purgingColor = new Color(0.95f, 0.3f, 0.3f);

        private readonly Dictionary<int, SpriteRenderer> _guards = new Dictionary<int, SpriteRenderer>();
        private UnityAction<float> _onTick;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void SpawnFoundation(int index, Vector2 position, bool built)
        {
            if (_guardPrefab == null)
            {
                Debug.LogError("[Guard] No guard prefab wired on GuardView.");
                return;
            }

            SpriteRenderer guard = Instantiate(_guardPrefab, transform);
            guard.transform.localPosition = new Vector3(position.x, position.y, 0f);

            _guards[index] = guard;
            SetBuilt(index, built);
        }

        public void SetBuilt(int index, bool built)
        {
            if (!_guards.TryGetValue(index, out SpriteRenderer guard) || guard == null)
            {
                return;
            }

            guard.color = built ? _armedColor : _foundationColor;
        }

        public void SetPosition(int index, Vector2 position)
        {
            if (_guards.TryGetValue(index, out SpriteRenderer guard) && guard != null)
            {
                guard.transform.localPosition = new Vector3(position.x, position.y, 0f);
            }
        }

        public void SetState(int index, GuardStateKind state, bool armed)
        {
            if (!_guards.TryGetValue(index, out SpriteRenderer guard) || guard == null)
            {
                return;
            }

            if (!armed)
            {
                guard.color = _foundationColor;
                return;
            }

            switch (state)
            {
                case GuardStateKind.Purging:
                    guard.color = _purgingColor;
                    break;
                case GuardStateKind.Retreating:
                case GuardStateKind.Healing:
                case GuardStateKind.Returning:
                    guard.color = _recoveringColor;
                    break;
                default:
                    guard.color = _armedColor;
                    break;
            }
        }

        public void ClearGuards()
        {
            foreach (SpriteRenderer guard in _guards.Values)
            {
                if (guard != null)
                {
                    Destroy(guard.gameObject);
                }
            }

            _guards.Clear();
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
