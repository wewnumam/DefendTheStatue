using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.BulletPool
{
    public class BulletPoolConnector : BaseConnector
    {
        private readonly BulletPoolController _bulletPool = new();

        protected override void Connect()
        {
            Subscribe<PlayerCharacterShootStartMessage>(_bulletPool.OnShootStart);
            Subscribe<PlayerCharacterShootEndMessage>(_bulletPool.OnShootEnd);
            Subscribe<DespawnBulletMessage>(_bulletPool.OnDespawnBullet);
            Subscribe<SwitchWeaponMessage>(_bulletPool.OnSwitchWeapon);
            Subscribe<GameStateMessage>(_bulletPool.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<PlayerCharacterShootStartMessage>(_bulletPool.OnShootStart);
            Unsubscribe<PlayerCharacterShootEndMessage>(_bulletPool.OnShootEnd);
            Unsubscribe<DespawnBulletMessage>(_bulletPool.OnDespawnBullet);
            Unsubscribe<SwitchWeaponMessage>(_bulletPool.OnSwitchWeapon);
            Unsubscribe<GameStateMessage>(_bulletPool.OnGameState);
        }
    }
}
