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
            Log.Info(ProfileTimers.FormatResults());
        }
#endif
    }
}

#endif
