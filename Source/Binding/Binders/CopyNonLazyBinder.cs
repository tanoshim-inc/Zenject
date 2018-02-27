using ModestTree;
namespace Zenject
{
    public class CopyNonLazyBinder : NonLazyBinder
    {
        public CopyNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public NonLazyBinder CopyIntoAllSubContainers()
        {
            BindInfo.BindingInheritanceMethod = BindingInheritanceMethods.CopyIntoAll;
            return this;
        }

        // Only copy the binding into children and not grandchildren
        public NonLazyBinder CopyIntoDirectSubContainers()
        {
            BindInfo.BindingInheritanceMethod = BindingInheritanceMethods.CopyDirectOnly;
            return this;
        }

        // Do not apply the binding on the current container
        public NonLazyBinder MoveIntoAllSubContainers()
        {
            BindInfo.BindingInheritanceMethod = BindingInheritanceMethods.MoveIntoAll;
            return this;
        }

        // Do not apply the binding on the current container
        public NonLazyBinder MoveIntoDirectSubContainers()
        {
            BindInfo.BindingInheritanceMethod = BindingInheritanceMethods.MoveDirectOnly;
            return this;
        }
    }
}
