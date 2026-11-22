using UnityEngine;

namespace ProjectTA.Message
{
    /// <summary>GDD §11: a pod is built into one of the three ring slots (20 essence).</summary>
    public struct TeleportPodBuiltMessage
    {
        public TeleportPodBuiltMessage(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }

        public int Index { get; }
        public Vector2 Position { get; }
    }

    /// <summary>The player asked to build the pod he is standing on.</summary>
    public struct RequestBuildTeleportPodMessage
    {
        public RequestBuildTeleportPodMessage(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }

    /// <summary>The player asked to travel from one built pod to another (10 essence).</summary>
    public struct RequestTeleportMessage
    {
        public RequestTeleportMessage(int fromIndex, int toIndex)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }

        public int FromIndex { get; }
        public int ToIndex { get; }
    }

    /// <summary>GDD §11: pick-a-destination mode. The camera pulls out while it is on.</summary>
    public struct TeleportSelectModeMessage
    {
        public TeleportSelectModeMessage(bool active)
        {
            Active = active;
        }

        public bool Active { get; }
    }

    /// <summary>Where the pawn lands. The player module owns the body, so it moves it.</summary>
    public struct TeleportPerformedMessage
    {
        public TeleportPerformedMessage(Vector2 destination)
        {
            Destination = destination;
        }

        public Vector2 Destination { get; }
    }
}
