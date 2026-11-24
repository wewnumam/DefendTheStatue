using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>Dev-console actions. Each module handles only the ones it owns.</summary>
    public struct DevAddEssenceMessage
    {
        public DevAddEssenceMessage(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }
    }

    public struct DevAddManaMessage
    {
        public DevAddManaMessage(float amount)
        {
            Amount = amount;
        }

        public float Amount { get; }
    }

    public struct DevHealStatueMessage
    {
    }

    public struct DevHealPlayerMessage
    {
    }

    public struct DevKillAllMessage
    {
    }

    public struct DevMaxLevelMessage
    {
    }

    /// <summary>GDD §16 Unlock All: every skill castable, whatever the statue level.</summary>
    public struct DevUnlockAllMessage
    {
    }

    /// <summary>GDD §16 Build All: every tower, wall and trap on the unlocked rings, free.</summary>
    public struct DevBuildAllMessage
    {
    }
}
