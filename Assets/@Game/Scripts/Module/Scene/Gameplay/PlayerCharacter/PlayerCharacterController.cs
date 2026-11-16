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
        /// <summary>Statue heal radius, in the same world units the statue view is drawn in.</summary>
        private const float HealRadius = 120f;

        public void InitModel(IGameConstantsModel gameConstants, IGameSettingsModel gameSettings)
        {
            if (!ValidateGameConstants(gameConstants) || !ValidateGameSettings(gameSettings))
                return;

            _model.SetPlayerConstants(gameConstants.GameConstants.PlayerConstants);
            _model.ResetStats();
            PublishStatsChanged();
        }

        public override void SetView(PlayerCharacterView view)
        {
            base.SetView(view);
            view.PlayerConstants = _model.PlayerConstants;
            view.SetTickCallback(OnTick);
        }

        /// <summary>
        /// Until skills and blocking exist there is nothing to lock regen with, so the tick only runs
        /// the idle regeneration and the heal aura. Wire the flags here once those land.
        /// </summary>
        private void OnTick(float deltaTime)
        {
            bool isInHealRadius = _view.transform.position.sqrMagnitude <= HealRadius * HealRadius;
            _model.Tick(deltaTime, false, false, false, isInHealRadius);
            PublishStatsChanged();
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

        /// <summary>Scroll up arms the bow, so the pawn's idle art follows the weapon.</summary>
        public void OnSwitchWeapon(SwitchWeaponMessage message)
        {
            _view.SetWeaponVisual(message.Scroll);
        }

        /// <summary>Mana is owned here, so the skill module reports the cost and this pays it.</summary>
        public void OnSkillCast(SkillCastMessage message)
        {
            if (message.ManaSpent <= 0 || _model.TrySpendMana(message.ManaSpent))
            {
                PublishStatsChanged();
            }
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
                _model.MaxStamina,
                _model.Mana,
                _model.MaxMana,
                _model.Gold,
                _model.Arrows));
        }
    }
}