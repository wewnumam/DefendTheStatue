using ProjectTA.Utility;

namespace ProjectTA.Message
{
    /// <summary>
    /// GDD §15: one skill box on the HUD. The view needs the cooldown clock and whether the skill is
    /// running, but it must not reach into the skill model — so the controller snapshots it here.
    /// </summary>
    public struct SkillBoxState
    {
        public SkillBoxState(
            EnumManager.SkillSlot slot,
            EnumManager.WeaponKind weapon,
            bool unlocked,
            int requiredLevel,
            float cooldownRemaining,
            float cooldownDuration,
            bool active,
            float activeRemaining)
        {
            Slot = slot;
            Weapon = weapon;
            Unlocked = unlocked;
            RequiredLevel = requiredLevel;
            CooldownRemaining = cooldownRemaining;
            CooldownDuration = cooldownDuration;
            Active = active;
            ActiveRemaining = activeRemaining;
        }

        public EnumManager.SkillSlot Slot { get; }
        public EnumManager.WeaponKind Weapon { get; }
        public bool Unlocked { get; }
        public int RequiredLevel { get; }
        public float CooldownRemaining { get; }
        public float CooldownDuration { get; }
        public bool Active { get; }
        public float ActiveRemaining { get; }
    }

    /// <summary>
    /// Eight boxes, sword 1-4 then bow 1-4. Published on a slow tick so the countdowns move without
    /// the HUD polling the skill module.
    /// </summary>
    public struct SkillStateMessage
    {
        public SkillStateMessage(EnumManager.WeaponKind weapon, SkillBoxState[] boxes)
        {
            Weapon = weapon;
            Boxes = boxes;
        }

        public EnumManager.WeaponKind Weapon { get; }
        public SkillBoxState[] Boxes { get; }
    }
}
