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
    /// Turns one key press into the nearest legal build, the way the reference game does it: the pawn
    /// must stand next to a slot, and E spends essence on whatever is closest. Everything it needs
    /// (unlocked rings, what is already built, statue level, essence) is mirrored from messages, so the
    /// gates stay pure BuildRules calls and this module holds no reference to any other controller.
    /// </summary>
    public class BuildController : ObjectController<BuildController, BuildView>
    {
        /// <summary>Reference game: how close the pawn must stand to a tower corner, wall or foundation.</summary>
        private const float InteractRadius = 60f;

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
        private Transform _player;

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

        public void SetPlayerTransform(Transform player)
        {
            _player = player;
        }

        public override void SetView(BuildView view)
        {
            base.SetView(view);
            view.SetCallbacks(OnInteract, OnUpgrade);
            view.SetTickCallback(OnTick);
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
            _essence = message.Essence;
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == EnumManager.CheatType.FreeBuild)
            {
                _freeBuild = message.Enabled;
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

        private static int GuardCostForLevel(int level)
        {
            if (level >= 10)
            {
                return BuildRules.GuardCostLevel10;
            }

            return level >= 5 ? BuildRules.GuardCostLevel5 : BuildRules.GuardCostLevel3;
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

        /// <summary>
        /// GDD §16 Build All. Reuses the normal build pipeline for every empty slot on every unlocked
        /// ring — towers first, then walls, then the traps that need the walls — and skips the cost.
        /// Guards are recruited from statue-granted foundations, not built, so they stay out.
        /// </summary>
        public void OnDevBuildAll(DevBuildAllMessage message)
        {
            for (int t = 0; t < _layout.RingCount; t++)
            {
                if (!_unlocked[t])
                {
                    continue;
                }

                for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
                {
                    if (_towersBuilt[t, corner])
                    {
                        continue;
                    }

                    MarkTower(t, corner);
                    Publish(new TowerBuiltMessage(t, corner, _layout.GetCornerPosition(t, corner)));
                }

                for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
                {
                    if (_wallsBuilt[t, segment])
                    {
                        continue;
                    }

                    _layout.GetWallSegment(t, segment, out Vector2 from, out Vector2 to);
                    MarkWall(t, segment);
                    Publish(new WallSegmentBuiltMessage(t, segment, from, to));
                }

                // One trap per wall gap; the model counts gaps in the same four slots as towers.
                for (int gap = 0; gap < TerritoryState.TowerCount; gap++)
                {
                    if (_trapsBuilt[t, gap])
                    {
                        continue;
                    }

                    _trapsBuilt[t, gap] = true;
                    Publish(new TrapBuiltMessage(t, gap, _layout.GetWallGap(t, gap)));
                }
            }
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                ResetProgress();
            }
        }

        /// <summary>
        /// E is one context key, straight from the reference game: the nearest thing within reach wins,
        /// the statue as the baseline. Nothing in range means nothing happens.
        /// </summary>
        private void OnInteract()
        {
            if (_player == null)
            {
                return;
            }

            if (!FindTarget(_player.position, out BuildKind kind, out int territory, out int slot, out _, out _))
            {
                return;
            }

            if (kind == BuildKind.Any)
            {
                Publish(new RequestToggleSkillUpgradePanelMessage());
                return;
            }

            Build(kind, territory, slot);
        }

        /// <summary>Drives the floating "E" prompt: same target the key press would act on.</summary>
        private void OnTick(float deltaSeconds)
        {
            if (_view == null)
            {
                return;
            }

            if (_player == null
                || !FindTarget(_player.position, out BuildKind kind, out _, out _, out Vector2 position, out int cost))
            {
                _view.HidePrompt();
                return;
            }

            string label = kind == BuildKind.Any ? null : _freeBuild ? "FREE" : cost + "e";
            _view.ShowPrompt(position, label);
        }

        /// <summary>
        /// Nearest interactable within reach of <paramref name="at"/>: statue, tower corner, wall
        /// segment, trap gap or guard foundation. Kind stays <see cref="BuildKind.Any"/> for the statue.
        /// </summary>
        private bool FindTarget(
            Vector2 at,
            out BuildKind kind,
            out int territory,
            out int slot,
            out Vector2 position,
            out int cost)
        {
            float bestSquared = float.MaxValue;
            bool statue = false;
            kind = BuildKind.Any;
            territory = -1;
            slot = -1;
            position = Vector2.zero;
            cost = 0;

            if (_view != null && _view.HasStatue)
            {
                float squared = (_view.StatuePosition - at).sqrMagnitude;
                if (squared <= _view.StatuePickRadius * _view.StatuePickRadius)
                {
                    bestSquared = squared;
                    statue = true;
                    position = _view.StatuePosition;
                }
            }

            float reach = InteractRadius * InteractRadius;

            for (int t = 0; t < _layout.RingCount; t++)
            {
                if (!_unlocked[t])
                {
                    continue;
                }

                if (BuildRules.CanBuildTowerAt(t, _statueLevel))
                {
                    for (int corner = 0; corner < TerritoryState.TowerCount; corner++)
                    {
                        if (_towersBuilt[t, corner])
                        {
                            continue;
                        }

                        Vector2 cornerPosition = _layout.GetCornerPosition(t, corner);
                        float squared = (cornerPosition - at).sqrMagnitude;
                        if (squared <= reach && squared < bestSquared)
                        {
                            bestSquared = squared;
                            statue = false;
                            kind = BuildKind.Tower;
                            territory = t;
                            slot = corner;
                            position = cornerPosition;
                            cost = BuildRules.TowerCost;
                        }
                    }
                }

                if (CanBuildWallAt(t))
                {
                    for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
                    {
                        if (_wallsBuilt[t, segment])
                        {
                            continue;
                        }

                        _layout.GetWallSegment(t, segment, out Vector2 from, out Vector2 to);
                        Vector2 midpoint = (from + to) * 0.5f;
                        float squared = (midpoint - at).sqrMagnitude;
                        if (squared <= reach && squared < bestSquared)
                        {
                            bestSquared = squared;
                            statue = false;
                            kind = BuildKind.Wall;
                            territory = t;
                            slot = segment;
                            position = midpoint;
                            cost = BuildRules.WallSegmentCost;
                        }
                    }
                }

                // Four gaps per ring: the layout folds the eight wall halves into four openings.
                if (AllWallsBuilt(t))
                {
                    for (int gap = 0; gap < TerritoryState.TowerCount; gap++)
                    {
                        if (_trapsBuilt[t, gap])
                        {
                            continue;
                        }

                        Vector2 gapPosition = _layout.GetWallGap(t, gap);
                        float squared = (gapPosition - at).sqrMagnitude;
                        if (squared <= reach && squared < bestSquared)
                        {
                            bestSquared = squared;
                            statue = false;
                            kind = BuildKind.Trap;
                            territory = t;
                            slot = gap;
                            position = gapPosition;
                            cost = BuildRules.SpikeTrapCost;
                        }
                    }
                }
            }

            int guardSlots = TotalGuardSlots();
            for (int i = 0; i < guardSlots; i++)
            {
                if (_guardsBuilt[i])
                {
                    continue;
                }

                int level = GuardLevelForSlot(i);
                Vector2 foundation = _guardLayout.GetFoundationPosition(level, i - FirstSlotOfLevel(level), level);
                float squared = (foundation - at).sqrMagnitude;
                if (squared <= reach && squared < bestSquared)
                {
                    bestSquared = squared;
                    statue = false;
                    kind = BuildKind.Guard;
                    territory = -1;
                    slot = i;
                    position = foundation;
                    cost = GuardCostForLevel(level);
                }
            }

            if (statue)
            {
                kind = BuildKind.Any;
                return true;
            }

            return kind != BuildKind.Any;
        }

        private void Build(BuildKind kind, int territory, int slot)
        {
            switch (kind)
            {
                case BuildKind.Guard:
                {
                    int level = GuardLevelForSlot(slot);
                    int cost = GuardCostForLevel(level);
                    if (!CanAfford(cost))
                    {
                        return;
                    }

                    SpendEssence(cost);
                    Publish(new GuardBuiltMessage(
                        slot,
                        level,
                        _guardLayout.GetFoundationPosition(level, slot - FirstSlotOfLevel(level), level)));
                    return;
                }

                case BuildKind.Trap:
                    if (!CanAfford(BuildRules.SpikeTrapCost))
                    {
                        return;
                    }

                    SpendEssence(BuildRules.SpikeTrapCost);
                    Publish(new TrapBuiltMessage(territory, slot, _layout.GetWallGap(territory, slot)));
                    return;

                case BuildKind.Tower:
                    if (!CanAfford(BuildRules.TowerCost))
                    {
                        return;
                    }

                    SpendEssence(BuildRules.TowerCost);
                    Publish(new TowerBuiltMessage(territory, slot, _layout.GetCornerPosition(territory, slot)));
                    return;

                case BuildKind.Wall:
                    if (!CanAfford(BuildRules.WallSegmentCost))
                    {
                        return;
                    }

                    _layout.GetWallSegment(territory, slot, out Vector2 from, out Vector2 to);
                    SpendEssence(BuildRules.WallSegmentCost);
                    Publish(new WallSegmentBuiltMessage(territory, slot, from, to));
                    return;
            }
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
