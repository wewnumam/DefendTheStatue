using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Teleport
{
    /// <summary>
    /// GDD §11: one pod per territory. Standing on a pod and pressing E builds it, then starts select
    /// mode; pressing E on a second built pod travels there. Essence is owned by the player module, so
    /// every price leaves as a bill.
    /// </summary>
    public class TeleportController : ObjectController<TeleportController, TeleportModel, TeleportView>
    {
        /// <summary>How close the pawn must stand to a pod, in world units.</summary>
        private const float InteractRadius = 24f;

        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());
        private int _essence;
        private Transform _player;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
        }

        public void SetPlayerTransform(Transform player)
        {
            _player = player;
        }

        public override void SetView(TeleportView view)
        {
            base.SetView(view);
            view.SetInteractRadius(InteractRadius);
            view.SetCallback(OnTick);
            LayOutPods();
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _essence = message.Essence;
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _model.SetStatueLevel(message.Level);

            if (_model.IsUnlocked && _view != null && _model.Count == 0)
            {
                LayOutPods();
            }
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();
            LayOutPods();
        }

        private void OnTick(float deltaSeconds)
        {
            if (_view == null || _player == null || !_model.IsUnlocked)
            {
                return;
            }

            if (_model.PendingFromIndex >= 0 && _view.CancelPressed)
            {
                CancelSelect();
                return;
            }

            if (!_view.InteractPressed)
            {
                return;
            }

            int podIndex = _view.FindPodNear(_player.position, _model.Pods);
            if (podIndex < 0)
            {
                return;
            }

            HandleInteract(podIndex);
        }

        private void HandleInteract(int podIndex)
        {
            TeleportPodState pod = _model.Get(podIndex);
            if (pod == null)
            {
                return;
            }

            if (!pod.Built)
            {
                if (!_model.CanBuild(podIndex) || _essence < TeleportModel.BuildCost)
                {
                    return;
                }

                Publish(new RequestSpendEssenceMessage(TeleportModel.BuildCost));
                _model.Build(podIndex);
                _view.SetBuilt(podIndex, true);
                Publish(new TeleportPodBuiltMessage(podIndex, pod.Position));
                return;
            }

            if (_model.PendingFromIndex < 0)
            {
                _model.BeginSelect(podIndex);
                _view.SetSelected(podIndex, true);
                Publish(new TeleportSelectModeMessage(true));
                return;
            }

            if (_model.PendingFromIndex == podIndex)
            {
                CancelSelect();
                return;
            }

            if (!_model.CanTeleport(_model.PendingFromIndex, podIndex) || _essence < TeleportModel.TeleportCost)
            {
                return;
            }

            int from = _model.PendingFromIndex;
            Publish(new RequestSpendEssenceMessage(TeleportModel.TeleportCost));
            CancelSelect();
            Publish(new TeleportPerformedMessage(pod.Position));

            if (_view != null)
            {
                _view.SetSelected(from, false);
            }
        }

        private void CancelSelect()
        {
            if (_model.PendingFromIndex >= 0 && _view != null)
            {
                _view.SetSelected(_model.PendingFromIndex, false);
            }

            _model.CancelSelect();
            Publish(new TeleportSelectModeMessage(false));
        }

        /// <summary>One pod per ring, parked on the ring's top edge so it reads as part of that territory.</summary>
        private void LayOutPods()
        {
            _view?.ClearPods();

            for (int ring = 0; ring < _layout.RingCount; ring++)
            {
                Vector2 size = _layout.GetRingSize(ring);
                Vector2 position = new Vector2(0f, size.y * 0.5f - 8f);
                _model.AddPod(ring, position);
            }

            _view?.SpawnPods(_model.Pods);
        }
    }
}
