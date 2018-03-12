using System;
using System.Collections.Generic;
using ModestTree;
using System.Linq;

#if !NOT_UNITY3D
using UnityEngine;
#endif

using Zenject.Internal;

namespace Zenject
{
    public abstract class FromBinder : ScopeArgConditionCopyNonLazyBinder
    {
        public FromBinder(
            DiContainer bindContainer, BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo)
        {
            FinalizerWrapper = finalizerWrapper;
            BindContainer = bindContainer;
        }

        protected DiContainer BindContainer
        {
            get; private set;
        }

        protected BindFinalizerWrapper FinalizerWrapper
        {
            get;
            private set;
        }

        protected IBindingFinalizer SubFinalizer
        {
            set { FinalizerWrapper.SubFinalizer = value; }
        }

        protected IEnumerable<Type> AllParentTypes
        {
            get { return BindInfo.ContractTypes.Concat(BindInfo.ToTypes); }
        }

        protected IEnumerable<Type> ConcreteTypes
        {
            get
            {
                if (BindInfo.ToChoice == ToChoices.Self)
                {
                    return BindInfo.ContractTypes;
                }

                Assert.IsNotEmpty(BindInfo.ToTypes);
                return BindInfo.ToTypes;
            }
        }

        // This is the default if nothing else is called
        public ScopeArgConditionCopyNonLazyBinder FromNew()
        {
            BindingUtil.AssertTypesAreNotComponents(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            return this;
        }

        public ScopeConditionCopyNonLazyBinder FromResolve()
        {
            return FromResolve(null);
        }

        public ScopeConditionCopyNonLazyBinder FromResolve(object subIdentifier)
        {
            return FromResolveInternal(subIdentifier, false);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveAll()
        {
            return FromResolveAll(null);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveAll(object subIdentifier)
        {
            return FromResolveInternal(subIdentifier, true);
        }

        ScopeConditionCopyNonLazyBinder FromResolveInternal(object subIdentifier, bool matchAll)
        {
            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;

            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new ResolveProvider(
                    type, container, subIdentifier, false, InjectSources.Any, matchAll));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

        public SubContainerBinder FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public SubContainerBinder FromSubContainerResolve(object subIdentifier)
        {
            // It's unlikely they will want to create the whole subcontainer with each binding
            // (aka transient) which is the default so require that they specify it
            BindInfo.RequireExplicitScope = true;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;

            return new SubContainerBinder(
                BindInfo, FinalizerWrapper, subIdentifier);
        }

        protected ScopeArgConditionCopyNonLazyBinder FromIFactoryBase<TContract>(
            Action<ConcreteBinderGeneric<IFactory<TContract>>> factoryBindGenerator)
        {
            // Use a random ID so that our provider is the only one that can find it and so it doesn't
            // conflict with anything else
            var factoryId = Guid.NewGuid();
            var subBinder = new ConcreteIdBinderGeneric<IFactory<TContract>>(
                // Very important here that we call StartBinding with false otherwise our placeholder
                // factory binding will be finalized early
                BindContainer, new BindInfo(typeof(IFactory<TContract>)), BindContainer.StartBinding(false)).WithId(factoryId);
            factoryBindGenerator(subBinder);

            // This is kind of like a look up method like FromMethod so don't enforce specifying scope
            // The internal binding will require an explicit scope so should be obvious enough
            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;

            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new IFactoryProvider<TContract>(container, factoryId));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public ScopeArgConditionCopyNonLazyBinder FromNewComponentOn(GameObject gameObject)
        {
            BindingUtil.AssertIsValidGameObject(gameObject);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new AddToExistingGameObjectComponentProvider(
                    gameObject, container, type, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ScopeArgConditionCopyNonLazyBinder FromNewComponentOn(Func<InjectContext, GameObject> gameObjectGetter)
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new AddToExistingGameObjectComponentProviderGetter(
                    gameObjectGetter, container, type, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ArgConditionCopyNonLazyBinder FromNewComponentSibling()
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new SingleProviderBindingFinalizer(
                BindInfo, (container, type) => new AddToCurrentGameObjectComponentProvider(
                    container, type, BindInfo.Arguments));

            return new ArgConditionCopyNonLazyBinder(BindInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewGameObject()
        {
            return FromNewComponentOnNewGameObject(new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewGameObject(
            GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new AddToNewGameObjectComponentProvider(
                    container,
                    type,
                    BindInfo.Arguments,
                    gameObjectInfo));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(string resourcePath)
        {
            return FromNewComponentOnNewPrefabResource(resourcePath, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(
            string resourcePath, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabResourceBindingFinalizer(
                BindInfo, gameObjectInfo, resourcePath,
                (contractType, instantiator) => new InstantiateOnPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(UnityEngine.Object prefab)
        {
            return FromNewComponentOnNewPrefab(prefab, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabBindingFinalizer(
                BindInfo, gameObjectInfo, prefab,
                (contractType, instantiator) => new InstantiateOnPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefab(UnityEngine.Object prefab)
        {
            return FromComponentInNewPrefab(
                prefab, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabBindingFinalizer(
                BindInfo, gameObjectInfo, prefab,
                (contractType, instantiator) => new GetFromPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefabResource(string resourcePath)
        {
            return FromComponentInNewPrefabResource(resourcePath, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefabResource(
            string resourcePath, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabResourceBindingFinalizer(
                BindInfo, gameObjectInfo, resourcePath,
                (contractType, instantiator) => new GetFromPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public ScopeArgConditionCopyNonLazyBinder FromNewScriptableObjectResource(string resourcePath)
        {
            return FromScriptableObjectResourceInternal(resourcePath, true);
        }

        public ScopeArgConditionCopyNonLazyBinder FromScriptableObjectResource(string resourcePath)
        {
            return FromScriptableObjectResourceInternal(resourcePath, false);
        }

        ScopeArgConditionCopyNonLazyBinder FromScriptableObjectResourceInternal(
            string resourcePath, bool createNew)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new ScriptableObjectResourceProvider(
                    resourcePath, type, container, BindInfo.Arguments, createNew));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ScopeConditionCopyNonLazyBinder FromResource(string resourcePath)
        {
            BindingUtil.AssertDerivesFromUnityObject(ConcreteTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (_, type) => new ResourceProvider(resourcePath, type));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

#endif

        public ScopeArgConditionCopyNonLazyBinder FromMethodUntyped(Func<InjectContext, object> method)
        {
            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new MethodProviderUntyped(method, container));

            return this;
        }

        protected ScopeArgConditionCopyNonLazyBinder FromMethodBase<TConcrete>(Func<InjectContext, TConcrete> method)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TConcrete), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new MethodProvider<TConcrete>(method, container));

            return this;
        }

        protected ScopeArgConditionCopyNonLazyBinder FromMethodMultipleBase<TConcrete>(Func<InjectContext, IEnumerable<TConcrete>> method)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TConcrete), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new MethodProviderMultiple<TConcrete>(method, container));

            return this;
        }

        protected ScopeConditionCopyNonLazyBinder FromResolveGetterBase<TObj, TResult>(
            object identifier, Func<TObj, TResult> method, bool matchMultiple)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TResult), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new GetterProvider<TObj, TResult>(identifier, method, container, matchMultiple));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

        protected ScopeConditionCopyNonLazyBinder FromInstanceBase(object instance)
        {
            BindingUtil.AssertInstanceDerivesFromOrEqual(instance, AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            // Don't know how it's created so can't assume here that it violates AsSingle
            BindInfo.MarkAsCreationBinding = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                (container, type) => new InstanceProvider(type, instance, container));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }
    }
}
