#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings
{
    public class TestFromComponentSibling : ZenjectIntegrationTestFixture
    {
        Foo _foo;
        Bar _bar;
        Qux _qux1;
        Qux _qux2;

        public void Setup1()
        {
            _foo = new GameObject().AddComponent<Foo>();

            _bar = _foo.gameObject.AddComponent<Bar>();
            _qux1 = _foo.gameObject.AddComponent<Qux>();
            _qux2 = _foo.gameObject.AddComponent<Qux>();
        }

        public void Setup2()
        {
            _foo = new GameObject().AddComponent<Foo>();
            _bar = _foo.gameObject.AddComponent<Bar>();
        }

        [UnityTest]
        public IEnumerator RunTestSingleMatch()
        {
            Setup1();
            PreInstall();

            Container.Bind<Qux>().FromComponentSibling();
            Container.Bind<Bar>().FromComponentSibling();
            Container.Bind<IBar>().FromComponentSibling();

            PostInstall();

            Assert.IsEqual(_foo.Bar, _bar);
            Assert.IsEqual(_foo.IBar, _bar);
            Assert.IsEqual(_foo.Qux.Count, 1);
            Assert.IsEqual(_foo.Qux[0], _qux1);
            Assert.IsEqual(_qux1.OtherQux, _qux1);
            yield break;
        }

        [UnityTest]
        public IEnumerator RunTestMultiple()
        {
            Setup1();
            PreInstall();

            Container.Bind<Qux>().FromComponentsSibling();
            Container.Bind<Bar>().FromComponentSibling();
            Container.Bind<IBar>().FromComponentSibling();

            PostInstall();

            Assert.IsEqual(_foo.Bar, _bar);
            Assert.IsEqual(_foo.IBar, _bar);
            Assert.IsEqual(_foo.Qux[0], _qux1);
            Assert.IsEqual(_foo.Qux[1], _qux2);

            // Should skip self
            Assert.IsEqual(_foo.Qux[0].OtherQux, _foo.Qux[1]);
            Assert.IsEqual(_foo.Qux[1].OtherQux, _foo.Qux[0]);
            yield break;
        }

        [UnityTest]
        public IEnumerator RunTestMissingFailure()
        {
            Setup2();
            PreInstall();

            Container.Bind<Qux>().FromComponentSibling();
            Container.Bind<Bar>().FromComponentSibling();
            Container.Bind<IBar>().FromComponentSibling();

            Assert.Throws(() => PostInstall());
            yield break;
        }

        [UnityTest]
        public IEnumerator RunTestMissingSuccess()
        {
            Setup2();
            PreInstall();

            Container.Bind<Qux>().FromComponentsSibling();
            Container.Bind<Bar>().FromComponentSibling();
            Container.Bind<IBar>().FromComponentSibling();

            PostInstall();

            Assert.That(_foo.Qux.IsEmpty());
            yield break;
        }

        public class Qux : MonoBehaviour
        {
            [Inject]
            public Qux OtherQux;
        }

        public interface IBar
        {
        }

        public class Bar : MonoBehaviour, IBar
        {
        }

        public class Foo : MonoBehaviour
        {
            [Inject]
            public Bar Bar;

            [Inject]
            public IBar IBar;

            [Inject]
            public List<Qux> Qux;
        }
    }
}

#endif
