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
        private bool _infiniteArrows;
        private float _stamina;

        /// <summary>GDD §4: every normal arrow costs 1 stamina, and no shot lands below that.</summary>
        private const float ArrowStaminaCost = 1f;

        /// <summary>GDD §13 bow skill 1: three arrows, 4 damage each, fanned 15 degrees apart.</summary>
        public const float TripleShotDamage = 4f;
        private const float TripleShotSpreadDegrees = 15f;

        /// <summary>World units each triple-shot arrow is offset sideways from the muzzle.</summary>
        private const float TripleShotLateralOffset = 8f;

        /// <summary>GDD §13 bow skill 3: clone mode fires two extra arrows for 2 damage each.</summary>
        private const float CloneArrowDamage = 2f;
        private const float CloneArrowSpreadDegrees = 10f;

        /// <summary>GDD §13 bow skill 2: lock-on doubles the next arrow and aims it at the nearest enemy.</summary>
        private const float LockOnDamageMultiplier = 2f;
        private const float LockOnRange = 600f;

        private bool _cloneActive;
        private bool _lockOnPending;
        private bool _lockOnFound;
        private Vector2 _lockOnTarget;

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
                _cloneActive = message.Active;
            }
        }

        /// <summary>Bow skills that change how the next shots leave the bow.</summary>
        public void OnSkillCast(SkillCastMessage message)
        {
            if (message.Weapon != Utility.EnumManager.WeaponKind.Bow)
            {
                return;
            }

            switch (message.Slot)
            {
                case Utility.EnumManager.SkillSlot.Skill1:
                    FireTripleShot();
                    break;
                case Utility.EnumManager.SkillSlot.Skill2:
                    _lockOnPending = true;
                    Publish(new RequestLockOnTargetMessage(_view.BulletSpawnPoint.position, LockOnRange));
                    break;
            }
        }

        public void OnLockOnTarget(LockOnTargetMessage message)
        {
            _lockOnFound = message.Found;
            _lockOnTarget = message.Position;
        }

        /// <summary>GDD §13: three arrows at 4 damage, one stamina and one arrow for the burst.</summary>
        private void FireTripleShot()
        {
            if (_stamina < ArrowStaminaCost || !HasArrow())
            {
                _lockOnPending = false;
                return;
            }

            // Fanned ±15° and nudged sideways, so the three arrows neither share an angle nor spawn
            // inside each other (same-layer non-trigger colliders would stop them dead).
            for (int i = -1; i <= 1; i++)
            {
                SpawnBullet(TripleShotDamage, i * TripleShotSpreadDegrees, i * TripleShotLateralOffset);
            }

            _stamina -= ArrowStaminaCost;
            Publish(new RequestSpendStaminaMessage(ArrowStaminaCost));

            if (!_arrowFree)
            {
                _arrows--;
                Publish(new RequestSpendArrowsMessage(1));
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
                FireNormalShot();
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

        /// <summary>
        /// One attack: a plain arrow, three when clone mode is up, or a locked-on arrow aimed at the enemy
        /// bow skill 2 picked.
        /// </summary>
        private void FireNormalShot()
        {
            float damage = ProjectTA.Module.PlayerWeapon.PlayerWeaponModel.ArrowDamage * _damageMultiplier;

            if (_cloneActive)
            {
                damage = CloneArrowDamage * _damageMultiplier;
            }

            if (_lockOnPending)
            {
                _lockOnPending = false;
                if (_lockOnFound)
                {
                    AimAtLockedTarget(damage * LockOnDamageMultiplier);
                    return;
                }
            }

            SpawnBullet(damage);

            if (_cloneActive)
            {
                SpawnBullet(CloneArrowDamage * _damageMultiplier, -CloneArrowSpreadDegrees);
                SpawnBullet(CloneArrowDamage * _damageMultiplier, CloneArrowSpreadDegrees);
            }
        }

        /// <summary>Lock-on skips the cursor: the arrow leaves along the line to the locked enemy.</summary>
        private void AimAtLockedTarget(float damage)
        {
            Transform spawn = _view.BulletSpawnPoint;
            Vector2 direction = _lockOnTarget - (Vector2)spawn.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                SpawnBullet(damage);
                return;
            }

            // The prefab flies along -up, so point +up opposite the target.
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, (Vector3)(-direction.normalized));
            SpawnBulletAt(damage, rotation);
        }

        /// <summary>Reference canShootArrow(): clone mode is the only bypass here; the admin cheat is not wired yet.</summary>
        private bool HasArrow()
        {
            return _infiniteArrows || _arrowFree || _arrows > 0;
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == Utility.EnumManager.CheatType.InfiniteArrows)
            {
                _infiniteArrows = message.Enabled;
            }
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


        private void SpawnBullet(float damageOverride = -1f, float angleOffsetDegrees = 0f, float lateralOffset = 0f)
        {
            Transform spawn = _view.BulletSpawnPoint;
            Quaternion rotation = spawn.rotation * Quaternion.Euler(0f, 0f, angleOffsetDegrees);
            Vector2 origin = spawn.position;
            Vector2 position = origin + (Vector2)(rotation * new Vector3(lateralOffset, 0f, 0f));
            SpawnBulletAt(damageOverride, rotation, position);
        }

        private void SpawnBulletAt(float damageOverride, Quaternion rotation, Vector2? position = null)
        {
            GameObject bullet = _model.GetPooledBullet();
            if (bullet == null)
            {
                return;
            }

            // Damage is stamped per spawn: the multiplier can change between two arrows of one burst.
            if (bullet.TryGetComponent<BulletView>(out var bulletView))
            {
                bulletView.Damage = damageOverride > 0f
                    ? damageOverride
                    : ProjectTA.Module.PlayerWeapon.PlayerWeaponModel.ArrowDamage * _damageMultiplier;
            }

            bullet.transform.SetParent(null);
            bullet.transform.SetPositionAndRotation(
                position ?? (Vector2)_view.BulletSpawnPoint.position, rotation);
            bullet.SetActive(true);
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
