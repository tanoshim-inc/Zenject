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
        public class UserJoinedSignal
        {
            public UserJoinedSignal(string username)
            {
                Username = username;
            }

            public string Username
            {
                get;
                private set;
            }
        }

        public class Greeter
        {
            public void SayHello(UserJoinedSignal signal)
            {
                Log.Trace("Hello " + signal.Username + "!");
            }
        }

        [Test]
        public void Test1()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<UserJoinedSignal>();

            Container.Bind<Greeter>().AsSingle();

            Container.BindSignal<UserJoinedSignal>()
                .ToMethod<Greeter>(x => x.SayHello).FromResolve().CopyIntoAllSubContainers();

            Container.ResolveRoots();

            var signalBus = Container.Resolve<SignalBus>();

            signalBus.Fire(new UserJoinedSignal("bob"));
        }
    }
}
