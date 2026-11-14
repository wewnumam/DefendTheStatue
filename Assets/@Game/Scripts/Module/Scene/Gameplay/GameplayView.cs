using Agate.MVC.Base;
using ProjectTA.Module.GameFlow;
using ProjectTA.Module.Monster;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Module.Build;
using ProjectTA.Module.Tower;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;
using ProjectTA.Module.PlayerCharacter;
using ProjectTA.Module.BulletPool;
using ProjectTA.Module.Sword;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Scene.Gameplay
{
    public class GameplayView : BaseSceneView
    {
        [field: SerializeField]
        public TutorialView TutorialView { get; private set; }
        [field: SerializeField]
        public SettingsView SettingsView { get; private set; }
        [field: SerializeField]
        public PlayerCharacterView PlayerCharacterView { get; private set; }
        [field: SerializeField]
        public BulletPoolView BulletPoolView { get; private set; }
        [field: SerializeField]
        public SwordView SwordView { get; private set; }
        [field: SerializeField]
        public GameFlowView GameFlowView { get; private set; }
        [field: SerializeField]
        public MonsterPoolView MonsterPoolView { get; private set; }
        [field: SerializeField]
        public StatueView StatueView { get; private set; }
        [field: SerializeField]
        public TerritoryView TerritoryView { get; private set; }
        [field: SerializeField]
        public BuildView BuildView { get; private set; }
        [field: SerializeField]
        public TowerView TowerView { get; private set; }

        private UnityAction _onMainMenu;

        public void MainMenu()
        {
            _onMainMenu?.Invoke();
        }

        public void SetCallbacks(UnityAction onMainMenu)
        {
            _onMainMenu = onMainMenu;
        }

        /// <summary>
        /// Next step: a serialized reference found in the scene always wins. The fallback builds the
        /// host object at runtime so the slice runs in a scene that was never wired by hand; replace
        /// it once the scene has the real object.
        /// </summary>
        public GameFlowView EnsureGameFlowView()
        {
            if (GameFlowView == null)
            {
                var host = new GameObject("GameFlow");
                host.transform.SetParent(transform, false);
                GameFlowView = host.AddComponent<GameFlowView>();
            }

            return GameFlowView;
        }

        public MonsterPoolView EnsureMonsterPoolView()
        {
            if (MonsterPoolView == null)
            {
                var host = new GameObject("MonsterPool");
                host.transform.SetParent(transform, false);
                MonsterPoolView = host.AddComponent<MonsterPoolView>();
            }

            return MonsterPoolView;
        }

        public StatueView EnsureStatueView()
        {
            if (StatueView == null)
            {
                var host = new GameObject("Statue");
                host.transform.SetParent(transform, false);
                StatueView = host.AddComponent<StatueView>();
            }

            return StatueView;
        }

        public TerritoryView EnsureTerritoryView()
        {
            if (TerritoryView == null)
            {
                var host = new GameObject("Territory");
                host.transform.SetParent(transform, false);
                TerritoryView = host.AddComponent<TerritoryView>();
            }

            return TerritoryView;
        }

        public BuildView EnsureBuildView()
        {
            if (BuildView == null)
            {
                var host = new GameObject("Build");
                host.transform.SetParent(transform, false);
                BuildView = host.AddComponent<BuildView>();
            }

            return BuildView;
        }

        public TowerView EnsureTowerView()
        {
            if (TowerView == null)
            {
                var host = new GameObject("Tower");
                host.transform.SetParent(transform, false);
                TowerView = host.AddComponent<TowerView>();
            }

            return TowerView;
        }
    }
}
