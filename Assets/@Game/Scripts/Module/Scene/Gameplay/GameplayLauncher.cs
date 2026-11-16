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
using ProjectTA.Module.PlayerSkill;
using ProjectTA.Module.Hud;
using ProjectTA.Module.SlowField;
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
        private readonly PlayerSkillController _playerSkill = new();
        private readonly HudController _hud = new();
        private readonly SlowFieldController _slowField = new();

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
                new TowerController(),
                new PlayerSkillController(),
                new HudController(),
                new SlowFieldController()
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
                new TowerConnector(),
                new PlayerSkillConnector(),
                new HudConnector(),
                new SlowFieldConnector()
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

            _gameFlow.InitModel(_gameConstants.Model);
            _gameFlow.SetView(_view.GameFlowView);
            _monsterPool.InitModel(_gameConstants.Model);
            _monsterPool.SetView(_view.MonsterPoolView);

            _territory.InitModel(_gameConstants.Model);
            _territory.SetView(_view.TerritoryView);
            _statue.InitModel(_gameConstants.Model);
            _statue.SetView(_view.StatueView);
            _build.InitModel(_gameConstants.Model);
            _build.SetView(_view.BuildView);
            _tower.InitModel(_gameConstants.Model);
            _tower.SetView(_view.TowerView);
            _playerSkill.InitModel(_gameConstants.Model);
            _playerSkill.SetView(_view.PlayerSkillView);
            _playerSkill.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _hud.SetView(_view.HudView);
            _slowField.SetView(_view.SlowFieldView);

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
