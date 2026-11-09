using Agate.MVC.Base;

namespace ProjectTA.Module.GameConstants
{
    public class GameConstantsModel : BaseModel, IGameConstantsModel
    {
        public SOGameConstants GameConstants { get; private set; }

        public void SetGameConstants(SOGameConstants gameConstants)
        {
            GameConstants = gameConstants;
            SetDataAsDirty();
        }
    }
}