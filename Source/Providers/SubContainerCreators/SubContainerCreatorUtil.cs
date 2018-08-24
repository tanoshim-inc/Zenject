using System;
using UnityEngine;

namespace Zenject
{
    public static class SubContainerCreatorUtil
    {
        public static void ApplyBindSettings(
            SubContainerCreatorBindInfo subContainerBindInfo, DiContainer subContainer)
        {
            if (subContainerBindInfo.DefaultParentName != null)
            {
                var defaultParent = new GameObject(
                    subContainerBindInfo.DefaultParentName);

                defaultParent.transform.SetParent(
                    subContainer.InheritedDefaultParent, false);

                subContainer.DefaultParent = defaultParent.transform;

                subContainer.Bind<IDisposable>()
                    .To<DefaultParentObjectDestroyer>().AsCached().WithArguments(defaultParent);

                // Always destroy the default parent last so that the non-monobehaviours get a chance
                // to clean it up if they want to first
                subContainer.BindDisposableExecutionOrder<DefaultParentObjectDestroyer>(int.MinValue);
            }
        }

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
    }
}
