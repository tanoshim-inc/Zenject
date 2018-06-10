using System;
using ModestTree;
using NUnit.Framework;
using Assert = ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestMisc : ZenjectUnitTestFixture
    {
        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
            public class Factory : PlaceholderFactory<Foo>
            {
            }
        }

        [Test]
        public void Test1()
        {
            Container.Bind<IFoo>().WithId(null).To<Foo>()
                .FromNew().AsSingle().WithArguments(null).When(null).CopyIntoAllSubContainers().NonLazy();
            //Container.BindFactory<Foo, Foo.Factory>();
        }
    }
}
