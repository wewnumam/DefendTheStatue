using UnityEngine;

namespace ProjectTA.Message
{
    public struct DespawnBulletMessage
    {
        public GameObject Bullet { get; }

        public DespawnBulletMessage(GameObject bullet)
        {
            Bullet = bullet;
        }
    }
}
