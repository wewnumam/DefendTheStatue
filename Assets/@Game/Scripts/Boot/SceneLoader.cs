using Agate.MVC.Base;
using ProjectTA.Utility;

namespace ProjectTA.Boot
{
    public class SceneLoader : BaseLoader<SceneLoader>
    {
        protected override string SplashScene { get { return TagManager.SCENE_SPLASHSCREEN; } }
    }
}
