using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// Scene view for the monster pool. It also owns the per-frame tick, the same way BulletPoolView
    /// does, so the model is advanced once per rendered frame.
    /// </summary>
    public class MonsterPoolView : BaseView
    {
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private Camera _camera;

        private UnityAction<float> _onTick;

        /// <summary>Optional. When null the pool builds a plain square, so nothing needs authoring.</summary>
        public GameObject MonsterPrefab => _monsterPrefab;

        public Camera Camera => _camera != null ? _camera : Camera.main;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }

        /// <summary>
        /// Builds the placeholder monster: a plain white square, centred on its transform. The pool sets
        /// the scale to MonsterState.Size so a prefab and a placeholder come out the same size.
        /// </summary>
        public MonsterView CreatePlaceholder()
        {
            var gameObject = new GameObject("Monster (placeholder)");
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprite.Square;
            renderer.color = PlaceholderColor;
            gameObject.transform.SetParent(transform, false);

            MonsterView view = gameObject.AddComponent<MonsterView>();
            view.Init(renderer, PlaceholderColor);
            return view;
        }

        private static readonly Color PlaceholderColor = new Color(0.85f, 0.35f, 0.35f);
    }
}
