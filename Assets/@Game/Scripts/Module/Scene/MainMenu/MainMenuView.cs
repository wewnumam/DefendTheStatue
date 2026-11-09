using Agate.MVC.Base;
using ProjectTA.Module.Settings;
using ProjectTA.Module.Tutorial;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Scene.MainMenu
{
    public class MainMenuView : BaseSceneView
    {
        [field: SerializeField]
        public TutorialView TutorialView { get; private set; }
        [field: SerializeField]
        public SettingsView SettingsView { get; private set; }

        private UnityAction _onPlay, _onQuit;

        public void Play()
        {
            _onPlay?.Invoke();
        }

        public void Quit()
        {
            _onQuit?.Invoke();
        }

        public void SetCallbacks(UnityAction onPlay, UnityAction onQuit)
        {
            _onPlay = onPlay;
            _onQuit = onQuit;
        }
    }
}