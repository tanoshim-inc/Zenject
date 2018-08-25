using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentConditionCopyNonLazyBinder : ConditionCopyNonLazyBinder
    {
        public DefaultParentConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            SubContainerCreatorBindInfo = subContainerBindInfo;
        }

        protected SubContainerCreatorBindInfo SubContainerCreatorBindInfo
        {
            get; private set;
        }

        public ConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            SubContainerCreatorBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}


