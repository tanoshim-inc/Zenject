using System;
using System.Collections.Generic;
using ModestTree;

#if !NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public class FactoryFromBinderBase : FactoryArgumentsBinder
    {
        public FactoryFromBinderBase(
            DiContainer bindContainer, Type contractType, BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindContainer, bindInfo, factoryBindInfo)
        {
            ContractType = contractType;
            factoryBindInfo.ProviderFunc =
                (container) => new TransientProvider(ContractType, container, BindInfo.Arguments, BindInfo.ContextInfo);
        }

        protected Func<DiContainer, IProvider> ProviderFunc
        {
            get { return FactoryBindInfo.ProviderFunc; }
            set { FactoryBindInfo.ProviderFunc = value; }
        }

        protected Type ContractType
        {
            get; private set;
        }

        public IEnumerable<Type> AllParentTypes
        {
            get
            {
                yield return ContractType;

                foreach (var type in BindInfo.ToTypes)
                {
                    yield return type;
                }
            }
        }

        // Note that this isn't necessary to call since it's the default
        public ConditionCopyNonLazyBinder FromNew()
        {
            BindingUtil.AssertIsNotComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            return this;
        }

        public ConditionCopyNonLazyBinder FromResolve()
        {
            return FromResolve(null);
        }

        public ConditionCopyNonLazyBinder FromInstance(object instance)
        {
            BindingUtil.AssertInstanceDerivesFromOrEqual(instance, AllParentTypes);

            ProviderFunc =
                (container) => new InstanceProvider(ContractType, instance, container);

            return this;
        }

        public ConditionCopyNonLazyBinder FromResolve(object subIdentifier)
        {
            ProviderFunc =
                (container) => new ResolveProvider(
                    ContractType, container,
                    subIdentifier, false, InjectSources.Any, false);

            return this;
        }

        protected ConcreteBinderGeneric<T> CreateIFactoryBinder<T>(out Guid factoryId)
        {
            // Use a random ID so that our provider is the only one that can find it and so it doesn't
            // conflict with anything else
            factoryId = Guid.NewGuid();

            return BindContainer.Bind<T>(
                new BindInfo(),
                // Very important here that we call StartBinding with false otherwise our placeholder
                // factory binding will be finalized early
                BindContainer.StartBinding(null, false))
                .WithId(factoryId);
        }

#if !NOT_UNITY3D

        public ConditionCopyNonLazyBinder FromNewComponentOn(GameObject gameObject)
        {
            BindingUtil.AssertIsValidGameObject(gameObject);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            ProviderFunc =
                (container) => new AddToExistingGameObjectComponentProvider(
                    gameObject, container, ContractType,
                    new List<TypeValuePair>());

            return this;
        }

        public ConditionCopyNonLazyBinder FromNewComponentOn(
            Func<InjectContext, GameObject> gameObjectGetter)
        {
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            ProviderFunc =
                (container) => new AddToExistingGameObjectComponentProviderGetter(
                    gameObjectGetter, container, ContractType,
                    new List<TypeValuePair>());

            return this;
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewGameObject()
        {
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new AddToNewGameObjectComponentProvider(
                    container, ContractType,
                    new List<TypeValuePair>(), gameObjectInfo);

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new InstantiateOnPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProvider(prefab)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromComponentInNewPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsInterfaceOrComponent(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new GetFromPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProvider(prefab)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromComponentInNewPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrComponent(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new GetFromPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProviderResource(resourcePath)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new InstantiateOnPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProviderResource(resourcePath)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public ConditionCopyNonLazyBinder FromNewScriptableObjectResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(ContractType);

            ProviderFunc =
                (container) => new ScriptableObjectResourceProvider(
                    resourcePath, ContractType, container, new List<TypeValuePair>(), true);

            return this;
        }

        public ConditionCopyNonLazyBinder FromScriptableObjectResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(ContractType);

            ProviderFunc =
                (container) => new ScriptableObjectResourceProvider(
                    resourcePath, ContractType, container, new List<TypeValuePair>(), false);

            return this;
        }

        public ConditionCopyNonLazyBinder FromResource(string resourcePath)
        {
            BindingUtil.AssertDerivesFromUnityObject(ContractType);

            ProviderFunc =
                (container) => new ResourceProvider(resourcePath, ContractType);

            return this;
        }
#endif
    }
}
