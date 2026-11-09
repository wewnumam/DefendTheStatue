using Agate.MVC.Base;
using Agate.MVC.Core;
using ProjectTA.Boot;
using ProjectTA.Utility;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using ProjectTA.Message;
using ProjectTA.Module.SaveSystem;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;

namespace ProjectTA.Scene.MainMenu
{
    public class MainMenuLauncherLauncher : SceneLauncher<MainMenuLauncherLauncher, MainMenuView>
    {
        public override string SceneName { get { return "MainMenu"; } }

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
            Time.timeScale = 1;

            Publish(new GameStateMessage(EnumManager.GameState.PreGame));

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneName));

            PlayerPrefs.SetString(TagManager.KEY_VERSION, Application.version);

            _view.SetCallbacks(OnPlay, OnQuit);

            _tutorial.SetView(_view.TutorialView);

            _settings.InitModel(_gameSettings.Model);
            _settings.SetView(_view.SettingsView);

            yield return null;
        }

        protected override IEnumerator InitSceneObject()
        {
            yield return null;
        }

        private void OnPlay()
        {
            SceneLoader.Instance.LoadScene(TagManager.SCENE_GAMEPLAY);
        }

        private void OnQuit()
        {
            Publish(new AppQuitMessage());
            Application.Quit();
        }
    }
}