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
using ProjectTA.Module.Boss;
using ProjectTA.Module.BalanceLog;
using ProjectTA.Module.CameraRig;
using ProjectTA.Module.DevConsole;
using ProjectTA.Module.GameFlow;
using ProjectTA.Module.Guard;
using ProjectTA.Module.GameStateUi;
using ProjectTA.Module.Monster;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Territory;
using ProjectTA.Module.Build;
using ProjectTA.Module.Tower;
using ProjectTA.Module.Trap;
using ProjectTA.Module.Tree;
using ProjectTA.Module.PlayerSkill;
using ProjectTA.Module.Hud;
using ProjectTA.Module.SlowField;
using ProjectTA.Module.Teleport;
using ProjectTA.Module.AdaptiveAi;
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
        private readonly TreeController _tree = new();
        private readonly TrapController _trap = new();
        private readonly GuardController _guard = new();
        private readonly BossController _boss = new();
        private readonly DevConsoleController _devConsole = new();
        private readonly CameraRigController _cameraRig = new();
        private readonly PlayerSkillController _playerSkill = new();
        private readonly HudController _hud = new();
        private readonly SlowFieldController _slowField = new();
        private readonly GameStateUiController _gameStateUi = new();
        private readonly TeleportController _teleport = new();
        private readonly AdaptiveAiController _adaptiveAi = new();

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
                new TreeController(),
                new TrapController(),
                new GuardController(),
                new BossController(),
                new DevConsoleController(),
                new CameraRigController(),
                new PlayerSkillController(),
                new HudController(),
                new SlowFieldController(),
                new GameStateUiController(),
                new TeleportController(),
                new AdaptiveAiController()
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
                new TreeConnector(),
                new TrapConnector(),
                new GuardConnector(),
                new BossConnector(),
                new BalanceLogConnector(),
                new CameraRigConnector(),
                new PlayerSkillConnector(),
                new HudConnector(),
                new SlowFieldConnector(),
                new GameStateUiConnector(),
                new TeleportConnector(),
                new AdaptiveAiConnector()
            };
        }

        protected override IEnumerator LaunchScene()
        {
            EnsureEventSystem();
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
            _build.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _tower.InitModel(_gameConstants.Model);
            _tower.SetView(_view.TowerView);
            _tree.InitModel(_gameConstants.Model);
            _tree.SetView(_view.TreeView);
            _trap.InitModel(_gameConstants.Model);
            _trap.SetView(_view.TrapView);
            _guard.InitModel(_gameConstants.Model);
            _guard.SetView(_view.GuardView);
            _boss.InitModel(_gameConstants.Model);
            _boss.SetView(_view.BossView);
            _devConsole.InitModel(_gameConstants.Model);
            _devConsole.SetView(_view.DevConsoleView);
            _cameraRig.SetView(_view.CameraRigView);
            _cameraRig.SetFollowTarget(_view.PlayerCharacterView.transform);
            _boss.SetStatuePosition(_view.GameFlowView.StatuePosition);
            _playerSkill.InitModel(_gameConstants.Model);
            _playerSkill.SetView(_view.PlayerSkillView);
            _playerSkill.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _monsterPool.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _teleport.InitModel(_gameConstants.Model);
            _teleport.SetView(_view.TeleportView);
            _teleport.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _adaptiveAi.SetPlayerTransform(_view.PlayerCharacterView.transform);
            _hud.InitModel(_gameConstants.Model);
            _hud.SetView(_view.HudView);
            _slowField.SetView(_view.SlowFieldView);
            _gameStateUi.SetSkillConstants(_gameConstants.Model.GameConstants.SkillConstants);
            _gameStateUi.SetView(_view.GameStateUiView);

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

        /// <summary>
        /// The Gameplay scene ships no EventSystem, so every runtime button (buy menu, Upgrade Statue)
        /// rendered but never got a click. Created before any panel is built; the UI input module
        /// assigns its own default actions on enable.
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                return;
            }

            new UnityEngine.GameObject(
                "EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }
    }
}
