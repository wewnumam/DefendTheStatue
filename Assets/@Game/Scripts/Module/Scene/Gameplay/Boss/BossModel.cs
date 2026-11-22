using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Boss
{
    /// <summary>
    /// The Destroyer (GDD §8). 20 HP, 10 melee, no projectile and no enrage: the only thing a phase
    /// changes is which bundle of minions it summons, plus a doubled attack rate in phase 3.
    /// </summary>
    public class BossModel : BaseModel
    {
        /// <summary>Phase 2 starts once health drops below this fraction.</summary>
        public const float Phase2Threshold = 0.6f;

        /// <summary>Phase 3 starts once health drops below this fraction.</summary>
        public const float Phase3Threshold = 0.3f;

        private BossConstants _constants = new BossConstants();

        public BossConstants Constants => _constants;

        public bool IsActive { get; private set; }

        /// <summary>Cheat flag mirrored from CheatToggledMessage; the statue takes no boss damage while on.</summary>
        public bool IsGodMode { get; private set; }

        public float Health { get; private set; }
        public int Phase { get; private set; }
        public Vector2 Position { get; set; }
        public float AttackCooldownRemaining { get; private set; }

        /// <summary>Summons the controller still owes this phase; drained with <see cref="TakeSummons"/>.</summary>
        private readonly List<EnumManager.MonsterKind> _pendingSummons = new List<EnumManager.MonsterKind>();

        public float MaxHealth => _constants.MaxHealth;
        public bool IsAlive => IsActive && Health > 0f;
        public float HealthFraction => MaxHealth > 0f ? Health / MaxHealth : 0f;

        /// <summary>GDD §8: the boss never retreats and never speeds up; phases only change the summons.</summary>
        public int PendingSummonCount => _pendingSummons.Count;

        /// <summary>GDD §6: phase 3 attacks twice as often.</summary>
        public float AttackIntervalSeconds => _constants.AttackIntervalForPhase(Phase);

        public void SetConstants(BossConstants constants)
        {
            _constants = constants ?? new BossConstants();
            SetDataAsDirty();
        }

        /// <summary>GDD §8 arrival: full health, phase 1, its minion bundle owed.</summary>
        public void Spawn()
        {
            IsActive = true;
            Health = MaxHealth;
            Phase = 1;
            AttackCooldownRemaining = 0f;
            _pendingSummons.Clear();
            AddSummons(_constants.SummonsForPhase(1));
            SetDataAsDirty();
        }

        private void AddSummons(IReadOnlyList<EnumManager.MonsterKind> summons)
        {
            if (summons == null)
            {
                return;
            }

            for (int i = 0; i < summons.Count; i++)
            {
                _pendingSummons.Add(summons[i]);
            }
        }

        /// <summary>Returns true when the hit crossed into a new phase.</summary>
        public bool ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return false;
            }

            int previousPhase = Phase;
            Health = Mathf.Max(0f, Health - amount);

            if (Health <= 0f)
            {
                SetDataAsDirty();
                return false;
            }

            int nextPhase = PhaseForFraction(HealthFraction);
            if (nextPhase == previousPhase)
            {
                SetDataAsDirty();
                return false;
            }

            Phase = nextPhase;
            AddSummons(_constants.SummonsForPhase(nextPhase));
            SetDataAsDirty();
            return true;
        }

        /// <summary>Hands the owed summons to the controller and clears the tally.</summary>
        public List<EnumManager.MonsterKind> TakeSummons()
        {
            List<EnumManager.MonsterKind> summons = new List<EnumManager.MonsterKind>(_pendingSummons);
            _pendingSummons.Clear();
            return summons;
        }

        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || !IsAlive)
            {
                return;
            }

            if (AttackCooldownRemaining > 0f)
            {
                AttackCooldownRemaining = Mathf.Max(0f, AttackCooldownRemaining - deltaSeconds);
            }
        }

        /// <summary>One attack per cooldown, like the other combatants.</summary>
        public bool TryAttack()
        {
            if (!IsAlive || AttackCooldownRemaining > 0f)
            {
                return false;
            }

            AttackCooldownRemaining = AttackIntervalSeconds;
            SetDataAsDirty();
            return true;
        }

        /// <summary>Cheat toggles arrive as messages; the model only remembers the flag.</summary>
        public void SetGodMode(bool isActive)
        {
            IsGodMode = isActive;
            SetDataAsDirty();
        }

        public void Reset()
        {
            IsActive = false;
            Health = 0f;
            Phase = 0;
            Position = Vector2.zero;
            AttackCooldownRemaining = 0f;
            _pendingSummons.Clear();
            IsGodMode = false;
            SetDataAsDirty();
        }

        /// <summary>60% and 30% are the boundaries; landing exactly on one moves to the next phase.</summary>
        public static int PhaseForFraction(float fraction)
        {
            if (fraction > Phase2Threshold)
            {
                return 1;
            }

            return fraction > Phase3Threshold ? 2 : 3;
        }
    }
}
