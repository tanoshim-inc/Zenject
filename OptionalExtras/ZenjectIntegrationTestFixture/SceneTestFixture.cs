#if UNITY_EDITOR

using ModestTree;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using System.Collections;
using UnityEngine.TestTools;
using Assert = ModestTree.Assert;
using System.Linq;

// Ignore warning about using SceneManager.UnloadScene instead of SceneManager.UnloadSceneAsync
#pragma warning disable 618

namespace Zenject
{
    public abstract class SceneTestFixture : IPrebuildSetup
    {
        bool _hasLoadedScene;

        public const string SettingsResourcePathParentName = "SceneTestFixtureSettings";

        const string EditorPrefsShouldRemoveSceneFromBuildSettingsKey = "SceneTestFixture.ShouldRemoveSceneFromBuildSettingsKey";

        // Hide this to avoid confusing with SetUp which is in playmode
        void IPrebuildSetup.Setup()
        {
            var scenePath = AssetDatabase.GetAssetPath(GetSceneAsset());

            var currentScenes = EditorBuildSettings.scenes.ToList();

            bool removeFromBuildSettings;

            if (currentScenes.Where(x => x.path == scenePath).IsEmpty())
            {
                removeFromBuildSettings = true;

                currentScenes.Add(
                    new EditorBuildSettingsScene()
                    {
                        path = scenePath,
                        enabled = true,
                    });

                EditorBuildSettings.scenes = currentScenes.ToArray();
            }
            else
            {
                removeFromBuildSettings = false;
            }

            EditorPrefs.SetBool(EditorPrefsShouldRemoveSceneFromBuildSettingsKey, removeFromBuildSettings);
        }

        SceneAsset GetSceneAsset()
        {
            var fixtureName = this.GetType().Name;
            var resourcePath = "{0}/{1}".Fmt(SettingsResourcePathParentName, fixtureName);
            var settingsList = Resources.LoadAll<SceneTestFixtureSceneReference>(resourcePath);

            Assert.That(!settingsList.IsEmpty(),
                "Could not find scene reference wrapper for scene test fixture '{0}'.  Expected to find it at resource path 'Resources/{1}'", fixtureName, resourcePath);

            Assert.That(settingsList.IsLength(1),
                "Found multiple scene reference wrappers for scene test fixture '{0}' at resource path 'Resources/{1}'", fixtureName, resourcePath);

            return settingsList[0].Scene;
        }

        protected DiContainer SceneContainer
        {
            get; private set;
        }

        public IEnumerator LoadScene()
        {
            var sceneName = GetSceneAsset().name;

            Assert.That(!_hasLoadedScene, "Attempted to load scene '{0}' twice", sceneName);
            _hasLoadedScene = true;

            // The [Teardown] event is not async so we have to do our cleanup for previous runs here
            // The LoadSceneMode.Single will automatically close all the open scenes for us
            if (ProjectContext.HasInstance)
            {
                var dontDestroyOnLoadRoots = ProjectContext.Instance.gameObject.scene
                    .GetRootGameObjects();

                foreach (var rootObj in dontDestroyOnLoadRoots)
                {
                    GameObject.Destroy(rootObj);
                }
            }

            var loader = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!loader.isDone)
            {
                yield return null;
            }

            if (ProjectContext.HasInstance)
            {
                var sceneContext = ProjectContext.Instance.Container.Resolve<SceneContextRegistry>()
                    .TryGetSceneContextForScene(SceneManager.GetSceneByName(sceneName));

                if (sceneContext != null)
                {
                    SceneContainer = sceneContext.Container;
                    SceneContainer.Inject(this);
                }
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            Assert.That(!StaticContext.HasContainer);
            _hasLoadedScene = false;
        }

        [TearDown]
        public virtual void Teardown()
        {
            if (EditorPrefs.GetBool(EditorPrefsShouldRemoveSceneFromBuildSettingsKey))
            {
                var scenePath = AssetDatabase.GetAssetPath(GetSceneAsset());
                var currentScenes = EditorBuildSettings.scenes.ToList();
                int numRemoved = currentScenes.RemoveAll(x => x.path == scenePath);
                Assert.IsEqual(numRemoved, 1);
                EditorBuildSettings.scenes = currentScenes.ToArray();
            }

            // Clear this now so that any tests can configure it before calling LoadScene above
            StaticContext.Clear();
        }
    }
}

#endif
