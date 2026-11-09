using Agate.MVC.Base;

namespace ProjectTA.Module.SaveSystem
{
    public interface IGameSettingsModel : IBaseModel
    {
        public SavedSettingsData SavedSettingsData { get; }
    }
}