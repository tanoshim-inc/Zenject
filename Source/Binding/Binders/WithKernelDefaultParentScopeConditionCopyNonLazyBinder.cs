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

        // This would be used in cases where you want to control the execution order for the 
        // subcontainer
        public DefaultParentScopeConditionCopyNonLazyBinder WithKernel<TKernel>()
            where TKernel : Kernel
        {
            SubContainerCreatorBindInfo.CreateKernel = true;
            SubContainerCreatorBindInfo.KernelType = typeof(TKernel);
            return this;
        }
    }
}
