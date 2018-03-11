using System;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestFactoryMemoryPoolCustomInterface : ZenjectUnitTestFixture
    {
        [Test]
        public void Test1()
        {
            var foo = new Foo();

            Container.BindFactoryInterface<Foo, Foo.Factory, Foo.IFooFactory>().FromInstance(foo);

            Assert.IsEqual(Container.Resolve<Foo.IFooFactory>().Create(), foo);
        }

        [Test]
        public void Test2()
        {
            var foo = new Foo();

            Container.BindMemoryPoolInterface<Foo, Foo.Pool, Foo.IFooPool>().FromInstance(foo);

            Assert.IsEqual(Container.Resolve<Foo.IFooPool>().Spawn(), foo);
        }

        public class Foo
        {
            public interface IFooFactory : IFactory<Foo>
            {
            }

            public interface IFooPool : IMemoryPool<Foo>
            {
            }

            public class Factory : Factory<Foo>, IFooFactory
            {
            }

            public class Pool : MemoryPool<Foo>, IFooPool
            {
            }
        }
    }
}
