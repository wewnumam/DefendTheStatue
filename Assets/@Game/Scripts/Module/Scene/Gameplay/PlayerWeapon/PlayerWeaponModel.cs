using Agate.MVC.Base;
using ProjectTA.Utility;

namespace ProjectTA.Module.PlayerWeapon
{
    /// <summary>
    /// Which weapon is in hand. Scroll up arms the bow, scroll down puts the sword back, matching
    /// the reference game's weapons[] order and the existing Sword/BulletPool scroll convention.
    /// </summary>
    public class PlayerWeaponModel : BaseModel
    {
        /// <summary>GDD §4: a sword swing does 1 damage.</summary>
        public const float SwordDamage = 1f;
        public const float SwordRadius = 60f;

        /// <summary>GDD §4: a plain arrow (and every clone-mode arrow) does 1 damage.</summary>
        public const float ArrowDamage = 1f;

        public EnumManager.WeaponKind Current { get; private set; } = EnumManager.WeaponKind.Sword;
        public bool IsWeaponLocked { get; private set; }

        public void Reset()
        {
            Current = EnumManager.WeaponKind.Sword;
            IsWeaponLocked = false;
            SetDataAsDirty();
        }

        /// <summary>Returns true when the weapon actually changed.</summary>
        public bool TrySwitch(float scrollY)
        {
            if (IsWeaponLocked)
            {
                return false;
            }

            if (scrollY > 0f && Current != EnumManager.WeaponKind.Bow)
            {
                Current = EnumManager.WeaponKind.Bow;
                SetDataAsDirty();
                return true;
            }

            if (scrollY < 0f && Current != EnumManager.WeaponKind.Sword)
            {
                Current = EnumManager.WeaponKind.Sword;
                SetDataAsDirty();
                return true;
            }

            return false;
        }

        public void SetWeaponLocked(bool isLocked)
        {
            IsWeaponLocked = isLocked;
            SetDataAsDirty();
        }
    }
}
