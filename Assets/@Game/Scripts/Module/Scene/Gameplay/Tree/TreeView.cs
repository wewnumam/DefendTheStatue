using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Tree
{
    /// <summary>
    /// One tree sprite per standing tree. Blocking is the prefab's collider, not this view; the view
    /// only spawns, scales and removes copies, and runs the shared tick the growth clock lives on.
    /// </summary>
    public class TreeView : BaseView
    {
        [SerializeField] private SpriteRenderer _treePrefab;

        /// <summary>Hidden until a hit lands, so a standing forest carries no bars.</summary>
        [SerializeField] private Utility.HealthBarView _healthBarPrefab;

        private readonly Dictionary<int, SpriteRenderer> _trees = new Dictionary<int, SpriteRenderer>();
        private readonly Dictionary<int, Utility.HealthBarView> _bars =
            new Dictionary<int, Utility.HealthBarView>();
        private UnityAction<float> _onTick;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        /// <summary>GDD §10 palette: red, purple, blue, gold.</summary>
        [SerializeField] private Color _redColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color _purpleColor = new Color(0.6f, 0.3f, 0.9f);
        [SerializeField] private Color _magicColor = new Color(0.3f, 0.55f, 0.95f);
        [SerializeField] private Color _arcanaColor = new Color(0.95f, 0.8f, 0.2f);

        /// <summary>
        /// GDD §10 art per kind. A null slot keeps the prefab's own sprite and the colour tint above,
        /// so a scene without these wired still tells the four kinds apart.
        /// </summary>
        [Header("Per-kind art (GDD §10)")]
        [SerializeField] private Sprite _redSprite;
        [SerializeField] private Sprite _purpleSprite;
        [SerializeField] private Sprite _magicSprite;
        [SerializeField] private Sprite _arcanaSprite;

        public void SpawnTree(int id, Vector2 position, float size)
        {
            SpawnTree(id, position, size, Utility.EnumManager.TreeKind.Red);
        }

        public void SpawnTree(int id, Vector2 position, float size, Utility.EnumManager.TreeKind kind)
        {
            if (_treePrefab == null)
            {
                Debug.LogError("[Tree] No tree prefab wired on TreeView.");
                return;
            }

            SpriteRenderer tree = Instantiate(_treePrefab, transform);
            tree.transform.localPosition = new Vector3(position.x, position.y, 0f);
            tree.transform.localScale = new Vector3(size, size, 1f);

            Sprite sprite = SpriteFor(kind);
            if (sprite != null)
            {
                tree.sprite = sprite;
            }

            tree.color = ColorFor(kind);

            // The collider's hitbox needs the id so an arrow can name the tree it hit.
            TreeHitbox hitbox = tree.GetComponent<TreeHitbox>();
            if (hitbox != null)
            {
                hitbox.TreeId = id;
            }

            _trees[id] = tree;

            // The bar is a child of the clone, so it dies with the tree and follows the stump's scale.
            if (_healthBarPrefab != null)
            {
                Utility.HealthBarView bar = Instantiate(_healthBarPrefab, tree.transform);
                bar.Init(tree.transform, size);
                _bars[id] = bar;
            }
        }

        /// <summary>Ratio 1 hides the bar: untouched, chopped or freshly grown trees show nothing.</summary>
        public void SetHealth(int id, float normalized)
        {
            if (_bars.TryGetValue(id, out Utility.HealthBarView bar) && bar != null)
            {
                bar.Show(normalized);
            }
        }

        private Sprite SpriteFor(Utility.EnumManager.TreeKind kind)
        {
            switch (kind)
            {
                case Utility.EnumManager.TreeKind.Purple:
                    return _purpleSprite;
                case Utility.EnumManager.TreeKind.Magic:
                    return _magicSprite;
                case Utility.EnumManager.TreeKind.Arcana:
                    return _arcanaSprite;
                default:
                    return _redSprite;
            }
        }

        private Color ColorFor(Utility.EnumManager.TreeKind kind)
        {
            switch (kind)
            {
                case Utility.EnumManager.TreeKind.Purple:
                    return _purpleColor;
                case Utility.EnumManager.TreeKind.Magic:
                    return _magicColor;
                case Utility.EnumManager.TreeKind.Arcana:
                    return _arcanaColor;
                default:
                    return _redColor;
            }
        }

        /// <summary>A chopped tree leaves nothing behind: sprite, collider and bar all follow the flag.</summary>
        public void SetVisible(int id, bool visible)
        {
            if (!_trees.TryGetValue(id, out SpriteRenderer tree) || tree == null)
            {
                return;
            }

            tree.enabled = visible;

            // A chopped tree is gone: its collider must not keep blocking or eating arrows.
            Collider2D hitbox = tree.GetComponent<Collider2D>();
            if (hitbox != null)
            {
                hitbox.enabled = visible;
            }

            // A chopped or regrown tree is untouched again, so its bar goes away with the sprite.
            SetHealth(id, 1f);
        }

        public void RemoveTree(int id)
        {
            if (!_trees.TryGetValue(id, out SpriteRenderer tree))
            {
                return;
            }

            if (tree != null)
            {
                Destroy(tree.gameObject);
            }

            _trees.Remove(id);
            _bars.Remove(id);
        }

        public void ClearTrees()
        {
            foreach (SpriteRenderer tree in _trees.Values)
            {
                if (tree != null)
                {
                    Destroy(tree.gameObject);
                }
            }

            _trees.Clear();
            _bars.Clear();
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
