using Agate.MVC.Base;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectTA.Module.Tutorial
{
    public class TutorialView : BaseView
    {
        [field: SerializeField]
        public Image ImageComponent {  get; set; }
        [field: SerializeField]
        public TMP_Text TitleText { get; set; }
        [field: SerializeField]
        public TMP_Text DescriptionText { get; set; }
        [field: SerializeField]
        public List<TutorialItem> TutorialItems { get; set; }

        private UnityAction _onNext, _onPrevious;

        public void SetCallbacks(UnityAction onNext, UnityAction onPrevious)
        {
            _onNext = onNext;
            _onPrevious = onPrevious;
        }

        public void Next()
        {
            _onNext?.Invoke();
        }

        public void Previous()
        {
            _onPrevious?.Invoke();
        }
    }
}