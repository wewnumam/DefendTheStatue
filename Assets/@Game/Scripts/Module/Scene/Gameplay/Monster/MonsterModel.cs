using System.Collections.Generic;
using Agate.MVC.Base;
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
        public MonsterState(int id, Vector2 position, bool inHealRadius)
        {
            Id = id;
            Position = position;
            InHealRadius = inHealRadius;

            // Reference: monsters spawned inside the statue heal radius are twice as tough and
            // worth three times the gold.
            MaxHealth = inHealRadius ? HealRadiusHealth : BaseHealth;
            Health = MaxHealth;
            Damage = inHealRadius ? HealRadiusDamage : BaseDamage;
            GoldValue = inHealRadius ? HealRadiusGoldValue : BaseGoldValue;
        }

        public const float Size = 25f;
        public const float Speed = 1.5f;

        /// <summary>Reference: 1.5 units per frame. The port ticks in seconds at the reference's 60 fps.</summary>
        public const float SpeedPerSecond = Speed * 60f;

        /// <summary>Reference statue size, which sets the distance a monster comes to rest at.</summary>
        public const float StatueSize = 50f;

        /// <summary>Reference: a monster walks while it is further out than (m.size + statue.size) / 2 + 2.</summary>
        public const float StatueStopDistance = (Size + StatueSize) * 0.5f + 2f;
        public const float BaseHealth = 20f;
        public const float HealRadiusHealth = 40f;
        public const float BaseDamage = 10f;
        public const float HealRadiusDamage = 30f;
        public const int BaseGoldValue = 5;
        public const int HealRadiusGoldValue = 15;

        /// <summary>
        /// Stable handle. Indexes shift when a corpse is removed, so views and messages talk in ids.
        /// </summary>
        public int Id { get; }

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
            _monsters.Add(new MonsterState(id, position, inHealRadius));
            SetDataAsDirty();
            return id;
        }

        /// <summary>
        /// Walks every live monster toward <paramref name="target"/> at the reference speed and stops it
        /// at <paramref name="stopDistance"/>. Returns how many moved.
        ///
        /// The reference also re-routes a monster that is outside a walled territory through that wall's
        /// nearest gap; walls are still deferred, so monsters walk straight for now.
        /// </summary>
        public int Advance(float deltaSeconds, Vector2 target, float stopDistance)
        {
            if (deltaSeconds <= 0f)
            {
                return 0;
            }

            float step = MonsterState.SpeedPerSecond * deltaSeconds;
            int moved = 0;

            for (int i = 0; i < _monsters.Count; i++)
            {
                MonsterState monster = _monsters[i];
                if (monster.IsDead)
                {
                    continue;
                }

                Vector2 toTarget = target - monster.Position;
                float distance = toTarget.magnitude;
                if (distance <= stopDistance || distance <= 0f)
                {
                    continue;
                }

                // Clamped, so a long frame stops on the clearance instead of stepping past it: the
                // reference recovers from the overshoot in its push-out pass, and there is none here.
                monster.Position += (toTarget / distance) * Mathf.Min(step, distance - stopDistance);
                moved++;
            }

            if (moved > 0)
            {
                SetDataAsDirty();
            }

            return moved;
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
