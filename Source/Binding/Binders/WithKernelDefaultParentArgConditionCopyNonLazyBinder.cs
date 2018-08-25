using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class WithKernelDefaultParentArgConditionCopyNonLazyBinder : DefaultParentArgConditionCopyNonLazyBinder
    {
        public WithKernelDefaultParentArgConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(subContainerBindInfo, bindInfo)
        {
        }

        public DefaultParentArgConditionCopyNonLazyBinder WithKernel()
        {
            SubContainerCreatorBindInfo.CreateKernel = true;
            return this;
        }
    }
}


