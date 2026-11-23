using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Guard;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Turns one key press into the nearest legal build. Everything it needs (unlocked rings, what
    /// is already built, statue level, essence) is mirrored from messages, so the gates stay pure
    /// BuildRules calls and this module holds no reference to any other controller.
    /// </summary>
    public class BuildController : ObjectController<BuildController, BuildView>
    {
        /// <summary>Ring geometry comes from the GameConstants asset, so the ring count is not fixed here.</summary>
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());
        private GuardLayout _guardLayout = new GuardLayout(new TerritoryConstants());

        private bool[] _unlocked = System.Array.Empty<bool>();
        private bool[,] _towersBuilt = new bool[0, 0];
        private bool[,] _wallsBuilt = new bool[0, 0];
        private bool[,] _trapsBuilt = new bool[0, 0];
        private bool[] _guardsBuilt = System.Array.Empty<bool>();
        private int _statueLevel = StatueModel.MinLevel;
        private int _essence;
        private bool _freeBuild;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
            _guardLayout = new GuardLayout(gameConstants.GameConstants.TerritoryConstants);
            ResetProgress();
        }

        public override void SetView(BuildView view)
        {
            base.SetView(view);
            view.SetCallbacks(OnBuild, OnUpgrade);
            view.SetBuildKindCallback(OnBuildKind);
            view.SetBuyMenuVisible(false);
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
            _view?.RefreshBuyMenu(_essence, _freeBuild, _statueLevel);
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _essence = message.Essence;
            _view?.RefreshBuyMenu(_essence, _freeBuild, _statueLevel);
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == EnumManager.CheatType.FreeBuild)
            {
                _freeBuild = message.Enabled;
                _view?.RefreshBuyMenu(_essence, _freeBuild, _statueLevel);
            }
        }

        /// <summary>All slots the ladder can ever hand out: 3 + 6 + 4.</summary>
        private const int MaxGuardSlots = 13;

        /// <summary>GDD §5/§6: 3 inner slots at statue level 3, 6 outer at 5, 4 perimeter at 10.</summary>
        private int TotalGuardSlots()
        {
            int slots = 0;
            if (_statueLevel >= 3)
            {
                slots += 3;
            }

            if (_statueLevel >= 5)
            {
                slots += 6;
            }

            if (_statueLevel >= 10)
            {
                slots += 4;
            }

            return slots;
        }

        /// <summary>Slot index to foundation level: 0-2 inner, 3-8 outer, 9-12 perimeter.</summary>
        private static int GuardLevelForSlot(int slot)
        {
            if (slot >= 9)
            {
                return 10;
            }

            return slot >= 3 ? 5 : 3;
        }

        private static int FirstSlotOfLevel(int level)
        {
            if (level >= 10)
            {
                return 9;
            }

            return level >= 5 ? 3 : 0;
        }

        private bool CanAfford(int cost)
        {
            return _freeBuild || _essence >= cost;
        }

        private void SpendEssence(int cost)
        {
            if (!_freeBuild)
            {
                Publish(new RequestSpendEssenceMessage(cost));
            }
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            MarkTower(message.TerritoryIndex, message.CornerIndex);
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            MarkWall(message.TerritoryIndex, message.SegmentIndex);
        }

        public void OnTrapBuilt(TrapBuiltMessage message)
        {
            if (message.TerritoryIndex >= 0 && message.TerritoryIndex < _unlocked.Length
                && message.GapIndex >= 0 && message.GapIndex < TerritoryState.WallSegmentCount)
            {
                _trapsBuilt[message.TerritoryIndex, message.GapIndex] = true;
            }
        }

        public void OnGuardBuilt(GuardBuiltMessage message)
        {
            if (message.FoundationIndex >= 0 && message.FoundationIndex < _guardsBuilt.Length)
            {
                _guardsBuilt[message.FoundationIndex] = true;
            }
        }

        public void OnGameState(GameStateMessage message)
        {
            bool playing = message.GameState == EnumManager.GameState.Playing;
            _view?.SetBuyMenuVisible(playing);

            if (playing)
            {
                ResetProgress();
                _view?.RefreshBuyMenu(_essence, _freeBuild, _statueLevel);
            }
        }

        /// <summary>
        /// Builds whichever legal slot, tower or wall, sits closest to the cursor. Over the god statue
        /// the same key opens the skill upgrade interface instead — nothing is built there.
        /// </summary>
        private void OnBuild(Vector2 cursor)
        {
            Build(cursor, BuildKind.Any);
        }

        /// <summary>Buy menu entry: build only the requested kind, at the cursor.</summary>
        private void OnBuildKind(BuildKind kind)
        {
            Build(_view.CursorPosition, kind);
        }

        private void Build(Vector2 cursor, BuildKind kind)
        {
            if (_view.IsOverStatue(cursor))
            {
                Publish(new RequestToggleSkillUpgradePanelMessage());
                return;
            }

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

                if ((kind == BuildKind.Any || kind == BuildKind.Tower)
                    && BuildRules.CanBuildTowerAt(t, _statueLevel) && CanAfford(BuildRules.TowerCost))
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

                if ((kind == BuildKind.Any || kind == BuildKind.Wall)
                    && CanBuildWallAt(t) && CanAfford(BuildRules.WallSegmentCost))
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

            int trapTerritory = -1;
            int trapGap = -1;
            float trapBestDistance = float.MaxValue;

            if (slot < 0 && (kind == BuildKind.Any || kind == BuildKind.Trap)
                && CanAfford(BuildRules.SpikeTrapCost))
            {
                for (int t = 0; t < _layout.RingCount; t++)
                {
                    if (!_unlocked[t] || !AllWallsBuilt(t))
                    {
                        continue;
                    }

                    // Four gaps per ring: the layout folds the eight wall halves into four openings.
                    for (int gap = 0; gap < TerritoryState.TowerCount; gap++)
                    {
                        if (_trapsBuilt[t, gap])
                        {
                            continue;
                        }

                        float distance = (_layout.GetWallGap(t, gap) - cursor).sqrMagnitude;
                        if (distance < trapBestDistance)
                        {
                            trapBestDistance = distance;
                            trapTerritory = t;
                            trapGap = gap;
                        }
                    }
                }
            }

            int guardIndex = -1;
            int guardLevel = 0;
            int guardCost = 0;
            float guardBestDistance = float.MaxValue;

            if (slot < 0 && (kind == BuildKind.Any || kind == BuildKind.Guard))
            {
                int total = TotalGuardSlots();

                for (int i = 0; i < total; i++)
                {
                    if (_guardsBuilt[i])
                    {
                        continue;
                    }

                    int level = GuardLevelForSlot(i);
                    int slotInGroup = i - FirstSlotOfLevel(level);
                    int cost = level >= 10
                        ? BuildRules.GuardCostLevel10
                        : level >= 5 ? BuildRules.GuardCostLevel5 : BuildRules.GuardCostLevel3;
                    if (!CanAfford(cost))
                    {
                        continue;
                    }

                    Vector2 position = _guardLayout.GetFoundationPosition(level, slotInGroup, level);
                    float distance = (position - cursor).sqrMagnitude;
                    if (distance < guardBestDistance)
                    {
                        guardBestDistance = distance;
                        guardIndex = i;
                        guardLevel = level;
                        guardCost = cost;
                    }
                }
            }

            if (guardIndex >= 0 && guardBestDistance <= trapBestDistance)
            {
                SpendEssence(guardCost);
                Publish(new GuardBuiltMessage(
                    guardIndex,
                    guardLevel,
                    _guardLayout.GetFoundationPosition(guardLevel, guardIndex - FirstSlotOfLevel(guardLevel), guardLevel)));
                return;
            }

            if (trapTerritory >= 0)
            {
                SpendEssence(BuildRules.SpikeTrapCost);
                Publish(new TrapBuiltMessage(trapTerritory, trapGap, _layout.GetWallGap(trapTerritory, trapGap)));
                return;
            }

            if (slot < 0)
            {
                return;
            }

            if (isTower)
            {
                SpendEssence(BuildRules.TowerCost);
                Publish(new TowerBuiltMessage(territory, slot, _layout.GetCornerPosition(territory, slot)));
                return;
            }

            _layout.GetWallSegment(territory, slot, out Vector2 from, out Vector2 to);
            SpendEssence(BuildRules.WallSegmentCost);
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
            _trapsBuilt = new bool[ringCount, TerritoryState.WallSegmentCount];
            _guardsBuilt = new bool[MaxGuardSlots];

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
