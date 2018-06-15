using System;
using ModestTree;
using NUnit.Framework;
using UnityEngine;
using Assert = ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestMisc : ZenjectUnitTestFixture
    {
        public class Foo
        {
        }

        public class Bar
        {
        }

        [Test]
        public void Test1()
        {
            Container.Bind<Foo>().AsSingle();
            Container.Bind<Bar>().AsSingle();
            Container.ResolveRoots();
        }
    }
}
