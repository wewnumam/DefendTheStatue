using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Guard
{
    /// <summary>
    /// Where guard foundations stand. Level 3 hands out three foundations on the inner ring, level 5
    /// hands out five on the outer ring. Plain C# so the build module and the guard controller place
    /// the same slots without sharing state.
    /// </summary>
    public class GuardLayout
    {
        public GuardLayout(TerritoryConstants territoryConstants)
        {
            Territory = territoryConstants ?? new TerritoryConstants();
        }

        public TerritoryConstants Territory { get; }

        /// <summary>
        /// Where the foundations stand: level 3 on the inner ring, level 5 on the ring between territories
        /// 1 and 2, level 10 on territory 2's outer edge (same ring, drawn outward).
        /// </summary>
        public int GetRingIndex(int foundationLevel)
        {
            return foundationLevel >= 5 ? 1 : 0;
        }

        /// <summary>GDD §5/§6 foundation counts: 3 inner, 6 outer (the level-6 gate needs six), 4 perimeter.</summary>
        public static int GetFoundationCount(int foundationLevel)
        {
            if (foundationLevel >= 10)
            {
                return 4;
            }

            if (foundationLevel >= 5)
            {
                return 6;
            }

            return foundationLevel >= 3 ? 3 : 0;
        }

        /// <summary>
        /// Foundations sit evenly around the ring's inscribed circle, starting at the top. Same call
        /// for the view and the build module, so a build lands on the foundation it looked at.
        /// </summary>
        public Vector2 GetFoundationPosition(int foundationLevel, int index, int count)
        {
            int ring = GetRingIndex(foundationLevel);
            Vector2 size = Territory.GetRingSize(ring);
            float radius = Mathf.Min(size.x, size.y) * 0.4f;
            int slots = Mathf.Max(1, count);
            float angle = -Mathf.PI * 0.5f + index * Mathf.PI * 2f / slots;

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        /// <summary>Territory 0 is the inner ring; everything outside its rectangle is "outer".</summary>
        public bool IsInsideRing(Vector2 position, int ringIndex)
        {
            Vector2 size = Territory.GetRingSize(ringIndex);
            return Mathf.Abs(position.x) <= size.x * 0.5f && Mathf.Abs(position.y) <= size.y * 0.5f;
        }
    }
}
