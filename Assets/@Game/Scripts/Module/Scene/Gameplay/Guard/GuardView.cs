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

        /// <summary>GDD §21 "⚡x4": the authored buffed body replaces the normal one while empowered.</summary>
        [SerializeField] private Sprite _buffedSprite;

        /// <summary>Loaded from Resources rather than wired by hand: the art lives there already and a
        /// serialized GUID would be one more thing to keep in sync.</summary>
        private const string BuffedSpritePath = "Sprites/Guard/Buffed (Effect Bow Skill 4)/Guard Buffed Idle";

        private readonly Dictionary<int, SpriteRenderer> _guards = new Dictionary<int, SpriteRenderer>();
        private readonly Dictionary<int, Sprite> _defaultSprites = new Dictionary<int, Sprite>();
        private UnityAction<float> _onTick;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
            EnsureBuffedSprite();
        }

        /// <summary>GDD §21 "⚡x4": falls back to the Resources copy when nothing is wired in the scene.</summary>
        private void EnsureBuffedSprite()
        {
            if (_buffedSprite == null)
            {
                _buffedSprite = Resources.Load<Sprite>(BuffedSpritePath);
            }
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
            _defaultSprites[index] = guard.sprite;
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
            SetState(index, state, armed, false);
        }

        /// <summary>GDD §21: the buffed sprite marks a guard under Guard Empower.</summary>
        public void SetState(int index, GuardStateKind state, bool armed, bool empowered)
        {
            if (!_guards.TryGetValue(index, out SpriteRenderer guard) || guard == null)
            {
                return;
            }

            SetEmpoweredSprite(index, guard, armed && empowered);

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

        /// <summary>A null buffed sprite leaves the normal body up, so the buff is never invisible.</summary>
        private void SetEmpoweredSprite(int index, SpriteRenderer guard, bool empowered)
        {
            if (_buffedSprite == null)
            {
                return;
            }

            Sprite target = empowered
                ? _buffedSprite
                : _defaultSprites.TryGetValue(index, out Sprite normal) ? normal : null;

            if (target != null && guard.sprite != target)
            {
                guard.sprite = target;
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
            _defaultSprites.Clear();
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
