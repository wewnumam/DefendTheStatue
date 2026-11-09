using Agate.MVC.Base;
using Agate.MVC.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ProjectTA.Boot
{
    public class SplashScreen : BaseSplash<SplashScreen>
    {
        [SerializeField] RectTransform splashScreenWindow;
        [SerializeField] TMP_Text versionText;

        protected override IMain GetMain()
        {
            return GameMain.Instance;
        }

        protected override ILoad GetLoader()
        {
            return SceneLoader.Instance;
        }

        protected override void StartSplash()
        {
            base.StartSplash();
            versionText.SetText($"v{Application.version}");
        }

        protected override void StartTransition()
        {
            base.StartTransition();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(splashScreenWindow.DOAnchorPosX(0, .2f));
            sequence.Play();
        }

        protected override void FinishTransition()
        {
            base.FinishTransition();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(splashScreenWindow.DOAnchorPosX(3000, 1f));
            sequence.Append(splashScreenWindow.DOAnchorPosX(-3000, 0f));
            sequence.Play();
        }
    }
}
