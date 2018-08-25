using System;
using System.Collections.Generic;
using Zenject;
using NUnit.Framework;
using System.Linq;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Bindings
{
    [TestFixture]
    public class TestWithKernel : ZenjectUnitTestFixture
    {
        public class Foo : IInitializable
        {
            public bool WasInitialized
            {
                get; private set;
            }

            public void Initialize()
            {
                WasInitialized = true;
            }
        }

        public class FooFacade
        {
            [Inject]
            public Foo Foo
            {
                get; private set;
            }
        }

        public class FooInstaller : Installer<FooInstaller>
        {
            public override void InstallBindings()
            {
                InstallFoo(Container);
            }
        }

        static void InstallFoo(DiContainer subContainer)
        {
            subContainer.Bind<FooFacade>().AsSingle();
            subContainer.BindInterfacesAndSelfTo<Foo>().AsSingle();
        }

        [Test]
        public void TestByInstaller()
        {
            Container.Bind<FooFacade>().FromSubContainerResolve()
                .ByInstaller<FooInstaller>().WithKernel().AsSingle();

            ZenjectManagersInstaller.Install(Container);
            Container.ResolveRoots();

            var facade = Container.Resolve<FooFacade>();

            Assert.That(!facade.Foo.WasInitialized);
            Container.Resolve<InitializableManager>().Initialize();
            Assert.That(facade.Foo.WasInitialized);
        }

        [Test]
        public void TestByMethod()
        {
            Container.Bind<FooFacade>().FromSubContainerResolve()
                .ByMethod(InstallFoo).WithKernel().AsSingle();

            ZenjectManagersInstaller.Install(Container);
            Container.ResolveRoots();

            var facade = Container.Resolve<FooFacade>();

            Assert.That(!facade.Foo.WasInitialized);
            Container.Resolve<InitializableManager>().Initialize();
            Assert.That(facade.Foo.WasInitialized);
        }
    }
}


