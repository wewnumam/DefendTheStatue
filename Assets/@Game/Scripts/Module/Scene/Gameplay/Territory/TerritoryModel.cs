using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
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
    /// The three concentric territories. Sizes come from <see cref="TerritoryConstants"/>; this class only
    /// keeps which ring is unlocked and what has been built on it.
    /// </summary>
    public class TerritoryModel : BaseModel
    {
        private readonly List<TerritoryState> _territories = new List<TerritoryState>();

        /// <summary>Defaults match the reference asset, so unit tests need no asset.</summary>
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());

        public TerritoryLayout Layout => _layout;

        public void SetLayout(TerritoryLayout layout)
        {
            if (layout != null)
            {
                _layout = layout;
            }

            SetDataAsDirty();
        }

        public IReadOnlyList<TerritoryState> Territories => _territories;
        public int Count => _territories.Count;

        public void Reset()
        {
            _territories.Clear();

            for (int i = 0; i < _layout.RingCount; i++)
            {
                Vector2 size = _layout.GetRingSize(i);
                _territories.Add(new TerritoryState(i, $"Wilayah {i + 1}", size.x, size.y, i == 0));
            }

            SetDataAsDirty();
        }

        public TerritoryState Get(int index)
        {
            return index >= 0 && index < _territories.Count ? _territories[index] : null;
        }

        public Vector2 GetRingSize(int index)
        {
            return _layout.GetRingSize(index);
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