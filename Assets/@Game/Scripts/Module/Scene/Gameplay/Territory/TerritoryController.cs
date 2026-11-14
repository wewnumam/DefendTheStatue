using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.Territory
{
    /// <summary>
    /// Ring unlock and build bookkeeping. It listens to what was built rather than being told by
    /// the build module, so either side can be replaced without the other noticing.
    /// </summary>
    public class TerritoryController : ObjectController<TerritoryController, TerritoryModel, TerritoryView>
    {
        public override void SetView(TerritoryView view)
        {
            base.SetView(view);
            _model.Reset();
            view.Refresh(_model);
        }

        public void OnTerritoryUnlocked(TerritoryUnlockedMessage message)
        {
            _model.Unlock(message.TerritoryIndex);
            _view.Refresh(_model);
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            _model.MarkTowerBuilt(message.TerritoryIndex, message.CornerIndex);
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            _model.MarkWallSegmentBuilt(message.TerritoryIndex, message.SegmentIndex);
            _view.AddWall(message.Start, message.End);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                _model.Reset();
                if (_view != null)
                {
                    _view.Refresh(_model);
                }
            }
        }
    }
}
