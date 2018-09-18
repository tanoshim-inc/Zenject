using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ModestTree;
using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zenject.ReflectionBaking
{
    public static class UnityReflectionBakingRunner
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

                var settings = TryCreateSettingsInstance();

                if (settings == null)
                {
                    Log.Info("Skipping reflection baking since no settings object was found");
                }
                else if (settings.ExecutionMode == ZenjectReflectionBakingSettings.ExecutionModes.InEditorAndBuilds
                    || settings.ExecutionMode == ZenjectReflectionBakingSettings.ExecutionModes.BuildsOnly)
                {
                    UnityReflectionBakingRunner.Run(settings);
                }
            }
        }

        public static ZenjectReflectionBakingSettings TryCreateSettingsInstance()
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

        public static void Run(ZenjectReflectionBakingSettings settings)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var dirtyAssemblies = new List<string>();
            var lastUpdateTime = ReflectionBakingSaveData.GetLastUpdateTime();

            foreach (var relativePath in GetAssemblyRelativePaths(settings))
            {
                var absolutePath = ConvertAssetPathToSystemPath(relativePath);
                var timestamp = GetFileTimestamp(absolutePath);

                if (timestamp > lastUpdateTime)
                {
                    dirtyAssemblies.Add(absolutePath);
                }
            }

            int numDllsChanged = 0;
            int numTypesChanged = 0;

            if (dirtyAssemblies.Count > 0)
            {
                WeaveAssemblies(dirtyAssemblies, settings, out numDllsChanged, out numTypesChanged);

                ReflectionBakingSaveData.SaveUpdateTime(DateTime.UtcNow.ToFileTime());
            }

            stopwatch.Start();

            if (numDllsChanged > 0)
            {
                UnityEngine.Debug.Log("Completed reflection baking in {0:0.00} seconds. Modified {1} types in {2} dlls."
                    .Fmt(stopwatch.Elapsed.TotalSeconds, numTypesChanged, numDllsChanged));
            }
        }

        static long GetFileTimestamp(string path)
        {
            return File.GetLastWriteTimeUtc(path).ToFileTime();
        }

        static List<string> GetAssemblyRelativePaths(ZenjectReflectionBakingSettings settings)
        {
            if (settings.AllAssemblies)
            {
                return AssemblyPathRegistry.GetAllGeneratedAssemblyRelativePaths();
            }

            return settings.WeavedAssemblies;
        }

        static string ConvertAssetPathToSystemPath(string assetPath)
        {
            string path = Application.dataPath;
            int pathLength = path.Length;
            path = path.Substring(0, pathLength - /* Assets */ 6);
            path = Path.Combine(path, assetPath);
            return path;
        }

        static void WeaveAssemblies(
            List<string> assemblyPaths, ZenjectReflectionBakingSettings settings,
            out int numDllsChanged, out int numTypesChanged)
        {
            var moduleDatas = CreateModules(assemblyPaths);

            var namespaceRegexes = settings.NamespacePatterns.Select(CreateRegex).ToList();

            numTypesChanged = 0;

            var assemblyMap = new Dictionary<string, Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;

                if (!assemblyMap.ContainsKey(name))
                {
                    assemblyMap.Add(name, assembly);
                }
            }

            for (int i = moduleDatas.Count - 1; i >= 0; i--)
            {
                var moduleData = moduleDatas[i];

                var allTypes = moduleData.Module.LoopupAllTypes();

                var weaver = new ReflectionBakingCodeWeaver(moduleData.Module);

                int numChanges = 0;

                var assemblyName = Path.GetFileNameWithoutExtension(moduleData.AbsolutePath);

                Assert.That(assemblyMap.ContainsKey(assemblyName),
                    "Could not find assembly '{0}'", assemblyName);

                var assembly = assemblyMap[assemblyName];

                foreach (var typeDef in allTypes)
                {
                    if (namespaceRegexes.Any() && !namespaceRegexes.Any(x => x.IsMatch(typeDef.FullName)))
                    {
                        continue;
                    }

                    var actualType = typeDef.TryGetActualType(assembly);

                    if (actualType == null)
                    {
                        Log.Error("Could not find actual type for type '{0}'", typeDef.FullName);
                        continue;
                    }

                    if (weaver.EditType(typeDef, actualType))
                    {
                        numChanges++;
                        numTypesChanged++;
                    }
                }

                moduleData.WasModified = numChanges > 0;
            }

            WriteModules(moduleDatas);

            numDllsChanged = moduleDatas.Where(x => x.WasModified).Count();
        }

        static Regex CreateRegex(string regexStr)
        {
            return new Regex(regexStr, RegexOptions.Compiled);
        }

        static void WriteModules(List<ModuleData> moduleDatas)
        {
            var writerParameters = new WriterParameters()
            {
                WriteSymbols = true
            };

            long? newLastUpdateTimestamp = null;

            foreach (var data in moduleDatas)
            {
                if (data.WasModified)
                {
                    data.Module.Write(data.AbsolutePath, writerParameters);

                    if (!newLastUpdateTimestamp.HasValue)
                    {
                        newLastUpdateTimestamp = File.GetLastWriteTimeUtc(data.AbsolutePath).ToFileTime();
                    }
                }
            }
        }

        static List<ModuleData> CreateModules(List<string> assemblyPaths)
        {
            var readerParameters = new ReaderParameters()
            {
                AssemblyResolver = new WeaverAssemblyResolver(),
                // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
                ReadSymbols = true,
            };

            var modules = new List<ModuleData>();

            for (int i = 0; i < assemblyPaths.Count; i++)
            {
                var data = new ModuleData();

                data.AbsolutePath = assemblyPaths[i];
                data.Module = ModuleDefinition.ReadModule(
                    data.AbsolutePath, readerParameters);

                modules.Add(data);
            }

            return modules;
        }

        public class ModuleData
        {
            public bool WasModified;
            public ModuleDefinition Module;
            public string AbsolutePath;
        }
    }
}
