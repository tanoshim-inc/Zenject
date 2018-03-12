using System;
using System.Collections.Generic;
using Zenject;
using NUnit.Framework;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings
{
    [TestFixture]
    public class TestFromGetter : ZenjectUnitTestFixture
    {
        [Test]
        public void TestTransient()
        {
            Container.Bind<Foo>().AsSingle();
            Container.Bind<Bar>().FromResolveGetter<Foo>(x => x.Bar);

            Assert.IsNotNull(Container.Resolve<Bar>());
            Assert.IsEqual(Container.Resolve<Bar>(), Container.Resolve<Foo>().Bar);
        }

        [Test]
        public void TestSingleFailure()
        {
            Container.Bind<Foo>().AsCached();
            Container.Bind<Foo>().AsCached();
            Container.Bind<Bar>().FromResolveGetter<Foo>(x => x.Bar).AsSingle();

            Assert.Throws(() => Container.Resolve<Bar>());
        }

        [Test]
        public void TestMultiple()
        {
            Container.Bind<Foo>().AsCached();
            Container.Bind<Foo>().AsCached();
            Container.Bind<Bar>().FromResolveAllGetter<Foo>(x => x.Bar).AsSingle();

            Assert.IsEqual(Container.ResolveAll<Bar>().Count, 2);
        }

        interface IBar
        {
        }

        class Bar : IBar
        {
        }

        class Foo
        {
            public Foo()
            {
                Bar = new Bar();
            }

            public Bar Bar
            {
                get; private set;
            }
        }
    }
}

