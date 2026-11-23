using UnityEngine;

namespace ProjectTA.Module.Boss
{
    /// <summary>
    /// Marker on the boss collider. Arrows check for it before the generic monster hitbox, so a boss
    /// hit never routes into the monster pool.
    /// </summary>
    public class BossHitbox : MonoBehaviour
    {
    }
}
