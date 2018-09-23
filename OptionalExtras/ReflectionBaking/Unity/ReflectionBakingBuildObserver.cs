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
            var settings = TryGetEnabledSettingsInstance();

            if (settings == null)
            {
                Log.Info("Skipping reflection baking since no settings object was found");
            }
            else
            {
                Assert.That(settings.IsEnabled);
                ReflectionBakingRunner.Run(settings);
            }
        }

        static ZenjectReflectionBakingSettings TryGetEnabledSettingsInstance()
        {
            string[] guids = AssetDatabase.FindAssets("t:ZenjectReflectionBakingSettings");

            if (guids.IsEmpty())
            {
                return null;
            }

            ZenjectReflectionBakingSettings enabledSettings = null;

            foreach (var guid in guids)
            {
                var candidate = AssetDatabase.LoadAssetAtPath<ZenjectReflectionBakingSettings>(
                    AssetDatabase.GUIDToAssetPath(guid));

                if (candidate.IsEnabled)
                {
                    Assert.IsNull(enabledSettings, "Found multiple enabled ZenjectReflectionBakingSettings objects!  Please disable/delete one to continue.");
                    enabledSettings = candidate;
                }
            }

            return enabledSettings;

        }
    }
}

