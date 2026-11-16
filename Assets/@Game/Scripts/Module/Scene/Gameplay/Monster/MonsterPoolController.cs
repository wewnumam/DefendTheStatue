using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.SlowField;
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

        /// <summary>
        /// The pool's own copy of the slow zones. A field is one small struct and both copies are fed by
        /// the same message, so this stays in sync without a controller-to-controller reference.
        /// </summary>
        private readonly SlowFieldModel _slowFields = new SlowFieldModel();

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _model.SetMonsterConstants(gameConstants.GameConstants.MonsterConstants);
        }

        public override void SetView(MonsterPoolView view)
        {
            base.SetView(view);

            for (int i = 0; i < PoolSize; i++)
            {
                MonsterController controller = CreateMonsterObject();
                if (controller != null)
                {
                    _pool.Add(controller);
                }
            }

            view.SetCallback(OnTick);
        }

        private MonsterController CreateMonsterObject()
        {
            if (_view.MonsterPrefab == null)
            {
                Debug.LogError("[MonsterPool] No monster prefab wired on MonsterPoolView.");
                return null;
            }

            GameObject instance = GameObject.Instantiate(_view.MonsterPrefab, _view.transform);
            MonsterView monsterView = instance.GetComponent<MonsterView>();
            // The reference monster is 25 units, so a prefab authored at 1 unit would be a speck beside
            // the 96-radius statue. Colliders scale with the transform.
            monsterView.transform.localScale = Vector3.one * _model.Constants.Size;

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
            float now = Time.time;
            _slowFields.RemoveExpired(now);

            for (int i = 0; i < _model.Count; i++)
            {
                MonsterState monster = _model.Monsters[i];
                if (!_bound.TryGetValue(monster.Id, out MonsterController controller))
                {
                    continue;
                }

                // Physics owns the body, so the model reads back where it actually stands before it
                // decides the next step: walls and neighbours have had their say by now.
                _model.SetPosition(monster.Id, controller.ViewRef.Position);
                float multiplier = _slowFields.GetSpeedMultiplierAt(monster.Position, now);
                controller.SetVelocity(_model.GetVelocity(monster, Vector2.zero, _model.Constants.StopDistance, multiplier));
            }

            // Bites are reported, not applied: statue health is the statue module's to change.
            float damage = _model.CollectStatueDamage(deltaTime, Vector2.zero);
            if (damage > 0f)
            {
                Publish(new RequestStatueDamageMessage(damage));
            }
        }

        public void OnSlowFieldAdded(SlowFieldAddedMessage message)
        {
            _slowFields.Add(message.Center, message.Radius, message.SlowFactor, Time.time, message.DurationSeconds);
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
            _slowFields.Clear();

            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].Release();
            }
        }
    }
}
