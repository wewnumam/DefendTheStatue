using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Sword
{
    public class SwordView : BaseView
    {
        [SerializeField] private Collider2D _hitbox;

        /// <summary>
        /// The sword is in hand at scene start, matching BulletPoolModel.IsWeaponActive == false.
        /// </summary>
        private bool _isEquipped = true;

        private void Awake()
        {
            SetHitboxActive(false);
        }

        /// <summary>
        /// Scroll up holsters the sword, scroll down equips it.
        /// </summary>
        public void SetWeapon(float scrollY)
        {
            if (scrollY > 0f)
            {
                _isEquipped = false;
                SetHitboxActive(false);
                gameObject.SetActive(false);
            }
            else if (scrollY < 0f)
            {
                _isEquipped = true;
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// The hitbox only ever goes live while the sword is the equipped weapon, so firing the gun
        /// cannot swing it.
        /// </summary>
        public void SetHitboxActive(bool isActive)
        {
            if (_hitbox != null)
            {
                _hitbox.enabled = isActive && _isEquipped;
            }
        }
    }
}
