using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Guard
{
    /// <summary>GDD §6 guard state machine.</summary>
    public enum GuardStateKind
    {
        Patrol = 0,
        Retreating = 1,
        Healing = 2,
        Returning = 3,
        Purging = 4
    }

    /// <summary>
    /// One guard. A foundation sits dim until the player recruits it; a recruited guard holds a
    /// cooldown between shots, can be hurt by monsters, and walks its retreat/heal/return loop.
    /// </summary>
    public class GuardState
    {
        public GuardState(int index, int foundationLevel, Vector2 position, GuardConstants constants)
        {
            Index = index;
            FoundationLevel = foundationLevel;
            Position = position;
            HomePosition = position;
            Constants = constants;
            MaxHealth = constants.HealthForLevel(foundationLevel);
            Health = MaxHealth;
        }

        public Vector2 HomePosition { get; }
        public float MaxHealth { get; }
        public float Health { get; private set; }
        public bool IsDead => Health <= 0f;

        /// <summary>GDD §6: PATROL -&gt; RETREATING -&gt; HEALING -&gt; RETURNING -&gt; PATROL, PURGING on a tainted zone.</summary>
        public GuardStateKind State { get; set; }

        public float HealFraction => MaxHealth <= 0f ? 0f : Health / MaxHealth;

        public float ApplyDamage(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0f;
            }

            Health = Mathf.Max(0f, Health - amount);
            return Health;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        public int Index { get; }
        public int FoundationLevel { get; }
        /// <summary>Settable: the state machine walks the guard home and back.</summary>
        public Vector2 Position { get; set; }
        public GuardConstants Constants { get; }

        public bool Built { get; set; }
        public float CooldownRemaining { get; set; }

        /// <summary>Clock between incoming monster bites.</summary>
        public float BiteCooldownRemaining { get; set; }

        /// <summary>GDD §13 bow skill 4: empowered guards hit harder and fire faster for 6 s.</summary>
        public float EmpowerRemaining { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public float AttackSpeedMultiplier { get; set; } = 1f;

        public bool IsArmed => Built;
    }

    /// <summary>
    /// Every guard on the map. Rules only: eligibility (level 3 guards the inner territory, level 5
    /// the outer ring), the attack cooldown and the target pick all live here so the controller just
    /// relays messages. Ring geometry arrives through <see cref="GuardLayout"/>.
    /// </summary>
    public class GuardModel : BaseModel
    {
        private readonly List<GuardState> _guards = new List<GuardState>();

        /// <summary>Defaults match the reference config; tests need no asset.</summary>
        private GuardConstants _constants = new GuardConstants();
        private GuardLayout _layout = new GuardLayout(new TerritoryConstants());

        public GuardConstants Constants => _constants;
        public GuardLayout Layout => _layout;

        public void SetConstants(GuardConstants constants, TerritoryConstants territoryConstants)
        {
            if (constants != null)
            {
                _constants = constants;
            }

            _layout = new GuardLayout(territoryConstants);
            SetDataAsDirty();
        }

        public IReadOnlyList<GuardState> Guards => _guards;
        public int Count => _guards.Count;

        public GuardState Get(int index)
        {
            if (index < 0 || index >= _guards.Count)
            {
                return null;
            }

            return _guards[index];
        }

        /// <summary>Returns the new foundation's index, or -1 when that slot already has one.</summary>
        public int AddFoundation(int foundationLevel, Vector2 position)
        {
            if (FindAt(position) != null)
            {
                return -1;
            }

            int index = _guards.Count;
            _guards.Add(new GuardState(index, foundationLevel, position, _constants));
            SetDataAsDirty();
            return index;
        }

        /// <summary>Reference recruiting a guard: arms it and resets its cooldown.</summary>
        public bool TryBuild(int index)
        {
            GuardState guard = Get(index);
            if (guard == null || guard.Built)
            {
                return false;
            }

            guard.Built = true;
            guard.CooldownRemaining = 0f;
            SetDataAsDirty();
            return true;
        }

        /// <summary>
        /// Reference guard shot: fires once per cooldown. Returns true when the shot should land.
        /// </summary>
        public bool TryFire(int index)
        {
            GuardState guard = Get(index);
            if (guard == null || !guard.IsArmed || guard.CooldownRemaining > 0f)
            {
                return false;
            }

            guard.CooldownRemaining = _constants.AttackIntervalSeconds / Mathf.Max(1f, guard.AttackSpeedMultiplier);
            SetDataAsDirty();
            return true;
        }

        /// <summary>Damage this guard lands, empower included.</summary>
        public float GetDamage(int index)
        {
            GuardState guard = Get(index);
            return guard == null ? 0f : _constants.Damage * guard.DamageMultiplier;
        }

        /// <summary>GDD §13 bow skill 4: empowers every recruited guard inside the radius. Returns the count.</summary>
        public int ApplyEmpower(Vector2 center, float radius, float damageMultiplier, float attackSpeedMultiplier, float durationSeconds)
        {
            int empowered = 0;
            float sqrRadius = radius * radius;

            for (int i = 0; i < _guards.Count; i++)
            {
                GuardState guard = _guards[i];
                if (!guard.IsArmed || (guard.Position - center).sqrMagnitude > sqrRadius)
                {
                    continue;
                }

                guard.EmpowerRemaining = Mathf.Max(guard.EmpowerRemaining, durationSeconds);
                guard.DamageMultiplier = Mathf.Max(1f, damageMultiplier);
                guard.AttackSpeedMultiplier = Mathf.Max(1f, attackSpeedMultiplier);
                empowered++;
            }

            if (empowered > 0)
            {
                SetDataAsDirty();
            }

            return empowered;
        }

        /// <summary>
        /// Level 3 guards only cover the inner territory (ring 0); level 5 guards only cover what is
        /// outside it. Any other level covers nothing.
        /// </summary>
        public bool IsEligibleTarget(Vector2 monsterPosition, int guardLevel)
        {
            return IsEligibleTarget(monsterPosition, guardLevel, GuardStateKind.Patrol);
        }

        /// <summary>
        /// GDD §6 patrol zones: inner guards hold territory 1, outer guards the band between 1 and 2,
        /// perimeter guards territory 2's edge. A PURGING guard ignores its zone and hits anything close.
        /// </summary>
        public bool IsEligibleTarget(Vector2 monsterPosition, int guardLevel, GuardStateKind state)
        {
            if (state == GuardStateKind.Purging)
            {
                return true;
            }

            bool insideInner = _layout.IsInsideRing(monsterPosition, 0);

            if (guardLevel >= 10)
            {
                return !_layout.IsInsideRing(monsterPosition, 1);
            }

            if (guardLevel >= 5)
            {
                return !insideInner;
            }

            return guardLevel >= 3 && insideInner;
        }

        /// <summary>
        /// The nearest recruited guard that may shoot this monster and is off cooldown, or -1.
        /// </summary>
        public int FindGuardFor(Vector2 monsterPosition)
        {
            float bestDistance = float.MaxValue;
            int best = -1;

            for (int i = 0; i < _guards.Count; i++)
            {
                GuardState guard = _guards[i];
                if (!guard.IsArmed || guard.CooldownRemaining > 0f)
                {
                    continue;
                }

                if (!IsEligibleTarget(monsterPosition, guard.FoundationLevel, guard.State))
                {
                    continue;
                }

                float distance = (guard.Position - monsterPosition).sqrMagnitude;
                if (distance > _constants.AttackRange * _constants.AttackRange)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// GDD §6 state machine plus movement. <paramref name="baseSpeed"/> is the Stacker's 1.0 speed in
        /// world units, so a guard walking at 2.0 uses twice that and a retreating one 1.5x more.
        /// </summary>
        public void Tick(float deltaSeconds, float baseSpeed, bool zoneTainted, float statueRadius)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            for (int i = 0; i < _guards.Count; i++)
            {
                GuardState guard = _guards[i];
                if (!guard.IsArmed || guard.IsDead)
                {
                    continue;
                }

                if (zoneTainted)
                {
                    guard.State = GuardStateKind.Purging;
                    continue;
                }

                if (guard.State == GuardStateKind.Purging)
                {
                    guard.State = GuardStateKind.Patrol;
                }

                if (guard.State != GuardStateKind.Retreating && guard.State != GuardStateKind.Healing
                    && guard.HealFraction < _constants.RetreatHealthFraction)
                {
                    guard.State = GuardStateKind.Retreating;
                }

                switch (guard.State)
                {
                    case GuardStateKind.Retreating:
                        MoveToward(guard, Vector2.zero, baseSpeed * _constants.RetreatSpeedMultiplier * deltaSeconds);
                        if (guard.Position.magnitude <= statueRadius)
                        {
                            guard.State = GuardStateKind.Healing;
                        }

                        break;

                    case GuardStateKind.Healing:
                        guard.Heal(guard.MaxHealth * _constants.HealFractionPerSecond * deltaSeconds);
                        if (guard.HealFraction >= _constants.ReturnHealthFraction)
                        {
                            guard.State = GuardStateKind.Returning;
                        }

                        break;

                    case GuardStateKind.Returning:
                        MoveToward(guard, guard.HomePosition, baseSpeed * _constants.SpeedMultiplier * deltaSeconds);
                        if ((guard.Position - guard.HomePosition).sqrMagnitude <= 4f)
                        {
                            guard.Position = guard.HomePosition;
                            guard.State = GuardStateKind.Patrol;
                        }

                        break;
                }
            }
        }

        private static void MoveToward(GuardState guard, Vector2 target, float step)
        {
            Vector2 toTarget = target - guard.Position;
            if (toTarget.sqrMagnitude <= step * step)
            {
                guard.Position = target;
                return;
            }

            guard.Position += toTarget.normalized * step;
        }

        /// <summary>Health loss from a monster bite. Returns the remaining health, or -1 when unarmed.</summary>
        public float ApplyDamage(int index, float amount)
        {
            GuardState guard = Get(index);
            if (guard == null || !guard.IsArmed)
            {
                return -1f;
            }

            float health = guard.ApplyDamage(amount);
            if (guard.IsDead)
            {
                guard.Built = false;
                guard.State = GuardStateKind.Patrol;
            }

            SetDataAsDirty();
            return health;
        }

        /// <summary>True when this guard may be bitten again; starts its bite cooldown.</summary>
        public bool TryTakeBite(int index)
        {
            GuardState guard = Get(index);
            if (guard == null || !guard.IsArmed || guard.IsDead || guard.BiteCooldownRemaining > 0f)
            {
                return false;
            }

            guard.BiteCooldownRemaining = _constants.AttackIntervalSeconds;
            return true;
        }

        /// <summary>Reference cooldown timer: ticks every frame, never negative.</summary>
        public void TickCooldowns(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            for (int i = 0; i < _guards.Count; i++)
            {
                GuardState guard = _guards[i];
                if (guard.CooldownRemaining > 0f)
                {
                    guard.CooldownRemaining = Mathf.Max(0f, guard.CooldownRemaining - deltaSeconds);
                }

                if (guard.BiteCooldownRemaining > 0f)
                {
                    guard.BiteCooldownRemaining = Mathf.Max(0f, guard.BiteCooldownRemaining - deltaSeconds);
                }

                if (guard.EmpowerRemaining <= 0f)
                {
                    continue;
                }

                guard.EmpowerRemaining = Mathf.Max(0f, guard.EmpowerRemaining - deltaSeconds);
                if (guard.EmpowerRemaining <= 0f)
                {
                    guard.DamageMultiplier = 1f;
                    guard.AttackSpeedMultiplier = 1f;
                }
            }
        }

        public void Clear()
        {
            _guards.Clear();
            SetDataAsDirty();
        }

        private GuardState FindAt(Vector2 position)
        {
            for (int i = 0; i < _guards.Count; i++)
            {
                if ((_guards[i].Position - position).sqrMagnitude < 0.01f)
                {
                    return _guards[i];
                }
            }

            return null;
        }
    }
}
