using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ModestTree;
using UnityEditor;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject.ReflectionBaking.Mono.Cecil;

namespace Zenject.ReflectionBaking
{
    public static class ReflectionBakingBuildObserver
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
        }

        static void OnAssemblyCompiled(string assemblyAssetPath, CompilerMessage[] messages)
        {
#if !UNITY_2018
            if (Application.isEditor && !BuildPipeline.isBuildingPlayer)
            {
                return;
            }
#endif

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer)
            {
                Log.Warn("Zenject reflection baking skipped because it is not currently supported on WSA platform!");
            }
            else
            {
                TryWeaveAssembly(assemblyAssetPath);
            }
        }

        static void TryWeaveAssembly(string assemblyAssetPath)
        {
            var settings = ReflectionBakingInternalUtil.TryGetEnabledSettingsInstance();

            if (settings == null)
            {
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var assemblyFullPath = ReflectionBakingInternalUtil.ConvertAssetPathToSystemPath(assemblyAssetPath);

            var readerParameters = new ReaderParameters()
            {
                AssemblyResolver = new UnityAssemblyResolver(),
                // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
                ReadSymbols = true,
            };

            var module = ModuleDefinition.ReadModule(assemblyFullPath, readerParameters);

            if (module.AssemblyReferences.Where(x => x.Name.ToLower() == "zenject-usage").IsEmpty())
            {
                // Zenject-usage is used by the generated methods
                // Important that we do this check otherwise we can corrupt some dlls that don't have access to it
                return;
            }

            var assemblyName = Path.GetFileNameWithoutExtension(assemblyAssetPath);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetName().Name == assemblyName).OnlyOrDefault();

            Assert.IsNotNull(assembly, "Could not find unique assembly '{0}' in currently loaded list of assemblies", assemblyName);

            int numTypesChanged = ReflectionBakingModuleEditor.WeaveAssembly(
                module, assembly, settings.NamespacePatterns);

            if (numTypesChanged > 0)
            {
                var writerParameters = new WriterParameters()
                {
                    WriteSymbols = true
                };

                module.Write(assemblyFullPath, writerParameters);

                UnityEngine.Debug.Log("Added reflection baking to '{0}' types in assembly '{1}', took {2:0.00} seconds"
                    .Fmt(numTypesChanged, Path.GetFileName(assemblyAssetPath), stopwatch.Elapsed.TotalSeconds));
            }
        }
    }
}
