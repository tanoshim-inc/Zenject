using ModestTree;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using System.Collections;
using UnityEngine.TestTools;
using Zenject.SpaceFighter;
using Assert = ModestTree.Assert;
using System.Linq;

// Ignore warning about using SceneManager.UnloadScene instead of SceneManager.UnloadSceneAsync
#pragma warning disable 618

namespace Zenject.SpaceFighter
{
    public abstract class ScenePlayModeTestBase
    {
        string _sceneName;
        bool _hasLoadedScene;

        public ScenePlayModeTestBase(string sceneName)
        {
            _sceneName = sceneName;
        }

        [SetUp]
        public void SetUp()
        {
            Assert.That(!StaticContext.HasContainer);
            _hasLoadedScene = false;
        }

        protected DiContainer SceneContainer
        {
            get; private set;
        }

        public IEnumerator LoadScene()
        {
            Assert.That(!_hasLoadedScene, "Attempted to load scene '{0}' twice", _sceneName);
            _hasLoadedScene = true;

            Assert.IsEqual(SceneManager.sceneCount, 1);

            var currentScene = SceneManager.GetSceneAt(0);
            Assert.That(currentScene.name.StartsWith("InitTestScene"));

            Assert.That(!ProjectContext.HasInstance);
            var loader = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);

            while (!loader.isDone)
            {
                yield return null;
            }

            Assert.That(ProjectContext.HasInstance);

            SceneContainer = ProjectContext.Instance.Container.Resolve<SceneContextRegistry>()
                .GetSceneContextForScene(_sceneName).Container;

            SceneContainer.Inject(this);
        }

        [TearDown]
        public void UnloadScene()
        {
            Assert.That(_hasLoadedScene, "Expected LoadScene to be called but was not");

            bool success = SceneManager.UnloadScene(_sceneName);
            Assert.That(success);

            var dontDestroyOnLoadRoots = ProjectContext.Instance.gameObject.scene.GetRootGameObjects();
            foreach (var rootObj in dontDestroyOnLoadRoots)
            {
                GameObject.DestroyImmediate(rootObj);
            }

            Assert.That(!ProjectContext.HasInstance);

            StaticContext.Clear();
        }
    }
}

