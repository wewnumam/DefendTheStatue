using Agate.MVC.Base;
using Agate.MVC.Core;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.SaveSystem;
using System.Collections;
using UnityEngine;

namespace ProjectTA.Boot
{
    public class GameMain : BaseMain<GameMain>, IMain
    {
        protected override IController[] GetDependencies()
        {
            return new IController[] {
                new GameSettingsController(),
                new GameConstantsController(),
            };
        }

        protected override IConnector[] GetConnectors()
        {
            return new IConnector[] {
                new GameSettingsConnector(),
            };
        }


        protected override IEnumerator StartInit()
        {
            Application.targetFrameRate = -1;

            yield return null;
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}
