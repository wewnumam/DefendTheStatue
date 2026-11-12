using Agate.MVC.Base;
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

        public Rigidbody2D Rb => _rb;
        public float LaunchForce => _launchForce;

        public void SetCallback(UnityAction onCollideWithCollider)
        {
            _onCollideWithCollider = onCollideWithCollider;
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
            // 2D: the aim axis is transform.right, transform.forward is the depth (Z) axis
            Rb.linearVelocity = Vector2.zero;
            Rb.AddForce(transform.right * LaunchForce, ForceMode2D.Impulse);

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
