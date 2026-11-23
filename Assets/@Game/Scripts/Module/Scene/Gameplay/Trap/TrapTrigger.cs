using System;
using ProjectTA.Module.Monster;
using UnityEngine;

namespace ProjectTA.Module.Trap
{
    /// <summary>
    /// The trigger collider on a spike trap. Reports which trap caught which monster; the controller
    /// decides whether the trap is armed and off cooldown.
    /// </summary>
    public class TrapTrigger : MonoBehaviour
    {
        public int TrapIndex { get; set; } = -1;

        public event Action<int, int, Vector2> Triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            MonsterHitbox hitbox = other.GetComponentInParent<MonsterHitbox>();
            if (hitbox == null || hitbox.MonsterId < 0)
            {
                return;
            }

            Triggered?.Invoke(TrapIndex, hitbox.MonsterId, other.ClosestPoint(transform.position));
        }
    }
}
