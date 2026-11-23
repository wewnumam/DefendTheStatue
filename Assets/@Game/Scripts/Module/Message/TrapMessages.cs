using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>A spike trap was built at a wall gap. The trap module owns the durability from here.</summary>
    public struct TrapBuiltMessage
    {
        public TrapBuiltMessage(int territoryIndex, int gapIndex, Vector2 position)
        {
            TerritoryIndex = territoryIndex;
            GapIndex = gapIndex;
            Position = position;
        }

        public int TerritoryIndex { get; }
        public int GapIndex { get; }
        public Vector2 Position { get; }
    }

    /// <summary>A spike trap fired. The results screen counts them; nothing else consumes this.</summary>
    public struct TrapTriggeredMessage
    {
        public TrapTriggeredMessage(int trapIndex, Vector2 position)
        {
            TrapIndex = trapIndex;
            Position = position;
        }

        public int TrapIndex { get; }
        public Vector2 Position { get; }
    }
}
