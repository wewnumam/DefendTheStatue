using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>Spawner says a monster should appear. Position comes from the reference spawn logic.</summary>
    public struct RequestSpawnMonsterMessage
    {
        public RequestSpawnMonsterMessage(Vector2 position, bool inHealRadius, Utility.EnumManager.MonsterKind kind)
        {
            Position = position;
            InHealRadius = inHealRadius;
            Kind = kind;
        }

        public Vector2 Position { get; }
        public bool InHealRadius { get; }
        public Utility.EnumManager.MonsterKind Kind { get; }
    }

    /// <summary>Tell an attacker to hit one monster. Ids stay valid, indexes do not.</summary>
    public struct RequestDamageMonsterMessage
    {
        public RequestDamageMonsterMessage(int monsterId, float amount, Vector2 hitPosition)
        {
            MonsterId = monsterId;
            Amount = amount;
            HitPosition = hitPosition;
        }

        public int MonsterId { get; }
        public float Amount { get; }
        public Vector2 HitPosition { get; }
    }

    /// <summary>Sword swing, dash and spike traps: hit everything within radius of a point.</summary>
    public struct RequestAreaDamageMessage
    {
        public RequestAreaDamageMessage(Vector2 center, float radius, float amount, Vector2 hitOrigin)
            : this(center, radius, amount, hitOrigin, false)
        {
        }

        public RequestAreaDamageMessage(Vector2 center, float radius, float amount, Vector2 hitOrigin, bool oncePerMonster)
        {
            Center = center;
            Radius = radius;
            Amount = amount;
            HitOrigin = hitOrigin;
            OncePerMonster = oncePerMonster;
        }

        public Vector2 Center { get; }
        public float Radius { get; }
        public float Amount { get; }
        public Vector2 HitOrigin { get; }
        public bool OncePerMonster { get; }
    }

    /// <summary>A dash ended: the next dash may hit the same monsters again.</summary>
    public struct RequestResetDashHitsMessage
    {
    }

    public struct MonsterSpawnedMessage
    {
        public MonsterSpawnedMessage(
            int monsterId,
            Vector2 position,
            bool inHealRadius,
            float health,
            Utility.EnumManager.MonsterKind kind = Utility.EnumManager.MonsterKind.Stacker)
        {
            MonsterId = monsterId;
            Position = position;
            InHealRadius = inHealRadius;
            Health = health;
            Kind = kind;
        }

        public int MonsterId { get; }
        public Vector2 Position { get; }
        public bool InHealRadius { get; }
        public float Health { get; }
        public Utility.EnumManager.MonsterKind Kind { get; }
    }

    public struct MonsterDamagedMessage
    {
        public MonsterDamagedMessage(int monsterId, float health, Vector2 hitPosition)
        {
            MonsterId = monsterId;
            Health = health;
            HitPosition = hitPosition;
        }

        public int MonsterId { get; }
        public float Health { get; }
        public Vector2 HitPosition { get; }
    }

    /// <summary>Bow skill 2 asks for the nearest enemy to lock onto. The pool answers with LockOnTargetMessage.</summary>
    public struct RequestLockOnTargetMessage
    {
        public RequestLockOnTargetMessage(Vector2 origin, float range)
        {
            Origin = origin;
            Range = range;
        }

        public Vector2 Origin { get; }
        public float Range { get; }
    }

    /// <summary>Where the locked-on enemy stands, or (-1,-1) when there is none.</summary>
    public struct LockOnTargetMessage
    {
        public LockOnTargetMessage(Vector2 position, bool found)
        {
            Position = position;
            Found = found;
        }

        public Vector2 Position { get; }
        public bool Found { get; }
    }

    /// <summary>GDD §13 sword skill 4: enemies inside the radius fight for the player for 6 s.</summary>
    public struct MindControlMessage
    {
        public MindControlMessage(Vector2 center, float radius, float durationSeconds, float damageMultiplier)
        {
            Center = center;
            Radius = radius;
            DurationSeconds = durationSeconds;
            DamageMultiplier = damageMultiplier;
        }

        public Vector2 Center { get; }
        public float Radius { get; }
        public float DurationSeconds { get; }
        public float DamageMultiplier { get; }
    }

    /// <summary>
    /// GDD §5: one monster inside the statue heal radius taints the zone, which stops the player's
    /// regeneration there. Published on change, not every frame.
    /// </summary>
    public struct HealZoneTaintMessage
    {
        public HealZoneTaintMessage(bool tainted)
        {
            Tainted = tainted;
        }

        public bool Tainted { get; }
    }

    /// <summary>A corpse to remove, and the essence the killer collects.</summary>
    public struct MonsterKilledMessage
    {
        public MonsterKilledMessage(
            int monsterId,
            Vector2 position,
            int essenceValue,
            Utility.EnumManager.MonsterKind kind = Utility.EnumManager.MonsterKind.Stacker)
        {
            MonsterId = monsterId;
            Position = position;
            EssenceValue = essenceValue;
            Kind = kind;
        }

        public int MonsterId { get; }
        public Vector2 Position { get; }
        public int EssenceValue { get; }
        public Utility.EnumManager.MonsterKind Kind { get; }
    }
}
