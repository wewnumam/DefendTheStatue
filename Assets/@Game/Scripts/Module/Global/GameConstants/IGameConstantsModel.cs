using Agate.MVC.Base;

namespace ProjectTA.Module.GameConstants
{
    public interface IGameConstantsModel : IBaseModel
    {
        SOGameConstants GameConstants { get; }
    }
}