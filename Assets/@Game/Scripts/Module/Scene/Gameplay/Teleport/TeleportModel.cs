using System.Collections.Generic;
using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.Teleport
{
    /// <summary>One of the three ring pods. A pod is a build site until the player pays for it.</summary>
    public class TeleportPodState
    {
        public TeleportPodState(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }

        public int Index { get; }
        public Vector2 Position { get; }
        public bool Built { get; set; }
    }

    /// <summary>
    /// GDD §11 teleport network: one pod per territory, unlocked at statue level 8, 20 essence to build
    /// and 10 per trip. Select mode is a two-step pick (source pod, then destination).
    /// </summary>
    public class TeleportModel : BaseModel
    {
        /// <summary>GDD §5: pods arrive with statue level 8.</summary>
        public const int UnlockStatueLevel = 8;

        /// <summary>GDD §12 prices.</summary>
        public const int BuildCost = 20;
        public const int TeleportCost = 10;

        private readonly List<TeleportPodState> _pods = new List<TeleportPodState>();
        private int _statueLevel = 1;

        public IReadOnlyList<TeleportPodState> Pods => _pods;
        public int Count => _pods.Count;
        public bool IsUnlocked => _statueLevel >= UnlockStatueLevel;

        /// <summary>Pod the player is picking a destination from, or -1.</summary>
        public int PendingFromIndex { get; private set; } = -1;

        public TeleportPodState Get(int index)
        {
            if (index < 0 || index >= _pods.Count)
            {
                return null;
            }

            return _pods[index];
        }

        public void SetStatueLevel(int level)
        {
            _statueLevel = level;
            SetDataAsDirty();
        }

        /// <summary>Lays out the pod sites. Called once per run from the controller.</summary>
        public void AddPod(int index, Vector2 position)
        {
            if (Get(index) != null)
            {
                return;
            }

            _pods.Add(new TeleportPodState(index, position));
            SetDataAsDirty();
        }

        public bool CanBuild(int index)
        {
            TeleportPodState pod = Get(index);
            return IsUnlocked && pod != null && !pod.Built;
        }

        public bool Build(int index)
        {
            if (!CanBuild(index))
            {
                return false;
            }

            _pods[index].Built = true;
            SetDataAsDirty();
            return true;
        }

        public bool CanTeleport(int fromIndex, int toIndex)
        {
            if (!IsUnlocked || fromIndex == toIndex)
            {
                return false;
            }

            TeleportPodState from = Get(fromIndex);
            TeleportPodState to = Get(toIndex);
            return from != null && to != null && from.Built && to.Built;
        }

        public void BeginSelect(int fromIndex)
        {
            PendingFromIndex = fromIndex;
            SetDataAsDirty();
        }

        public void CancelSelect()
        {
            PendingFromIndex = -1;
            SetDataAsDirty();
        }

        /// <summary>True while any pod is built, so the HUD can tell the player the network exists.</summary>
        public bool AnyBuilt
        {
            get
            {
                for (int i = 0; i < _pods.Count; i++)
                {
                    if (_pods[i].Built)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Reset()
        {
            _pods.Clear();
            PendingFromIndex = -1;
            _statueLevel = 1;
            SetDataAsDirty();
        }
    }
}
