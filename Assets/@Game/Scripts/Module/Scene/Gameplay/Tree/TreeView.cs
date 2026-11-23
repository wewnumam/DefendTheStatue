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

        private readonly Dictionary<int, SpriteRenderer> _trees = new Dictionary<int, SpriteRenderer>();
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

        /// <summary>Hides a chopped tree without dropping its collider, so the stump stays solid.</summary>
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

        public void SetVisible(int id, bool visible)
        {
            if (!_trees.TryGetValue(id, out SpriteRenderer tree) || tree == null)
            {
                return;
            }

            tree.enabled = visible;
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
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
