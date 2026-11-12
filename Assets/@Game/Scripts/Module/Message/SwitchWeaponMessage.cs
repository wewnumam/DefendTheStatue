using UnityEngine;

namespace ProjectTA.Message
{
    public struct SwitchWeaponMessage
    {
        public float Scroll { get; }

        public SwitchWeaponMessage(float scroll)
        {
            Scroll = scroll;
        }
    }
}
