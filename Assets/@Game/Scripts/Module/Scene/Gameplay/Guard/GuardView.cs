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

        /// <summary>Shot feedback: the body flashes and a short tracer runs to the target.</summary>
        [SerializeField] private Color _attackFlashColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private float _attackFlashSeconds = 0.12f;
        [SerializeField] private float _tracerSeconds = 0.08f;

        /// <summary>Loaded from Resources rather than wired by hand: the art lives there already and a
        /// serialized GUID would be one more thing to keep in sync.</summary>
        private const string BuffedSpritePath = "Sprites/Guard/Buffed (Effect Bow Skill 4)/Guard Buffed Idle";

        private readonly Dictionary<int, SpriteRenderer> _guards = new Dictionary<int, SpriteRenderer>();
        private readonly Dictionary<int, Sprite> _defaultSprites = new Dictionary<int, Sprite>();
        private readonly Dictionary<int, float> _flashUntil = new Dictionary<int, float>();

        /// <summary>World bar: the shared bar prefab plus the guard gauge art, both from Resources.</summary>
        private const string BarPrefabPath = "Modules/MonsterHealthBar";
        private const string BarArtRoot = "Sprites/UI (Pohon, Guard, Monster, Tower)/Guard/";

        private readonly Dictionary<int, Utility.HealthBarView> _bars =
            new Dictionary<int, Utility.HealthBarView>();
        private Utility.HealthBarView _barPrefab;
        private Sprite _barFrame;
        private Sprite _barFill;

        private LineRenderer _tracer;
        private float _tracerUntil;
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
            EnsureBar(index, guard);
        }

        /// <summary>
        /// One world bar per guard, built on spawn so the pool's scale is already applied. The bar is
        /// parented to the guard body and hides itself at full health.
        /// </summary>
        private void EnsureBar(int index, SpriteRenderer guard)
        {
            if (_barPrefab == null)
            {
                _barPrefab = Resources.Load<Utility.HealthBarView>(BarPrefabPath);
                _barFrame = LoadFirstSprite(BarArtRoot + "Frame (Guard)");
                _barFill = LoadFirstSprite(BarArtRoot + "Fill (Guard)");
            }

            if (_barPrefab == null)
            {
                return;
            }

            Utility.HealthBarView bar = Instantiate(_barPrefab, guard.transform);
            bar.Init(guard.transform, guard.transform.lossyScale.x, _barFrame, _barFill);
            _bars[index] = bar;
        }

        private static Sprite LoadFirstSprite(string path)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            return sprites != null && sprites.Length > 0 ? sprites[0] : null;
        }

        /// <summary>Bite damage: the bar shows the ratio and hides again at full or dead.</summary>
        public void SetHealth(int index, float health, float maxHealth)
        {
            if (!_bars.TryGetValue(index, out Utility.HealthBarView bar) || bar == null)
            {
                return;
            }

            bar.Show(health <= 0f ? 0f : maxHealth > 0f ? health / maxHealth : 0f);
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

        /// <summary>Shot feedback: flash the guard and draw a short tracer from its muzzle to the target.</summary>
        public void PlayAttack(int index, Vector2 from, Vector2 to)
        {
            _flashUntil[index] = Time.time + _attackFlashSeconds;

            EnsureTracer();
            if (_tracer == null)
            {
                return;
            }

            _tracer.SetPosition(0, new Vector3(from.x, from.y, -0.1f));
            _tracer.SetPosition(1, new Vector3(to.x, to.y, -0.1f));
            _tracer.enabled = true;
            _tracerUntil = Time.time + _tracerSeconds;
        }

        private void EnsureTracer()
        {
            if (_tracer != null)
            {
                return;
            }

            GameObject tracer = new GameObject("GuardTracer");
            tracer.transform.SetParent(transform, false);
            _tracer = tracer.AddComponent<LineRenderer>();
            _tracer.useWorldSpace = false;
            _tracer.positionCount = 2;
            _tracer.startWidth = 4f;
            _tracer.endWidth = 1f;
            _tracer.numCapVertices = 0;
            _tracer.material = new Material(Shader.Find("Sprites/Default"));
            _tracer.startColor = _attackFlashColor;
            _tracer.endColor = _attackFlashColor;
            _tracer.enabled = false;
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

            if (_flashUntil.TryGetValue(index, out float until) && Time.time < until)
            {
                guard.color = _attackFlashColor;
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
            _flashUntil.Clear();
            _bars.Clear();
        }

        private void Update()
        {
            if (_tracer != null && _tracer.enabled && Time.time >= _tracerUntil)
            {
                _tracer.enabled = false;
            }

            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
