using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Monster;
using ProjectTA.Module.PlayerWeapon;
using UnityEngine;

namespace ProjectTA.Module.Bullet
{
    public class BulletController : ObjectController<BulletController, BulletView>
    {
        public void Init(BulletView view, float launchForce, float destroyDelay)
        {
            SetView(view);
            view.SetLaunchForce(launchForce);
            view.SetDestroyDelay(destroyDelay);
        }

        public override void SetView(BulletView view)
        {
            base.SetView(view);
            view.SetCallback(OnCollideWithCollider);
            view.SetMonsterHitCallback(OnHitMonster);
            view.SetTreeHitCallback(OnHitTree);
            view.SetBossHitCallback(OnHitBoss);
        }

        private void OnCollideWithCollider()
        {
            Publish(new DespawnBulletMessage(_view.gameObject));
        }

        /// <summary>
        /// Arrow damage leaves as a message, so the bullet module never touches the monster model.
        /// </summary>
        private void OnHitMonster(int monsterId, Vector2 hitPosition)
        {
            Publish(new RequestDamageMonsterMessage(monsterId, _view.Damage, hitPosition));
        }

        /// <summary>An arrow stuck in a tree chops it; the tree module owns the health.</summary>
        private void OnHitTree(int treeId, Vector2 hitPosition)
        {
            Publish(new RequestTreeDamageMessage(treeId, _view.Damage, hitPosition));
        }

        private void OnHitBoss(Vector2 hitPosition)
        {
            Publish(new RequestDamageBossMessage(_view.Damage, hitPosition));
        }
    }
}