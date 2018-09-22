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
            if (_settings.WeavedAssemblies.IsEmpty())
            {
                numDllsChanged = 0;
                numTypesChanged = 0;
                return;
            }

            LoadModules();

            var namespaceRegexes = _settings.NamespacePatterns.Select(CreateRegex).ToList();

            var assemblyMap = new Dictionary<string, Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;

                if (!assemblyMap.ContainsKey(name))
                {
                    assemblyMap.Add(name, assembly);
                }
            }

            var edittedTypes = new List<TypeDefAndType>();
            var entryPointMethods = new List<MethodDefinition>();

            for (int i = _moduleData.Count - 1; i >= 0; i--)
            {
                var moduleData = _moduleData[i];

                var assemblyName = Path.GetFileNameWithoutExtension(moduleData.AbsolutePath);

                Assert.That(assemblyMap.ContainsKey(assemblyName),
                    "Could not find assembly '{0}'", assemblyName);

                var assembly = assemblyMap[assemblyName];

                var typesInModule = moduleData.Module.LookupAllTypes();

                var moduleEditor = new ReflectionBakingModuleEditor(moduleData.Module, assembly);

                foreach (var typeDef in typesInModule)
                {
                    entryPointMethods.AddRange(
                        typeDef.Methods.Where(
                            x => x.HasCustomAttribute<ReflectionBakingEntryPointAttribute>()));

                    var actualType = typeDef.TryGetActualType(assembly);

                    if (actualType == null)
                    {
                        Log.Warn("Could not find actual type for type '{0}', skipping", typeDef.FullName);
                        continue;
                    }

                    if (namespaceRegexes.Any() && !namespaceRegexes.Any(x => x.IsMatch(typeDef.FullName)))
                    {
                        continue;
                    }

                    if (moduleEditor.TryEditType(typeDef, actualType))
                    {
                        edittedTypes.Add(new TypeDefAndType(typeDef, actualType));
                        moduleData.WasModified = true;
                    }
                }
            }

            if (edittedTypes.IsEmpty())
            {
                numDllsChanged = 0;
                numTypesChanged = 0;
            }
            else
            {
                if (entryPointMethods.IsEmpty())
                {
                    throw Assert.CreateException(
                        "Could not find reflection baking entry point method!  See documentation for details");
                }

                if (entryPointMethods.Count > 1)
                {
                    throw Assert.CreateException(
                        "Found multiple methods marked with [ReflectionBakingEntryPoint] attribute!  There should only be one");
                }

                PopulateRegisterMethod(entryPointMethods[0], edittedTypes);

                _moduleData.Where(x => x.Module == entryPointMethods[0].Module).Single().WasModified = true;

                WriteModules();

                numDllsChanged = _moduleData.Where(x => x.WasModified).Count();
                numTypesChanged = edittedTypes.Count;
            }
        }

        static IEnumerable<Type> GetNestParentsAndSelf(Type type)
        {
            yield return type;

            if (type.DeclaringType != null)
            {
                foreach (var ancestor in GetNestParentsAndSelf(type.DeclaringType))
                {
                    yield return ancestor;
                }
            }
        }

        void PopulateRegisterMethod(
            MethodDefinition method, List<TypeDefAndType> edittedTypes)
        {
            var body = method.Body;

            body.Instructions.Clear();

            var processor = body.GetILProcessor();

            var module = method.Module;

            var registerMethod = module.ImportMethod(typeof(TypeAnalyzer), "RegisterTypeInfoCreator");
            var funcConstructor = module.ImportMethod<ZenTypeInfoGetter>(".ctor");
            var getTypeFromHandleMethod = module.ImportMethod<Type>("GetTypeFromHandle", 1);

            var allGenericInstances = new Dictionary<string, List<TypeReference>>();

            foreach (var genericType in _moduleData.SelectMany(x => x.Module.LookupAllTypes())
                .SelectMany(FindAllReferencedTypes).OfType<GenericInstanceType>())
            {
                if (genericType.HasGenericParameters || genericType.ContainsGenericParameter)
                {
                    continue;
                }

                var typeDef = genericType.TryResolve();

                if (typeDef == null)
                {
                    continue;
                }

                List<TypeReference> genericTypeList;

                if (!allGenericInstances.TryGetValue(typeDef.FullName, out genericTypeList))
                {
                    genericTypeList = new List<TypeReference>();
                    allGenericInstances.Add(typeDef.FullName, genericTypeList);
                }

                genericTypeList.Add(genericType);
            }

            var typesToRegister = new List<TypeDefAndRef>();

            foreach (var typeDefAndType in edittedTypes)
            {
                var typeDef = typeDefAndType.TypeDefinition;

                // Should either be generic type definitions or non generic types
                Assert.That(!typeDef.ContainsGenericParameter);

                // Can't do much in this case because we need to be able to reference the class from
                // the baking entry point
                // They will still have the CreateInjectTypeInfo method which will still be better
                // than direct reflection
                //if (GetNestParentsAndSelf(typeDefAndType.ActualType).Any(x => !x.IsPublic && !x.IsNestedPublic))
                //{
                    //continue;
                //}

                if (typeDef.HasGenericParameters)
                {
                    List<TypeReference> genericInstances;

                    if (allGenericInstances.TryGetValue(typeDef.FullName, out genericInstances))
                    {
                        var processedTypes = new HashSet<string>();

                        foreach (var genericInstance in genericInstances)
                        {
                            // It seems that IsEqual is not implemented for type reference, so
                            // we use string comparison on the full name instead
                            if (processedTypes.Contains(genericInstance.FullName))
                            {
                                continue;
                            }

                            processedTypes.Add(genericInstance.FullName);

                            typesToRegister.Add(
                                new TypeDefAndRef(typeDef, module.Import(genericInstance)));
                        }
                    }
                }
                else
                {
                    typesToRegister.Add(
                        new TypeDefAndRef(typeDef, module.Import(typeDef)));
                }
            }

            foreach (var info in typesToRegister)
            {
                processor.Emit(OpCodes.Ldtoken, info.TypeReference);
                processor.Emit(OpCodes.Call, getTypeFromHandleMethod);

                var methodDef = info.TypeDefinition.GetMethod(
                    TypeAnalyzer.ReflectionBakingGetInjectInfoMethodName);

                var methodRef = module.Import(methodDef)
                    .ChangeDeclaringType(info.TypeReference);

                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ldftn, methodRef);
                processor.Emit(OpCodes.Newobj, funcConstructor);

                processor.Emit(OpCodes.Call, registerMethod);
            }

            processor.Emit(OpCodes.Ret);
        }

        IEnumerable<TypeReference> GetParentTypesInModules(TypeReference typeRef)
        {
            if (typeRef.TryResolve() == null)
            {
                // This happens for things like MonoBehaviour since our assembly resolver
                // isn't used for that
                yield break;
            }

            var baseType = typeRef.TryGetSpecificBaseType();

            if (baseType != null)
            {
                yield return baseType;

                foreach (var ancestorType in GetParentTypesInModules(baseType))
                {
                    yield return ancestorType;
                }
            }
        }

        IEnumerable<TypeReference> FindAllReferencedTypes(TypeDefinition typeDef)
        {
            if (typeDef.HasGenericParameters || typeDef.ContainsGenericParameter)
            {
                return Enumerable.Empty<TypeReference>();
            }

            return typeDef.Fields.Select(x => x.FieldType)
                .Concat(typeDef.Properties.Select(x => x.PropertyType))
                .Concat(GetParentTypesInModules(typeDef))
                .Concat(typeDef.Methods.SelectMany(FindAllReferencedTypes));
        }

        IEnumerable<TypeReference> FindAllReferencedTypes(MethodDefinition method)
        {
            yield return method.ReturnType;

            foreach (var param in method.Parameters)
            {
                yield return param.ParameterType;
            }

            if (method.Body != null)
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Ldtoken)
                    {
                        var typeRef = instruction.Operand as TypeReference;

                        if (typeRef != null)
                        {
                            yield return typeRef;
                        }
                    }
                    else if (instruction.OpCode == OpCodes.Call
                        || instruction.OpCode == OpCodes.Callvirt)
                    {
                        var methodRef = instruction.Operand as GenericInstanceMethod;

                        if (methodRef != null)
                        {
                            foreach (var arg in methodRef.GenericArguments)
                            {
                                yield return arg;
                            }
                        }
                    }
                }
            }
        }

        Regex CreateRegex(string regexStr)
        {
            return new Regex(regexStr, RegexOptions.Compiled);
        }

        void WriteModules()
        {
            var writerParameters = new WriterParameters()
            {
                WriteSymbols = true
            };

            long? newLastUpdateTimestamp = null;

            foreach (var data in _moduleData)
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

        void LoadModules()
        {
            var assemblyPaths = new List<string>();

            foreach (var relativePath in _settings.WeavedAssemblies)
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

            Assert.That(_moduleData.IsEmpty());

            for (int i = 0; i < assemblyPaths.Count; i++)
            {
                var data = new ModuleData();

                data.AbsolutePath = assemblyPaths[i];
                data.Module = ModuleDefinition.ReadModule(
                    data.AbsolutePath, readerParameters);

                _moduleData.Add(data);
            }
        }

        class TypeDefAndType
        {
            public readonly TypeDefinition TypeDefinition;
            public readonly Type ActualType;

            public TypeDefAndType(TypeDefinition typeDefinition, Type actualType)
            {
                TypeDefinition = typeDefinition;
                ActualType = actualType;
            }
        }

        class TypeDefAndRef
        {
            public TypeDefinition TypeDefinition;
            public TypeReference TypeReference;

            public TypeDefAndRef(
                TypeDefinition typeDefinition, TypeReference typeReference)
            {
                TypeDefinition = typeDefinition;
                TypeReference = typeReference;
            }
        }

        class ModuleData
        {
            public bool WasModified;
            public ModuleDefinition Module;
            public string AbsolutePath;
        }
    }
}
