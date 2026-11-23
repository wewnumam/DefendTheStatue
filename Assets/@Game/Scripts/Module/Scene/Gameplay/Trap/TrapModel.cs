using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.Trap
{
    /// <summary>
    /// One spike trap. Foundations appear at the four wall gaps once a ring's walls are finished; a
    /// built trap holds a durability pool and only fires once per cooldown.
    /// </summary>
    public class TrapState
    {
        public TrapState(int index, int territoryIndex, int gapIndex, Vector2 position, TrapConstants constants)
        {
            Index = index;
            TerritoryIndex = territoryIndex;
            GapIndex = gapIndex;
            Position = position;
            Constants = constants;
        }

        public int Index { get; }
        public int TerritoryIndex { get; }
        public int GapIndex { get; }
        public Vector2 Position { get; }
        public TrapConstants Constants { get; }

        public bool Built { get; set; }
        public int Durability { get; set; }
        public float CooldownRemaining { get; set; }

        public bool IsArmed => Built && Durability > 0;
    }

    /// <summary>
    /// Every spike trap on the map. Rules only: the reference game's trap cooldown and durability
    /// decrement live here so the controller just relays messages.
    /// </summary>
    public class TrapModel : BaseModel
    {
        private readonly List<TrapState> _traps = new List<TrapState>();

        /// <summary>Defaults match the reference config; tests need no asset.</summary>
        private TrapConstants _constants = new TrapConstants();

        public TrapConstants Constants => _constants;

        public void SetConstants(TrapConstants constants)
        {
            if (constants != null)
            {
                _constants = constants;
            }

            SetDataAsDirty();
        }

        public IReadOnlyList<TrapState> Traps => _traps;
        public int Count => _traps.Count;

        public TrapState Get(int index)
        {
            if (index < 0 || index >= _traps.Count)
            {
                return null;
            }

            return _traps[index];
        }

        public TrapState Find(int territoryIndex, int gapIndex)
        {
            for (int i = 0; i < _traps.Count; i++)
            {
                if (_traps[i].TerritoryIndex == territoryIndex && _traps[i].GapIndex == gapIndex)
                {
                    return _traps[i];
                }
            }

            return null;
        }

        /// <summary>Returns the new trap's index, or -1 when that gap already has a foundation.</summary>
        public int AddFoundation(int territoryIndex, int gapIndex, Vector2 position)
        {
            if (Find(territoryIndex, gapIndex) != null)
            {
                return -1;
            }

            int index = _traps.Count;
            _traps.Add(new TrapState(index, territoryIndex, gapIndex, position, _constants));
            SetDataAsDirty();
            return index;
        }

        /// <summary>Reference building a trap: arms it with a full durability pool.</summary>
        public bool TryBuild(int index)
        {
            TrapState trap = Get(index);
            if (trap == null || trap.Built)
            {
                return false;
            }

            trap.Built = true;
            trap.Durability = _constants.MaxDurability;
            trap.CooldownRemaining = 0f;
            SetDataAsDirty();
            return true;
        }

        /// <summary>
        /// Reference trigger: a monster inside the radius fires the trap once per cooldown and burns
        /// one durability. Returns true when the hit should land.
        /// </summary>
        public bool TryTrigger(int index)
        {
            TrapState trap = Get(index);
            if (trap == null || !trap.IsArmed || trap.CooldownRemaining > 0f)
            {
                return false;
            }

            trap.Durability--;
            trap.CooldownRemaining = _constants.CooldownSeconds;

            if (trap.Durability <= 0)
            {
                trap.Durability = 0;
                trap.Built = false;
            }

            SetDataAsDirty();
            return true;
        }

        /// <summary>Reference cooldown timer: ticks every frame, never negative.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            for (int i = 0; i < _traps.Count; i++)
            {
                TrapState trap = _traps[i];
                if (trap.CooldownRemaining > 0f)
                {
                    trap.CooldownRemaining = Mathf.Max(0f, trap.CooldownRemaining - deltaSeconds);
                }
            }
        }

        public void Clear()
        {
            _traps.Clear();
            SetDataAsDirty();
        }
    }
}
