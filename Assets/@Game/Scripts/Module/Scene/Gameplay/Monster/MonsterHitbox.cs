using UnityEngine;

namespace ProjectTA.Module.Monster
{
    /// <summary>
    /// Carries a monster's stable id on its collider, so a hit can be resolved by whoever collided
    /// with it even though pooled monster controllers are never registered in the container.
    /// </summary>
    public class MonsterHitbox : MonoBehaviour
    {
        public int MonsterId { get; set; } = -1;
    }
}
