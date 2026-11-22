using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.CameraRig
{
    /// <summary>
    /// Pulls the camera back while the skill upgrade panel is open and restores it on close. Nothing
    /// else moves the camera: Cinemachine owns the follow, this owns the lens size.
    /// </summary>
    public class CameraRigController : ObjectController<CameraRigController, CameraRigModel, CameraRigView>
    {
        public override void SetView(CameraRigView view)
        {
            base.SetView(view);

            _model.SetDefaultSize(view.CurrentSize);
            view.SetZoomTarget(_model.TargetSize);
        }

        public void SetFollowTarget(UnityEngine.Transform target)
        {
            if (_view != null)
            {
                _view.SetFollowTarget(target);
            }
        }

        public void OnSkillUpgradePanelToggled(SkillUpgradePanelToggledMessage message)
        {
            _model.SetZoomed(message.IsOpen);

            if (_view != null)
            {
                _view.SetZoomTarget(_model.TargetSize);
            }
        }

        public void OnTeleportSelectMode(TeleportSelectModeMessage message)
        {
            _model.SetSelectMode(message.Active);

            if (_view != null)
            {
                _view.SetZoomTarget(_model.TargetSize);
            }
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                return;
            }

            _model.SetZoomed(false);
            _model.SetSelectMode(false);

            if (_view != null)
            {
                _view.SetZoomTarget(_model.TargetSize);
            }
        }
    }
}
