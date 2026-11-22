using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.AdaptiveAi
{
    /// <summary>
    /// View-less: it reads the bus and publishes one threat dial. Ticks on the run clock (once a second),
    /// which is plenty for a difficulty knob, and mirrors monster positions so it can tell whether the
    /// statue is being leaned on.
    /// </summary>
    public class AdaptiveAiController : BaseController<AdaptiveAiController>
    {
        /// <summary>Statue heal radius (GDD §5); anything inside counts as pressure on the statue.</summary>
        private const float StatuePressureRadius = 120f;

        private readonly AdaptiveAiModel _model = new AdaptiveAiModel();
        private readonly Dictionary<int, Vector2> _monsterPositions = new Dictionary<int, Vector2>();

        private Transform _player;
        private float _statueHealthFraction = 1f;
        private float _playerHealthFraction = 1f;
        private float _lastPublishedBuff = -1f;

        public AdaptiveAiModel Model => _model;

        public void SetPlayerTransform(Transform player)
        {
            _player = player;
        }

        public void OnRunClock(RunClockMessage message)
        {
            if (_player == null)
            {
                return;
            }

            _model.Tick(
                1f,
                IsPlayerNearStatue(),
                CountMonstersNearStatue(),
                _statueHealthFraction,
                _playerHealthFraction);

            float buff = _model.MonsterBuff;
            if (Mathf.Abs(buff - _lastPublishedBuff) < 0.01f)
            {
                return;
            }

            _lastPublishedBuff = buff;
            Publish(new ThreatLevelMessage(_model.Threat, buff));
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _playerHealthFraction = message.MaxHealth <= 0f ? 1f : message.Health / message.MaxHealth;
        }

        public void OnStatueDamaged(StatueDamagedMessage message)
        {
            _statueHealthFraction = message.MaxHealth <= 0f ? 1f : message.Health / message.MaxHealth;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _monsterPositions.Remove(message.MonsterId);
        }

        public void OnMonsterSpawned(MonsterSpawnedMessage message)
        {
            _monsterPositions[message.MonsterId] = message.Position;
        }

        public void OnGuardFired(GuardFiredMessage message)
        {
            _model.RegisterGuardShot();
        }

        public void OnMonsterKilledByPlayer(MonsterKilledMessage message)
        {
            _model.RegisterPlayerKill();
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();
            _monsterPositions.Clear();
            _statueHealthFraction = 1f;
            _playerHealthFraction = 1f;
            _lastPublishedBuff = -1f;
        }

        private bool IsPlayerNearStatue()
        {
            return ((Vector2)_player.position).sqrMagnitude <= StatuePressureRadius * StatuePressureRadius;
        }

        private int CountMonstersNearStatue()
        {
            int count = 0;
            float sqrRadius = StatuePressureRadius * StatuePressureRadius;

            foreach (KeyValuePair<int, Vector2> monster in _monsterPositions)
            {
                if (monster.Value.sqrMagnitude <= sqrRadius)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
