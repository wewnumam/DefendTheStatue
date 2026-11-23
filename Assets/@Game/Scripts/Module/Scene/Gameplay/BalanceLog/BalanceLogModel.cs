using System;
using System.Collections.Generic;
using Agate.MVC.Base;

namespace ProjectTA.Module.BalanceLog
{
    /// <summary>
    /// The numbers a balance pass needs, sampled on the run clock: statue health fraction, live
    /// monsters, kills and whether the boss is out. Formatting lives here so the CSV shape is testable
    /// without a scene.
    /// </summary>
    public class BalanceLogModel : BaseModel
    {
        public const int SampleIntervalSeconds = 30;

        public struct Sample
        {
            public int Seconds;
            public float HealthFraction;
            public int Monsters;
            public int Kills;
            public bool BossPresent;
        }

        private readonly List<Sample> _samples = new List<Sample>();
        private int _spawned;
        private int _killed;
        private int _lastSampleSecond = -1;
        private float _healthFraction = 1f;
        private int _bossArrivalSeconds = -1;
        private bool _bossAlive;

        public bool IsEnabled { get; private set; }
        public IReadOnlyList<Sample> Samples => _samples;
        public int Monsters => Math.Max(0, _spawned - _killed);
        public int Kills => _killed;
        public int BossArrivalSeconds => _bossArrivalSeconds;

        public const string Header = "seconds,statue_hp,monsters,kills,boss";

        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            SetDataAsDirty();
        }

        public void OnSpawned()
        {
            _spawned++;
        }

        public void OnKilled()
        {
            _killed++;
        }

        public void OnStatueDamaged(float healthFraction)
        {
            _healthFraction = healthFraction;
        }

        public void OnBossSpawned(int totalSeconds)
        {
            if (_bossArrivalSeconds < 0)
            {
                _bossArrivalSeconds = totalSeconds;
            }

            _bossAlive = true;
        }

        public void OnBossDefeated()
        {
            _bossAlive = false;
        }

        /// <summary>One row every <see cref="SampleIntervalSeconds"/>; the clock tick decides when.</summary>
        public bool TryTakeSample(int totalSeconds, out Sample sample)
        {
            sample = default;

            if (!IsEnabled || totalSeconds < 0 || totalSeconds % SampleIntervalSeconds != 0)
            {
                return false;
            }

            if (totalSeconds == _lastSampleSecond)
            {
                return false;
            }

            _lastSampleSecond = totalSeconds;
            sample = new Sample
            {
                Seconds = totalSeconds,
                HealthFraction = _healthFraction,
                Monsters = Monsters,
                Kills = _killed,
                BossPresent = _bossAlive,
            };
            _samples.Add(sample);
            SetDataAsDirty();
            return true;
        }

        public static string Format(Sample sample)
        {
            return string.Format(
                "{0},{1:F3},{2},{3},{4}",
                sample.Seconds,
                sample.HealthFraction,
                sample.Monsters,
                sample.Kills,
                sample.BossPresent ? 1 : 0);
        }

        public string Summary()
        {
            return string.Format(
                "summary,{0},{1},{2},{3}",
                _lastSampleSecond < 0 ? 0 : _lastSampleSecond,
                _bossArrivalSeconds,
                _killed,
                _samples.Count);
        }
    }
}
