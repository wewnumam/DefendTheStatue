using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>The ten-minute timer fired. The boss module owns spawning from here.</summary>
    public struct BossSpawnedMessage
    {
    }

    /// <summary>The boss died. The run ends in victory.</summary>
    public struct BossDefeatedMessage
    {
    }

    /// <summary>An arrow or a sword swing hit the boss. The boss owns its health, not the pool.</summary>
    public struct RequestDamageBossMessage
    {
        public RequestDamageBossMessage(float amount, Vector2 hitPosition)
        {
            Amount = amount;
            HitPosition = hitPosition;
        }

        public float Amount { get; }
        public Vector2 HitPosition { get; }
    }

    /// <summary>Boss health moved; the HUD's bar can follow this.</summary>
    public struct BossDamagedMessage
    {
        public BossDamagedMessage(float health, float maxHealth, int phase)
        {
            Health = health;
            MaxHealth = maxHealth;
            Phase = phase;
        }

        public float Health { get; }
        public float MaxHealth { get; }
        public int Phase { get; }
    }

    /// <summary>The boss crossed a 60% / 30% boundary. The view tints and the HUD relabels.</summary>
    public struct BossPhaseChangedMessage
    {
        public BossPhaseChangedMessage(int phase)
        {
            Phase = phase;
        }

        public int Phase { get; }
    }
}
