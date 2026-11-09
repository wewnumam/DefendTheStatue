using Agate.MVC.Base;
using Agate.MVC.Core;
using ProjectTA.Boot;
using ProjectTA.Message;
using ProjectTA.Module.SaveSystem;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;
using ProjectTA.Utility;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ProjectTA.Scene.Gameplay
{
    public class GameplayLauncherLauncher : SceneLauncher<GameplayLauncherLauncher, GameplayView>
    {
        public override string SceneName { get { return "Gameplay"; } }

        private readonly GameSettingsController _gameSettings = new();
        private readonly SettingsController _settings = new();
        private readonly TutorialController _tutorial = new();

        protected override IController[] GetSceneDependencies()
        {
            return new IController[] {
                new SettingsController(),
                new TutorialController(),
            };
        }

        protected override IConnector[] GetSceneConnectors()
        {
            return new IConnector[] {
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