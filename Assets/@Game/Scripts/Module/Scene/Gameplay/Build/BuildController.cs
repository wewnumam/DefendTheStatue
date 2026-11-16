using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Turns one key press into the nearest legal build. Everything it needs (unlocked rings, what
    /// is already built, statue level, gold) is mirrored from messages, so the gates stay pure
    /// BuildRules calls and this module holds no reference to any other controller.
    /// </summary>
    public class BuildController : ObjectController<BuildController, BuildView>
    {
        /// <summary>Ring geometry comes from the GameConstants asset, so the ring count is not fixed here.</summary>
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());

        private bool[] _unlocked = System.Array.Empty<bool>();
        private bool[,] _towersBuilt = new bool[0, 0];
        private bool[,] _wallsBuilt = new bool[0, 0];
        private int _statueLevel = StatueModel.MinLevel;
        private int _gold;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
            ResetProgress();
        }

        public override void SetView(BuildView view)
        {
            base.SetView(view);
            view.SetCallbacks(OnBuild, OnUpgrade);
            ResetProgress();
        }

        public void OnTerritoryUnlocked(TerritoryUnlockedMessage message)
        {
            if (message.TerritoryIndex >= 0 && message.TerritoryIndex < _unlocked.Length)
            {
                _unlocked[message.TerritoryIndex] = true;
            }
        }

        public void OnStatueUpgraded(StatueUpgradedMessage message)
        {
            _statueLevel = message.Level;
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _gold = message.Gold;
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            MarkTower(message.TerritoryIndex, message.CornerIndex);
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            MarkWall(message.TerritoryIndex, message.SegmentIndex);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                ResetProgress();
            }
        }

        /// <summary>Builds whichever legal slot, tower or wall, sits closest to the cursor.</summary>
        private void OnBuild(Vector2 cursor)
        {
            int territory = -1;
            int slot = -1;
            bool isTower = false;
            float bestDistance = float.MaxValue;

            for (int t = 0; t < _layout.RingCount; t++)
            {
                if (!_unlocked[t])
                {
                    continue;
                }

                if (BuildRules.CanBuildTowerAt(t, _statueLevel) && _gold >= BuildRules.TowerCost)
                {
                    for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
                    {
                        if (_towersBuilt[t, corner])
                        {
                            continue;
                        }

                        float distance = (_layout.GetCornerPosition(t, corner) - cursor).sqrMagnitude;
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            territory = t;
                            slot = corner;
                            isTower = true;
                        }
                    }
                }

                if (CanBuildWallAt(t) && _gold >= BuildRules.WallSegmentCost)
                {
                    for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
                    {
                        if (_wallsBuilt[t, segment])
                        {
                            continue;
                        }

                        _layout.GetWallSegment(t, segment, out Vector2 start, out Vector2 end);
                        float distance = ((start + end) * 0.5f - cursor).sqrMagnitude;
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            territory = t;
                            slot = segment;
                            isTower = false;
                        }
                    }
                }
            }

            if (slot < 0)
            {
                return;
            }

            if (isTower)
            {
                Publish(new RequestSpendGoldMessage(BuildRules.TowerCost));
                Publish(new TowerBuiltMessage(territory, slot, _layout.GetCornerPosition(territory, slot)));
                return;
            }

            _layout.GetWallSegment(territory, slot, out Vector2 from, out Vector2 to);
            Publish(new RequestSpendGoldMessage(BuildRules.WallSegmentCost));
            Publish(new WallSegmentBuiltMessage(territory, slot, from, to));
        }

        private void OnUpgrade()
        {
            Publish(new RequestUpgradeStatueMessage());
        }

        private bool CanBuildWallAt(int territoryIndex)
        {
            if (!_unlocked[territoryIndex] || !AllTowersBuilt(territoryIndex))
            {
                return false;
            }

            return territoryIndex != 1 || AllWallsBuilt(0);
        }

        private bool AllTowersBuilt(int territoryIndex)
        {
            for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
            {
                if (!_towersBuilt[territoryIndex, corner])
                {
                    return false;
                }
            }

            return true;
        }

        private bool AllWallsBuilt(int territoryIndex)
        {
            for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
            {
                if (!_wallsBuilt[territoryIndex, segment])
                {
                    return false;
                }
            }

            return true;
        }

        private void MarkTower(int territoryIndex, int cornerIndex)
        {
            if (territoryIndex >= 0 && territoryIndex < _unlocked.Length
                && cornerIndex >= 0 && cornerIndex < TerritoryState.TowerCount)
            {
                _towersBuilt[territoryIndex, cornerIndex] = true;
            }
        }

        private void MarkWall(int territoryIndex, int segmentIndex)
        {
            if (territoryIndex >= 0 && territoryIndex < _unlocked.Length
                && segmentIndex >= 0 && segmentIndex < TerritoryState.WallSegmentCount)
            {
                _wallsBuilt[territoryIndex, segmentIndex] = true;
            }
        }

        private void ResetProgress()
        {
            int ringCount = _layout.RingCount;
            _unlocked = new bool[ringCount];
            _towersBuilt = new bool[ringCount, TerritoryState.TowerCount];
            _wallsBuilt = new bool[ringCount, TerritoryState.WallSegmentCount];

            for (int t = 0; t < ringCount; t++)
            {
                _unlocked[t] = t == 0;

                for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
                {
                    _towersBuilt[t, corner] = false;
                }

                for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
                {
                    _wallsBuilt[t, segment] = false;
                }
            }

            _statueLevel = StatueModel.MinLevel;
        }
    }
}
