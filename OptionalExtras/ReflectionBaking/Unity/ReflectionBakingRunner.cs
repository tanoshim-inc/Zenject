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
    public class ReflectionBakingRunner
    {
        readonly ZenjectReflectionBakingSettings _settings;
        readonly List<ModuleData> _moduleData = new List<ModuleData>();

        public ReflectionBakingRunner(ZenjectReflectionBakingSettings settings)
        {
            _settings = settings;
        }

        public static void Run(ZenjectReflectionBakingSettings settings)
        {
            Assert.That(settings.IsEnabled);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var runner = new ReflectionBakingRunner(settings);

            int numDllsChanged, numTypesChanged;

            runner.Run(out numDllsChanged, out numTypesChanged);

            stopwatch.Start();

            if (numDllsChanged > 0)
            {
                UnityEngine.Debug.Log("Completed reflection baking in {0:0.00} seconds. Modified {1} types in {2} dlls."
                    .Fmt(stopwatch.Elapsed.TotalSeconds, numTypesChanged, numDllsChanged));
            }
        }

        void Run(out int numDllsChanged, out int numTypesChanged)
        {
            LoadModules();

            if (_moduleData.IsEmpty())
            {
                numDllsChanged = 0;
                numTypesChanged = 0;
                return;
            }

            var namespaceRegexes = _settings.NamespacePatterns.Select(CreateRegex).ToList();

            int numTypesEditted = 0;

            for (int i = _moduleData.Count - 1; i >= 0; i--)
            {
                var moduleData = _moduleData[i];

                var typesInModule = moduleData.Module.LookupAllTypes();

                var moduleEditor = new ReflectionBakingModuleEditor(moduleData.Module);

                foreach (var typeDef in typesInModule)
                {
                    if (namespaceRegexes.Any() && !namespaceRegexes.Any(x => x.IsMatch(typeDef.FullName)))
                    {
                        continue;
                    }

                    var actualType = typeDef.TryGetActualType(moduleData.Assembly);

                    if (actualType == null)
                    {
                        Log.Warn("Could not find actual type for type '{0}', skipping", typeDef.FullName);
                        continue;
                    }

                    if (moduleEditor.TryEditType(typeDef, actualType))
                    {
                        numTypesEditted++;
                        moduleData.WasModified = true;
                    }
                }
            }

            WriteModifiedModules();

            numDllsChanged = _moduleData.Where(x => x.WasModified).Count();
            numTypesChanged = numTypesEditted;
        }

        Regex CreateRegex(string regexStr)
        {
            return new Regex(regexStr, RegexOptions.Compiled);
        }

        void WriteModifiedModules()
        {
            var writerParameters = new WriterParameters()
            {
                WriteSymbols = true
            };

            foreach (var data in _moduleData)
            {
                if (data.WasModified)
                {
                    data.Module.Write(data.AbsolutePath, writerParameters);
                }
            }
        }

        void LoadModules()
        {
            var assemblyPaths = new List<string>();

            var assemblyRelativePaths = _settings.AllGeneratedAssemblies ?
                AssemblyPathRegistry.GetAllGeneratedAssemblyRelativePaths() : _settings.WeavedAssemblies;

            foreach (var relativePath in assemblyRelativePaths)
            {
                assemblyPaths.Add(
                    ReflectionBakingInternalUtil.ConvertAssetPathToSystemPath(relativePath));
            }

            var readerParameters = new ReaderParameters()
            {
                AssemblyResolver = new WeaverAssemblyResolver(),
                // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
                ReadSymbols = true,
            };

            var assemblyMap = new Dictionary<string, Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;

                if (!assemblyMap.ContainsKey(name))
                {
                    assemblyMap.Add(name, assembly);
                }
            }

            Assert.That(_moduleData.IsEmpty());

            for (int i = 0; i < assemblyPaths.Count; i++)
            {
                var absolutePath = assemblyPaths[i];
                var data = new ModuleData();

                data.AbsolutePath = absolutePath;
                data.Module = ModuleDefinition.ReadModule(
                    data.AbsolutePath, readerParameters);

                var assemblyName = Path.GetFileNameWithoutExtension(absolutePath);

                Assert.That(assemblyMap.ContainsKey(assemblyName),
                    "Could not find assembly '{0}'", assemblyName);

                data.Assembly = assemblyMap[assemblyName];

                _moduleData.Add(data);
            }
        }

        class ModuleData
        {
            public bool WasModified;
            public ModuleDefinition Module;
            public Assembly Assembly;
            public string AbsolutePath;
        }
    }
}
