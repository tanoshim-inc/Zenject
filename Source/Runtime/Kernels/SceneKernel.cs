#if !NOT_UNITY3D

using System.Text;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class SceneKernel : MonoKernel
    {
        // Only needed to set "script execution order" in unity project settings

#if ZEN_INTERNAL_PROFILING
        public override void Start()
        {
            base.Start();

            var result = new StringBuilder();

            result.Append("Completed scene startup.  Profile info:");

            for (int i = 0; i < (int)InternalTimers.Count; i++)
            {
                var type = (InternalTimers)i;
                var time = ProfileTimers.GetTime(type);

                result.Append("\n    {0}: {1:0.0000} seconds".Fmt(type, time));
            }

            Log.Info(result.ToString());
        }
#endif
    }
}

#endif
