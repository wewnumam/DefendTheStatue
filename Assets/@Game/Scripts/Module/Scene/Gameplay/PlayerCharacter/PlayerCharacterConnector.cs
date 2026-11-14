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
        }

        protected override void Disconnect()
        {
            Unsubscribe<MovePlayerCharacterMessage>(_playerCharacter.OnMove);
            Unsubscribe<RotatePlayerCharacterMessage>(_playerCharacter.OnRotate);
            Unsubscribe<MonsterKilledMessage>(_playerCharacter.OnMonsterKilled);
        }
    }
}
