using UnityEngine;

namespace ProjectTA.Utility
{
    public interface IResourceLoader
    {
        T Load<T>(string path) where T : Object;
    }

    public class ResourceLoader : IResourceLoader
    {
        public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
    }
}
