using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Tower
{
    /// <summary>
    /// Owns every built tower and fires them on the reference cadence. Monster positions are
    /// mirrored from spawn and kill messages, so towers never read the monster model directly.
    /// </summary>
    public class TowerController : ObjectController<TowerController, TowerModel, TowerView>
    {
        private readonly Dictionary<int, Vector2> _monsterPositions = new Dictionary<int, Vector2>();
        private readonly List<int> _targets = new List<int>();

        public override void SetView(TowerView view)
        {
            base.SetView(view);
            _model.Clear();
            _monsterPositions.Clear();
            view.SetCallback(OnTick);
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            _model.Add(message.Position, message.TerritoryIndex, message.CornerIndex);
            _view.AddTower(message.Position);
        }

        public void OnMonsterSpawned(MonsterSpawnedMessage message)
        {
            _monsterPositions[message.MonsterId] = message.Position;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _monsterPositions.Remove(message.MonsterId);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();
            _monsterPositions.Clear();

            // The launcher publishes the playing state before it wires the views; the other
            // controllers guard their view for the same reason.
            if (_view != null)
            {
                _view.ClearTowers();
            }
        }

        /// <summary>Every monster in range is shot at by the nearest tower that is off cooldown.</summary>
        private void OnTick(float deltaSeconds)
        {
            if (_model.Count == 0 || _monsterPositions.Count == 0)
            {
                return;
            }

            _model.TickCooldowns(deltaSeconds);
            _targets.Clear();
            _targets.AddRange(_monsterPositions.Keys);

            for (int i = 0; i < _targets.Count; i++)
            {
                int monsterId = _targets[i];
                if (!_monsterPositions.TryGetValue(monsterId, out Vector2 target))
                {
                    continue;
                }

                int shooter = _model.FindShooterFor(target);
                if (shooter < 0)
                {
                    continue;
                }

                TowerState tower = _model.Get(shooter);
                float damage = _model.TryFire(shooter, target);
                if (damage <= 0f)
                {
                    continue;
                }

                _view.ShowShot(tower.Position, target);
                Publish(new TowerFiredMessage(tower.Position, tower.AimAngle, target, damage));
                Publish(new RequestDamageMonsterMessage(monsterId, damage, target));
            }
        }
    }
}
