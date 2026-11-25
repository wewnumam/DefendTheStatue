using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.AdaptiveAi;
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
        private readonly List<int> _attackedIds = new List<int>();

        /// <summary>
        /// The pool's own copy of the slow zones. A field is one small struct and both copies are fed by
        /// the same message, so this stays in sync without a controller-to-controller reference.
        /// </summary>
        private readonly SlowFieldModel _slowFields = new SlowFieldModel();

        /// <summary>
        /// The pool's own copy of the living trees, fed by TreeSpawnedMessage / TreeChoppedMessage, so a
        /// hurt monster can walk to a tree without the monster module reaching into the tree model.
        /// </summary>
        private readonly Dictionary<int, Vector2> _trees = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, Utility.EnumManager.TreeKind> _treeKinds =
            new Dictionary<int, Utility.EnumManager.TreeKind>();

        private TreeConstants _treeConstants;
        private bool _oneHitKill;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _model.SetMonsterConstants(gameConstants.GameConstants.MonsterConstants);
            _treeConstants = gameConstants.GameConstants.TreeConstants;
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

        /// <summary>Statue heal radius (GDD §5: 120), in world units.</summary>
        private const float StatueHealRadius = 120f;

        private bool _healZoneTainted;

        /// <summary>GDD §14 adaptive difficulty: applied to every monster as it spawns.</summary>
        private float _statMultiplier = 1f;
        private readonly List<MonsterModel.AllyAttack> _allyAttacks = new List<MonsterModel.AllyAttack>();

        /// <summary>GDD §7: monsters bite whoever is closer, so the pool needs the pawn's position.</summary>
        private Transform _player;

        /// <summary>How close the player must stand before a hurt monster bolts (GDD §7).</summary>
        private const float RetreatNearDistance = 120f;

        public void SetPlayerTransform(Transform player)
        {
            _player = player;
        }

        private void OnTick(float deltaTime)
        {
            // The statue sits on the world origin — every territory ring is centred there — so the walk
            // target is a constant rather than a lookup through another module.
            float now = Time.time;
            _slowFields.RemoveExpired(now);
            _model.TickHealCooldowns(deltaTime);

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

                // GDD §21: the ally body marks a monster under sword skill 4, whatever else it is doing.
                controller.SetMindControlled(monster.IsMindControlled);

                float multiplier = _slowFields.GetSpeedMultiplierAt(monster.Position, now);

                Vector2 target = Vector2.zero;
                float stopDistance = _model.Constants.StopDistance;

                // GDD §7 retreat wins over healing and biting: hurt + player nearby means run away.
                monster.IsRetreating = _player != null
                    && monster.WantsRetreat(_player.position, RetreatNearDistance);

                if (monster.IsRetreating)
                {
                    target = monster.Position + (monster.Position - (Vector2)_player.position).normalized * 1000f;
                    stopDistance = 0f;
                    controller.SetVelocity(_model.GetVelocity(
                        monster, target, stopDistance, multiplier * _model.Constants.RetreatSpeedMultiplier));
                    controller.TickAnimation(deltaTime);
                    continue;
                }

                TryHealAtTree(monster, ref target, ref stopDistance);

                controller.SetVelocity(_model.GetVelocity(monster, target, stopDistance, multiplier));
                controller.TickAnimation(deltaTime);
            }

            // GDD §5: the statue's heal zone is tainted while anything hostile stands inside it.
            bool tainted = _model.FindNearestInRange(Vector2.zero, StatueHealRadius) >= 0;
            if (tainted != _healZoneTainted)
            {
                _healZoneTainted = tainted;
                Publish(new HealZoneTaintMessage(tainted));
            }

            // GDD §13 sword skill 4: allies swing at the nearest hostile monster.
            _allyAttacks.Clear();
            if (_model.CollectAllyAttacks(deltaTime, _allyAttacks) > 0)
            {
                for (int i = 0; i < _allyAttacks.Count; i++)
                {
                    MonsterModel.AllyAttack attack = _allyAttacks[i];
                    OnDamageMonster(new RequestDamageMonsterMessage(attack.VictimId, attack.Damage, attack.HitPosition));
                }
            }

            // Bites are reported, not applied: statue health is the statue module's to change.
            _attackedIds.Clear();
            float damage = _model.CollectStatueDamage(deltaTime, Vector2.zero, _attackedIds);
            if (damage > 0f)
            {
                Publish(new RequestStatueDamageMessage(damage));
            }

            for (int i = 0; i < _attackedIds.Count; i++)
            {
                if (_bound.TryGetValue(_attackedIds[i], out MonsterController biter))
                {
                    biter.PlayAttack();
                }
            }

            // GDD §20: whoever is closer to the pawn bites the pawn instead of the statue.
            if (_player != null)
            {
                _attackedIds.Clear();
                float playerDamage = _model.CollectPlayerDamage(
                    deltaTime, _player.position, Vector2.zero, _attackedIds);

                if (playerDamage > 0f)
                {
                    Publish(new RequestPlayerDamageMessage(playerDamage));
                }

                for (int i = 0; i < _attackedIds.Count; i++)
                {
                    if (_bound.TryGetValue(_attackedIds[i], out MonsterController biter))
                    {
                        biter.PlayAttack();
                    }
                }
            }
        }

        /// <summary>
        /// Reference hpRatio &lt; 0.3 branch: walk to the nearest tree, bite it for its health, and take
        /// the tree's heal. Leaves the statue walk untouched when no tree is close enough.
        /// </summary>
        private void TryHealAtTree(MonsterState monster, ref Vector2 target, ref float stopDistance)
        {
            if (_treeConstants == null || !monster.WantsHeal)
            {
                return;
            }

            int treeId = FindNearestTree(monster.Position);
            if (treeId < 0)
            {
                return;
            }

            target = _trees[treeId];
            stopDistance = _treeConstants.MonsterHealRadius;

            if ((monster.Position - target).magnitude > stopDistance)
            {
                return;
            }

            if (!monster.TryStartHealCooldown(_model.Constants.HealCooldownSeconds))
            {
                return;
            }

            Utility.EnumManager.TreeKind kind = _treeKinds.ContainsKey(treeId)
                ? _treeKinds[treeId]
                : Utility.EnumManager.TreeKind.Red;
            TreeKindStats stats = _treeConstants.GetKind(kind);

            _model.Heal(monster.Id, stats.HpDrop);

            // A tree bite heals the monster, so the bar has to climb back (and vanish at full health).
            if (_bound.TryGetValue(monster.Id, out MonsterController healed))
            {
                healed.SetHealth(monster.Health, monster.MaxHealth);
            }

            Publish(new RequestTreeDamageMessage(
                treeId,
                stats.Health * _treeConstants.MonsterHealTreeDamageFraction,
                target,
                true));
        }

        private int FindNearestTree(Vector2 from)
        {
            int nearest = -1;
            float bestSquared = _treeConstants.HealSearchRange * _treeConstants.HealSearchRange;

            foreach (KeyValuePair<int, Vector2> tree in _trees)
            {
                // GDD §10: monsters never eat a Purple tree.
                if (_treeKinds.ContainsKey(tree.Key)
                    && _treeKinds[tree.Key] == Utility.EnumManager.TreeKind.Purple)
                {
                    continue;
                }

                float squared = (tree.Value - from).sqrMagnitude;
                if (squared < bestSquared)
                {
                    bestSquared = squared;
                    nearest = tree.Key;
                }
            }

            return nearest;
        }

        public void OnTreeSpawned(TreeSpawnedMessage message)
        {
            _trees[message.TreeId] = message.Position;
            _treeKinds[message.TreeId] = message.Kind;
        }

        public void OnTreeChopped(TreeChoppedMessage message)
        {
            _trees.Remove(message.TreeId);
            _treeKinds.Remove(message.TreeId);
        }

        public void OnSlowFieldAdded(SlowFieldAddedMessage message)
        {
            _slowFields.Add(message.Center, message.Radius, message.SlowFactor, Time.time, message.DurationSeconds);
        }

        public void OnSpawnMonster(RequestSpawnMonsterMessage message)
        {
            int id = _model.Add(message.Position, message.InHealRadius, message.Kind, _statMultiplier);
            MonsterState monster = _model.Get(id);

            MonsterController controller = Rent(id);
            if (controller != null)
            {
                controller.Sync(monster.Position);

                // GDD §7: the pool recycles one prefab, so the kind's body is applied before the object
                // is shown. A recycled Stacker must not spend a frame wearing the Deasor sprite.
                controller.ViewRef.SetKind(monster.Kind);
                controller.SetHealth(monster.Health, monster.MaxHealth);
                controller.ViewRef.gameObject.SetActive(true);
            }

            Publish(new MonsterSpawnedMessage(id, monster.Position, monster.InHealRadius, monster.Health, monster.Kind));
        }

        public void OnDamageMonster(RequestDamageMonsterMessage message)
        {
            MonsterState monster = _model.Get(message.MonsterId);
            if (monster == null || monster.IsDead)
            {
                return;
            }

            _model.ApplyDamage(message.MonsterId, _oneHitKill ? 999999f : message.Amount);
            ReportHit(message.MonsterId, message.HitPosition);
        }

        public void OnAreaDamage(RequestAreaDamageMessage message)
        {
            _hitIds.Clear();
            _model.ApplyAreaDamage(message.Center, message.Radius, _oneHitKill ? 999999f : message.Amount, _hitIds, message.OncePerMonster);

            for (int i = 0; i < _hitIds.Count; i++)
            {
                ReportHit(_hitIds[i], message.HitOrigin);
            }
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == Utility.EnumManager.CheatType.OneHitKill)
            {
                _oneHitKill = message.Enabled;
            }
        }

        public void OnDevKillAll(DevKillAllMessage message)
        {
            for (int i = _model.Monsters.Count - 1; i >= 0; i--)
            {
                MonsterState monster = _model.Monsters[i];
                OnDamageMonster(new RequestDamageMonsterMessage(monster.Id, 999999f, monster.Position));
            }
        }

        public void OnThreatLevel(ThreatLevelMessage message)
        {
            _statMultiplier = Mathf.Clamp(message.MonsterBuff, 1f, AdaptiveAiModel.MaxMonsterBuff);
        }

        public void OnMindControl(MindControlMessage message)
        {
            _model.ApplyMindControl(message.Center, message.Radius, message.DurationSeconds, message.DamageMultiplier);
        }

        /// <summary>Bow skill 2 asks for something to lock onto; the pool answers with the nearest live monster.</summary>
        public void OnRequestLockOnTarget(RequestLockOnTargetMessage message)
        {
            int id = _model.FindNearestInRange(message.Origin, message.Range);
            MonsterState target = id >= 0 ? _model.Get(id) : null;
            Publish(target != null
                ? new LockOnTargetMessage(target.Position, true)
                : new LockOnTargetMessage(Vector2.zero, false));
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
                if (_bound.TryGetValue(monsterId, out MonsterController hurt))
                {
                    hurt.PlayHit();
                    hurt.Flash();
                    hurt.SetHealth(monster.Health, monster.MaxHealth);
                }

                Publish(new MonsterDamagedMessage(monsterId, monster.Health, hitPosition));
                return;
            }

            Publish(new MonsterKilledMessage(monsterId, monster.Position, monster.EssenceValue, monster.Kind));

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
            _trees.Clear();

            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].Release();
            }
        }
    }
}
