#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class SubContainerPrefabResourceBindingFinalizer : ProviderBindingFinalizer
    {
        readonly string _resourcePath;
        readonly object _subIdentifier;
        readonly GameObjectCreationParameters _gameObjectBindInfo;

        public SubContainerPrefabResourceBindingFinalizer(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectBindInfo,
            string resourcePath,
            object subIdentifier)
            : base(bindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _subIdentifier = subIdentifier;
            _resourcePath = resourcePath;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            if (BindInfo.ToChoice == ToChoices.Self)
            {
                Assert.IsEmpty(BindInfo.ToTypes);
                FinalizeBindingSelf(container);
            }
            else
            {
                FinalizeBindingConcrete(container, BindInfo.ToTypes);
            }
        }

        void FinalizeBindingConcrete(DiContainer container, List<Type> concreteTypes)
        {
            var scope = GetScope();

            switch (scope)
            {
                case ScopeTypes.Transient:
                {
                    RegisterProvidersForAllContractsPerConcreteType(
                        container,
                        concreteTypes,
                        (_, concreteType) => new SubContainerDependencyProvider(
                            concreteType, _subIdentifier,
                            new SubContainerCreatorByNewPrefab(
                                container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo)));
                    break;
                }
                case ScopeTypes.Singleton:
                {
                    var containerCreator = new SubContainerCreatorCached(
                        new SubContainerCreatorByNewPrefab(container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo));

                    RegisterProvidersForAllContractsPerConcreteType(
                        container,
                        concreteTypes,
                        (_, concreteType) =>
                        new SubContainerDependencyProvider(
                            concreteType, _subIdentifier, containerCreator));
                    break;
                }
                default:
                {
                    throw Assert.CreateException();
                }
            }
        }

        void FinalizeBindingSelf(DiContainer container)
        {
            var scope = GetScope();

            switch (scope)
            {
                case ScopeTypes.Transient:
                {
                    RegisterProviderPerContract(
                        container,
                        (_, contractType) => new SubContainerDependencyProvider(
                            contractType, _subIdentifier,
                            new SubContainerCreatorByNewPrefab(
                                container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo)));
                    break;
                }
                case ScopeTypes.Singleton:
                {
                    var containerCreator = new SubContainerCreatorCached(
                        new SubContainerCreatorByNewPrefab(
                            container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo));

                    RegisterProviderPerContract(
                        container,
                        (_, contractType) =>
                        new SubContainerDependencyProvider(
                            contractType, _subIdentifier, containerCreator));
                    break;
                }
                default:
                {
                    throw Assert.CreateException();
                }
            }
        }
    }
}

#endif
