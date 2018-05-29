
using System;
using System.Collections.Generic;
using Zenject;
using NUnit.Framework;
using System.Linq;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings.Singletons
{
    [TestFixture]
    public class TestValidation
    {
        DiContainer Container
        {
            get; set;
        }

        [SetUp]
        public virtual void Setup()
        {
            Container = new DiContainer(true);
            Container.BindInstance(
                new ZenjectSettings()
                {
                    ValidationErrorResponse = ZenjectSettings.ValidationErrorResponses.Throw
                });
        }

        [Test]
        public void TestFailure()
        {
            Container.Bind<Bar>().AsSingle().NonLazy();

            Assert.Throws(() => Container.ExecuteResolve());
        }

        [Test]
        public void TestSuccess()
        {
            Container.Bind<Foo>().AsSingle();
            Container.Bind<Bar>().AsSingle().NonLazy();

            Container.ExecuteResolve();
        }

        [Test]
        public void TestNumCalls()
        {
            Gorp.CallCount = 0;

            Container.BindInterfacesAndSelfTo<Gorp>().AsSingle().NonLazy();

            Container.ExecuteResolve();

            Assert.IsEqual(Gorp.CallCount, 1);
        }

        [Test]
        public void TestFactoryFail()
        {
            Container.BindFactory<Bar, Bar.Factory>().NonLazy();

            Assert.Throws(() => Container.ExecuteResolve());
        }

        [Test]
        public void TestFactorySuccess()
        {
            Container.Bind<Foo>().AsSingle();
            Container.BindFactory<Bar, Bar.Factory>().NonLazy();

            Container.ExecuteResolve();
        }

        [Test]
        public void TestSubContainerMethodSuccess()
        {
            Container.Bind<Qux>().FromSubContainerResolve().ByMethod(
                container =>
                {
                    container.Bind<Qux>().AsSingle();
                    container.Bind<Foo>().AsSingle();
                    container.Bind<Bar>().AsSingle().NonLazy();
                })
                .AsSingle().NonLazy();

            Container.ExecuteResolve();
        }

        [Test]
        public void TestSubContainerMethodFailure()
        {
            Container.Bind<Qux>().FromSubContainerResolve().ByMethod(
                container =>
                {
                    container.Bind<Qux>().AsSingle();
                    container.Bind<Bar>().AsSingle().NonLazy();
                })
                .AsSingle().NonLazy();

            Assert.Throws(() => Container.ExecuteResolve());
        }

        [Test]
        public void TestSubContainerInstallerFailure()
        {
            Container.Bind<Qux>().FromSubContainerResolve().ByInstaller<QuxInstaller>().AsSingle().NonLazy();

            Assert.Throws(() => Container.ExecuteResolve());
        }

        [Test]
        public void TestLazyFail()
        {
            Container.Bind<Jaze>().AsSingle().NonLazy();

            Assert.Throws(() => Container.ExecuteResolve());
        }

        [Test]
        public void TestLazySuccess()
        {
            Container.Bind<Qux>().AsSingle();
            Container.Bind<Jaze>().AsSingle().NonLazy();

            Container.ExecuteResolve();
        }

        public class Jaze
        {
            [Inject]
            public Lazy<Qux> Qux;
        }

        public class QuxInstaller : Installer<QuxInstaller>
        {
            public override void InstallBindings()
            {
                Container.Bind<Qux>().AsSingle();
                Container.Bind<Bar>().AsSingle().NonLazy();
            }
        }

        public class Qux
        {
        }

        public class Bar
        {
            public Bar(Foo foo)
            {
            }

            public class Factory : Factory<Bar>
            {
            }
        }

        public class Foo
        {
        }

        public interface IGorp
        {
        }

        public class Gorp : IGorp, IValidatable
        {
            public static int CallCount
            {
                get; set;
            }

            public void Validate()
            {
                CallCount++;
            }
        }
    }
}

