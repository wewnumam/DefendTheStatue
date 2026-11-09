using Agate.MVC.Base;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectTA.Module.Settings
{
    public class SettingsView : BaseView
    {
        [field: SerializeField]
        public Toggle SfxToggle { get; set; }
        [field: SerializeField]
        public Toggle BgmToggle { get; set; }
        [field: SerializeField]
        public AudioMixer AudioMixer { get; set; }

        public void SetCallbacks(UnityAction<bool> sfx, UnityAction<bool> bgm)
        {
            SfxToggle.onValueChanged.RemoveAllListeners();
            SfxToggle.onValueChanged.AddListener(sfx);

            BgmToggle.onValueChanged.RemoveAllListeners();
            BgmToggle.onValueChanged.AddListener(bgm);
        }
    }
}