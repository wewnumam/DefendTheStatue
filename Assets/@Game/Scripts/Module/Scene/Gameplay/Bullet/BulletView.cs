using Agate.MVC.Base;
using ProjectTA.Module.Boss;
using ProjectTA.Module.Monster;
using ProjectTA.Module.Tree;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Module.Bullet
{
    public class BulletView : BaseView
    {
        [SerializeField] private Rigidbody2D _rb;

        private float _launchForce;
        private float _destroyDelay;
        private bool _hasHitTarget;
        private UnityAction _onCollideWithCollider;
        private UnityAction<int, Vector2> _onHitMonster;
        private UnityAction<int, Vector2> _onHitTree;
        private UnityAction<Vector2> _onHitBoss;

        public Rigidbody2D Rb => _rb;
        public float LaunchForce => _launchForce;

        /// <summary>
        /// Damage this arrow carries. The pool rewrites it on every spawn, so clone mode and berserker
        /// scale the hit without the bullet module asking anyone about skills.
        /// </summary>
        public float Damage { get; set; } = ProjectTA.Module.PlayerWeapon.PlayerWeaponModel.ArrowDamage;

        public void SetCallback(UnityAction onCollideWithCollider)
        {
            _onCollideWithCollider = onCollideWithCollider;
        }

        /// <summary>
        /// Reports the monster the arrow buried itself in. The id comes off the collider, so a cheap
        /// area resolve is not needed to know what was hit.
        /// </summary>
        public void SetMonsterHitCallback(UnityAction<int, Vector2> onHitMonster)
        {
            _onHitMonster = onHitMonster;
        }

        public void SetTreeHitCallback(UnityAction<int, Vector2> onHitTree)
        {
            _onHitTree = onHitTree;
        }

        /// <summary>The boss is not a pooled monster, so its hit gets its own callback.</summary>
        public void SetBossHitCallback(UnityAction<Vector2> onHitBoss)
        {
            _onHitBoss = onHitBoss;
        }

        public void SetLaunchForce(float launchForce)
        {
            _launchForce = launchForce;
        }

        public void SetDestroyDelay(float destroyDelay)
        {
            _destroyDelay = destroyDelay;
        }

        private void OnEnable()
        {
            _hasHitTarget = false;
            Rb.isKinematic = false;
            // The rig faces down (local -Y), and the spawn point's rotation carries the aim, so the
            // arrow flies along its own -up. transform.right would send it sideways.
            Rb.linearVelocity = Vector2.zero;
            Rb.AddForce((Vector2)(-transform.up) * LaunchForce, ForceMode2D.Impulse);

            // Lifetime: a bullet that never hits anything must still return to the pool
            Invoke(nameof(OnCollideWithCollider), _destroyDelay);
        }

        private void OnDisable()
        {
            // pooled bullet: drop the despawn that was scheduled in its previous life
            CancelInvoke();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if the bullet has already hit something to prevent multiple hits
            if (_hasHitTarget) return;

            _hasHitTarget = true;

            // Stop the bullet from continuing to move
            Rb.linearVelocity = Vector2.zero;
            Rb.isKinematic = true;

            // Stick the bullet into the target
            transform.position = collision.GetContact(0).point;
            transform.SetParent(collision.transform, true);

            if (collision.collider.GetComponentInParent<BossHitbox>() != null)
            {
                _onHitBoss?.Invoke(transform.position);
            }

            MonsterHitbox hitbox = collision.collider.GetComponentInParent<MonsterHitbox>();
            if (hitbox != null && hitbox.MonsterId >= 0)
            {
                _onHitMonster?.Invoke(hitbox.MonsterId, transform.position);
            }

            TreeHitbox treeHitbox = collision.collider.GetComponentInParent<TreeHitbox>();
            if (treeHitbox != null && treeHitbox.TreeId >= 0)
            {
                _onHitTree?.Invoke(treeHitbox.TreeId, transform.position);
            }

            // Despawn the bullet after a delay
            CancelInvoke(nameof(OnCollideWithCollider));
            Invoke(nameof(OnCollideWithCollider), _destroyDelay);
        }

        private void OnCollideWithCollider()
        {
            _onCollideWithCollider?.Invoke();
        }
    }
}
