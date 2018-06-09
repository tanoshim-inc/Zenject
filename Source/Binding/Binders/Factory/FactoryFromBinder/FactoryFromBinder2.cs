using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class FactoryFromBinder<TParam1, TParam2, TContract> : FactoryFromBinderBase
    {
        public FactoryFromBinder(
            DiContainer container, BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(container, typeof(TContract), bindInfo, factoryBindInfo)
        {
        }

        public ArgConditionCopyNonLazyBinder FromPoolableMemoryPool<TContractAgain>(
            Action<MemoryPoolInitialSizeMaxSizeBinder<TContractAgain>> poolBindGenerator)
            // Unfortunately we have to pass the same contract in again to satisfy the generic
            // constraints on PoolableMemoryPool below
            where TContractAgain : IPoolable<TParam1, TParam2, IMemoryPool>
        {
            Assert.IsEqual(typeof(TContractAgain), typeof(TContract));

            // Use a random ID so that our provider is the only one that can find it and so it doesn't
            // conflict with anything else
            var poolId = Guid.NewGuid();

            var binder = BindContainer.BindMemoryPoolCustomInterface<TContractAgain, PoolableMemoryPool<TParam1, TParam2, IMemoryPool, TContractAgain>, PoolableMemoryPool<TParam1, TParam2, IMemoryPool, TContractAgain>>(
                false,
                // Very important here that we call StartBinding with false otherwise the other
                // binding will be finalized early
                BindContainer.StartBinding(null, false))
                .WithId(poolId);

            poolBindGenerator(binder);

            ProviderFunc =
                (container) => { return new PoolableMemoryPoolProvider<TParam1, TParam2, TContractAgain>(container, poolId); };

            return new ArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ConditionCopyNonLazyBinder FromMethod(Func<DiContainer, TParam1, TParam2, TContract> method)
        {
            ProviderFunc =
                (container) => new MethodProviderWithContainer<TParam1, TParam2, TContract>(method);

            return this;
        }

        // Shortcut for FromIFactory and also for backwards compatibility
        public ConditionCopyNonLazyBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TParam1, TParam2, TContract>
        {
            return FromIFactory(x => x.To<TSubFactory>().AsCached());
        }

        public ArgConditionCopyNonLazyBinder FromIFactory(
            Action<ConcreteBinderGeneric<IFactory<TParam1, TParam2, TContract>>> factoryBindGenerator)
        {
            Guid factoryId;
            factoryBindGenerator(
                CreateIFactoryBinder<IFactory<TParam1, TParam2, TContract>>(out factoryId));

            ProviderFunc =
                (container) => { return new IFactoryProvider<TParam1, TParam2, TContract>(container, factoryId); };

            return new ArgConditionCopyNonLazyBinder(BindInfo);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TParam1, TParam2, TContract>(
                BindContainer, BindInfo, FactoryBindInfo, subIdentifier);
        }
    }
}
