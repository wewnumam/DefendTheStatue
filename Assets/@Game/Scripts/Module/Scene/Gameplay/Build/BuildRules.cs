using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using UnityEngine;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Build and upgrade gates, ported one to one from the reference game's canBuild* and
    /// canUpgradeToLevel functions. Pure rules: no state, so every branch is testable.
    /// </summary>
    public static class BuildRules
    {
        public const int TowerCost = 200;
        public const int WallSegmentCost = 125;
        public const int SpikeTrapCost = 150;
        public const int GuardCostLevel3 = 500;
        public const int GuardCostLevel5 = 800;

        /// <summary>Reference game: territory 0 needs statue level 2, territory 1 needs level 4.</summary>
        public static bool CanBuildTowerAt(int territoryIndex, int statueLevel)
        {
            switch (territoryIndex)
            {
                case 0:
                    return statueLevel >= StatueModel.MinLevel + 1;
                case 1:
                    return statueLevel >= StatueModel.MinLevel + 3;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Walls need the territory unlocked, every tower on it built, and for the second ring the
        /// inner ring's walls finished first.
        /// </summary>
        public static bool CanBuildWallSegmentAt(TerritoryState territory, bool innerTerritoryAllWallsBuilt)
        {
            if (territory == null || !territory.Unlocked || !territory.AllTowersBuilt)
            {
                return false;
            }

            return territory.Index != 1 || innerTerritoryAllWallsBuilt;
        }

        /// <summary>The reference game's upgrade ladder, expressed as the requirement for each level.</summary>
        public static bool CanUpgradeToLevel(
            int nextLevel,
            bool territory0AllWallsBuilt,
            int guardCountAtLevel3,
            bool territory1AllWallsBuilt,
            int guardCountAtLevel5,
            int monstersKilled)
        {
            switch (nextLevel)
            {
                case 2:
                    return true;
                case 3:
                    return territory0AllWallsBuilt;
                case 4:
                    return guardCountAtLevel3 >= 3;
                case 5:
                    return territory1AllWallsBuilt;
                case 6:
                    return guardCountAtLevel5 >= 6;
                case 7:
                    return monstersKilled >= 100;
                default:
                    return false;
            }
        }

        /// <summary>Reference game: max(40, 180 - minutes * 14) frames between spawns.</summary>
        public static int GetSpawnIntervalFrames(float gameSeconds)
        {
            float minutes = gameSeconds / 60f;
            return Mathf.Max(40, Mathf.FloorToInt(180f - minutes * 14f));
        }
    }
}
