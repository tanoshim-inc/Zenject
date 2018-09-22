using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ModestTree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zenject.ReflectionBaking
{
    public static class ReflectionBakingBuildObserver
    {
        static bool _hasExecutedReflectionBakingForBuild;

        [UsedImplicitly]
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            TryCreateSettingsInstance();
        }

        [PostProcessScene]
        public static void PostprocessScene()
        {
            if (BuildPipeline.isBuildingPlayer && !_hasExecutedReflectionBakingForBuild)
            {
                // Only need to do this once
                _hasExecutedReflectionBakingForBuild = true;
                ExecuteBaking();
            }
        }

        public static void ExecuteBaking()
        {
            var settings = TryCreateSettingsInstance();

            if (settings == null)
            {
                Log.Info("Skipping reflection baking since no settings object was found");
            }
            else if (settings.IsEnabled)
            {
                ReflectionBakingRunner.Run(settings);
            }
        }

        static ZenjectReflectionBakingSettings TryCreateSettingsInstance()
        {
            string[] guids = AssetDatabase.FindAssets("t:ZenjectReflectionBakingSettings");

            if (guids.IsEmpty())
            {
                return null;
            }

            if (guids.Length > 1)
            {
                UnityEngine.Debug.LogError(
                    "Zenject code weaving failed!  Found multiple ZenjectReflectionBakingSettings objects!");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<ZenjectReflectionBakingSettings>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}

