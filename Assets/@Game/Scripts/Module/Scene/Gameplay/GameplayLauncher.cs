using Agate.MVC.Base;
using Agate.MVC.Core;
using ProjectTA.Boot;
using ProjectTA.Message;
using ProjectTA.Module.SaveSystem;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;
using ProjectTA.Utility;
using ProjectTA.Module.Input;
using ProjectTA.Module.PlayerCharacter;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.SaveSystem;
using ProjectTA.Module.BulletPool;
using ProjectTA.Module.Sword;
using ProjectTA.Module.GameFlow;
using ProjectTA.Module.Monster;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Module.Build;
using ProjectTA.Module.Tower;
using System.Collections;
using UnityEngine.SceneManagement;
namespace ProjectTA.Scene.Gameplay
{
    public class GameplayLauncherLauncher : SceneLauncher<GameplayLauncherLauncher, GameplayView>
    {
        public override string SceneName { get { return "Gameplay"; } }

        private readonly GameSettingsController _gameSettings = new();
        private readonly GameConstantsController _gameConstants = new();
        private readonly InputController _input = new();
        private readonly SettingsController _settings = new();
        private readonly TutorialController _tutorial = new();
        private readonly PlayerCharacterController _playerCharacter = new();
        private readonly BulletPoolController _bulletPool = new();
        private readonly SwordController _sword = new();
        private readonly GameFlowController _gameFlow = new();
        private readonly MonsterPoolController _monsterPool = new();
        private readonly StatueController _statue = new();
        private readonly TerritoryController _territory = new();
        private readonly BuildController _build = new();
        private readonly TowerController _tower = new();

        protected override IController[] GetSceneDependencies()
        {
            return new IController[] {
                new SettingsController(),
                new TutorialController(),
                new InputController(),
                new PlayerCharacterController(),
                new BulletPoolController(),
                new SwordController(),
                new GameFlowController(),
                new MonsterPoolController(),
                new StatueController(),
                new TerritoryController(),
                new BuildController(),
                new TowerController()
            };
        }

        protected override IConnector[] GetSceneConnectors()
        {
            return new IConnector[] {
                new PlayerCharacterConnector(),
                new BulletPoolConnector(),
                new SwordConnector(),
                new GameFlowConnector(),
                new MonsterPoolConnector(),
                new StatueConnector(),
                new TerritoryConnector(),
                new BuildConnector(),
                new TowerConnector()
            };
        }

        protected override IEnumerator LaunchScene()
        {
            Publish(new GameStateMessage(EnumManager.GameState.Playing));

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneName));

            _view.SetCallbacks(OnMainMenu);

            _tutorial.SetView(_view.TutorialView);
            if (_gameSettings.Model.SavedSettingsData.IsGameInductionActive)
            {
                _view.TutorialView.gameObject.SetActive(true);
                Publish(new ToggleGameInductionMessage(false));
            }

            _settings.InitModel(_gameSettings.Model);
            _settings.SetView(_view.SettingsView);

            _playerCharacter.InitModel(_gameConstants.Model, _gameSettings.Model);
            _playerCharacter.SetView(_view.PlayerCharacterView);

            _bulletPool.InitModel(_gameConstants.Model);
            _bulletPool.SetView(_view.BulletPoolView);

            _sword.SetView(_view.SwordView);

            _gameFlow.SetView(_view.EnsureGameFlowView());
            _monsterPool.SetView(_view.EnsureMonsterPoolView());

            _territory.SetView(_view.EnsureTerritoryView());
            _statue.SetView(_view.EnsureStatueView());
            _build.SetView(_view.EnsureBuildView());
            _tower.SetView(_view.EnsureTowerView());

            _input.SetReady();

            yield return null;
        }

        protected override IEnumerator InitSceneObject()
        {
            yield return null;
        }

        private void OnMainMenu()
        {
            SceneLoader.Instance.LoadScene(TagManager.SCENE_MAINMENU);
        }
    }
}
