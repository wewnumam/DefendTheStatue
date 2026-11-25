using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Tree
{
    /// <summary>
    /// Scatters the twenty trees per territory, grows one more per interval, and clears whatever a
    /// newly built tower or wall stands on. Unlocked rings arrive as messages, so this module never
    /// reads the territory controller.
    /// </summary>
    public class TreeController : ObjectController<TreeController, TreeModel, TreeView>
    {
        /// <summary>Ring geometry comes from the GameConstants asset, so the ring count is not fixed here.</summary>
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());

        private bool[] _unlocked = System.Array.Empty<bool>();
        private readonly List<Vector2> _towerPositions = new List<Vector2>();
        private readonly List<TreeState> _clearedTrees = new List<TreeState>();
        private readonly List<TreeState> _damagedTrees = new List<TreeState>();
        private readonly List<TreeState> _revivedTrees = new List<TreeState>();

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
            _model.SetTreeConstants(gameConstants.GameConstants.TreeConstants);
            ResetProgress();
        }

        public override void SetView(TreeView view)
        {
            base.SetView(view);
            _model.Clear();
            view.ClearTrees();
            ResetProgress();
            view.SetCallback(OnTick);

            // The first ring is open from the start, exactly like the reference's spawnTreesForTerritory(0).
            SpawnTerritory(0);
        }

        public void OnTerritoryUnlocked(TerritoryUnlockedMessage message)
        {
            int index = message.TerritoryIndex;
            if (index < 0 || index >= _unlocked.Length || _unlocked[index])
            {
                return;
            }

            _unlocked[index] = true;
            SpawnTerritory(index);
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            _towerPositions.Add(message.Position);

            _clearedTrees.Clear();
            if (_model.ClearAt(message.Position, _model.Constants.ClearRadius, _clearedTrees) == 0)
            {
                return;
            }

            PublishCleared(message.TerritoryIndex);
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            _clearedTrees.Clear();
            if (_model.ClearAlongSegment(
                    message.Start, message.End, _model.Constants.ClearRadius, _clearedTrees) == 0)
            {
                return;
            }

            PublishCleared(message.TerritoryIndex);
        }

        /// <summary>Sword swings and dashes: the reference's findNearestTree + damageTree, widened to the swing radius.</summary>
        public void OnAreaDamage(RequestAreaDamageMessage message)
        {
            _clearedTrees.Clear();
            _damagedTrees.Clear();
            if (_model.DamageArea(message.Center, message.Radius, message.Amount, _clearedTrees, _damagedTrees) == 0)
            {
                ShowBars(_damagedTrees);
                return;
            }

            ShowBars(_damagedTrees);
            PublishManaFrom(_clearedTrees);
            PublishChopped();
        }

        /// <summary>Arrows and monster bites land on one tree.</summary>
        public void OnTreeDamage(RequestTreeDamageMessage message)
        {
            TreeState tree = _model.Get(message.TreeId);
            if (tree == null || !tree.CanHeal)
            {
                return;
            }

            _clearedTrees.Clear();
            _damagedTrees.Clear();
            _damagedTrees.Add(tree);
            _model.Damage(message.TreeId, message.Amount);
            ShowBars(_damagedTrees);

            if (!tree.IsDestroyed)
            {
                return;
            }

            if (!message.FromMonster)
            {
                PublishMana(tree);
            }

            _clearedTrees.Add(tree);
            PublishChopped();
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();
            ResetProgress();

            // The launcher publishes the playing state before it wires the views.
            if (_view == null)
            {
                return;
            }

            _view.ClearTrees();
            SpawnTerritory(0);
        }

        private void OnTick(float deltaSeconds)
        {
            int due = _model.AdvanceGrowth(deltaSeconds, AnyTerritoryUnlocked());
            for (int i = 0; i < due; i++)
            {
                SpawnOneTree();
            }

            _revivedTrees.Clear();
            if (_model.TickRespawn(deltaSeconds, _revivedTrees) == 0)
            {
                return;
            }

            for (int i = 0; i < _revivedTrees.Count; i++)
            {
                TreeState tree = _revivedTrees[i];

                if (_view != null)
                {
                    _view.SetVisible(tree.Id, true);
                }

                Publish(new TreeSpawnedMessage(tree.Id, tree.Position, tree.TerritoryIndex));
            }
        }

        /// <summary>GDD §10 distribution per ring: 15 Red, 15 Purple, 4 Magic, 2 Arcana.</summary>
        private void SpawnTerritory(int territoryIndex)
        {
            for (int k = 0; k < TreeKindCount; k++)
            {
                Utility.EnumManager.TreeKind kind = (Utility.EnumManager.TreeKind)k;
                SpawnTrees(territoryIndex, _model.Constants.GetKind(kind).CountPerTerritory, kind);
            }
        }

        private const int TreeKindCount = 4;

        private void SpawnTrees(int territoryIndex, int count, Utility.EnumManager.TreeKind kind)
        {
            if (_view == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (!TryFindPlacement(territoryIndex, out Vector2 position))
                {
                    continue;
                }

                int id = _model.Add(position, territoryIndex, kind);
                _view.SpawnTree(id, position, _model.Constants.Size, kind);
                Publish(new TreeSpawnedMessage(id, position, territoryIndex, kind));
            }
        }

        /// <summary>Reference treeGrowthTimer path: one random open ring, one tree, anywhere legal.</summary>
        private void SpawnOneTree()
        {
            int unlockedCount = 0;
            for (int i = 0; i < _unlocked.Length; i++)
            {
                if (_unlocked[i])
                {
                    unlockedCount++;
                }
            }

            if (unlockedCount == 0)
            {
                return;
            }

            int pick = Random.Range(0, unlockedCount);
            for (int i = 0; i < _unlocked.Length; i++)
            {
                if (!_unlocked[i])
                {
                    continue;
                }

                if (pick-- > 0)
                {
                    continue;
                }

                SpawnTrees(i, 1, Utility.EnumManager.TreeKind.Red);
                return;
            }
        }

        /// <summary>Reference getRandomTreePosition: inset from the ring edge, clear of statue, towers and trees.</summary>
        private bool TryFindPlacement(int territoryIndex, out Vector2 position)
        {
            Vector2 size = _layout.GetRingSize(territoryIndex);
            float halfWidth = Mathf.Max(0f, size.x * 0.5f - _model.Constants.PlacementInset);
            float halfHeight = Mathf.Max(0f, size.y * 0.5f - _model.Constants.PlacementInset);
            float statueClearanceSquared = _model.Constants.StatueClearance * _model.Constants.StatueClearance;
            float spacingSquared = _model.Constants.MinSpacing * _model.Constants.MinSpacing;

            for (int attempt = 0; attempt < 40; attempt++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(-halfWidth, halfWidth),
                    Random.Range(-halfHeight, halfHeight));

                if (candidate.sqrMagnitude < statueClearanceSquared || IsBlocked(candidate, spacingSquared))
                {
                    continue;
                }

                position = candidate;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        private bool IsBlocked(Vector2 candidate, float spacingSquared)
        {
            for (int i = 0; i < _towerPositions.Count; i++)
            {
                if ((_towerPositions[i] - candidate).sqrMagnitude < spacingSquared)
                {
                    return true;
                }
            }

            for (int i = 0; i < _model.Count; i++)
            {
                TreeState tree = _model.Trees[i];
                if (tree.IsDestroyed)
                {
                    continue;
                }

                if ((tree.Position - candidate).sqrMagnitude < spacingSquared)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Chopped trees keep their id and position; the sprite is hidden until it grows back.</summary>
        /// <summary>GDD §10: only Purple, Magic and Arcana carry mana.</summary>
        private void PublishManaFrom(List<TreeState> trees)
        {
            for (int i = 0; i < trees.Count; i++)
            {
                if (trees[i].IsDestroyed)
                {
                    PublishMana(trees[i]);
                }
            }
        }

        private void PublishMana(TreeState tree)
        {
            if (tree == null || tree.Stats == null || tree.Stats.ManaDrop <= 0f)
            {
                return;
            }

            Publish(new RequestManaGainMessage(tree.Stats.ManaDrop));
        }

        /// <summary>Chop order and clean-up order differ, so bars are refreshed from whatever took the hit.</summary>
        private void ShowBars(List<TreeState> trees)
        {
            if (_view == null)
            {
                return;
            }

            for (int i = 0; i < trees.Count; i++)
            {
                TreeState tree = trees[i];
                if (tree.IsDestroyed)
                {
                    continue;
                }

                _view.SetHealth(tree.Id, tree.Health / tree.MaxHealth);
            }
        }

        private void PublishChopped()
        {
            for (int i = 0; i < _clearedTrees.Count; i++)
            {
                TreeState tree = _clearedTrees[i];

                if (_view != null)
                {
                    _view.SetVisible(tree.Id, false);
                }

                Publish(new TreeChoppedMessage(tree.Id, tree.Position, tree.TerritoryIndex));
            }

            _clearedTrees.Clear();
        }

        private void PublishCleared(int territoryIndex)
        {
            for (int i = 0; i < _clearedTrees.Count; i++)
            {
                TreeState tree = _clearedTrees[i];

                if (_view != null)
                {
                    _view.RemoveTree(tree.Id);
                }

                Publish(new TreeChoppedMessage(tree.Id, tree.Position, territoryIndex));
            }

            _clearedTrees.Clear();
        }

        private bool AnyTerritoryUnlocked()
        {
            for (int i = 0; i < _unlocked.Length; i++)
            {
                if (_unlocked[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void ResetProgress()
        {
            _unlocked = new bool[_layout.RingCount];
            _towerPositions.Clear();
            _clearedTrees.Clear();
            _damagedTrees.Clear();

            if (_unlocked.Length > 0)
            {
                _unlocked[0] = true;
            }
        }
    }
}
