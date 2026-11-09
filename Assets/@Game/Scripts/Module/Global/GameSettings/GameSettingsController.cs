using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using System.Collections;

namespace ProjectTA.Module.SaveSystem
{
    public class GameSettingsController : DataController<GameSettingsController, GameSettingsModel, IGameSettingsModel>
    {
        #region UTILITY

        private ISaveSystem<SavedSettingsData> _savedSettingsData = null;

        public void SetSaveSystem(ISaveSystem<SavedSettingsData> savedSettingsData)
        {
            _savedSettingsData = savedSettingsData;
        }

        public void SetModel(GameSettingsModel model)
        {
            _model = model;
        }

        #endregion

        public override IEnumerator Initialize()
        {
            if (_savedSettingsData == null)
            {
                _savedSettingsData = new SaveSystem<SavedSettingsData>(TagManager.FILENAME_SAVEDGAMESETTINGS);
            }
            _model.SetSaveData(_savedSettingsData.Load());

            yield return base.Initialize();
        }

        #region MESSAGE LISTENER

        public void DeleteSaveData(DeleteSaveDataMessage message)
        {
            _savedSettingsData.Delete();
        }

        public void ToggleGameInduction(ToggleGameInductionMessage message)
        {
            _model.SetIsGameIndctionActive(message.IsGameInductionActive);
            _savedSettingsData.Save(_model.SavedSettingsData);
        }

        public void ToggleSfx(ToggleSfxMessage message)
        {
            _model.SetIsSfxOn(message.Sfx);
            _savedSettingsData.Save(_model.SavedSettingsData);
        }

        public void ToggleBgm(ToggleBgmMessage message)
        {
            _model.SetIsBgmOn(message.Bgm);
            _savedSettingsData.Save(_model.SavedSettingsData);
        }

        #endregion
    }
}
