using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Tree
{
    /// <summary>One tree. Standing trees block movement through their prefab collider; position is fixed.</summary>
    public class TreeState
    {
        public TreeState(int id, Vector2 position, int territoryIndex, Utility.EnumManager.TreeKind kind, TreeKindStats stats)
        {
            Id = id;
            Position = position;
            TerritoryIndex = territoryIndex;
            Kind = kind;
            Stats = stats;
            MaxHealth = stats.Health;
            Health = MaxHealth;
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public int TerritoryIndex { get; }
        public Utility.EnumManager.TreeKind Kind { get; }
        public TreeKindStats Stats { get; }
        public float MaxHealth { get; }
        public float Health { get; set; }

        /// <summary>GDD §10: monsters never eat a Purple tree — it only ever yields mana to the player.</summary>
        public bool IsEdibleByMonsters => Kind != Utility.EnumManager.TreeKind.Purple;

        /// <summary>Reference isDestroyed: a chopped tree leaves a stump and grows back later.</summary>
        public bool IsDestroyed { get; set; }

        public float RespawnRemaining { get; set; }

        public bool CanHeal => !IsDestroyed && Health > 0f;
    }

    /// <summary>
    /// Every tree on the map, plus the one-tree-per-minute growth clock. Rules only: the reference
    /// game's getRandomTreePosition / removeTreesAt / removeTreesAlongSegment are the source of truth
    /// for the geometry here, the controller owns the randomness.
    /// </summary>
    public class TreeModel : BaseModel
    {
        private readonly List<TreeState> _trees = new List<TreeState>();

        /// <summary>Defaults match the reference asset, so unit tests need no asset.</summary>
        private TreeConstants _constants = new TreeConstants();

        private int _nextId;
        private float _growthElapsed;

        public TreeConstants Constants => _constants;

        public void SetTreeConstants(TreeConstants constants)
        {
            if (constants != null)
            {
                _constants = constants;
            }

            SetDataAsDirty();
        }

        public IReadOnlyList<TreeState> Trees => _trees;
        public int Count => _trees.Count;

        public TreeState Get(int id)
        {
            for (int i = 0; i < _trees.Count; i++)
            {
                if (_trees[i].Id == id)
                {
                    return _trees[i];
                }
            }

            return null;
        }

        public int Add(
            Vector2 position,
            int territoryIndex,
            Utility.EnumManager.TreeKind kind = Utility.EnumManager.TreeKind.Red)
        {
            int id = _nextId++;
            _trees.Add(new TreeState(id, position, territoryIndex, kind, _constants.GetKind(kind)));
            SetDataAsDirty();
            return id;
        }

        public bool Remove(int id)
        {
            for (int i = 0; i < _trees.Count; i++)
            {
                if (_trees[i].Id != id)
                {
                    continue;
                }

                _trees.RemoveAt(i);
                SetDataAsDirty();
                return true;
            }

            return false;
        }

        /// <summary>Reference removeTreesAt: every tree inside the radius goes.</summary>
        public int ClearAt(Vector2 center, float radius, List<TreeState> cleared = null)
        {
            float radiusSquared = radius * radius;
            int count = 0;

            for (int i = _trees.Count - 1; i >= 0; i--)
            {
                if ((_trees[i].Position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                cleared?.Add(_trees[i]);
                _trees.RemoveAt(i);
                count++;
            }

            if (count > 0)
            {
                SetDataAsDirty();
            }

            return count;
        }

        /// <summary>Reference removeTreesAlongSegment: a built wall clears what stands on its line.</summary>
        public int ClearAlongSegment(Vector2 start, Vector2 end, float radius, List<TreeState> cleared = null)
        {
            int count = 0;

            for (int i = _trees.Count - 1; i >= 0; i--)
            {
                if (PointToSegmentDistance(_trees[i].Position, start, end) >= radius)
                {
                    continue;
                }

                cleared?.Add(_trees[i]);
                _trees.RemoveAt(i);
                count++;
            }

            if (count > 0)
            {
                SetDataAsDirty();
            }

            return count;
        }

        /// <summary>Reference treeGrowthTimer: advances only while at least one territory is open.</summary>
        public int AdvanceGrowth(float deltaSeconds, bool anyTerritoryUnlocked)
        {
            if (deltaSeconds <= 0f || !anyTerritoryUnlocked || _constants.GrowthIntervalSeconds <= 0f)
            {
                return 0;
            }

            _growthElapsed += deltaSeconds;
            int due = 0;

            while (_growthElapsed >= _constants.GrowthIntervalSeconds)
            {
                _growthElapsed -= _constants.GrowthIntervalSeconds;
                due++;
            }

            return due;
        }

        /// <summary>
        /// Reference damageTree. Returns true when this hit chopped the tree; the tree stays in the list
        /// with a respawn timer so its id and position survive for the monster heal targets.
        /// </summary>
        public bool Damage(int id, float amount)
        {
            TreeState tree = Get(id);
            if (tree == null || !tree.CanHeal || amount <= 0f)
            {
                return false;
            }

            tree.Health = Mathf.Max(0f, tree.Health - amount);
            SetDataAsDirty();

            if (tree.Health > 0f)
            {
                return false;
            }

            tree.IsDestroyed = true;
            tree.RespawnRemaining = tree.Stats.RespawnSeconds;
            return true;
        }

        /// <summary>Reference sword swing's findNearestTree + damageTree: every tree inside the radius takes the hit.</summary>
        public int DamageArea(Vector2 center, float radius, float amount, List<TreeState> chopped = null)
        {
            float radiusSquared = radius * radius;
            int count = 0;

            for (int i = 0; i < _trees.Count; i++)
            {
                TreeState tree = _trees[i];
                if (!tree.CanHeal || (tree.Position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                if (Damage(tree.Id, amount))
                {
                    chopped?.Add(tree);
                    count++;
                }
            }

            return count;
        }

        /// <summary>Reference respawnTimer: a chopped tree grows back after its cooldown.</summary>
        public int TickRespawn(float deltaSeconds, List<TreeState> revived = null)
        {
            if (deltaSeconds <= 0f)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < _trees.Count; i++)
            {
                TreeState tree = _trees[i];
                if (!tree.IsDestroyed)
                {
                    continue;
                }

                tree.RespawnRemaining -= deltaSeconds;
                if (tree.RespawnRemaining > 0f)
                {
                    continue;
                }

                tree.IsDestroyed = false;
                tree.RespawnRemaining = 0f;
                tree.Health = tree.MaxHealth;
                revived?.Add(tree);
                count++;
            }

            if (count > 0)
            {
                SetDataAsDirty();
            }

            return count;
        }

        /// <summary>Reference findNearestHealTree: nearest living tree inside the search range.</summary>
        public TreeState FindNearestHealTree(Vector2 from, float maxRange)
        {
            TreeState nearest = null;
            float bestSquared = maxRange * maxRange;

            for (int i = 0; i < _trees.Count; i++)
            {
                TreeState tree = _trees[i];
                if (!tree.CanHeal || !tree.IsEdibleByMonsters)
                {
                    continue;
                }

                float squared = (tree.Position - from).sqrMagnitude;
                if (squared < bestSquared)
                {
                    bestSquared = squared;
                    nearest = tree;
                }
            }

            return nearest;
        }

        public void Clear()
        {
            _trees.Clear();
            _nextId = 0;
            _growthElapsed = 0f;
            SetDataAsDirty();
        }

        public static float PointToSegmentDistance(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared <= 0.000001f)
            {
                return (point - start).magnitude;
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
            return (point - (start + segment * t)).magnitude;
        }
    }
}
