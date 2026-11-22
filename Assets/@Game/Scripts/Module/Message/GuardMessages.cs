using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>A guard was recruited on a foundation. The guard module owns its attack from here.</summary>
    public struct GuardBuiltMessage
    {
        public GuardBuiltMessage(int foundationIndex, int foundationLevel, Vector2 position)
        {
            FoundationIndex = foundationIndex;
            FoundationLevel = foundationLevel;
            Position = position;
        }

        public int FoundationIndex { get; }
        public int FoundationLevel { get; }
        public Vector2 Position { get; }
    }

    /// <summary>A guard lost health to a monster bite.</summary>
    public struct GuardDamagedMessage
    {
        public GuardDamagedMessage(int guardIndex, float health, float maxHealth)
        {
            GuardIndex = guardIndex;
            Health = health;
            MaxHealth = maxHealth;
        }

        public int GuardIndex { get; }
        public float Health { get; }
        public float MaxHealth { get; }
    }

    /// <summary>GDD §13 bow skill 4: guards near the caster hit harder and fire faster for 6 s.</summary>
    public struct GuardEmpowerMessage
    {
        public GuardEmpowerMessage(Vector2 center, float radius, float damageMultiplier, float attackSpeedMultiplier, float durationSeconds)
        {
            Center = center;
            Radius = radius;
            DamageMultiplier = damageMultiplier;
            AttackSpeedMultiplier = attackSpeedMultiplier;
            DurationSeconds = durationSeconds;
        }

        public Vector2 Center { get; }
        public float Radius { get; }
        public float DamageMultiplier { get; }
        public float AttackSpeedMultiplier { get; }
        public float DurationSeconds { get; }
    }

    /// <summary>A guard fired. The view only needs the muzzle and the target for feedback.</summary>
    public struct GuardFiredMessage
    {
        public GuardFiredMessage(Vector2 from, Vector2 to)
        {
            From = from;
            To = to;
        }

        public Vector2 From { get; }
        public Vector2 To { get; }
    }
}
