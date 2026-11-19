using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.PlayerSkill
{
    public class PlayerSkillConnector : BaseConnector
    {
        private readonly PlayerSkillController _playerSkill = new PlayerSkillController();

        protected override void Connect()
        {
            Subscribe<RequestCastSkillMessage>(_playerSkill.OnRequestCastSkill);
            Subscribe<SwitchWeaponMessage>(_playerSkill.OnSwitchWeapon);
            Subscribe<PlayerStatsChangedMessage>(_playerSkill.OnPlayerStatsChanged);
            Subscribe<StatueUpgradedMessage>(_playerSkill.OnStatueUpgraded);
            Subscribe<GameStateMessage>(_playerSkill.OnGameState);
            Subscribe<RequestToggleSkillUpgradePanelMessage>(_playerSkill.OnRequestToggleSkillUpgradePanel);
        }

        protected override void Disconnect()
        {
            Unsubscribe<RequestCastSkillMessage>(_playerSkill.OnRequestCastSkill);
            Unsubscribe<SwitchWeaponMessage>(_playerSkill.OnSwitchWeapon);
            Unsubscribe<PlayerStatsChangedMessage>(_playerSkill.OnPlayerStatsChanged);
            Unsubscribe<StatueUpgradedMessage>(_playerSkill.OnStatueUpgraded);
            Unsubscribe<GameStateMessage>(_playerSkill.OnGameState);
            Unsubscribe<RequestToggleSkillUpgradePanelMessage>(_playerSkill.OnRequestToggleSkillUpgradePanel);
        }
    }
}