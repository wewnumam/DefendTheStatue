using UnityEngine;

namespace ProjectTA.Message
{
    public struct MovePlayerCharacterMessage
    {
        public Vector2 Direction { get; }

        public MovePlayerCharacterMessage(Vector2 direction)
        {
            Direction = direction;
        }
    }
}