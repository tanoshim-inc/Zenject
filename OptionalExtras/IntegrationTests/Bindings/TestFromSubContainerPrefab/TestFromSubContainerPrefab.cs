using System;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ModestTree;
using Assert=ModestTree.Assert;
using Zenject.Tests.Bindings.FromSubContainerPrefab;

#pragma warning disable 649

namespace Zenject.Tests.Bindings
{
    public class TestFromSubContainerPrefab : ZenjectIntegrationTestFixture
    {
        GameObject FooPrefab
        {
            get
            {
                return FixtureUtil.GetPrefab("TestFromSubContainerPrefab/Foo");
            }
        }

        GameObject FooPrefab2
        {
            get
            {
                return FixtureUtil.GetPrefab("TestFromSubContainerPrefab/Foo2");
            }
        }

        [UnityTest]
        public IEnumerator TestSelfSingle()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve()
                .ByNewPrefab(FooPrefab).AsSingle();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        [ValidateOnly]
        public IEnumerator TestSelfSingleValidate()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve()
                .ByNewPrefab(FooPrefab).AsSingle().NonLazy();

            PostInstall();
            yield break;
        }

        [UnityTest]
        [ValidateOnly]
        public IEnumerator TestSelfSingleValidateFails()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve()
                .ByNewPrefab(FooPrefab2).AsSingle().NonLazy();

            Assert.Throws(() => PostInstall());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfTransient()
        {
            PreInstall();
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsTransient();

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
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsCached();

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
            Container.Bind<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle();
            Container.Bind<Bar>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Bar>();

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
            Container.Bind(typeof(Foo), typeof(Bar)).FromSubContainerResolve().ByNewPrefab(FooPrefab).AsCached();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Bar>();

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
            Container.Bind(typeof(Foo), typeof(Bar)).FromSubContainerResolve().ByNewPrefab(FooPrefab).AsTransient();

            Container.BindRootResolve<Foo>();
            Container.BindRootResolve<Bar>();

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
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle();

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
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsTransient();

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
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsCached();

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
            Container.Bind<IFoo>().To<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle();
            Container.Bind<Bar>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle();

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
            Container.Bind(typeof(Foo), typeof(IFoo)).To<Foo>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsCached();

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
            Container.Bind<Gorp>().FromSubContainerResolve().ByNewPrefab(FooPrefab).AsSingle().NonLazy();

            Assert.Throws(() => PostInstall());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSelfIdentifiers()
        {
            PreInstall();
            Container.Bind<Gorp>().FromSubContainerResolve("gorp").ByNewPrefab(FooPrefab).AsSingle();

            Container.BindRootResolve<Gorp>();
            Container.BindRootResolve<Gorp>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            yield break;
        }
    }
}
