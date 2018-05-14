using System;
using System.Collections.Generic;
using ModestTree;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings
{
    [TestFixture]
    public class TestNewableMemoryPool
    {
        class Foo : IDisposable
        {
            public static readonly NewableMemoryPool<string, Foo> Pool =
                new NewableMemoryPool<string, Foo>(OnSpawned, OnDespawned);

            public void Dispose()
            {
                Pool.Despawn(this);
            }

            static void OnDespawned(Foo that)
            {
                that.Value = null;
            }

            static void OnSpawned(string value, Foo that)
            {
                that.Value = value;
            }

            public string Value
            {
                get; private set;
            }
        }

        [Test]
        public void Test2()
        {
            Foo.Pool.ClearPool();

            Assert.IsEqual(Foo.Pool.NumTotal, 0);
            Assert.IsEqual(Foo.Pool.NumActive, 0);
            Assert.IsEqual(Foo.Pool.NumInactive, 0);

            using (var block = DisposeBlock.Spawn())
            {
                Foo.Pool.Spawn("asdf").AttachedTo(block);

                Assert.IsEqual(Foo.Pool.NumTotal, 1);
                Assert.IsEqual(Foo.Pool.NumActive, 1);
                Assert.IsEqual(Foo.Pool.NumInactive, 0);
            }

            Assert.IsEqual(Foo.Pool.NumTotal, 1);
            Assert.IsEqual(Foo.Pool.NumActive, 0);
            Assert.IsEqual(Foo.Pool.NumInactive, 1);
        }
    }
}
