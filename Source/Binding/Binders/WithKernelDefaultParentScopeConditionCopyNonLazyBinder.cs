using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class WithKernelDefaultParentScopeConditionCopyNonLazyBinder : DefaultParentScopeConditionCopyNonLazyBinder
    {
        public WithKernelDefaultParentScopeConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(subContainerBindInfo, bindInfo)
        {
        }

        public DefaultParentScopeConditionCopyNonLazyBinder WithKernel()
        {
            SubContainerCreatorBindInfo.CreateKernel = true;
            return this;
        }
    }
}
