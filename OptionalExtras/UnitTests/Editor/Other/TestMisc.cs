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
        public class Foo
        {
            public class Factory : PlaceholderFactory<Foo>
            {
            }
        }

        [Test]
        public void Test1()
        {
            Container.BindFactory<Foo, Foo.Factory>();
        }
    }
}
