using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentScopeConditionCopyNonLazyBinder : ScopeConditionCopyNonLazyBinder
    {
        SubContainerCreatorBindInfo _subContainerBindInfo;

        public DefaultParentScopeConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            _subContainerBindInfo = subContainerBindInfo;
        }

        public ScopeConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            _subContainerBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}
