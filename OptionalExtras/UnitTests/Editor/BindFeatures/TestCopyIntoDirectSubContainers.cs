using System;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestCopyIntoDirectSubContainers : ZenjectUnitTestFixture
    {
        [Test]
        public void TestFromNew()
        {
            Container.Bind<Foo>().AsSingle().CopyIntoDirectSubContainers();

            var sub1 = Container.CreateSubContainer();
            var sub2 = sub1.CreateSubContainer();

            Assert.That(Container.HasBinding(
                new InjectContext(Container, typeof(Foo))
                {
                    SourceType = InjectSources.Local
                }));

            Assert.That(sub1.HasBinding(
                new InjectContext(sub1, typeof(Foo))
                {
                    SourceType = InjectSources.Local
                }));

            Assert.That(!sub2.HasBinding(
                new InjectContext(sub2, typeof(Foo))
                {
                    SourceType = InjectSources.Local
                }));
        }

        public interface IBar
        {
        }

        public class Foo
        {
        }

        public class Bar : IBar
        {
        }
    }
}

