using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.CameraRig
{
    /// <summary>Feeds the camera rig: the upgrade panel opens and closes, the run starts and ends.</summary>
    public class CameraRigConnector : BaseConnector
    {
        private readonly CameraRigController _cameraRig = new CameraRigController();

        protected override void Connect()
        {
            Subscribe<SkillUpgradePanelToggledMessage>(_cameraRig.OnSkillUpgradePanelToggled);
            Subscribe<TeleportSelectModeMessage>(_cameraRig.OnTeleportSelectMode);
            Subscribe<GameStateMessage>(_cameraRig.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<SkillUpgradePanelToggledMessage>(_cameraRig.OnSkillUpgradePanelToggled);
            Unsubscribe<TeleportSelectModeMessage>(_cameraRig.OnTeleportSelectMode);
            Unsubscribe<GameStateMessage>(_cameraRig.OnGameState);
        }
    }
}
