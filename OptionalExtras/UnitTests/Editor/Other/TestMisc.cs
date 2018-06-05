using System;
using ModestTree;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestMisc : ZenjectUnitTestFixture
    {
        public class Tester
        {
            public Tester(IFoo<IA> a, IFoo<IB> b){}
        }

        public interface IB {}
        public interface IA {}

        public class A : IA {}
        public class B : IB {}

        public interface IFoo<T>{}

        public class Foo<T> : IFoo<T>
        {
            public Foo()
            {
                Log.Trace(typeof(T).PrettyName());
            }
        }

        [Test]
        public void Test1()
        {
            Container.Bind<IA>().To<A>().AsTransient();
            Container.Bind<IB>().To<B>().AsTransient();

            Container.Bind(typeof(IFoo<>)).To(typeof(Foo<>)).AsSingle();
            Container.Bind<Tester>().ToSelf().AsSingle().NonLazy();

            Container.ResolveRoots();

            var result = Container.Resolve<Tester>();
            Assert.IsNotNull(result);
        }
    }
}
