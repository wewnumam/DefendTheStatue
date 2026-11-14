using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Territory;
using UnityEngine;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Drives the run: ticks the clock, spawns monsters on the reference cadence, and keeps the tally.
    /// It never calls into the monster pool directly — spawns go out as a message.
    ///
    /// The ring the monsters appear outside is tracked from wall-built messages rather than read off
    /// the territory controller, so the two modules stay independent.
    /// </summary>
    public class GameFlowController : ObjectController<GameFlowController, GameFlowModel, GameFlowView>
    {
        private readonly bool[] _ringHasWall = new bool[TerritoryModel.RingSizes.Length];

        private int _outermostWallIndex = -1;

        public override void SetView(GameFlowView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetCallback(OnTick);
        }

        private void OnTick(float deltaSeconds)
        {
            int due = _model.Advance(deltaSeconds);
            for (int i = 0; i < due; i++)
            {
                SpawnMonster();
            }
        }

        private void SpawnMonster()
        {
            Vector2 statue = _view.StatuePosition;
            int side = Random.Range(0, 4);
            Vector2 position;

            if (_outermostWallIndex >= 0)
            {
                Vector2 ring = TerritoryModel.GetRingSize(_outermostWallIndex);
                position = GameFlowModel.GetRingSpawnPosition(side, statue, ring.x, ring.y, Random.value);
            }
            else
            {
                _view.GetHalfViewSize(out float halfWidth, out float halfHeight);
                Camera camera = _view.Camera;
                Vector2 cameraPosition = camera != null ? (Vector2)camera.transform.position : statue;
                position = GameFlowModel.GetCameraSpawnPosition(side, cameraPosition, halfWidth, halfHeight, Random.value);
            }

            // Reference spawns always land outside the statue heal radius, so the tougher, richer
            // variant never appears from this path. The flag stays in the message for the boss pass.
            Publish(new RequestSpawnMonsterMessage(position, false));
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            if (message.TerritoryIndex < 0 || message.TerritoryIndex >= _ringHasWall.Length)
            {
                return;
            }

            _ringHasWall[message.TerritoryIndex] = true;
            RecomputeOutermostWall();
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            if (message.TerritoryIndex < 0 || message.TerritoryIndex >= _ringHasWall.Length)
            {
                return;
            }

            _ringHasWall[message.TerritoryIndex] = true;
            RecomputeOutermostWall();
        }

        private void RecomputeOutermostWall()
        {
            for (int i = _ringHasWall.Length - 1; i >= 0; i--)
            {
                if (_ringHasWall[i])
                {
                    _outermostWallIndex = i;
                    return;
                }
            }

            _outermostWallIndex = -1;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.RegisterMonsterKilled(message.GoldValue);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != Utility.EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();
            for (int i = 0; i < _ringHasWall.Length; i++)
            {
                _ringHasWall[i] = false;
            }

            _outermostWallIndex = -1;
        }
    }
}
