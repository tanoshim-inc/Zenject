using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FactoryFromBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>
        : FactoryFromBinderBase
    {
        public FactoryFromBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(typeof(TContract), bindInfo, factoryBindInfo)
        {
        }

        public ConditionCopyNonLazyBinder FromMethod(ModestTree.Util.Func<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract> method)
        {
            ProviderFunc =
                (container) => new MethodProviderWithContainer<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>(method);

            return this;
        }

        public ConditionCopyNonLazyBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>
        {
            ProviderFunc =
                (container) => new FactoryProvider<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract, TSubFactory>(container, new List<TypeValuePair>());

            return this;
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve()
        {
            return FromIFactoryResolve(null);
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve(object subIdentifier)
        {
            ProviderFunc =
                (container) => new IFactoryResolveProvider<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>(container, subIdentifier);

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>(
                BindInfo, FactoryBindInfo, subIdentifier);
        }
    }
}
