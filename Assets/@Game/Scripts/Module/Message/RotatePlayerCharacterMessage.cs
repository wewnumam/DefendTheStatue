using UnityEngine;

namespace ProjectTA.Message
{
    public struct RotatePlayerCharacterMessage
    {
        public Vector2 Aim { get; }

        public RotatePlayerCharacterMessage(Vector2 direction)
        {
            Aim = direction;
        }
    }
}