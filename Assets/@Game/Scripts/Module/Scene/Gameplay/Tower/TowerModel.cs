using System.Collections.Generic;
using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Tower
{
    /// <summary>One built tower. Reference game: range 300, 50-frame cadence, 15 damage.</summary>
    public class TowerState
    {
        public TowerState(Vector2 position, int territoryIndex, int cornerIndex)
        {
            Position = position;
            TerritoryIndex = territoryIndex;
            CornerIndex = cornerIndex;
        }

        public const float Size = 32f;
        public const float AttackRange = 300f;
        public const int AttackRateFrames = 50;
        public const float Damage = 15f;

        public Vector2 Position { get; }
        public int TerritoryIndex { get; }
        public int CornerIndex { get; }
        public float AttackRangeSquared => AttackRange * AttackRange;
        public float CooldownRemaining { get; set; }
        public float AimAngle { get; set; }

        public bool IsReadyToFire => CooldownRemaining <= 0f;

        public void Tick(float deltaSeconds)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaSeconds);
            }
        }

        /// <summary>Restarts the reference 50-frame cadence.</summary>
        public void StartCooldown()
        {
            CooldownRemaining = GameTime.FramesToSeconds(AttackRateFrames);
        }
    }

    public class TowerModel : BaseModel
    {
        private readonly List<TowerState> _towers = new List<TowerState>();

        public IReadOnlyList<TowerState> Towers => _towers;
        public int Count => _towers.Count;

        public TowerState Get(int index)
        {
            return index >= 0 && index < _towers.Count ? _towers[index] : null;
        }

        public void Add(Vector2 position, int territoryIndex, int cornerIndex)
        {
            _towers.Add(new TowerState(position, territoryIndex, cornerIndex));
            SetDataAsDirty();
        }

        public void TickCooldowns(float deltaSeconds)
        {
            for (int i = 0; i < _towers.Count; i++)
            {
                _towers[i].Tick(deltaSeconds);
            }
        }

        /// <summary>
        /// First tower that can hit the target: the reference game walks its tower list in order and
        /// fires every tower whose cooldown has run out.
        /// </summary>
        public int FindShooterFor(Vector2 targetPosition)
        {
            for (int i = 0; i < _towers.Count; i++)
            {
                TowerState tower = _towers[i];
                if (!tower.IsReadyToFire)
                {
                    continue;
                }

                if ((targetPosition - tower.Position).sqrMagnitude <= tower.AttackRangeSquared)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Fires when in range and off cooldown; returns the damage dealt, else 0.</summary>
        public float TryFire(int index, Vector2 targetPosition)
        {
            TowerState tower = Get(index);
            if (tower == null || !tower.IsReadyToFire)
            {
                return 0f;
            }

            Vector2 toTarget = targetPosition - tower.Position;
            if (toTarget.sqrMagnitude > tower.AttackRangeSquared)
            {
                return 0f;
            }

            tower.AimAngle = Mathf.Atan2(toTarget.y, toTarget.x);
            tower.StartCooldown();
            SetDataAsDirty();
            return TowerState.Damage;
        }

        public void Clear()
        {
            _towers.Clear();
            SetDataAsDirty();
        }
    }
}
