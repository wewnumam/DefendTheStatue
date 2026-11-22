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
        // GDD §12 economy, in essence.
        public const int TowerCost = 20;
        public const int WallSegmentCost = 20;
        public const int SpikeTrapCost = 5;
        public const int GuardCostLevel3 = 20;
        public const int GuardCostLevel5 = 25;
        public const int GuardCostLevel10 = 30;

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

        /// <summary>
        /// GDD §5 upgrade ladder, one requirement per target level. Levels 8 and 9 accept either the
        /// boss kill or the kill count; level 10 demands both.
        /// </summary>
        public static bool CanUpgradeToLevel(
            int nextLevel,
            bool territory0AllWallsBuilt,
            int guardCountAtLevel3,
            bool territory1AllWallsBuilt,
            int guardCountAtLevel5,
            int monstersKilled,
            bool bossDefeated = false)
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
                case 8:
                    return bossDefeated || monstersKilled >= 150;
                case 9:
                    return bossDefeated || monstersKilled >= 200;
                case 10:
                    return bossDefeated && monstersKilled >= 250;
                default:
                    return false;
            }
        }

        /// <summary>
        /// GDD §2/§7 spawn mix: Stackers in the first four minutes, Waltroys join for the middle phase,
        /// and every role is on the field after eight minutes. Roll is a 0..1 uniform.
        /// </summary>
        public static Utility.EnumManager.MonsterKind PickMonsterKind(float gameSeconds, float roll)
        {
            if (gameSeconds < 240f)
            {
                return Utility.EnumManager.MonsterKind.Stacker;
            }

            if (gameSeconds < 480f)
            {
                return roll < 0.75f
                    ? Utility.EnumManager.MonsterKind.Stacker
                    : Utility.EnumManager.MonsterKind.Waltroy;
            }

            if (roll < 0.5f)
            {
                return Utility.EnumManager.MonsterKind.Stacker;
            }

            return roll < 0.8f
                ? Utility.EnumManager.MonsterKind.Waltroy
                : Utility.EnumManager.MonsterKind.Deasor;
        }

        /// <summary>Reference game: max(40, 180 - minutes * 14) frames between spawns.</summary>
        public static int GetSpawnIntervalFrames(float gameSeconds)
        {
            float minutes = gameSeconds / 60f;
            return Mathf.Max(40, Mathf.FloorToInt(180f - minutes * 14f));
        }
    }
}
