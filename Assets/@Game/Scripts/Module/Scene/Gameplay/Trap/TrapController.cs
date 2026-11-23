using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Trap
{
    /// <summary>
    /// Spike traps at the four wall gaps. A ring's foundations appear once its walls are finished; the
    /// player builds one with a TrapBuiltMessage. Triggers arrive from the trap's own collider, so the
    /// controller never scans monsters.
    /// </summary>
    public class TrapController : ObjectController<TrapController, TrapModel, TrapView>
    {
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());

        private bool[] _unlocked = System.Array.Empty<bool>();
        private bool[,] _wallsBuilt = new bool[0, 0];
        private bool[] _foundationsSpawned = System.Array.Empty<bool>();
        private readonly List<bool> _shownBuilt = new List<bool>();

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
            _model.SetConstants(gameConstants.GameConstants.TrapConstants);
            ResetProgress();
        }

        public override void SetView(TrapView view)
        {
            base.SetView(view);
            _model.Clear();
            _shownBuilt.Clear();
            view.ClearTraps();
            view.SetCallback(OnTick);
            view.Triggered += OnTrapTriggered;
            ResetProgress();
        }

        public void OnTerritoryUnlocked(TerritoryUnlockedMessage message)
        {
            if (message.TerritoryIndex >= 0 && message.TerritoryIndex < _unlocked.Length)
            {
                _unlocked[message.TerritoryIndex] = true;
            }
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            int territory = message.TerritoryIndex;
            if (territory < 0 || territory >= _unlocked.Length)
            {
                return;
            }

            _wallsBuilt[territory, message.SegmentIndex] = true;
            SpawnFoundationsIfReady(territory);
        }

        public void OnTrapBuilt(TrapBuiltMessage message)
        {
            TrapState trap = _model.Find(message.TerritoryIndex, message.GapIndex);
            if (trap == null)
            {
                // A build can arrive before the foundation messages; make the foundation first.
                SpawnFoundationsIfReady(message.TerritoryIndex);
                trap = _model.Find(message.TerritoryIndex, message.GapIndex);
                if (trap == null)
                {
                    return;
                }
            }

            if (!_model.TryBuild(trap.Index))
            {
                return;
            }

            _view?.SetBuilt(trap.Index, true);
            ShowBuilt(trap.Index, true);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();
            _shownBuilt.Clear();
            _view?.ClearTraps();
            ResetProgress();
        }

        private void SpawnFoundationsIfReady(int territoryIndex)
        {
            if (territoryIndex < 0 || territoryIndex >= _unlocked.Length)
            {
                return;
            }

            if (!_unlocked[territoryIndex] || _foundationsSpawned[territoryIndex])
            {
                return;
            }

            for (int segment = 0; segment < TerritoryState.WallSegmentCount; segment++)
            {
                if (!_wallsBuilt[territoryIndex, segment])
                {
                    return;
                }
            }

            _foundationsSpawned[territoryIndex] = true;

            // Four gaps per ring: the layout folds the eight wall halves into four openings.
            for (int gap = 0; gap < TerritoryState.TowerCount; gap++)
            {
                Vector2 position = _layout.GetWallGap(territoryIndex, gap);
                int index = _model.AddFoundation(territoryIndex, gap, position);
                if (index < 0)
                {
                    continue;
                }

                _view?.SpawnTrap(index, position, false);
                ShowBuilt(index, false);
            }
        }

        private void OnTrapTriggered(int trapIndex, int monsterId, Vector2 position)
        {
            if (!_model.TryTrigger(trapIndex))
            {
                return;
            }

            TrapState trap = _model.Get(trapIndex);
            if (trap == null)
            {
                return;
            }

            Publish(new TrapTriggeredMessage(trapIndex, position));
            Publish(new RequestDamageMonsterMessage(monsterId, trap.Constants.Damage, position));
            Publish(new SlowFieldAddedMessage(
                position,
                trap.Constants.Radius,
                trap.Constants.SlowFactor,
                trap.Constants.SlowDurationSeconds));

            if (!trap.IsArmed)
            {
                _view?.SetBuilt(trapIndex, false);
                ShowBuilt(trapIndex, false);
            }
        }

        private void OnTick(float deltaSeconds)
        {
            _model.Tick(deltaSeconds);

            for (int i = 0; i < _model.Count; i++)
            {
                TrapState trap = _model.Get(i);
                if (trap != null && !trap.IsArmed && IsShownBuilt(i))
                {
                    _view?.SetBuilt(i, false);
                    ShowBuilt(i, false);
                }
            }
        }

        private bool IsShownBuilt(int index)
        {
            return index >= 0 && index < _shownBuilt.Count && _shownBuilt[index];
        }

        private void ShowBuilt(int index, bool built)
        {
            while (_shownBuilt.Count <= index)
            {
                _shownBuilt.Add(false);
            }

            _shownBuilt[index] = built;
        }

        private void ResetProgress()
        {
            _unlocked = new bool[_layout.RingCount];
            _wallsBuilt = new bool[_layout.RingCount, TerritoryState.WallSegmentCount];
            _foundationsSpawned = new bool[_layout.RingCount];
            _shownBuilt.Clear();

            if (_unlocked.Length > 0)
            {
                _unlocked[0] = true;
            }
        }
    }
}
