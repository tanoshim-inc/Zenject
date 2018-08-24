using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentArgConditionCopyNonLazyBinder : ArgConditionCopyNonLazyBinder
    {
        SubContainerCreatorBindInfo _subContainerBindInfo;

        public DefaultParentArgConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            _subContainerBindInfo = subContainerBindInfo;
        }

        public ArgConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            _subContainerBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}

