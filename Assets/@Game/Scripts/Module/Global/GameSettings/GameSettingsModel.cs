using Agate.MVC.Base;

namespace ProjectTA.Module.SaveSystem
{
    public class GameSettingsModel : BaseModel, IGameSettingsModel
    {
        public SavedSettingsData SavedSettingsData { get; private set; } = new();

        public void SetSaveData(SavedSettingsData saveData)
        {
            SavedSettingsData = saveData;
            SetDataAsDirty();
        }

        public void SetIsGameIndctionActive(bool isGameIndctionActive)
        {
            SavedSettingsData.IsGameInductionActive = isGameIndctionActive;
            SetDataAsDirty();
        }

        public void SetIsSfxOn(bool isSfxOn)
        {
            SavedSettingsData.IsSfxOn = isSfxOn;
            SetDataAsDirty();
        }

        public void SetIsBgmOn(bool isBgmOn)
        {
            SavedSettingsData.IsBgmOn = isBgmOn;
            SetDataAsDirty();
        }
    }
}