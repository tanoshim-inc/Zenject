using System.Collections;
using System.Linq;
using ModestTree;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject.Tests.TickableManagers.Parenting;
using Assert = NUnit.Framework.Assert;

namespace Zenject.Tests.TickableManagers
{
    public partial class TestTickableManagerParenting : ZenjectIntegrationTestFixture
    {
        GameObject NestedOnePrefab
        {
            get { return FixtureUtil.GetPrefab("TestParenting/NestedGameObjectContextOne"); }
        }

        GameObject NestedTwoPrefab
        {
            get { return FixtureUtil.GetPrefab("TestParenting/NestedGameObjectContextTwo"); }
        }

        [UnityTest]
        public IEnumerator TestPausingRoot()
        {
            RootTickable.TickCount = 0;
            NestedTickableOne.TickCount = 0;
            NestedTickableTwo.TickCount = 0;
            DoublyNestedTickable.TickCount = 0;

            PreInstall();
            Container.BindInterfacesAndSelfTo<RootTickable>().AsSingle().NonLazy();

            Container.Bind<NestedTickableOne>()
                .FromSubContainerResolve()
                .ByNewPrefab(NestedOnePrefab)
                .AsSingle();

            Container.Bind<NestedTickableTwo>()
                .FromSubContainerResolve()
                .ByNewPrefab(NestedTwoPrefab)
                .AsSingle();

            PostInstall();

            Container.Resolve<NestedTickableOne>();
            Container.Resolve<NestedTickableTwo>();

            var tickManager = Container.Resolve<TickableManager>();

            // Contexts (GameObjectContext/SceneContext) use a MonoKernel to
            // hook into the Unity Update. We need to simulate the Update loop.
            var monoKernels = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<MonoKernel>()).ToList();

            Assert.AreEqual(0, RootTickable.TickCount);
            Assert.AreEqual(0, NestedTickableOne.TickCount);
            Assert.AreEqual(0, NestedTickableTwo.TickCount);
            Assert.AreEqual(0, DoublyNestedTickable.TickCount);

            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(1, RootTickable.TickCount);
            Assert.AreEqual(1, NestedTickableOne.TickCount);
            Assert.AreEqual(1, NestedTickableTwo.TickCount);
            Assert.AreEqual(1, DoublyNestedTickable.TickCount);

            tickManager.Pause();
            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(1, RootTickable.TickCount);
            Assert.AreEqual(1, NestedTickableOne.TickCount);
            Assert.AreEqual(1, NestedTickableTwo.TickCount);
            Assert.AreEqual(1, DoublyNestedTickable.TickCount);

            tickManager.Resume();
            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(2, RootTickable.TickCount);
            Assert.AreEqual(2, NestedTickableOne.TickCount);
            Assert.AreEqual(2, NestedTickableTwo.TickCount);
            Assert.AreEqual(2, DoublyNestedTickable.TickCount);
            yield break;
        }

        [UnityTest]
        public IEnumerator TestPausingNested()
        {
            RootTickable.TickCount = 0;
            NestedTickableOne.TickCount = 0;
            NestedTickableTwo.TickCount = 0;
            DoublyNestedTickable.TickCount = 0;

            PreInstall();
            Container.BindInterfacesAndSelfTo<RootTickable>().AsSingle().NonLazy();

            Container.Bind<NestedTickableOne>()
                .FromSubContainerResolve()
                .ByNewPrefab(NestedOnePrefab)
                .AsSingle();

            Container.Bind<NestedTickableTwo>()
                .FromSubContainerResolve()
                .ByNewPrefab(NestedTwoPrefab)
                .AsSingle();

            Container.Bind<NestedTickableManagerHolder>()
                .FromSubContainerResolve()
                .ByNewPrefab(NestedTwoPrefab)
                .AsSingle();

            PostInstall();

            Container.Resolve<NestedTickableOne>();
            Container.Resolve<NestedTickableTwo>();

            var nestedTickManager = Container.Resolve<NestedTickableManagerHolder>().TickManager;

            // Contexts (GameObjectContext/SceneContext) use a MonoKernel to
            // hook into the Unity Update. We need to simulate the Update loop.
            var monoKernels = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<MonoKernel>()).ToList();

            Assert.AreEqual(0, RootTickable.TickCount);
            Assert.AreEqual(0, NestedTickableOne.TickCount);
            Assert.AreEqual(0, NestedTickableTwo.TickCount);
            Assert.AreEqual(0, DoublyNestedTickable.TickCount);

            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(1, RootTickable.TickCount);
            Assert.AreEqual(1, NestedTickableOne.TickCount);
            Assert.AreEqual(1, NestedTickableTwo.TickCount);
            Assert.AreEqual(1, DoublyNestedTickable.TickCount);

            nestedTickManager.Pause();
            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(2, RootTickable.TickCount);
            Assert.AreEqual(2, NestedTickableOne.TickCount);
            Assert.AreEqual(1, NestedTickableTwo.TickCount);
            Assert.AreEqual(1, DoublyNestedTickable.TickCount);

            nestedTickManager.Resume();
            foreach (var kernel in monoKernels)
            {
                kernel.Update();
            }

            Assert.AreEqual(3, RootTickable.TickCount);
            Assert.AreEqual(3, NestedTickableOne.TickCount);
            Assert.AreEqual(2, NestedTickableTwo.TickCount);
            Assert.AreEqual(2, DoublyNestedTickable.TickCount);
            yield break;
        }
    }
}
