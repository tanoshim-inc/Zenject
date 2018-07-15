using ModestTree.Util;
#if !NOT_UNITY3D
using JetBrains.Annotations;
#endif

namespace Zenject
{
#if !NOT_UNITY3D
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
#endif
    public abstract class InjectAttributeBase : PreserveAttribute
    {
        public bool Optional
        {
            get;
            set;
        }

        public object Id
        {
            get;
            set;
        }

        public InjectSources Source
        {
            get;
            set;
        }
    }
}
