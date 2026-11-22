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
            Subscribe<WeaponSwitchedMessage>(_bulletPool.OnWeaponSwitched);
            Subscribe<GameStateMessage>(_bulletPool.OnGameState);
            Subscribe<SkillMultipliersMessage>(_bulletPool.OnSkillMultipliers);
            Subscribe<PlayerStatsChangedMessage>(_bulletPool.OnPlayerStatsChanged);
            Subscribe<SkillEffectToggledMessage>(_bulletPool.OnSkillEffectToggled);
            Subscribe<RequestHeavyShotMessage>(_bulletPool.OnRequestHeavyShot);
            Subscribe<CheatToggledMessage>(_bulletPool.OnCheatToggled);
            Subscribe<SkillCastMessage>(_bulletPool.OnSkillCast);
            Subscribe<LockOnTargetMessage>(_bulletPool.OnLockOnTarget);
        }

        protected override void Disconnect()
        {
            Unsubscribe<PlayerCharacterShootStartMessage>(_bulletPool.OnShootStart);
            Unsubscribe<PlayerCharacterShootEndMessage>(_bulletPool.OnShootEnd);
            Unsubscribe<DespawnBulletMessage>(_bulletPool.OnDespawnBullet);
            Unsubscribe<WeaponSwitchedMessage>(_bulletPool.OnWeaponSwitched);
            Unsubscribe<GameStateMessage>(_bulletPool.OnGameState);
            Unsubscribe<SkillMultipliersMessage>(_bulletPool.OnSkillMultipliers);
            Unsubscribe<PlayerStatsChangedMessage>(_bulletPool.OnPlayerStatsChanged);
            Unsubscribe<SkillEffectToggledMessage>(_bulletPool.OnSkillEffectToggled);
            Unsubscribe<RequestHeavyShotMessage>(_bulletPool.OnRequestHeavyShot);
            Unsubscribe<CheatToggledMessage>(_bulletPool.OnCheatToggled);
            Unsubscribe<SkillCastMessage>(_bulletPool.OnSkillCast);
            Unsubscribe<LockOnTargetMessage>(_bulletPool.OnLockOnTarget);
        }
    }
}
