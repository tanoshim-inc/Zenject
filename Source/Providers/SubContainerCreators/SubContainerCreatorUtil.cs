using System;
using ModestTree;
#if !NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public static class SubContainerCreatorUtil
    {
        public static void ApplyBindSettings(
            SubContainerCreatorBindInfo subContainerBindInfo, DiContainer subContainer)
        {
            if (subContainerBindInfo.CreateKernel)
            {
                var parentContainer = subContainer.ParentContainers.OnlyOrDefault();
                Assert.IsNotNull(parentContainer, "Could not find unique container when using WithKernel!");

                if (subContainerBindInfo.KernelType != null)
                {
                    parentContainer.Bind(typeof(Kernel).Interfaces()).To(subContainerBindInfo.KernelType)
                        .FromSubContainerResolve()
                        .ByInstance(subContainer).AsCached();
                    subContainer.Bind(subContainerBindInfo.KernelType).AsCached();
                }
                else
                {
                    parentContainer.BindInterfacesTo<Kernel>().FromSubContainerResolve()
                        .ByInstance(subContainer).AsCached();
                    subContainer.Bind<Kernel>().AsCached();
                }

                if (subContainerBindInfo.DefaultParentName != null)
                {
                    DefaultGameObjectParentInstaller.Install(
                        subContainer, subContainerBindInfo.DefaultParentName);
                }
            }
        }

#if !NOT_UNITY3D
        class DefaultParentObjectDestroyer : IDisposable
        {
            readonly GameObject _gameObject;

            public DefaultParentObjectDestroyer(GameObject gameObject)
            {
                _gameObject = gameObject;
            }

            public void Dispose()
            {
                GameObject.Destroy(_gameObject);
            }
        }
#endif
    }
}
