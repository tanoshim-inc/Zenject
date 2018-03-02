using System;
using System.Collections.Generic;
using Zenject;
using NUnit.Framework;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Injection
{
    [TestFixture]
    public class TestConstructorInjection : ZenjectUnitTestFixture
    {
        [Test]
        public void TestResolve()
        {
            Container.Bind<Foo>().AsSingle().NonLazy();
            Container.Bind<Bar>().AsSingle().NonLazy();

            Assert.IsNotNull(Container.Resolve<Foo>());
        }

        [Test]
        public void TestInstantiate()
        {
            Container.Bind<Foo>().AsSingle();
            Assert.IsNotNull(Container.Instantiate<Foo>(new object[] { new Bar() }));
        }

        [Test]
        public void TestMultipleWithOneTagged()
        {
            Container.Bind<Bar>().AsSingle().NonLazy();
            Container.Bind<Qux>().AsSingle().NonLazy();

            Assert.IsNotNull(Container.Resolve<Qux>());
        }

        class Bar
        {
        }

        class Foo
        {
            public Foo(Bar bar)
            {
                Bar = bar;
            }

            public Bar Bar
            {
                get; private set;
            }
        }

        class Qux
        {
            public Qux()
            {
            }

            [Inject]
            public Qux(Bar val)
            {
            }
        }
    }
}


