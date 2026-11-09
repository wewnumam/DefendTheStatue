using Agate.MVC.Base;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;
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

        private UnityAction _onMainMenu;

        public void MainMenu()
        {
            _onMainMenu?.Invoke();
        }

        public void SetCallbacks(UnityAction onMainMenu)
        {
            _onMainMenu = onMainMenu;
        }
    }
}