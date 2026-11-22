using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.Teleport
{
    /// <summary>
    /// Draws the three pods and reads the interaction key. A pod is dim until it is built. The view owns
    /// no rules: it reports "the player pressed E next to pod N" and the controller decides.
    /// </summary>
    public class TeleportView : BaseView
    {
        /// <summary>Optional: with no prefab wired the pods stay invisible but the logic still runs.</summary>
        [SerializeField] private SpriteRenderer _podPrefab;
        [SerializeField] private Color _unbuiltColor = new Color(0.5f, 0.5f, 0.6f, 0.7f);
        [SerializeField] private Color _builtColor = new Color(0.35f, 0.85f, 0.95f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.85f, 0.3f);

        private readonly Dictionary<int, SpriteRenderer> _pods = new Dictionary<int, SpriteRenderer>();
        private UnityAction<float> _onTick;
        private float _interactRadius = 24f;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        public void SetInteractRadius(float radius)
        {
            _interactRadius = radius;
        }

        public void SpawnPods(IReadOnlyList<TeleportPodState> pods)
        {
            ClearPods();

            if (_podPrefab == null)
            {
                return;
            }

            for (int i = 0; i < pods.Count; i++)
            {
                SpriteRenderer pod = Instantiate(_podPrefab, transform);
                pod.transform.localPosition = new Vector3(pods[i].Position.x, pods[i].Position.y, 0f);
                pod.color = _unbuiltColor;
                _pods[pods[i].Index] = pod;
            }
        }

        public void SetBuilt(int index, bool built)
        {
            if (_pods.TryGetValue(index, out SpriteRenderer pod) && pod != null)
            {
                pod.color = built ? _builtColor : _unbuiltColor;
            }
        }

        public void SetSelected(int index, bool selected)
        {
            if (_pods.TryGetValue(index, out SpriteRenderer pod) && pod != null)
            {
                pod.color = selected ? _selectedColor : _builtColor;
            }
        }

        public void ClearPods()
        {
            foreach (SpriteRenderer pod in _pods.Values)
            {
                if (pod != null)
                {
                    Destroy(pod.gameObject);
                }
            }

            _pods.Clear();
        }

        /// <summary>Index of the closest pod inside the interaction radius, or -1.</summary>
        public int FindPodNear(Vector2 position, IReadOnlyList<TeleportPodState> pods)
        {
            int best = -1;
            float bestSquared = _interactRadius * _interactRadius;

            for (int i = 0; i < pods.Count; i++)
            {
                float squared = (pods[i].Position - position).sqrMagnitude;
                if (squared <= bestSquared)
                {
                    bestSquared = squared;
                    best = pods[i].Index;
                }
            }

            return best;
        }

        public bool InteractPressed =>
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        public bool CancelPressed =>
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
