using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;
using UnityEngine;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.SaveSystem;

namespace ProjectTA.Module.PlayerCharacter
{
    public class PlayerCharacterController : ObjectController<PlayerCharacterController, PlayerCharacterModel, PlayerCharacterView>
    {
        public void InitModel(IGameConstantsModel gameConstants, IGameSettingsModel gameSettings)
        {
            if (!ValidateGameConstants(gameConstants) || !ValidateGameSettings(gameSettings))
                return;

            _model.SetPlayerConstants(gameConstants.GameConstants.PlayerConstants);
        }

        public override void SetView(PlayerCharacterView view)
        {
            base.SetView(view);
            view.PlayerConstants = _model.PlayerConstants;
        }

         #region PRIVATE METHOD

        private bool ValidateGameConstants(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null)
                return LogError("GAMECONSTANTS IS NULL");

            if (gameConstants.GameConstants == null)
                return LogError("SOGAMECONSTANTS IS NULL");

            if (gameConstants.GameConstants.PlayerConstants == null)
                return LogError("PLAYERCONSTANTS IS NULL");

            return true;
        }

        private bool ValidateGameSettings(IGameSettingsModel gameSettings)
        {
            if (gameSettings == null)
                return LogError("GAMESETTINGS IS NULL");

            if (gameSettings.SavedSettingsData == null)
                return LogError("SAVEDSETTINGSDATA IS NULL");

            return true;
        }

        private bool LogError(string message)
        {
            Debug.LogError(message);
            return false;
        }

        #endregion

        public void OnMove(MovePlayerCharacterMessage message)
        {
            _view.SetDirection(message.Direction);
        }

        public void OnRotate(RotatePlayerCharacterMessage message)
        {
            _view.SetAim(message.Aim);
        }

        /// <summary>
        /// A kill pays out here rather than in the monster module, so gold keeps a single owner.
        /// The model already has the gold field; this only routes the message into it.
        /// </summary>
        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.AddGold(message.GoldValue);
            PublishStatsChanged();
        }

        private void PublishStatsChanged()
        {
            Publish(new PlayerStatsChangedMessage(
                _model.Health,
                _model.MaxHealth,
                _model.Stamina,
                _model.Mana,
                _model.Gold,
                _model.Arrows));
        }
    }
}
