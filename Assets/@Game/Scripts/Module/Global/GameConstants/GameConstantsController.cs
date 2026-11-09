using Agate.MVC.Base;
using ProjectTA.Utility;
using System.Collections;
using UnityEngine;

namespace ProjectTA.Module.GameConstants
{
    public class GameConstantsController : DataController<GameConstantsController, GameConstantsModel, IGameConstantsModel>
    {
        #region UTILITY

        private IResourceLoader _resourceLoader = null;

        public void SetResourceLoader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void SetModel(GameConstantsModel model)
        {
            _model = model;
        }

        #endregion

        public override IEnumerator Initialize()
        {
            if (_resourceLoader == null)
            {
                _resourceLoader = new ResourceLoader();
            }

            SOGameConstants gameConstants = _resourceLoader.Load<SOGameConstants>(TagManager.SO_GAMECONSTANTS);
            if (gameConstants != null)
            {
                _model.SetGameConstants(gameConstants);
            }
            else
            {
                Debug.LogError("GAMECONSTANT SCRIPTABLE NOT FOUND!");
                Debug.LogWarning($"Add Assets/Resources/{TagManager.SO_GAMECONSTANTS} files");
            }

            yield return base.Initialize();
        }
    }
}
