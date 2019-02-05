using System;
using NUnit.Framework;
using Assert = ModestTree.Assert;

namespace Zenject.Tests.Signals
{
    [TestFixture]
    public class TestSignalIdentifiers : ZenjectUnitTestFixture
    {
        [SetUp]
        public void CommonInstall()
        {
            SignalBusInstaller.Install(Container);
        }

        [Test]
        public void TestMissingDeclaration()
        {
            var signalBus = Container.Resolve<SignalBus>();

            Assert.Throws(() => signalBus.Fire<FooSignal>("asdf"));
        }

        [Test]
        public void TestSubscribeAndUnsubscribe()
        {
            var signalId = "asdf";

            Container.DeclareSignal<FooSignal>().WithId(signalId);

            var signalBus = Container.Resolve<SignalBus>();

            bool received = false;

            Action callback = () => received = true;
            signalBus.Subscribe<FooSignal>(callback, signalId);

            Assert.Throws(() => signalBus.Subscribe<FooSignal>(callback));

            Assert.That(!received);
            signalBus.Fire<FooSignal>(signalId);
            Assert.That(received);

            Assert.Throws(() => signalBus.Fire<FooSignal>());
            Assert.Throws(() => signalBus.Fire<FooSignal>("asdfz"));

            received = false;
            signalBus.Fire<FooSignal>(signalId);
            Assert.That(received);

            signalBus.Unsubscribe<FooSignal>(callback, signalId);

            received = false;
            signalBus.Fire<FooSignal>(signalId);
            Assert.That(!received);
        }

        [Test]
        public void TestIncompleteBinding()
        {
            Container.DeclareSignal<FooSignal>().WithId("asdf");
            Container.BindSignal<FooSignal>().WithId("asdf");

            Assert.Throws(() => Container.FlushBindings());
        }

        [Test]
        public void TestBindWithoutDeclaration()
        {
            Container.BindSignal<FooSignal>().WithId("asdf").ToMethod(() => {});

            Assert.Throws(() => Container.ResolveRoots());
        }

        [Test]
        public void TestStaticMethodHandler()
        {
            Container.DeclareSignal<FooSignal>().WithId("asdf");

            bool received = false;

            Container.BindSignal<FooSignal>().WithId("asdf").ToMethod(() => received = true);
            Container.ResolveRoots();

            var signalBus = Container.Resolve<SignalBus>();

            Assert.That(!received);
            signalBus.Fire<FooSignal>("asdf");
            Assert.That(received);
        }

        [Test]
        public void TestStaticMethodHandlerWithArgs()
        {
            Container.DeclareSignal<FooSignal>().WithId("asdf");

            FooSignal received = null;

            Container.BindSignal<FooSignal>().WithId("asdf").ToMethod(x => received = x);
            Container.ResolveRoots();

            var signalBus = Container.Resolve<SignalBus>();
            var sent = new FooSignal();

            Assert.IsNull(received);
            signalBus.Fire(sent, "asdf");
            Assert.IsEqual(received, sent);
        }

        [Test]
        public void TestInstanceMethodHandler()
        {
            Container.DeclareSignal<FooSignal>().WithId("asdf");

            var qux = new Qux();
            Container.BindSignal<FooSignal>().WithId("asdf")
                .ToMethod<Qux>(x => x.OnFoo).From(b => b.FromInstance(qux));
            Container.ResolveRoots();

            var signalBus = Container.Resolve<SignalBus>();

            Assert.That(!qux.HasRecievedSignal);
            signalBus.Fire<FooSignal>("asdf");
            Assert.That(qux.HasRecievedSignal);
        }

        public class Qux
        {
            public void OnFoo()
            {
                HasRecievedSignal = true;
            }

            public bool HasRecievedSignal
            {
                get; private set;
            }
        }

        public class FooSignal
        {
        }

        public class Foo
        {
        }
    }
}

