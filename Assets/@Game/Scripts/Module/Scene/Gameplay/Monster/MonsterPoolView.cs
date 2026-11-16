using Agate.MVC.Base;
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

        public GameObject MonsterPrefab => _monsterPrefab;

        public Camera Camera => _camera;

        public void SetCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);
        }
    }
}
