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
        public MonsterState(int id, Vector2 position, bool inHealRadius, MonsterConstants constants)
        {
            Id = id;
            Position = position;
            InHealRadius = inHealRadius;
            Constants = constants;

            // Reference: monsters spawned inside the statue heal radius are twice as tough and
            // worth three times the gold.
            MaxHealth = inHealRadius ? constants.HealRadiusHealth : constants.BaseHealth;
            Health = MaxHealth;
            Damage = inHealRadius ? constants.HealRadiusDamage : constants.BaseDamage;
            GoldValue = inHealRadius ? constants.HealRadiusGoldValue : constants.BaseGoldValue;
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
        public float MaxHealth { get; }
        public float Health { get; private set; }
        public float Damage { get; }
        public int GoldValue { get; }
        public bool IsDead => Health <= 0f;

        /// <summary>Dash is the one attack that may hit each monster only once per dash.</summary>
        public bool DashHitTaken { get; set; }

        /// <summary>Returns the gold the killer collects, or 0 when the monster survives.</summary>
        public int ApplyDamage(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0;
            }

            Health = Mathf.Max(0f, Health - amount);
            return IsDead ? GoldValue : 0;
        }

        public void TickAttackCooldown(float deltaSeconds)
        {
            AttackCooldown = Mathf.Max(0f, AttackCooldown - deltaSeconds);
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
    /// and keeps paying out gold. RemoveDead fixes that.
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
        public int Add(Vector2 position, bool inHealRadius)
        {
            int id = _nextId++;
            _monsters.Add(new MonsterState(id, position, inHealRadius, _constants));
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

            return (toTarget / distance) * (_constants.SpeedPerSecond * Mathf.Clamp01(speedMultiplier));
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
        /// tick. The reference bites once a second for the monster's rolled damage.
        /// </summary>
        public float CollectStatueDamage(float deltaSeconds, Vector2 statuePosition)
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

                float distance = (monster.Position - statuePosition).magnitude;
                if (monster.TryAttack(distance))
                {
                    total += monster.Damage;
                }
            }

            return total;
        }

        /// <summary>Returns the gold awarded, so the caller can credit the player.</summary>
        public int ApplyDamage(int monsterId, float amount)
        {
            MonsterState monster = Get(monsterId);
            if (monster == null)
            {
                return 0;
            }

            int gold = monster.ApplyDamage(amount);
            if (gold > 0)
            {
                SetDataAsDirty();
            }

            return gold;
        }

        /// <summary>Nearest live monster inside range, or -1. Squared distance, so no square roots.</summary>
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
        /// and returns the gold earned from the kills.
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
            int gold = 0;

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
                gold += monster.ApplyDamage(amount);
            }

            if (hitIds != null && hitIds.Count > 0)
            {
                SetDataAsDirty();
            }

            return gold;
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
