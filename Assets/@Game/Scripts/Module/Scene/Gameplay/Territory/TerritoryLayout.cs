using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Territory
{
    /// <summary>
    /// Ring geometry, computed from <see cref="TerritoryConstants"/> instead of hard-coded numbers.
    /// Plain C# so the build module, the spawner and the statue can all ask the same layout without
    /// reaching into the territory controller.
    /// </summary>
    public class TerritoryLayout
    {
        private static readonly Vector2[] CornerDirections =
        {
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, -1f),
            new Vector2(-1f, -1f)
        };

        public TerritoryLayout(TerritoryConstants constants)
        {
            Constants = constants ?? new TerritoryConstants();
        }

        public TerritoryConstants Constants { get; }

        public int RingCount => Constants.RingCount;

        public Vector2 GetRingSize(int index)
        {
            return Constants.GetRingSize(index);
        }

        /// <summary>Territory corners, clockwise from the top-left. Ring is centred on the world origin.</summary>
        public Vector2 GetCornerPosition(int territoryIndex, int cornerIndex)
        {
            Vector2 size = GetRingSize(territoryIndex);
            int corner = ((cornerIndex % TerritoryState.TowerCount) + TerritoryState.TowerCount)
                % TerritoryState.TowerCount;
            Vector2 direction = CornerDirections[corner];

            return new Vector2(direction.x * size.x * 0.5f, direction.y * size.y * 0.5f);
        }

        /// <summary>
        /// Wall segments run clockwise from the top-left corner, two per side: 0 top-left half,
        /// 1 top-right half, 2 right-top half, 3 right-bottom half, and so on. Each half stops short of
        /// the side's middle by the configured gap, which leaves the four openings monsters walk through.
        /// </summary>
        public void GetWallSegment(int territoryIndex, int segmentIndex, out Vector2 start, out Vector2 end)
        {
            int segment = ((segmentIndex % TerritoryState.WallSegmentCount) + TerritoryState.WallSegmentCount)
                % TerritoryState.WallSegmentCount;
            int corner = segment / 2;

            Vector2 from = GetCornerPosition(territoryIndex, corner);
            Vector2 to = GetCornerPosition(territoryIndex, corner + 1);
            Vector2 middle = (from + to) * 0.5f;

            // Pull both halves away from the middle so a 2 × GapHalfWidth opening stays walkable.
            Vector2 halfDirection = (middle - from).normalized * Constants.GapHalfWidth;

            if (segment % 2 == 0)
            {
                start = from;
                end = middle - halfDirection;
            }
            else
            {
                start = middle + halfDirection;
                end = to;
            }
        }
    }
}