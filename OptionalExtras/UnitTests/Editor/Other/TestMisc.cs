using System;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestMisc : ZenjectUnitTestFixture
    {
        public class Foo
        {
        }

        [Test]
        public void Test1()
        {
            Container.BindInterfacesTo<Foo>().AsSingle();
        }
    }
}
