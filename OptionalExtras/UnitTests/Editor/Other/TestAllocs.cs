// In order to run this, install dotMemoryPeek through nuget and then change this to 1
#if false

using System;
using System.Diagnostics;
using JetBrains.dotMemoryUnit;
using ModestTree;
using NUnit.Framework;
using Assert=ModestTree.Assert;
using System.Linq;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestAllocs : ZenjectUnitTestFixture
    {
        interface IFoo
        {
        }

        public class Foo1 : IFoo
        {
        }

        public class Foo2 : IFoo
        {
        }

        [Test]
        [DotMemoryUnit(CollectAllocations=true)]
        public void TryStuff()
        {
            Container.Bind<IFoo>().To<Foo1>().AsSingle();

            IFoo foo;

            foo = Container.Resolve<IFoo>();

            var point1 = dotMemory.Check();

            foo = Container.Resolve<IFoo>();

            dotMemory.Check(memory =>
                {
                    var traffic = memory.GetTrafficFrom(point1).Where(x => x.Namespace.Like("Zenject"));
                    var bytesAllocated = traffic.AllocatedMemory.SizeInBytes;

                    if (bytesAllocated != 0)
                    {
                        Log.Trace("Found unnecessary memory allocations ({0} kb) in Container.Resolve. Allocated Types: {1}",
                            (float)bytesAllocated / 1024f, traffic.GroupByType().Select(x => x.TypeFullyQualifiedName).Join(", "));
                    }
                });
        }
    }
}

#endif
