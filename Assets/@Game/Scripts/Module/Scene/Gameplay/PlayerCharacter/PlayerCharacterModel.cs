using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterModel : BaseModel
    {
        public PlayerConstants PlayerConstants { get; private set; } = null;
        public bool IsJoystickActive { get; private set; }

        public void SetPlayerConstants(PlayerConstants playerConstants)
        {
            PlayerConstants = playerConstants;
        }
    }
}
