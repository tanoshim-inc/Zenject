using System;

namespace Zenject
{
    public class FactoryFromBinderUntyped : FactoryFromBinderBase
    {
        public FactoryFromBinderUntyped(
            Type contractType, BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(contractType, bindInfo, factoryBindInfo)
        {
        }
    }
}
