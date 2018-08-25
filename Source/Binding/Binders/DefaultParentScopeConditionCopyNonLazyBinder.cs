using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentScopeConditionCopyNonLazyBinder : ScopeConditionCopyNonLazyBinder
    {
        public DefaultParentScopeConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            SubContainerCreatorBindInfo = subContainerBindInfo;
        }

        protected SubContainerCreatorBindInfo SubContainerCreatorBindInfo
        {
            get; private set;
        }

        public ScopeConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            SubContainerCreatorBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}
