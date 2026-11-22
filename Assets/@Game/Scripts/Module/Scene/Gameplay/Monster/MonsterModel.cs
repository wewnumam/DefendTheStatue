using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// One live monster. Numbers come from the reference game's spawnMonster.
    ///
    /// Reference behaviour: a monster walks straight at the statue and stops touching it, then attacks
    /// it on a cooldown. Only the walk is ported — the attack needs the statue's damage path, which is
    /// deferred with the boss. Damage stays here because spawnMonster rolls it from the heal radius.
    /// </summary>
    public class MonsterState
    {
        public MonsterState(
            int id,
            Vector2 position,
            bool inHealRadius,
            MonsterConstants constants,
            Utility.EnumManager.MonsterKind kind = Utility.EnumManager.MonsterKind.Stacker,
            float statMultiplier = 1f)
        {
            Id = id;
            Position = position;
            InHealRadius = inHealRadius;
            Constants = constants;
            Kind = kind;

            // GDD §7: one stat block per role, and damage is split by what the monster is chewing on.
            // GDD §14: adaptive difficulty buffs hp, damage and speed, never past 1.5x.
            float buff = Mathf.Max(0.1f, statMultiplier);
            MonsterKindStats stats = constants.GetKind(kind);
            MaxHealth = stats.Health * buff;
            Health = MaxHealth;
            EssenceValue = stats.Essence;
            SpeedMultiplier = stats.SpeedMultiplier * buff;
            StatueDamage = stats.StatueDamage * buff;
            StructureDamage = stats.StructureDamage * buff;
            PlayerDamage = stats.PlayerDamage * buff;
            GuardDamage = stats.GuardDamage * buff;
            AttackIntervalSeconds = constants.AttackIntervalSeconds;
            AttackReach = constants.AttackReach;
        }

        /// <summary>Body, stats and bite cadence: owned by the GameConstants asset, not by code.</summary>
        public MonsterConstants Constants { get; }

        public float AttackIntervalSeconds { get; }
        public float AttackReach { get; }

        /// <summary>Stable handle. Indexes shift when a corpse is removed, so views and messages talk in ids.</summary>
        public int Id { get; }

        public float AttackCooldown { get; private set; }

        public Vector2 Position { get; set; }
        public bool InHealRadius { get; }
        public Utility.EnumManager.MonsterKind Kind { get; }
        public float MaxHealth { get; }
        public float Health { get; private set; }
        public int EssenceValue { get; }

        /// <summary>GDD §7 damage columns: statue, wall/tower, player, guard.</summary>
        public float StatueDamage { get; }
        public float StructureDamage { get; }
        public float PlayerDamage { get; }
        public float GuardDamage { get; }

        /// <summary>Role speed over the shared base speed (Stacker 1, Waltroy 0.5, Deasor 2).</summary>
        public float SpeedMultiplier { get; }

        /// <summary>Set by the pool each tick: a hurt monster near the player runs away instead of biting.</summary>
        public bool IsRetreating { get; set; }

        /// <summary>Retreat clock: a monster cannot flee twice without reaching the retreat point first.</summary>
        public float RetreatRemaining { get; set; }

        /// <summary>GDD §7: bites for this role's statue column.</summary>
        public float Damage => StatueDamage;
        public bool IsDead => Health <= 0f;

        /// <summary>Dash is the one attack that may hit each monster only once per dash.</summary>
        public bool DashHitTaken { get; set; }

        /// <summary>GDD §13 sword skill 4: a controlled monster fights for the player, it does not bite the statue.</summary>
        public bool IsMindControlled { get; private set; }
        public float MindControlRemaining { get; private set; }
        public float AllyDamageMultiplier { get; private set; } = 1f;

        public void ApplyMindControl(float durationSeconds, float damageMultiplier)
        {
            if (IsDead)
            {
                return;
            }

            IsMindControlled = true;
            MindControlRemaining = Mathf.Max(MindControlRemaining, durationSeconds);
            AllyDamageMultiplier = damageMultiplier > 0f ? damageMultiplier : 1f;
        }

        public void TickMindControl(float deltaSeconds)
        {
            if (!IsMindControlled)
            {
                return;
            }

            MindControlRemaining = Mathf.Max(0f, MindControlRemaining - deltaSeconds);
            if (MindControlRemaining <= 0f)
            {
                IsMindControlled = false;
                AllyDamageMultiplier = 1f;
            }
        }

        /// <summary>Reference healCooldown: blocks a second bite from the same tree.</summary>
        public float HealCooldownRemaining { get; private set; }

        /// <summary>Reference hpRatio &lt; 0.3: hurt enough to look for a tree.</summary>
        public bool WantsHeal => !IsDead && Health < MaxHealth * 0.3f;

        public void TickHealCooldown(float deltaSeconds)
        {
            HealCooldownRemaining = Mathf.Max(0f, HealCooldownRemaining - deltaSeconds);
        }

        public bool TryStartHealCooldown(float seconds)
        {
            if (HealCooldownRemaining > 0f)
            {
                return false;
            }

            HealCooldownRemaining = seconds;
            return true;
        }

        public void Heal(float amount)
        {
            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        /// <summary>Returns the essence the killer collects, or 0 when the monster survives.</summary>
        public int ApplyDamage(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0;
            }

            Health = Mathf.Max(0f, Health - amount);
            return IsDead ? EssenceValue : 0;
        }

        public void TickAttackCooldown(float deltaSeconds)
        {
            AttackCooldown = Mathf.Max(0f, AttackCooldown - deltaSeconds);
        }

        /// <summary>Arms the next attack; used by an ally that just swung at another monster.</summary>
        public void StartAttackCooldown()
        {
            AttackCooldown = AttackIntervalSeconds;
        }

        /// <summary>GDD §7: under 30% hp with the player nearby, the monster bolts instead of fighting.</summary>
        public bool WantsRetreat(Vector2 playerPosition, float nearDistance)
        {
            if (IsDead || IsMindControlled || Health >= MaxHealth * 0.3f)
            {
                return false;
            }

            return (playerPosition - Position).sqrMagnitude <= nearDistance * nearDistance;
        }

        /// <summary>True when this monster bit the statue this tick; starts the next cooldown.</summary>
        public bool TryAttack(float distanceToStatue)
        {
            if (IsDead || distanceToStatue > AttackReach || AttackCooldown > 0f)
            {
                return false;
            }

            AttackCooldown = AttackIntervalSeconds;
            return true;
        }
    }

    /// <summary>
    /// Holds the live monsters. Unlike the reference, dead monsters are dropped: the reference
    /// never removes a corpse from its array, so a killed monster keeps absorbing hits at hp &lt;= 0
    /// and keeps paying out essence. RemoveDead fixes that.
    /// </summary>
    public class MonsterModel : BaseModel
    {
        private readonly List<MonsterState> _monsters = new List<MonsterState>();
        private int _nextId;

        /// <summary>Filled from the GameConstants asset by the pool controller; defaults keep unit tests honest.</summary>
        private MonsterConstants _constants = new MonsterConstants();

        public MonsterConstants Constants => _constants;

        public void SetMonsterConstants(MonsterConstants constants)
        {
            if (constants != null)
            {
                _constants = constants;
            }

            SetDataAsDirty();
        }

        public IReadOnlyList<MonsterState> Monsters => _monsters;
        public int Count => _monsters.Count;

        public MonsterState Get(int monsterId)
        {
            for (int i = 0; i < _monsters.Count; i++)
            {
                if (_monsters[i].Id == monsterId)
                {
                    return _monsters[i];
                }
            }

            return null;
        }

        /// <summary>Adds a monster and returns its id.</summary>
        public int Add(
            Vector2 position,
            bool inHealRadius,
            Utility.EnumManager.MonsterKind kind = Utility.EnumManager.MonsterKind.Stacker,
            float statMultiplier = 1f)
        {
            int id = _nextId++;
            _monsters.Add(new MonsterState(id, position, inHealRadius, _constants, kind, statMultiplier));
            SetDataAsDirty();
            return id;
        }

        /// <summary>
        /// Desired move for one monster walking at <paramref name="target"/>, in units per second. Zero
        /// once it is on the statue clearance: physics then holds it there while it bites.
        /// </summary>
        public Vector2 GetVelocity(MonsterState monster, Vector2 target, float stopDistance, float speedMultiplier)
        {
            if (monster == null || monster.IsDead)
            {
                return Vector2.zero;
            }

            Vector2 toTarget = target - monster.Position;
            float distance = toTarget.magnitude;
            if (distance <= stopDistance || distance <= 0f)
            {
                return Vector2.zero;
            }

            float roleSpeed = monster.SpeedMultiplier > 0f ? monster.SpeedMultiplier : 1f;
            return (toTarget / distance) * (_constants.SpeedPerSecond * roleSpeed * Mathf.Clamp01(speedMultiplier));
        }

        /// <summary>
        /// Writes back where physics actually put a monster. Deliberately no SetDataAsDirty: the body
        /// moves it, and the view already knows. Other modules read this copy for range checks.
        /// </summary>
        public void SetPosition(int monsterId, Vector2 position)
        {
            MonsterState monster = Get(monsterId);
            if (monster != null)
            {
                monster.Position = position;
            }
        }

        /// <summary>
        /// Ticks every monster's attack clock and returns the total damage the ones in reach dealt this
        /// tick. The reference bites once a second for the monster's rolled damage. Appends the ids that
        /// bit to <paramref name="attackedIds"/>, so the pool can play their attack pose.
        /// </summary>
        public float CollectStatueDamage(float deltaSeconds, Vector2 statuePosition, System.Collections.Generic.List<int> attackedIds = null)
        {
            float total = 0f;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead)
                {
                    continue;
                }

                monster.TickAttackCooldown(deltaSeconds);

                if (monster.IsMindControlled || monster.IsRetreating)
                {
                    continue;
                }

                float distance = (monster.Position - statuePosition).magnitude;
                if (monster.TryAttack(distance))
                {
                    attackedIds?.Add(monster.Id);
                    total += monster.Damage;
                }
            }

            return total;
        }

        /// <summary>
        /// GDD §7/§20: monsters close enough to the player bite him instead of the statue. Returns the
        /// total damage and reports who bit, so the pool can play their attack pose.
        /// </summary>
        public float CollectPlayerDamage(
            float deltaSeconds,
            Vector2 playerPosition,
            Vector2 statuePosition,
            List<int> attackedIds = null)
        {
            float total = 0f;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead || monster.IsMindControlled || monster.IsRetreating)
                {
                    continue;
                }

                float toPlayer = (monster.Position - playerPosition).magnitude;
                float toStatue = (monster.Position - statuePosition).magnitude;
                if (toPlayer > monster.AttackReach || toPlayer >= toStatue)
                {
                    continue;
                }

                if (!monster.TryAttack(toPlayer))
                {
                    continue;
                }

                attackedIds?.Add(monster.Id);
                total += monster.PlayerDamage;
            }

            return total;
        }

        /// <summary>An allied monster's swing this tick: who hit whom, for how much, and where.</summary>
        public readonly struct AllyAttack
        {
            public AllyAttack(int attackerId, int victimId, float damage, Vector2 hitPosition)
            {
                AttackerId = attackerId;
                VictimId = victimId;
                Damage = damage;
                HitPosition = hitPosition;
            }

            public int AttackerId { get; }
            public int VictimId { get; }
            public float Damage { get; }
            public Vector2 HitPosition { get; }
        }

        /// <summary>GDD §13: turns every live monster inside the radius into an ally for a while.</summary>
        public int ApplyMindControl(Vector2 center, float radius, float durationSeconds, float damageMultiplier)
        {
            int controlled = 0;
            float sqrRadius = radius * radius;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead || (monster.Position - center).sqrMagnitude > sqrRadius)
                {
                    continue;
                }

                monster.ApplyMindControl(durationSeconds, damageMultiplier);
                controlled++;
            }

            if (controlled > 0)
            {
                SetDataAsDirty();
            }

            return controlled;
        }

        /// <summary>
        /// Ticks mind-control timers and reports the swings allies are ready to land. Damage is applied by
        /// the caller through the normal damage path, so kills still pay essence and play feedback.
        /// </summary>
        public int CollectAllyAttacks(float deltaSeconds, List<AllyAttack> attacks)
        {
            int count = 0;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState ally = _monsters[i];
                if (ally.IsDead)
                {
                    continue;
                }

                ally.TickMindControl(deltaSeconds);
                if (!ally.IsMindControlled || ally.AttackCooldown > 0f)
                {
                    continue;
                }

                int victimIndex = FindNearestEnemyFor(ally);
                if (victimIndex < 0)
                {
                    continue;
                }

                MonsterState victim = _monsters[victimIndex];
                float reach = ally.AttackReach + Constants.Size;
                if ((victim.Position - ally.Position).sqrMagnitude > reach * reach)
                {
                    continue;
                }

                ally.StartAttackCooldown();
                attacks?.Add(new AllyAttack(ally.Id, victim.Id, ally.Damage * ally.AllyDamageMultiplier, victim.Position));
                count++;
            }

            return count;
        }

        /// <summary>Nearest monster that is still hostile to the player, or -1.</summary>
        private int FindNearestEnemyFor(MonsterState ally)
        {
            int best = -1;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState other = _monsters[i];
                if (other.IsDead || other.IsMindControlled || other.Id == ally.Id)
                {
                    continue;
                }

                float sqr = (other.Position - ally.Position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Returns the essence awarded, so the caller can credit the player.</summary>
        public int ApplyDamage(int monsterId, float amount)
        {
            MonsterState monster = Get(monsterId);
            if (monster == null)
            {
                return 0;
            }

            int essence = monster.ApplyDamage(amount);
            if (essence > 0)
            {
                SetDataAsDirty();
            }

            return essence;
        }

        /// <summary>Reference tree heal: tops the monster up and reports whether the heal landed.</summary>
        public bool Heal(int monsterId, float amount)
        {
            MonsterState monster = Get(monsterId);
            if (monster == null || monster.IsDead || amount <= 0f)
            {
                return false;
            }

            monster.Heal(amount);
            SetDataAsDirty();
            return true;
        }

        public void TickHealCooldowns(float deltaSeconds)
        {
            for (int i = 0; i < _monsters.Count; i++)
            {
                _monsters[i].TickHealCooldown(deltaSeconds);
            }
        }

        /// <summary>Nearest live monster in range, or -1. Squared distance, so no square roots.</summary>
        public int FindNearestInRange(Vector2 origin, float range)
        {
            int best = -1;
            float bestSqr = range * range;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead)
                {
                    continue;
                }

                float sqr = (monster.Position - origin).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = monster.Id;
                }
            }

            return best;
        }

        /// <summary>Drops the dead. Optionally reports which ids were removed, so views can recycle them.</summary>
        public int RemoveDead(List<int> removedIds = null)
        {
            int removed = 0;
            for (int i = _monsters.Count - 1; i >= 0; i--)
            {
                if (_monsters[i].IsDead)
                {
                    removedIds?.Add(_monsters[i].Id);
                    _monsters.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
            {
                SetDataAsDirty();
            }

            return removed;
        }

        /// <summary>
        /// Sword swing, dash and spike traps all hit everything inside a radius. Appends the ids that
        /// took damage to <paramref name="hitIds"/> so the caller can publish one message per monster,
        /// and returns the essence earned from the kills.
        /// </summary>
        public int ApplyAreaDamage(Vector2 center, float radius, float amount, List<int> hitIds)
        {
            return ApplyAreaDamage(center, radius, amount, hitIds, false);
        }

        /// <summary>
        /// Dash variant: <paramref name="oncePerMonster"/> skips monsters that already took this
        /// dash's hit, and marks the ones it does hit.
        /// </summary>
        public int ApplyAreaDamage(Vector2 center, float radius, float amount, List<int> hitIds, bool oncePerMonster)
        {
            if (amount <= 0f || radius <= 0f)
            {
                return 0;
            }

            float radiusSquared = radius * radius;
            int essence = 0;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead || (oncePerMonster && monster.DashHitTaken))
                {
                    continue;
                }

                if ((monster.Position - center).sqrMagnitude >= radiusSquared)
                {
                    continue;
                }

                monster.DashHitTaken = oncePerMonster;
                hitIds?.Add(monster.Id);
                essence += monster.ApplyDamage(amount);
            }

            if (hitIds != null && hitIds.Count > 0)
            {
                SetDataAsDirty();
            }

            return essence;
        }

        /// <summary>Called when the dash ends, so the next dash can hit the same monsters again.</summary>
        public void ResetDashHits()
        {
            for (int i = 0; i < _monsters.Count; i++)
            {
                _monsters[i].DashHitTaken = false;
            }
        }

        public void Clear()
        {
            _monsters.Clear();
            SetDataAsDirty();
        }
    }
}
