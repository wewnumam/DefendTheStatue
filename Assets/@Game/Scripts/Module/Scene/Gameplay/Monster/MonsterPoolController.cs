using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// Owns every monster on screen. One pooled object per monster, rebound by id, so the pool never
    /// drifts when a corpse is removed from the middle of the model list.
    ///
    /// Nothing damages monsters except the messages below: the reference update loop never reads a
    /// monster's damage on anything but the statue, and that path is deferred with the boss. Keeping the
    /// damage API here means the player's attacks reach them through the bus, not through a reference to
    /// this controller.
    /// </summary>
    public class MonsterPoolController : ObjectController<MonsterPoolController, MonsterModel, MonsterPoolView>
    {
        private const int PoolSize = 64;

        private readonly List<MonsterController> _pool = new List<MonsterController>();
        private readonly Dictionary<int, MonsterController> _bound = new Dictionary<int, MonsterController>();
        private readonly List<int> _hitIds = new List<int>();
        private readonly List<int> _removedIds = new List<int>();

        public override void SetView(MonsterPoolView view)
        {
            base.SetView(view);

            for (int i = 0; i < PoolSize; i++)
            {
                _pool.Add(CreateMonsterObject());
            }

            view.SetCallback(OnTick);
        }

        private MonsterController CreateMonsterObject()
        {
            MonsterView monsterView;
            if (_view.MonsterPrefab != null)
            {
                GameObject instance = GameObject.Instantiate(_view.MonsterPrefab, _view.transform);
                monsterView = instance.GetComponent<MonsterView>();
            }
            else
            {
                monsterView = _view.CreatePlaceholder();
            }

            // Both paths end up the same size. The reference monster is 25 units, so a prefab authored at
            // 1 unit would be a speck beside the 96-radius statue. Colliders scale with the transform.
            monsterView.transform.localScale = Vector3.one * MonsterState.Size;

            var controller = new MonsterController();
            InjectDependencies(controller);
            controller.Init(monsterView, -1);
            monsterView.SetMonsterId(-1);
            monsterView.gameObject.SetActive(false);

            return controller;
        }

        private void OnTick(float deltaTime)
        {
            // The statue sits on the world origin — every territory ring is centred there — so the walk
            // target is a constant rather than a lookup through another module.
            _model.Advance(deltaTime, Vector2.zero, MonsterState.StatueStopDistance);

            for (int i = 0; i < _model.Count; i++)
            {
                MonsterState monster = _model.Monsters[i];
                if (_bound.TryGetValue(monster.Id, out MonsterController controller))
                {
                    controller.Sync(monster.Position);
                }
            }
        }

        public void OnSpawnMonster(RequestSpawnMonsterMessage message)
        {
            int id = _model.Add(message.Position, message.InHealRadius);
            MonsterState monster = _model.Get(id);

            MonsterController controller = Rent(id);
            if (controller != null)
            {
                controller.Sync(monster.Position);
                controller.ViewRef.gameObject.SetActive(true);
            }

            Publish(new MonsterSpawnedMessage(id, monster.Position, monster.InHealRadius, monster.Health));
        }

        public void OnDamageMonster(RequestDamageMonsterMessage message)
        {
            MonsterState monster = _model.Get(message.MonsterId);
            if (monster == null || monster.IsDead)
            {
                return;
            }

            _model.ApplyDamage(message.MonsterId, message.Amount);
            ReportHit(message.MonsterId, message.HitPosition);
        }

        public void OnAreaDamage(RequestAreaDamageMessage message)
        {
            _hitIds.Clear();
            _model.ApplyAreaDamage(message.Center, message.Radius, message.Amount, _hitIds, message.OncePerMonster);

            for (int i = 0; i < _hitIds.Count; i++)
            {
                ReportHit(_hitIds[i], message.HitOrigin);
            }
        }

        public void OnResetDashHits(RequestResetDashHitsMessage message)
        {
            _model.ResetDashHits();
        }

        /// <summary>Publishes the hit, recycles the view when it was the killing blow, then drops corpses.</summary>
        private void ReportHit(int monsterId, Vector2 hitPosition)
        {
            MonsterState monster = _model.Get(monsterId);
            if (monster == null)
            {
                return;
            }

            if (!monster.IsDead)
            {
                Publish(new MonsterDamagedMessage(monsterId, monster.Health, hitPosition));
                return;
            }

            Publish(new MonsterKilledMessage(monsterId, monster.Position, monster.GoldValue));

            _removedIds.Clear();
            _removedIds.Add(monsterId);
            _model.RemoveDead(_removedIds);

            for (int i = 0; i < _removedIds.Count; i++)
            {
                if (_bound.TryGetValue(_removedIds[i], out MonsterController controller))
                {
                    _bound.Remove(_removedIds[i]);
                    controller.ViewRef.SetMonsterId(-1);
                    controller.Release();
                }
            }
        }

        private MonsterController Rent(int monsterId)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].MonsterId < 0)
                {
                    _pool[i].Init(_pool[i].ViewRef, monsterId);
                    _pool[i].ViewRef.SetMonsterId(monsterId);
                    _bound[monsterId] = _pool[i];
                    return _pool[i];
                }
            }

            return null;
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != Utility.EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();
            _bound.Clear();

            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].Release();
            }
        }
    }
}
