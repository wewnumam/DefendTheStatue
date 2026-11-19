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

    /// <summary>One bow shot's ammo. The pool asks before it fires; the player module owns the count.</summary>
    public struct RequestSpendArrowsMessage
    {
        public RequestSpendArrowsMessage(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }

    /// <summary>
    /// Stamina bill for a swing, an arrow or a charged shot. The player module owns the pool, so every
    /// other module posts here the way it does for gold and arrows.
    /// </summary>
    public struct RequestSpendStaminaMessage
    {
        public RequestSpendStaminaMessage(float amount)
        {
            Amount = amount;
        }

        public float Amount { get; }
    }

    /// <summary>
    /// The bow's held shot: released after a charge, 50 damage flat — the reference fires it with a
    /// fixed 50 and applies no damage multiplier to it.
    /// </summary>
    public struct RequestHeavyShotMessage
    {
        public RequestHeavyShotMessage(float damage)
        {
            Damage = damage;
        }

        public float Damage { get; }
    }

    /// <summary>Health, stamina, mana or gold moved: the HUD reads from here.</summary>
    public struct PlayerStatsChangedMessage
    {
        public PlayerStatsChangedMessage(float health, float maxHealth, float stamina, float maxStamina, float mana, float maxMana, int gold, int arrows)
        {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            MaxStamina = maxStamina;
            Mana = mana;
            MaxMana = maxMana;
            Gold = gold;
            Arrows = arrows;
        }

        public float Health { get; }
        public float MaxHealth { get; }
        public float Stamina { get; }
        public float MaxStamina { get; }
        public float Mana { get; }
        public float MaxMana { get; }
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

    /// <summary>
    /// The reference game's getSpeedMult / getDmgMult / getAtkCdMult, for the weapon in hand. Pushed
    /// when they can change (a cast, an effect timing out, a weapon switch) so no consumer has to hold
    /// a reference to the skill model.
    /// </summary>
    public struct SkillMultipliersMessage
    {
        public SkillMultipliersMessage(float speed, float damage, float attackCooldown)
        {
            SpeedMultiplier = speed;
            DamageMultiplier = damage;
            AttackCooldownMultiplier = attackCooldown;
        }

        public float SpeedMultiplier { get; }
        public float DamageMultiplier { get; }
        public float AttackCooldownMultiplier { get; }
    }
}
