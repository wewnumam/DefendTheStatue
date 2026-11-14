using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Territory
{
    public enum TerritoryCorner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomRight = 2,
        BottomLeft = 3
    }

    /// <summary>One ring of the map: a rectangle of walls with a tower on each corner.</summary>
    public class TerritoryState
    {
        public const int TowerCount = 4;
        public const int WallSegmentCount = 8;

        public TerritoryState(int index, string name, float width, float height, bool unlocked)
        {
            Index = index;
            Name = name;
            Width = width;
            Height = height;
            Unlocked = unlocked;
            TowersBuilt = new bool[TowerCount];
            WallSegmentsBuilt = new bool[WallSegmentCount];
        }

        public int Index { get; }
        public string Name { get; }
        public float Width { get; }
        public float Height { get; }
        public bool Unlocked { get; private set; }
        public bool[] TowersBuilt { get; }
        public bool[] WallSegmentsBuilt { get; }

        public float HalfWidth => Width * 0.5f;
        public float HalfHeight => Height * 0.5f;

        public int TowerBuiltCount => CountTrue(TowersBuilt);
        public int WallBuiltCount => CountTrue(WallSegmentsBuilt);
        public bool AllTowersBuilt => TowerBuiltCount >= TowerCount;
        public bool AllWallBuilt => WallBuiltCount >= WallSegmentCount;

        public void Unlock()
        {
            Unlocked = true;
        }

        public void MarkTowerBuilt(int cornerIndex)
        {
            if (cornerIndex >= 0 && cornerIndex < TowerCount)
            {
                TowersBuilt[cornerIndex] = true;
            }
        }

        public void MarkWallSegmentBuilt(int segmentIndex)
        {
            if (segmentIndex >= 0 && segmentIndex < WallSegmentCount)
            {
                WallSegmentsBuilt[segmentIndex] = true;
            }
        }

        private static int CountTrue(bool[] flags)
        {
            int count = 0;
            foreach (bool flag in flags)
            {
                if (flag)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// The three concentric territories. Sizes, names and the level 2 / level 4 unlock points are
    /// the reference game's values.
    /// </summary>
    public class TerritoryModel : BaseModel
    {
        /// <summary>Ring extents, index-aligned with the territories. Static so the spawn ring can be measured without holding a controller.</summary>
        public static readonly Vector2[] RingSizes =
        {
            new Vector2(800f, 600f),
            new Vector2(1600f, 1200f),
            new Vector2(3200f, 2400f)
        };

        public static Vector2 GetRingSize(int index)
        {
            return index >= 0 && index < RingSizes.Length ? RingSizes[index] : RingSizes[0];
        }

        /// <summary>Territory corners, clockwise from the top-left. Ring is centred on the world origin.</summary>
        public static Vector2 GetCornerPosition(int territoryIndex, int cornerIndex)
        {
            Vector2 size = GetRingSize(territoryIndex);
            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;
            switch (((cornerIndex % TerritoryState.TowerCount) + TerritoryState.TowerCount) % TerritoryState.TowerCount)
            {
                case 0:
                    return new Vector2(-halfWidth, halfHeight);
                case 1:
                    return new Vector2(halfWidth, halfHeight);
                case 2:
                    return new Vector2(halfWidth, -halfHeight);
                default:
                    return new Vector2(-halfWidth, -halfHeight);
            }
        }

        /// <summary>
        /// Wall segments run clockwise from the top-left corner, two per side: 0 top-left half,
        /// 1 top-right half, 2 right-top half, 3 right-bottom half, and so on.
        /// </summary>
        public static void GetWallSegment(int territoryIndex, int segmentIndex, out Vector2 start, out Vector2 end)
        {
            Vector2 size = GetRingSize(territoryIndex);
            Vector2[] corners =
            {
                new Vector2(-size.x * 0.5f, size.y * 0.5f),
                new Vector2(size.x * 0.5f, size.y * 0.5f),
                new Vector2(size.x * 0.5f, -size.y * 0.5f),
                new Vector2(-size.x * 0.5f, -size.y * 0.5f)
            };

            int segment = ((segmentIndex % TerritoryState.WallSegmentCount) + TerritoryState.WallSegmentCount)
                % TerritoryState.WallSegmentCount;
            int corner = segment / 2;
            Vector2 from = corners[corner];
            Vector2 to = corners[(corner + 1) % TerritoryState.TowerCount];
            Vector2 middle = (from + to) * 0.5f;

            if (segment % 2 == 0)
            {
                start = from;
                end = middle;
            }
            else
            {
                start = middle;
                end = to;
            }
        }

        private readonly List<TerritoryState> _territories = new List<TerritoryState>();

        public IReadOnlyList<TerritoryState> Territories => _territories;
        public int Count => _territories.Count;

        public void Reset()
        {
            _territories.Clear();
            _territories.Add(new TerritoryState(0, "Wilayah 1", RingSizes[0].x, RingSizes[0].y, true));
            _territories.Add(new TerritoryState(1, "Wilayah 2", RingSizes[1].x, RingSizes[1].y, false));
            _territories.Add(new TerritoryState(2, "Wilayah 3", RingSizes[2].x, RingSizes[2].y, false));
            SetDataAsDirty();
        }

        public TerritoryState Get(int index)
        {
            return index >= 0 && index < _territories.Count ? _territories[index] : null;
        }

        public void Unlock(int index)
        {
            TerritoryState territory = Get(index);
            if (territory == null || territory.Unlocked)
            {
                return;
            }

            territory.Unlock();
            SetDataAsDirty();
        }

        public void MarkTowerBuilt(int territoryIndex, int cornerIndex)
        {
            TerritoryState territory = Get(territoryIndex);
            if (territory == null || cornerIndex < 0 || cornerIndex >= TerritoryState.TowerCount
                || territory.TowersBuilt[cornerIndex])
            {
                return;
            }

            territory.MarkTowerBuilt(cornerIndex);
            SetDataAsDirty();
        }

        public void MarkWallSegmentBuilt(int territoryIndex, int segmentIndex)
        {
            TerritoryState territory = Get(territoryIndex);
            if (territory == null || segmentIndex < 0 || segmentIndex >= TerritoryState.WallSegmentCount
                || territory.WallSegmentsBuilt[segmentIndex])
            {
                return;
            }

            territory.MarkWallSegmentBuilt(segmentIndex);
            SetDataAsDirty();
        }

        /// <summary>Reference game: the outermost territory with any wall built, else -1.</summary>
        public int OutermostWallIndex
        {
            get
            {
                for (int i = _territories.Count - 1; i >= 0; i--)
                {
                    if (_territories[i].WallBuiltCount > 0)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>Highest unlocked territory index; the camera and build rules use it.</summary>
        public int HighestUnlockedIndex
        {
            get
            {
                for (int i = _territories.Count - 1; i >= 0; i--)
                {
                    if (_territories[i].Unlocked)
                    {
                        return i;
                    }
                }

                return 0;
            }
        }
    }
}
