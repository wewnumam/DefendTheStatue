using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Bullet;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.BulletPool
{
    public class BulletPoolController : ObjectController<BulletPoolController, BulletPoolModel, BulletPoolView>
    {
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
        }

        private void OnSpawnBullet(float deltaTime)
        {
            if (_model.CanShoot())
            {
                SpawnBullet();
                _model.ResetCountdown();
            }
            _model.DecreaseCountdown(deltaTime);
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


        private void SpawnBullet()
        {
            GameObject bullet = _model.GetPooledBullet();
            if (bullet != null)
            {
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

        public void OnSwitchWeapon(SwitchWeaponMessage message)
        {
            _model.SetWeapon(message.Scroll);
            _view.SetWeapon(message.Scroll);
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
