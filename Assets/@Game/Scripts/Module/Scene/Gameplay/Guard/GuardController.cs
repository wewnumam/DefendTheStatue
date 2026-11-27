using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Guard
{
    /// <summary>
    /// Guards recruited from the foundations the statue hands out at level 3 and level 5. Monster
    /// positions are mirrored from spawn, damage and kill messages, so the guard never reads the
    /// monster model directly. Eligibility and cooldowns live in the model.
    /// </summary>
    public class GuardController : ObjectController<GuardController, GuardModel, GuardView>
    {
        private readonly Dictionary<int, Vector2> _monsterPositions = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, Utility.EnumManager.MonsterKind> _monsterKinds =
            new Dictionary<int, Utility.EnumManager.MonsterKind>();

        private readonly List<int> _targets = new List<int>();

        /// <summary>Monster positions in the same order they are mirrored: the guards chase these.</summary>
        private readonly List<Vector2> _monsterTargets = new List<Vector2>();

        /// <summary>GDD §6 body numbers: guard walk speed is a multiple of the Stacker's base speed.</summary>
        private MonsterConstants _monsterConstants = new MonsterConstants();

        /// <summary>Mirror of the statue's heal radius, which is also where a retreating guard heals.</summary>
        private const float StatueHealRadius = 120f;

        /// <summary>How close a monster must be to bite a guard.</summary>
        private const float GuardBiteRadius = 40f;

        private bool _zoneTainted;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _model.SetConstants(
                gameConstants.GameConstants.GuardConstants,
                gameConstants.GameConstants.TerritoryConstants);

            if (gameConstants.GameConstants.MonsterConstants != null)
            {
                _monsterConstants = gameConstants.GameConstants.MonsterConstants;
            }
        }

        public override void SetView(GuardView view)
        {
            base.SetView(view);
            _model.Clear();
            _monsterPositions.Clear();
            _monsterKinds.Clear();
            _view.ClearGuards();
            view.SetCallback(OnTick);
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            int foundationLevel = message.Effect.GuardFoundationLevel;
            if (foundationLevel <= 0)
            {
                return;
            }

            SpawnFoundations(foundationLevel);
        }

        public void OnGuardBuilt(GuardBuiltMessage message)
        {
            GuardState guard = _model.Get(message.FoundationIndex);
            if (guard == null)
            {
                return;
            }

            if (!_model.TryBuild(guard.Index))
            {
                return;
            }

            _view?.SetBuilt(guard.Index, true);
        }

        /// <summary>GDD §13 bow skill 4: the skill module names a centre and a radius, the guard model applies it.</summary>
        public void OnGuardEmpower(GuardEmpowerMessage message)
        {
            _model.ApplyEmpower(
                message.Center,
                message.Radius,
                message.DamageMultiplier,
                message.AttackSpeedMultiplier,
                message.DurationSeconds);
        }

        public void OnMonsterSpawned(MonsterSpawnedMessage message)
        {
            _monsterPositions[message.MonsterId] = message.Position;
            _monsterKinds[message.MonsterId] = message.Kind;
        }

        public void OnHealZoneTaint(HealZoneTaintMessage message)
        {
            _zoneTainted = message.Tainted;
        }

        public void OnMonsterDamaged(MonsterDamagedMessage message)
        {
            _monsterPositions[message.MonsterId] = message.HitPosition;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _monsterPositions.Remove(message.MonsterId);
            _monsterKinds.Remove(message.MonsterId);
        }

        public void OnGuardDamaged(GuardDamagedMessage message)
        {
            _view?.SetHealth(message.GuardIndex, message.Health, message.MaxHealth);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();
            _monsterPositions.Clear();
            _monsterKinds.Clear();
            _zoneTainted = false;
            _view?.ClearGuards();
        }

        private void SpawnFoundations(int foundationLevel)
        {
            int count = GuardLayout.GetFoundationCount(foundationLevel);
            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector2 position = _model.Layout.GetFoundationPosition(foundationLevel, i, count);
                int index = _model.AddFoundation(foundationLevel, position);
                if (index < 0)
                {
                    continue;
                }

                _view?.SpawnFoundation(index, position, false);
            }
        }

        private void OnTick(float deltaSeconds)
        {
            _model.TickCooldowns(deltaSeconds);

            _monsterTargets.Clear();
            foreach (Vector2 position in _monsterPositions.Values)
            {
                _monsterTargets.Add(position);
            }

            _model.Tick(
                deltaSeconds,
                _monsterConstants.SpeedPerSecond,
                _zoneTainted,
                StatueHealRadius * 0.5f,
                _monsterTargets);

            SyncViews();
            BiteGuards(deltaSeconds);

            if (_model.Count == 0 || _monsterPositions.Count == 0)
            {
                return;
            }

            _targets.Clear();
            _targets.AddRange(_monsterPositions.Keys);

            for (int i = 0; i < _targets.Count; i++)
            {
                int monsterId = _targets[i];
                if (!_monsterPositions.TryGetValue(monsterId, out Vector2 target))
                {
                    continue;
                }

                int shooter = _model.FindGuardFor(target);
                if (shooter < 0)
                {
                    continue;
                }

                GuardState guard = _model.Get(shooter);
                if (!_model.TryFire(shooter))
                {
                    continue;
                }

                Publish(new GuardFiredMessage(guard.Position, target));
                _view?.PlayAttack(shooter, guard.Position, target);
                Publish(new RequestDamageMonsterMessage(
                    monsterId, _model.GetDamage(shooter), target));
            }
        }

        /// <summary>GDD §7: a Deasor chews guards (4 damage), everything else nibbles for 1.</summary>
        private void BiteGuards(float deltaSeconds)
        {
            if (_model.Count == 0 || _monsterPositions.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<int, Vector2> entry in _monsterPositions)
            {
                if (!_monsterKinds.TryGetValue(entry.Key, out Utility.EnumManager.MonsterKind kind))
                {
                    kind = Utility.EnumManager.MonsterKind.Stacker;
                }

                float damage = _monsterConstants.GetKind(kind).GuardDamage;
                if (damage <= 0f)
                {
                    continue;
                }

                for (int i = 0; i < _model.Count; i++)
                {
                    GuardState guard = _model.Get(i);
                    if (guard == null || !guard.IsArmed || guard.IsDead)
                    {
                        continue;
                    }

                    if ((guard.Position - entry.Value).sqrMagnitude > GuardBiteRadius * GuardBiteRadius)
                    {
                        continue;
                    }

                    if (!_model.TryTakeBite(i))
                    {
                        continue;
                    }

                    float health = _model.ApplyDamage(i, damage);
                    if (health >= 0f)
                    {
                        Publish(new GuardDamagedMessage(i, health, guard.MaxHealth));
                    }

                    break;
                }
            }
        }

        private void SyncViews()
        {
            if (_view == null)
            {
                return;
            }

            for (int i = 0; i < _model.Count; i++)
            {
                GuardState guard = _model.Get(i);
                if (guard == null)
                {
                    continue;
                }

                _view.SetPosition(i, guard.Position);
                _view.SetState(i, guard.State, guard.IsArmed && !guard.IsDead, guard.EmpowerRemaining > 0f);
            }
        }
    }
}
