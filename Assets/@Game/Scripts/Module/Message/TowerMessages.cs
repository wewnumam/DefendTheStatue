using UnityEngine;

namespace ProjectTA.Message
{
    public struct RequestBuildTowerMessage
    {
        public RequestBuildTowerMessage(int territoryIndex, int cornerIndex)
        {
            TerritoryIndex = territoryIndex;
            CornerIndex = cornerIndex;
        }

        public int TerritoryIndex { get; }
        public int CornerIndex { get; }
    }

    public struct RequestBuildWallSegmentMessage
    {
        public RequestBuildWallSegmentMessage(int territoryIndex, int segmentIndex)
        {
            TerritoryIndex = territoryIndex;
            SegmentIndex = segmentIndex;
        }

        public int TerritoryIndex { get; }
        public int SegmentIndex { get; }
    }

    public struct TowerBuiltMessage
    {
        public TowerBuiltMessage(int territoryIndex, int cornerIndex, Vector2 position)
        {
            TerritoryIndex = territoryIndex;
            CornerIndex = cornerIndex;
            Position = position;
        }

        public int TerritoryIndex { get; }
        public int CornerIndex { get; }
        public Vector2 Position { get; }
    }

    public struct WallSegmentBuiltMessage
    {
        public WallSegmentBuiltMessage(int territoryIndex, int segmentIndex, Vector2 start, Vector2 end)
        {
            TerritoryIndex = territoryIndex;
            SegmentIndex = segmentIndex;
            Start = start;
            End = end;
        }

        public int TerritoryIndex { get; }
        public int SegmentIndex { get; }
        public Vector2 Start { get; }
        public Vector2 End { get; }
    }

    /// <summary>A tower fired: the arrow view spawns here and flies along AimAngle.</summary>
    public struct TowerFiredMessage
    {
        public TowerFiredMessage(Vector2 position, float aimAngle, Vector2 targetPosition, float damage)
        {
            Position = position;
            AimAngle = aimAngle;
            TargetPosition = targetPosition;
            Damage = damage;
        }

        public Vector2 Position { get; }
        public float AimAngle { get; }
        public Vector2 TargetPosition { get; }
        public float Damage { get; }
    }
}
