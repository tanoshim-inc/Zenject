using System;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ModestTree;
using Assert=ModestTree.Assert;
using Zenject.Tests.Bindings.FromSubContainerPrefabResource;

namespace Zenject.Tests.Bindings
{
    public class TestFromSubContainerPrefabResource : ZenjectIntegrationTestFixture
    {
        const string PathPrefix = "TestFromSubContainerPrefabResource/";
        const string FooResourcePath = PathPrefix + "FooSubContainer";

        [UnityTest]
        public IEnumerator TestTransientError()
        {
            PreInstall();
            // Validation should detect that it doesn't exist
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefabResource(PathPrefix + "asdfasdfas").AsTransient().NonLazy();

            Assert.Throws(() => PostInstall());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfSingle()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfTransient()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsTransient();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(3);
            FixtureUtil.AssertComponentCount<Foo>(3);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfCached()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsCached();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfSingleMultipleContracts()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle().NonLazy();
            Container.Bind<Bar>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle().NonLazy();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            FixtureUtil.AssertComponentCount<Bar>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfCachedMultipleContracts()
        {
            PreInstall();
            Container.Bind(typeof(Foo), typeof(Bar)).FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsCached().NonLazy();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            FixtureUtil.AssertComponentCount<Bar>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfTransientMultipleContracts()
        {
            PreInstall();
            Container.Bind(typeof(Foo), typeof(Bar)).FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsTransient().NonLazy();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            FixtureUtil.AssertComponentCount<Foo>(2);
            FixtureUtil.AssertComponentCount<Bar>(2);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestConcreteSingle()
        {
            PreInstall();
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestConcreteTransient()
        {
            PreInstall();
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsTransient();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(3);
            FixtureUtil.AssertComponentCount<Foo>(3);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestConcreteCached()
        {
            PreInstall();
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsCached();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestConcreteSingleMultipleContracts()
        {
            PreInstall();
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle();
            Container.Bind<Bar>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Bar>();
            Container.BindRootResolve<Bar>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            FixtureUtil.AssertComponentCount<Bar>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestConcreteCachedMultipleContracts()
        {
            PreInstall();
            Container.Bind(typeof(Foo), typeof(IFoo)).To<Foo>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsCached();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfIdentifiersFails()
        {
            PreInstall();
            Container.Bind<Gorp>().FromSubContainerResolve().ByNewPrefabResource(FooResourcePath).AsSingle().NonLazy();

            Assert.Throws(() => PostInstall());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfIdentifiers()
        {
            PreInstall();
            Container.Bind<Gorp>().FromSubContainerResolve("gorp").ByNewPrefabResource(FooResourcePath).AsSingle();

            Container.BindRootResolve<Gorp>();
            Container.BindRootResolve<Gorp>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            yield break;
        }
    }
}
