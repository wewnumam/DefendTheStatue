using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// Per-monster controller. The pool builds one per pooled object and binds it, never registers it
    /// — Agate's container is type-keyed and first-registration-wins, so registering these would make
    /// every monster share one instance.
    /// </summary>
    public class MonsterController : ObjectController<MonsterController, MonsterView>
    {
        public int MonsterId { get; private set; } = -1;

        public MonsterView ViewRef => _view;

        public void Init(MonsterView view, int monsterId)
        {
            SetView(view);
            MonsterId = monsterId;
        }

        /// <summary>Hands the object back to the pool, view hidden and unbound.</summary>
        public void Release(int monsterId = -1)
        {
            MonsterId = monsterId;
            _view.gameObject.SetActive(false);
        }

        public void Sync(Vector2 position)
        {
            _view.SetPosition(position);
        }

        /// <summary>Pool-driven walk; the body does the moving, so the model can read it back later.</summary>
        public void SetVelocity(Vector2 velocity)
        {
            _view.SetVelocity(velocity);
        }

        public void Flash()
        {
            _view.PlayHitFlash();
            _view.EndHitFlash();
        }

        /// <summary>Bite pose; the pool tick polls it back to idle.</summary>
        public void PlayAttack()
        {
            _view.PlayAttackAnimation();
        }

        /// <summary>Damage pose; the pool tick polls it back to idle.</summary>
        public void PlayHit()
        {
            _view.PlayHitAnimation();
        }

        public void TickAnimation(float deltaTime)
        {
            _view.TickAnimation(deltaTime);
        }
    }
}
