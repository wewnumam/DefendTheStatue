using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>A tree was removed by a build or a hit: the run tally and the tree view both care.</summary>
    public struct TreeChoppedMessage
    {
        public TreeChoppedMessage(int treeId, Vector2 position, int territoryIndex)
        {
            TreeId = treeId;
            Position = position;
            TerritoryIndex = territoryIndex;
        }

        public int TreeId { get; }
        public Vector2 Position { get; }
        public int TerritoryIndex { get; }
    }

    /// <summary>
    /// A tree is standing at this position — on spawn and again when it grows back. Monsters keep their
    /// own copy of the list from this message, so the pool never reaches into the tree model.
    /// </summary>
    public struct TreeSpawnedMessage
    {
        public TreeSpawnedMessage(
            int treeId,
            Vector2 position,
            int territoryIndex,
            Utility.EnumManager.TreeKind kind = Utility.EnumManager.TreeKind.Red)
        {
            TreeId = treeId;
            Position = position;
            TerritoryIndex = territoryIndex;
            Kind = kind;
        }

        public int TreeId { get; }
        public Vector2 Position { get; }
        public int TerritoryIndex { get; }
        public Utility.EnumManager.TreeKind Kind { get; }
    }

    /// <summary>
    /// A hit landed on a tree. Arrows and monster bites publish this instead of touching the tree
    /// controller, so the tree module stays the only owner of tree health.
    /// </summary>
    public struct RequestTreeDamageMessage
    {
        public RequestTreeDamageMessage(int treeId, float amount, Vector2 hitPosition, bool fromMonster = false)
        {
            TreeId = treeId;
            Amount = amount;
            HitPosition = hitPosition;
            FromMonster = fromMonster;
        }

        public int TreeId { get; }
        public float Amount { get; }
        public Vector2 HitPosition { get; }

        /// <summary>A monster bite only drains the tree; it never pays the player mana.</summary>
        public bool FromMonster { get; }
    }
}
