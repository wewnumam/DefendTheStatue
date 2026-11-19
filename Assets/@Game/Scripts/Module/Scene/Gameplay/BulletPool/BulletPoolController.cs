using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Bullet;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.BulletPool
{
    public class BulletPoolController : ObjectController<BulletPoolController, BulletPoolModel, BulletPoolView>
    {
        /// <summary>Berserker's x3 for arrows, mirrored off the bus. One whenever no skill is running.</summary>
        private float _damageMultiplier = 1f;

        /// <summary>
        /// Arrow count mirrored from the player module, so the pool can refuse a shot without owning
        /// ammo. clone mode is the reference's "bow skill3 makes arrows free".
        /// </summary>
        private int _arrows;
        private bool _arrowFree;
        private float _stamina;

        /// <summary>Reference: every normal arrow costs 3 stamina, and no shot lands below that.</summary>
        private const float ArrowStaminaCost = 3f;

        public void OnSkillMultipliers(SkillMultipliersMessage message)
        {
            _damageMultiplier = message.DamageMultiplier;
            _model.SetRateMultiplier(message.AttackCooldownMultiplier);
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _arrows = message.Arrows;
            _stamina = message.Stamina;
        }

        public void OnSkillEffectToggled(SkillEffectToggledMessage message)
        {
            if (message.Slot == Utility.EnumManager.SkillSlot.Skill3
                && message.Weapon == Utility.EnumManager.WeaponKind.Bow)
            {
                _arrowFree = message.Active;
            }
        }

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            if (gameConstants.GameConstants == null)
            {
                Debug.LogError("SOGAMECONSTANTS IS NULL");
                return;
            }

            if (gameConstants.GameConstants.Shooting == null)
            {
                Debug.LogError("SHOOTINGCONSTANTS IS NULL");
                return;
            }

            _model.SetShootingConstants(gameConstants.GameConstants.Shooting);
        }

        public override void SetView(BulletPoolView view)
        {
            base.SetView(view);
            SetPoolObject();
            view.SetCallback(OnSpawnBullet);

            // Catches the launch-time switch up: the model kept it while no view was wired.
            view.SetWeapon(_model.IsWeaponActive ? 1f : -1f);
        }

        private void OnSpawnBullet(float deltaTime)
        {
            if (_model.CanShoot() && HasArrow() && _stamina >= ArrowStaminaCost)
            {
                SpawnBullet();
                _model.ResetCountdown();

                // The mirror drops at once so a burst cannot outshoot the ammo; the player module's
                // next PlayerStatsChangedMessage carries the authoritative number.
                _stamina -= ArrowStaminaCost;
                Publish(new RequestSpendStaminaMessage(ArrowStaminaCost));

                if (!_arrowFree)
                {
                    _arrows--;
                    Publish(new RequestSpendArrowsMessage(1));
                }
            }
            _model.DecreaseCountdown(deltaTime);
        }

        /// <summary>Reference canShootArrow(): clone mode is the only bypass here; the admin cheat is not wired yet.</summary>
        private bool HasArrow()
        {
            return _arrowFree || _arrows > 0;
        }

        /// <summary>
        /// The bow's charged shot. The player module already paid the 15 stamina and the arrow, so this
        /// only fires: no fire-rate gate, and the reference applies no damage multiplier to it.
        /// </summary>
        public void OnRequestHeavyShot(RequestHeavyShotMessage message)
        {
            SpawnBullet(message.Damage);
        }

        private void SetPoolObject()
        {
            for (int i = 0; i < _model.ShootingConstants.BulletPoolSize; i++)
            {
                _model.AddBullet(CreateBulletObject());
            }
        }

        private GameObject CreateBulletObject()
        {
            GameObject bullet = GameObject.Instantiate(_view.BulletPrefab, _view.BulletSpawnPoint.position, Quaternion.identity);
            bullet.SetActive(false);

            BulletView bulletView = null;
            if (bullet.TryGetComponent<BulletView>(out var view))
            {
                bulletView = view;
            }

            BulletController bulletController = new BulletController();
            InjectDependencies(bulletController);
            bulletController.Init(bulletView, _model.ShootingConstants.BulletForce, _model.ShootingConstants.BulletDestroyDelay);

            return bullet;
        }


        private void SpawnBullet(float damageOverride = -1f)
        {
            GameObject bullet = _model.GetPooledBullet();
            if (bullet != null)
            {
                // Damage is stamped per spawn: the multiplier can change between two arrows of one burst.
                if (bullet.TryGetComponent<BulletView>(out var bulletView))
                {
                    bulletView.Damage = damageOverride > 0f
                        ? damageOverride
                        : ProjectTA.Module.PlayerWeapon.PlayerWeaponModel.ArrowDamage * _damageMultiplier;
                }

                bullet.transform.SetParent(null);
                bullet.transform.SetPositionAndRotation(
                    _view.BulletSpawnPoint.position,
                    _view.BulletSpawnPoint.rotation
                );
                bullet.SetActive(true);
            }
        }

        public void OnShootStart(PlayerCharacterShootStartMessage message)
        {
            _model.SetIsShooting(true);
        }

        public void OnShootEnd(PlayerCharacterShootEndMessage message)
        {
            _model.SetIsShooting(false);
        }

        /// <summary>Weapon changes are resolved by the skill module, so this only follows the result.</summary>
        public void OnWeaponSwitched(WeaponSwitchedMessage message)
        {
            float scroll = message.Weapon == Utility.EnumManager.WeaponKind.Bow ? 1f : -1f;
            _model.SetWeapon(scroll);

            // The launcher publishes Playing before views are wired, so the view can still be null here.
            if (_view != null)
            {
                _view.SetWeapon(scroll);
            }
        }

        public void OnDespawnBullet(DespawnBulletMessage message)
        {
            if (message.Bullet == null)
            {
                return;
            }

            // Return the pooled bullet to its pool, it may still be parented to what it hit
            message.Bullet.transform.SetParent(_view.transform, false);
            message.Bullet.SetActive(false);
        }

        public void OnGameState(GameStateMessage message)
        {
            _model.SetIsPlaying(message.GameState == Utility.EnumManager.GameState.Playing);
        }
    }
}
