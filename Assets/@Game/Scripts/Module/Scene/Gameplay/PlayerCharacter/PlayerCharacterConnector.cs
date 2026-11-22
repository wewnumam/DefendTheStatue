using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterConnector : BaseConnector
    {
        private readonly PlayerCharacterController _playerCharacter = new();

        protected override void Connect()
        {
            Subscribe<MovePlayerCharacterMessage>(_playerCharacter.OnMove);
            Subscribe<RotatePlayerCharacterMessage>(_playerCharacter.OnRotate);
            Subscribe<MonsterKilledMessage>(_playerCharacter.OnMonsterKilled);
            Subscribe<HealZoneTaintMessage>(_playerCharacter.OnHealZoneTaint);
            Subscribe<RequestPlayerDamageMessage>(_playerCharacter.OnRequestPlayerDamage);
            Subscribe<WeaponSwitchedMessage>(_playerCharacter.OnWeaponSwitched);
            Subscribe<PlayerCharacterShootStartMessage>(_playerCharacter.OnAttackStart);
            Subscribe<SkillCastMessage>(_playerCharacter.OnSkillCast);
            Subscribe<RequestSpendEssenceMessage>(_playerCharacter.OnRequestSpendEssence);
            Subscribe<RequestSpendArrowsMessage>(_playerCharacter.OnRequestSpendArrows);
            Subscribe<RequestSpendStaminaMessage>(_playerCharacter.OnRequestSpendStamina);
            Subscribe<RequestManaGainMessage>(_playerCharacter.OnRequestManaGain);
            Subscribe<TeleportPerformedMessage>(_playerCharacter.OnTeleportPerformed);
            Subscribe<SkillMultipliersMessage>(_playerCharacter.OnSkillMultipliers);
            Subscribe<CheatToggledMessage>(_playerCharacter.OnCheatToggled);
            Subscribe<DevAddEssenceMessage>(_playerCharacter.OnDevAddEssence);
            Subscribe<DevAddManaMessage>(_playerCharacter.OnDevAddMana);
            Subscribe<DevHealPlayerMessage>(_playerCharacter.OnDevHealPlayer);
        }

        protected override void Disconnect()
        {
            Unsubscribe<MovePlayerCharacterMessage>(_playerCharacter.OnMove);
            Unsubscribe<RotatePlayerCharacterMessage>(_playerCharacter.OnRotate);
            Unsubscribe<MonsterKilledMessage>(_playerCharacter.OnMonsterKilled);
            Unsubscribe<HealZoneTaintMessage>(_playerCharacter.OnHealZoneTaint);
            Unsubscribe<RequestPlayerDamageMessage>(_playerCharacter.OnRequestPlayerDamage);
            Unsubscribe<WeaponSwitchedMessage>(_playerCharacter.OnWeaponSwitched);
            Unsubscribe<PlayerCharacterShootStartMessage>(_playerCharacter.OnAttackStart);
            Unsubscribe<SkillCastMessage>(_playerCharacter.OnSkillCast);
            Unsubscribe<RequestSpendEssenceMessage>(_playerCharacter.OnRequestSpendEssence);
            Unsubscribe<RequestSpendArrowsMessage>(_playerCharacter.OnRequestSpendArrows);
            Unsubscribe<RequestSpendStaminaMessage>(_playerCharacter.OnRequestSpendStamina);
            Unsubscribe<RequestManaGainMessage>(_playerCharacter.OnRequestManaGain);
            Unsubscribe<TeleportPerformedMessage>(_playerCharacter.OnTeleportPerformed);
            Unsubscribe<SkillMultipliersMessage>(_playerCharacter.OnSkillMultipliers);
            Unsubscribe<CheatToggledMessage>(_playerCharacter.OnCheatToggled);
            Unsubscribe<DevAddEssenceMessage>(_playerCharacter.OnDevAddEssence);
            Unsubscribe<DevAddManaMessage>(_playerCharacter.OnDevAddMana);
            Unsubscribe<DevHealPlayerMessage>(_playerCharacter.OnDevHealPlayer);
        }
    }
}
