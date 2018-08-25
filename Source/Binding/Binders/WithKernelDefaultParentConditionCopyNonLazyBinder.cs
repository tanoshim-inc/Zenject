using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class WithKernelDefaultParentConditionCopyNonLazyBinder : DefaultParentConditionCopyNonLazyBinder
    {
        public WithKernelDefaultParentConditionCopyNonLazyBinder(
            SubContainerCreatorBindInfo subContainerBindInfo, BindInfo bindInfo)
            : base(subContainerBindInfo, bindInfo)
        {
        }

        public DefaultParentConditionCopyNonLazyBinder WithKernel()
        {
            SubContainerCreatorBindInfo.CreateKernel = true;
            return this;
        }
    }
}

