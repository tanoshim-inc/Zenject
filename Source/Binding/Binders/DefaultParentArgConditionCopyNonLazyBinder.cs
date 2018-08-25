using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentArgConditionCopyNonLazyBinder : ArgConditionCopyNonLazyBinder
    {
        public DefaultParentArgConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            SubContainerCreatorBindInfo = subContainerBindInfo;
        }

        protected SubContainerCreatorBindInfo SubContainerCreatorBindInfo
        {
            get; private set;
        }

        public ArgConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            SubContainerCreatorBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}

