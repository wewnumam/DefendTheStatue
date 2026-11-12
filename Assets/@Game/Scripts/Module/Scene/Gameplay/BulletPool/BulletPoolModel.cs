using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTA.Module.BulletPool
{
    public class BulletPoolModel : BaseModel
    {
        public bool IsPlaying { get; private set; } = false;
        public bool IsShooting { get; private set; } = false;
        public bool IsWeaponActive { get; private set; } = false;
        public ShootingConstants ShootingConstants { get; private set; } = new();

        private float _currentCountdown = 0;
        private readonly List<GameObject> _bulletPool = new List<GameObject>();

        public void SetIsPlaying(bool isPlaying)
        {
            IsPlaying = isPlaying;
            SetDataAsDirty();
        }

        public void SetIsShooting(bool isShooting)
        {
            IsShooting = isShooting;
            SetDataAsDirty();
        }

        public void SetShootingConstants(ShootingConstants shootingConstants)
        {
            ShootingConstants = shootingConstants;
            SetDataAsDirty();
        }

        /// <summary>
        /// Mirrors SwordView.SetWeapon: scroll up holsters the sword and arms the gun,
        /// scroll down equips the sword and holsters the gun.
        /// </summary>
        public void SetWeapon(float scrollY)
        {
            if (scrollY > 0f)
            {
                IsWeaponActive = true;
                SetDataAsDirty();
            }
            else if (scrollY < 0f)
            {
                IsWeaponActive = false;
                SetDataAsDirty();
            }
        }

        public bool CanShoot()
        {
            return IsWeaponActive && IsShooting && IsPlaying && _currentCountdown <= 0;
        }

        public void ResetCountdown()
        {
            _currentCountdown = ShootingConstants.BulletShootingRate;
        }

        public void DecreaseCountdown(float amount)
        {
            _currentCountdown -= amount;
        }

        public void AddBullet(GameObject bullet)
        {
            _bulletPool.Add(bullet);
        }

        public GameObject GetPooledBullet()
        {
            for (int i = 0; i < _bulletPool.Count; i++)
            {
                if (_bulletPool[i] != null && !_bulletPool[i].activeInHierarchy)
                {
                    return _bulletPool[i];
                }
            }
            return null;
        }
    }
}
