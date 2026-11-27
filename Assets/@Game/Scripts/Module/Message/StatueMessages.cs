using ProjectTA.Module.Statue;

namespace ProjectTA.Message
{
    /// <summary>A monster landed a hit on the statue.</summary>
    public struct RequestStatueDamageMessage
    {
        public RequestStatueDamageMessage(float amount)
        {
            Amount = amount;
        }

        public float Amount { get; }
    }

    /// <summary>The player asked for the next statue level. Refused when essence or gates say no.</summary>
    public struct RequestUpgradeStatueMessage
    {
    }

    public struct StatueDamagedMessage
    {
        public StatueDamagedMessage(float health, float maxHealth)
        {
            Health = health;
            MaxHealth = maxHealth;
        }

        public float Health { get; }
        public float MaxHealth { get; }

        public float HealthFraction => MaxHealth > 0f ? Health / MaxHealth : 0f;
    }

    public struct StatueUpgradedMessage
    {
        public StatueUpgradedMessage(int level, StatueUpgradeEffect effect, int essenceSpent)
        {
            Level = level;
            Effect = effect;
            EssenceSpent = essenceSpent;
        }

        public int Level { get; }
        public StatueUpgradeEffect Effect { get; }
        public int EssenceSpent { get; }
    }

    /// <summary>Statue health hit zero: the run is over.</summary>
    public struct StatueDestroyedMessage
    {
        public StatueDestroyedMessage(int monstersKilled, float survivalSeconds)
        {
            MonstersKilled = monstersKilled;
            SurvivalSeconds = survivalSeconds;
        }

        public int MonstersKilled { get; }
        public float SurvivalSeconds { get; }
    }

    public struct TerritoryUnlockedMessage
    {
        public TerritoryUnlockedMessage(int territoryIndex)
        {
            TerritoryIndex = territoryIndex;
        }

        public int TerritoryIndex { get; }
    }

    /// <summary>The upgrade panel asks the statue what the next level still needs.</summary>
    public struct RequestStatueGateStatusMessage
    {
    }

    /// <summary>
    /// GDD §5 gate progress for the next level, already formatted for the panel: the statue owns the
    /// counters, so the UI never recounts walls or kills itself.
    /// </summary>
    public struct StatueGateStatusMessage
    {
        public StatueGateStatusMessage(int nextLevel, bool ready, string[] lines)
        {
            NextLevel = nextLevel;
            Ready = ready;
            Lines = lines;
        }

        public int NextLevel { get; }
        public bool Ready { get; }
        public string[] Lines { get; }
    }
}
