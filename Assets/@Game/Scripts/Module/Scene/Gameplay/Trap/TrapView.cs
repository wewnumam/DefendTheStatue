using System;
using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Trap
{
    /// <summary>
    /// One spike sprite per trap. A foundation sits dim until the player builds it; an armed trap
    /// carries its trigger collider. The view only mirrors the model's built flag.
    /// </summary>
    public class TrapView : BaseView
    {
        [SerializeField] private SpriteRenderer _trapPrefab;
        [SerializeField] private Color _foundationColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        [SerializeField] private Color _armedColor = Color.white;

        private readonly Dictionary<int, SpriteRenderer> _traps = new Dictionary<int, SpriteRenderer>();
        private UnityAction<float> _onTick;

        public event Action<int, int, Vector2> Triggered;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void SpawnTrap(int index, Vector2 position, bool built)
        {
            if (_trapPrefab == null)
            {
                Debug.LogError("[Trap] No trap prefab wired on TrapView.");
                return;
            }

            SpriteRenderer trap = Instantiate(_trapPrefab, transform);
            trap.transform.localPosition = new Vector3(position.x, position.y, 0f);

            TrapTrigger trigger = trap.GetComponent<TrapTrigger>();
            if (trigger != null)
            {
                trigger.TrapIndex = index;
                trigger.Triggered += OnTrapTriggered;
            }

            _traps[index] = trap;
            SetBuilt(index, built);
        }

        public void SetBuilt(int index, bool built)
        {
            if (!_traps.TryGetValue(index, out SpriteRenderer trap) || trap == null)
            {
                return;
            }

            trap.color = built ? _armedColor : _foundationColor;
        }

        public void ClearTraps()
        {
            foreach (SpriteRenderer trap in _traps.Values)
            {
                if (trap != null)
                {
                    Destroy(trap.gameObject);
                }
            }

            _traps.Clear();
        }

        private void OnTrapTriggered(int trapIndex, int monsterId, Vector2 position)
        {
            Triggered?.Invoke(trapIndex, monsterId, position);
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
