using System;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings
{
    public class TestFromGameObject : ZenjectIntegrationTestFixture
    {
        const string GameObjName = "TestObj";

        [UnityTest]
        public IEnumerator TestBasic()
        {
            PreInstall();
            Container.Bind<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsSingle();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestSingle()
        {
            PreInstall();
            Container.Bind<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsSingle();
            Container.Bind<IFoo>().To<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsSingle();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestTransient()
        {
            PreInstall();
            Container.Bind<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsTransient();
            Container.Bind<IFoo>().To<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsTransient();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            FixtureUtil.AssertComponentCount<Foo>(2);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestCached1()
        {
            PreInstall();
            Container.Bind<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsCached();
            Container.Bind<IFoo>().To<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsCached();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            FixtureUtil.AssertComponentCount<Foo>(2);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestCached2()
        {
            PreInstall();
            Container.Bind(typeof(Foo), typeof(IFoo)).To<Foo>().FromNewComponentOnNewGameObject().WithGameObjectName(GameObjName).AsCached();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<Foo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(1);
            FixtureUtil.AssertComponentCount<Foo>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestMultipleConcreteTransient1()
        {
            PreInstall();
            Container.Bind<IFoo>().To(typeof(Foo), typeof(Bar)).FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).AsTransient();

            Container.BindRootResolve<IFoo>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            FixtureUtil.AssertComponentCount<Foo>(1);
            FixtureUtil.AssertComponentCount<Bar>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestMultipleConcreteTransient2()
        {
            PreInstall();
            Container.Bind(typeof(IFoo), typeof(IBar)).To(new List<Type>() {typeof(Foo), typeof(Bar)}).FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).AsTransient();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IBar>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(4);
            FixtureUtil.AssertComponentCount<Foo>(2);
            FixtureUtil.AssertComponentCount<Bar>(2);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestMultipleConcreteCached()
        {
            PreInstall();
            Container.Bind(typeof(IFoo), typeof(IBar)).To(new List<Type>() {typeof(Foo), typeof(Bar)}).FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).AsCached();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IBar>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            FixtureUtil.AssertComponentCount<Foo>(1);
            FixtureUtil.AssertComponentCount<Bar>(1);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestMultipleConcreteSingle()
        {
            PreInstall();
            Container.Bind(typeof(IFoo), typeof(IBar)).To(new List<Type>() {typeof(Foo), typeof(Bar)}).FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).AsSingle();

            Container.BindRootResolve<IFoo>();
            Container.BindRootResolve<IBar>();

            PostInstall();

            FixtureUtil.AssertNumGameObjects(2);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestUnderTransformGroup()
        {
            PreInstall();

            Container.Bind<Foo>().FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).UnderTransformGroup("Foo").AsSingle();
            Container.BindRootResolve<Foo>();

            PostInstall();

            var foo = GameObject.Find("Foo").transform.GetChild(0);

            Assert.IsNotNull(foo.GetComponent<Foo>());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestUnderTransform()
        {
            PreInstall();
            var tempGameObject = new GameObject("Foo");

            Container.Bind<Foo>().FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).UnderTransform(tempGameObject.transform).AsSingle();
            Container.BindRootResolve<Foo>();

            PostInstall();

            Assert.IsNotNull(tempGameObject.transform.GetChild(0).GetComponent<Foo>());
            yield break;
        }

        [UnityTest]
        public IEnumerator TestUnderTransformGetter()
        {
            PreInstall();
            var tempGameObject = new GameObject("Foo");

            Container.Bind<Foo>().FromNewComponentOnNewGameObject()
                .WithGameObjectName(GameObjName).UnderTransform((context) => tempGameObject.transform).AsSingle();
            Container.BindRootResolve<Foo>();

            PostInstall();

            Assert.IsNotNull(tempGameObject.transform.GetChild(0).GetComponent<Foo>());
            yield break;
        }

        public interface IBar
        {
        }

        public interface IFoo
        {
        }

        public class Foo : MonoBehaviour, IFoo, IBar
        {
        }

        public class Bar : MonoBehaviour, IFoo, IBar
        {
        }
    }
}
