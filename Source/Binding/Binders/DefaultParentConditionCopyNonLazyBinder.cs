using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class DefaultParentConditionCopyNonLazyBinder : ConditionCopyNonLazyBinder
    {
        SubContainerCreatorBindInfo _subContainerBindInfo;

        public DefaultParentConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            _subContainerBindInfo = subContainerBindInfo;
        }

        public ConditionCopyNonLazyBinder UsingDefaultGameObjectParent(string defaultParentName)
        {
            _subContainerBindInfo.DefaultParentName = defaultParentName;
            return this;
        }
    }
}


