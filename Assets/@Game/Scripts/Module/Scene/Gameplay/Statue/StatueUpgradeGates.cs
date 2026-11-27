using ProjectTA.Module.Build;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>One gate row on the upgrade panel: what it wants and how far the run has got.</summary>
    public readonly struct StatueGateLine
    {
        public StatueGateLine(string label, int current, int target, bool met)
        {
            Label = label;
            Current = Mathf.Clamp(current, 0, target);
            Target = target;
            Met = met;
        }

        public string Label { get; }
        public int Current { get; }
        public int Target { get; }
        public bool Met { get; }

        /// <summary>Panel row, e.g. "OK  Inner guards 3/3" or "--  Kills 40/100".</summary>
        public string Format()
        {
            return (Met ? "OK  " : "--  ") + Label + "  " + Current + "/" + Target;
        }
    }

    /// <summary>
    /// GDD §5 ladder rendered for the upgrade panel: the same rules as
    /// <see cref="BuildRules.CanUpgradeToLevel"/>, but per-row so the player sees progress instead of a
    /// dead button.
    /// </summary>
    public static class StatueUpgradeGates
    {
        public static bool IsReady(
            int nextLevel,
            int wallsTerritory0,
            int guardsAtLevel3,
            int wallsTerritory1,
            int guardsAtLevel5,
            int monstersKilled,
            bool bossDefeated)
        {
            return BuildRules.CanUpgradeToLevel(
                nextLevel, wallsTerritory0 >= WallsNeeded, guardsAtLevel3,
                wallsTerritory1 >= WallsNeeded, guardsAtLevel5, monstersKilled, bossDefeated);
        }

        /// <summary>GDD §9: eight wall segments per ring.</summary>
        public const int WallsNeeded = 8;

        /// <summary>Rows for the next level; empty when the level has no gate (or the statue is maxed).</summary>
        public static StatueGateLine[] Describe(
            int nextLevel,
            int wallsTerritory0,
            int guardsAtLevel3,
            int wallsTerritory1,
            int guardsAtLevel5,
            int monstersKilled,
            bool bossDefeated)
        {
            switch (nextLevel)
            {
                case 3:
                    return new[] { new StatueGateLine("Walls W1", wallsTerritory0, WallsNeeded, wallsTerritory0 >= WallsNeeded) };
                case 4:
                    return new[] { new StatueGateLine("Inner guards", guardsAtLevel3, 3, guardsAtLevel3 >= 3) };
                case 5:
                    return new[] { new StatueGateLine("Walls W2", wallsTerritory1, WallsNeeded, wallsTerritory1 >= WallsNeeded) };
                case 6:
                    return new[] { new StatueGateLine("Outer guards", guardsAtLevel5, 6, guardsAtLevel5 >= 6) };
                case 7:
                    return new[] { new StatueGateLine("Kills", monstersKilled, 100, monstersKilled >= 100) };
                case 8:
                    return new[] { KillsOrBoss(monstersKilled, 150, bossDefeated) };
                case 9:
                    return new[] { KillsOrBoss(monstersKilled, 200, bossDefeated) };
                case 10:
                    return new[]
                    {
                        new StatueGateLine("Boss defeated", bossDefeated ? 1 : 0, 1, bossDefeated),
                        new StatueGateLine("Kills", monstersKilled, 250, monstersKilled >= 250)
                    };
                default:
                    return new StatueGateLine[0];
            }
        }

        /// <summary>Levels 8/9: the boss kill stands in for the kill count.</summary>
        private static StatueGateLine KillsOrBoss(int monstersKilled, int target, bool bossDefeated)
        {
            return new StatueGateLine(
                "Kills (or boss)",
                bossDefeated ? target : monstersKilled,
                target,
                bossDefeated || monstersKilled >= target);
        }
    }
}
