using Agate.MVC.Base;
using ProjectTA.Module.PlayerSkill;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>What one statue level raises. Port of the reference game's upgrade ladder.</summary>
    public readonly struct StatueUpgradeEffect
    {
        public StatueUpgradeEffect(
            int level,
            int unlockedTerritoryIndex,
            int guardFoundationLevel,
            EnumManager.SkillSlot unlockedSkill,
            float maxHealthDelta)
        {
            Level = level;
            UnlockedTerritoryIndex = unlockedTerritoryIndex;
            GuardFoundationLevel = guardFoundationLevel;
            UnlockedSkill = unlockedSkill;
            MaxHealthDelta = maxHealthDelta;
        }

        public int Level { get; }
        public int UnlockedTerritoryIndex { get; }
        public int GuardFoundationLevel { get; }
        public EnumManager.SkillSlot UnlockedSkill { get; }
        public float MaxHealthDelta { get; }

        public bool UnlocksTerritory => UnlockedTerritoryIndex >= 0;
        public bool SpawnsGuardFoundations => GuardFoundationLevel > 0;
        public bool UnlocksSkill => UnlockedSkill != EnumManager.SkillSlot.None;
    }

    /// <summary>
    /// Statue health, level and the upgrade ladder. Numbers come straight from the reference
    /// game: 200 base HP, +100 at level 6, +200 at level 7, upgrade cost 500 x level.
    /// </summary>
    public class StatueModel : BaseModel
    {
        public const int MaxLevel = 7;
        public const int MinLevel = 1;
        public const float BaseMaxHealth = 200f;
        public const int UpgradeCostPerLevel = 500;

        private static readonly StatueUpgradeEffect[] Effects =
        {
            new StatueUpgradeEffect(1, -1, 0, EnumManager.SkillSlot.None, 0f),
            new StatueUpgradeEffect(2, 1, 0, EnumManager.SkillSlot.None, 0f),
            new StatueUpgradeEffect(3, -1, 3, EnumManager.SkillSlot.Skill1, 0f),
            new StatueUpgradeEffect(4, 2, 0, EnumManager.SkillSlot.None, 0f),
            new StatueUpgradeEffect(5, -1, 5, EnumManager.SkillSlot.Skill2, 0f),
            new StatueUpgradeEffect(6, -1, 0, EnumManager.SkillSlot.None, 100f),
            new StatueUpgradeEffect(7, -1, 0, EnumManager.SkillSlot.Skill3, 200f)
        };

        public int Level { get; private set; } = MinLevel;
        public float MaxHealth { get; private set; } = BaseMaxHealth;
        public float Health { get; private set; } = BaseMaxHealth;

        /// <summary>
        /// Running total of guard foundations the ladder has handed out. Guards themselves are
        /// deferred, but the level 4 and 6 gates count them, so the tally has to live somewhere.
        /// </summary>
        public int GuardFoundationsSpawned { get; private set; }

        public bool IsMaxLevel => Level >= MaxLevel;
        public bool IsAlive => Health > 0f;

        /// <summary>Reference game: cost = 500 * statue.level, charged before the level rises.</summary>
        public int UpgradeCost => UpgradeCostPerLevel * Level;

        public static StatueUpgradeEffect GetEffect(int level)
        {
            int index = Mathf.Clamp(level, MinLevel, MaxLevel) - 1;
            return Effects[index];
        }

        public void Reset()
        {
            Level = MinLevel;
            MaxHealth = BaseMaxHealth;
            Health = BaseMaxHealth;
            GuardFoundationsSpawned = 0;
            SetDataAsDirty();
        }

        public bool CanUpgrade(int gold)
        {
            return !IsMaxLevel && gold >= UpgradeCost;
        }

        /// <summary>Raises the level and applies that level's health bump. Caller pays the cost.</summary>
        public StatueUpgradeEffect Upgrade()
        {
            if (IsMaxLevel)
            {
                return Effects[MaxLevel - 1];
            }

            Level++;
            StatueUpgradeEffect effect = GetEffect(Level);
            GuardFoundationsSpawned += effect.GuardFoundationLevel;
            if (effect.MaxHealthDelta > 0f)
            {
                MaxHealth += effect.MaxHealthDelta;
                Health += effect.MaxHealthDelta;
            }

            SetDataAsDirty();
            return effect;
        }

        /// <summary>Returns false when the statue was already destroyed.</summary>
        public bool ApplyDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return false;
            }

            Health = Mathf.Max(0f, Health - amount);
            SetDataAsDirty();
            return true;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            Health = Mathf.Min(MaxHealth, Health + amount);
            SetDataAsDirty();
        }
    }
}
