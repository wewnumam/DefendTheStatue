using System;
using System.IO;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.BalanceLog
{
    /// <summary>
    /// Writes one CSV beside the build while a debug build runs: a row every 30 s plus a summary line
    /// when the run ends. Nothing ships in a release build, because nothing is enabled without
    /// <c>IsDebugBuild</c>.
    /// </summary>
    public class BalanceLogController : BaseController<BalanceLogController>
    {
        private readonly BalanceLogModel _model = new BalanceLogModel();
        private int _seconds;
        private string _path;

        public BalanceLogModel Model => _model;

        public BalanceLogController()
        {
            // Player builds keep the log off; Editor play mode and dev builds are where balance runs happen.
            _model.SetEnabled(UnityEngine.Debug.isDebugBuild);
        }

        public void OnRunClock(RunClockMessage message)
        {
            _seconds = message.TotalSeconds;

            if (_model.TryTakeSample(message.TotalSeconds, out BalanceLogModel.Sample sample))
            {
                Append(BalanceLogModel.Format(sample));
            }
        }

        public void OnMonsterSpawned(MonsterSpawnedMessage message)
        {
            _model.OnSpawned();
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.OnKilled();
        }

        public void OnStatueDamaged(StatueDamagedMessage message)
        {
            _model.OnStatueDamaged(message.HealthFraction);
        }

        public void OnBossSpawned(BossSpawnedMessage message)
        {
            _model.OnBossSpawned(_seconds);
        }

        public void OnBossDefeated(BossDefeatedMessage message)
        {
            _model.OnBossDefeated();
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                return;
            }

            if (!_model.IsEnabled)
            {
                return;
            }

            Append(_model.Summary());
            Debug.Log("Balance log: " + (_path ?? "no samples"));
        }

        private void Append(string line)
        {
            if (!_model.IsEnabled)
            {
                return;
            }

            if (_path == null)
            {
                _path = Path.Combine(
                    Application.persistentDataPath,
                    "balance_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
                File.WriteAllText(_path, BalanceLogModel.Header + Environment.NewLine);
            }

            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}
