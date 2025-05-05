#if UNITY_2018_1_OR_NEWER
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace AllianceGamesSdk.Unity
{
    public static class PreserveLinker
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreserveTypes()
        {
            // This method will be called by Unity and ensures the assembly is linked
            var types = typeof(Common.PreserveAttribute).Assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(Common.PreserveAttribute), true).Length > 0);
        }
    }
}
#endif