using ProjectTA.Utility;

namespace ProjectTA.Message
{
    /// <summary>Player pressed a skill key. Refused when locked, on cooldown or short of mana.</summary>
    public struct RequestCastSkillMessage
    {
        public RequestCastSkillMessage(EnumManager.SkillSlot slot)
        {
            Slot = slot;
        }

        public EnumManager.SkillSlot Slot { get; }
    }

    public struct SkillCastMessage
    {
        public SkillCastMessage(EnumManager.SkillSlot slot, EnumManager.WeaponKind weapon, int manaSpent)
        {
            Slot = slot;
            Weapon = weapon;
            ManaSpent = manaSpent;
        }

        public EnumManager.SkillSlot Slot { get; }
        public EnumManager.WeaponKind Weapon { get; }
        public int ManaSpent { get; }
    }

    /// <summary>A timed slow area appeared (sword time slash, spike trap).</summary>
    public struct SlowFieldAddedMessage
    {
        public SlowFieldAddedMessage(UnityEngine.Vector2 center, float radius, float slowFactor, float durationSeconds)
        {
            Center = center;
            Radius = radius;
            SlowFactor = slowFactor;
            DurationSeconds = durationSeconds;
        }

        public UnityEngine.Vector2 Center { get; }
        public float Radius { get; }
        public float SlowFactor { get; }
        public float DurationSeconds { get; }
    }

    /// <summary>The player was asked to pay: gold stays owned by the player module, so spenders post this.</summary>
    public struct RequestSpendGoldMessage
    {
        public RequestSpendGoldMessage(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }
    }

    /// <summary>Health, stamina, mana or gold moved: the HUD reads from here.</summary>
    public struct PlayerStatsChangedMessage
    {
        public PlayerStatsChangedMessage(float health, float maxHealth, float stamina, float mana, int gold, int arrows)
        {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            Mana = mana;
            Gold = gold;
            Arrows = arrows;
        }

        public float Health { get; }
        public float MaxHealth { get; }
        public float Stamina { get; }
        public float Mana { get; }
        public int Gold { get; }
        public int Arrows { get; }
    }

    public struct WeaponSwitchedMessage
    {
        public WeaponSwitchedMessage(EnumManager.WeaponKind weapon)
        {
            Weapon = weapon;
        }

        public EnumManager.WeaponKind Weapon { get; }
    }

    /// <summary>Bow clone mode and the sword berserker window both start and end here.</summary>
    public struct SkillEffectToggledMessage
    {
        public SkillEffectToggledMessage(EnumManager.SkillSlot slot, EnumManager.WeaponKind weapon, bool active)
        {
            Slot = slot;
            Weapon = weapon;
            Active = active;
        }

        public EnumManager.SkillSlot Slot { get; }
        public EnumManager.WeaponKind Weapon { get; }
        public bool Active { get; }
    }
}
