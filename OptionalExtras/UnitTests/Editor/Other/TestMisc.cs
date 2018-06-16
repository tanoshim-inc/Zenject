using System;
using System.Diagnostics;
using ModestTree;
using NUnit.Framework;
using UnityEngine;
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
        }

        [Test]
        public void Test1()
        {
            Container.Bind<Foo>().AsSingle();
            Container.Bind<IFoo>().To<Foo>().FromResolve();

            Container.ResolveRoots();
        }
    }
}
