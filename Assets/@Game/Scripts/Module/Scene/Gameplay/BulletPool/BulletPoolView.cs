using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.BulletPool
{
    public class BulletPoolView : BaseView
    {
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private GameObject _weaponSprite;
        private UnityAction<float> _onSpawnBullet;

        public GameObject BulletPrefab => _bulletPrefab;
        public Transform BulletSpawnPoint => _bulletSpawnPoint;

        private void Awake()
        {
            SetWeaponSpriteActive(false);
        }

        /// <summary>
        /// Scroll up shows the gun, scroll down holsters it. Mirrors SwordView.SetWeapon.
        /// </summary>
        public void SetWeapon(float scrollY)
        {
            if (scrollY > 0f)
            {
                SetWeaponSpriteActive(true);
            }
            else if (scrollY < 0f)
            {
                SetWeaponSpriteActive(false);
            }
        }

        private void SetWeaponSpriteActive(bool isActive)
        {
            if (_weaponSprite != null)
            {
                _weaponSprite.SetActive(isActive);
            }
        }

        public void SetCallback(UnityAction<float> onSpawnBullet)
        {
            _onSpawnBullet = onSpawnBullet;
        }

        private void Update()
        {
            _onSpawnBullet?.Invoke(Time.deltaTime);
        }
    }
}