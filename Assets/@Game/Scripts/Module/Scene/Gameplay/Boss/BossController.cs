using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Boss
{
    /// <summary>
    /// The Destroyer fight: walk at the statue, bite when in reach, drop the summon bundle each phase
    /// starts with and the eight-minion escort it arrives with. No projectiles, no enrage (GDD §8).
    /// </summary>
    public class BossController : ObjectController<BossController, BossModel, BossView>
    {
        private Vector2 _statuePosition;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _model.SetConstants(gameConstants.GameConstants.BossConstants);
        }

        public void SetStatuePosition(Vector2 statuePosition)
        {
            _statuePosition = statuePosition;
        }

        public override void SetView(BossView view)
        {
            base.SetView(view);
            view.SetCallback(OnTick);
            view.Hide();
        }

        public void OnBossSpawned(BossSpawnedMessage message)
        {
            // No Reset here: a cheat toggled before the boss arrives must survive the spawn.
            _model.Spawn();
            _view.Spawn(_statuePosition + Vector2.up * _model.Constants.SpawnDistance);
            _view.SetPhase(_model.Phase);
            SpawnEscort();
            SpawnSummons();
        }

        private void OnTick(float deltaSeconds)
        {
            if (!_model.IsAlive)
            {
                return;
            }

            _model.Tick(deltaSeconds);
            Vector2 position = _view.Position;
            Vector2 toStatue = _statuePosition - position;
            float distance = toStatue.magnitude;

            if (distance > _model.Constants.AttackRange)
            {
                float step = _model.Constants.Speed * deltaSeconds;
                _view.SetPosition(Vector2.MoveTowards(position, _statuePosition, step));
                return;
            }

            if (_model.TryAttack())
            {
                _view.PlayAttack();

                if (!_model.IsGodMode)
                {
                    Publish(new RequestStatueDamageMessage(_model.Constants.Damage));
                }
            }

            SpawnSummons();
        }

        /// <summary>Each phase owes a bundle at the moment it starts; the model only tallies it.</summary>
        private void SpawnSummons()
        {
            System.Collections.Generic.List<Utility.EnumManager.MonsterKind> summons = _model.TakeSummons();
            for (int i = 0; i < summons.Count; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * _model.Constants.SummonRadius;
                Publish(new RequestSpawnMonsterMessage(_view.Position + offset, false, summons[i]));
            }
        }

        /// <summary>GDD §8: the boss arrives surrounded by four Stackers, two Waltroys and two Deasors.</summary>
        private void SpawnEscort()
        {
            System.Collections.Generic.IReadOnlyList<Utility.EnumManager.MonsterKind> escort = _model.Constants.Escort;
            if (escort == null)
            {
                return;
            }

            for (int i = 0; i < escort.Count; i++)
            {
                float angle = Mathf.PI * 2f * i / escort.Count;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _model.Constants.SummonRadius;
                Publish(new RequestSpawnMonsterMessage(_view.Position + offset, false, escort[i]));
            }
        }

        public void OnRequestDamageBoss(RequestDamageBossMessage message)
        {
            if (!_model.IsAlive)
            {
                return;
            }

            bool phaseChanged = _model.ApplyDamage(message.Amount);
            _view.PlayHit();
            Publish(new BossDamagedMessage(_model.Health, _model.MaxHealth, _model.Phase));

            if (!_model.IsAlive)
            {
                _view.Hide();
                Publish(new BossDefeatedMessage());
                return;
            }

            if (phaseChanged)
            {
                _view.SetPhase(_model.Phase);
                Publish(new BossPhaseChangedMessage(_model.Phase));
            }
        }

        /// <summary>The sword hits through the shared area-damage path; the boss is not a pooled monster.</summary>
        public void OnAreaDamage(RequestAreaDamageMessage message)
        {
            if (!_model.IsAlive || message.Amount <= 0f)
            {
                return;
            }

            if (Vector2.Distance(_view.Position, message.Center) > message.Radius)
            {
                return;
            }

            OnRequestDamageBoss(new RequestDamageBossMessage(message.Amount, message.Center));
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == EnumManager.CheatType.GodMode)
            {
                _model.SetGodMode(message.Enabled);
            }
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                _model.Reset();

                if (_view != null)
                {
                    _view.Hide();
                }

                return;
            }

            if (message.GameState == EnumManager.GameState.GameOver
                || message.GameState == EnumManager.GameState.GameWin)
            {
                if (_view != null)
                {
                    _view.Hide();
                }
            }
        }
    }
}
