using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>Spawner says a monster should appear. Position comes from the reference spawn logic.</summary>
    public struct RequestSpawnMonsterMessage
    {
        public RequestSpawnMonsterMessage(Vector2 position, bool inHealRadius)
        {
            Position = position;
            InHealRadius = inHealRadius;
        }

        public Vector2 Position { get; }
        public bool InHealRadius { get; }
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
        public MonsterSpawnedMessage(int monsterId, Vector2 position, bool inHealRadius, float health)
        {
            MonsterId = monsterId;
            Position = position;
            InHealRadius = inHealRadius;
            Health = health;
        }

        public int MonsterId { get; }
        public Vector2 Position { get; }
        public bool InHealRadius { get; }
        public float Health { get; }
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

    /// <summary>A corpse to remove, and the gold the killer collects.</summary>
    public struct MonsterKilledMessage
    {
        public MonsterKilledMessage(int monsterId, Vector2 position, int goldValue)
        {
            MonsterId = monsterId;
            Position = position;
            GoldValue = goldValue;
        }

        public int MonsterId { get; }
        public Vector2 Position { get; }
        public int GoldValue { get; }
    }
}
