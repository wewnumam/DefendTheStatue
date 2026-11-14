using System.Reflection;
using Agate.MVC.Base;
using Agate.MVC.Core;
using UnityEngine;

namespace ProjectTA.Boot
{
    /// <summary>
    /// Agate keeps its container, its bus and its context in static singletons, and a launcher only
    /// empties them when it is unloaded. This project runs with domain reload disabled
    /// (<c>m_EnterPlayModeOptions: DisableDomainReload</c>), so leaving Play mode destroys the game
    /// objects but keeps those statics. On the next run the scene's controllers silently lose
    /// Agate's first-registration-wins race, the launcher injects the dead run's controllers instead of the
    /// fresh ones, and the first message publish reaches views that were destroyed with the last
    /// session — <c>MissingReferenceException</c> from the middle of a controller that looks innocent.
    /// Clearing the singletons at runtime start gives every Play session a clean container.
    /// </summary>
    public static class AgateStaticState
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            ClearInstance(typeof(Context));
            ClearInstance(typeof(DependencyInjection));
            ClearInstance(typeof(PublishSubscribe));
        }

        private static void ClearInstance(System.Type type)
        {
            FieldInfo instance = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);

            if (instance != null)
            {
                instance.SetValue(null, null);
            }
        }
    }
}
