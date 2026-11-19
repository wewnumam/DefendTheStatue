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
            Subscribe<WeaponSwitchedMessage>(_playerCharacter.OnWeaponSwitched);
            Subscribe<PlayerCharacterShootStartMessage>(_playerCharacter.OnAttackStart);
            Subscribe<SkillCastMessage>(_playerCharacter.OnSkillCast);
            Subscribe<RequestSpendGoldMessage>(_playerCharacter.OnRequestSpendGold);
            Subscribe<RequestSpendArrowsMessage>(_playerCharacter.OnRequestSpendArrows);
            Subscribe<RequestSpendStaminaMessage>(_playerCharacter.OnRequestSpendStamina);
            Subscribe<SkillMultipliersMessage>(_playerCharacter.OnSkillMultipliers);
        }

        protected override void Disconnect()
        {
            Unsubscribe<MovePlayerCharacterMessage>(_playerCharacter.OnMove);
            Unsubscribe<RotatePlayerCharacterMessage>(_playerCharacter.OnRotate);
            Unsubscribe<MonsterKilledMessage>(_playerCharacter.OnMonsterKilled);
            Unsubscribe<WeaponSwitchedMessage>(_playerCharacter.OnWeaponSwitched);
            Unsubscribe<PlayerCharacterShootStartMessage>(_playerCharacter.OnAttackStart);
            Unsubscribe<SkillCastMessage>(_playerCharacter.OnSkillCast);
            Unsubscribe<RequestSpendGoldMessage>(_playerCharacter.OnRequestSpendGold);
            Unsubscribe<RequestSpendArrowsMessage>(_playerCharacter.OnRequestSpendArrows);
            Unsubscribe<RequestSpendStaminaMessage>(_playerCharacter.OnRequestSpendStamina);
            Unsubscribe<SkillMultipliersMessage>(_playerCharacter.OnSkillMultipliers);
        }
    }
}
