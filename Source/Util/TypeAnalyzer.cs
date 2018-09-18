using System.Diagnostics;
using System.Linq.Expressions;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if !NOT_UNITY3D
using UnityEngine;
#endif

using Zenject.Internal;

namespace Zenject
{
    public enum ReflectionBakingCoverageModes
    {
        FallbackToDirectReflection,
        NoCheckAssumeFullCoverage,
        FallbackToDirectReflectionWithWarning,
    }

    public static class TypeAnalyzer
    {
        static Dictionary<Type, InjectTypeInfo> _typeInfo = new Dictionary<Type, InjectTypeInfo>();

#if UNITY_EDITOR
        // We store this separately from InjectTypeInfo because this flag is needed for contract
        // types whereas InjectTypeInfo is only needed for types that are instantiated, and
        // we want to minimize the types that generate InjectTypeInfo for
        static Dictionary<Type, bool> _allowDuringValidation = new Dictionary<Type, bool>();
#endif

        public const string ReflectionBakingGetInjectInfoMethodName = "__zenGetTypeInfo";

        public static ReflectionBakingCoverageModes ReflectionBakingCoverageMode
        {
            get; set;
        }

        public static bool ShouldAllowDuringValidation<T>()
        {
            return ShouldAllowDuringValidation(typeof(T));
        }

#if !UNITY_EDITOR
        public static bool ShouldAllowDuringValidation(Type type)
        {
            return false;
        }
#else
        public static bool ShouldAllowDuringValidation(Type type)
        {
            bool shouldAllow;

            if (!_allowDuringValidation.TryGetValue(type, out shouldAllow))
            {
                shouldAllow = ShouldAllowDuringValidationInternal(type);
                _allowDuringValidation.Add(type, shouldAllow);
            }

            return shouldAllow;
        }

        static bool ShouldAllowDuringValidationInternal(Type type)
        {
            // During validation, do not instantiate or inject anything except for
            // Installers, IValidatable's, or types marked with attribute ZenjectAllowDuringValidation
            // You would typically use ZenjectAllowDuringValidation attribute for data that you
            // inject into factories

            if (type.DerivesFrom<IInstaller>() || type.DerivesFrom<IValidatable>())
            {
                return true;
            }

#if !NOT_UNITY3D
            if (type.DerivesFrom<Context>())
            {
                return true;
            }
#endif

            return type.HasAttribute<ZenjectAllowDuringValidationAttribute>();
        }
#endif

        public static InjectTypeInfo GetInfo<T>()
        {
            return GetInfo(typeof(T));
        }

        public static InjectTypeInfo GetInfo(Type type)
        {
            var info = TryGetInfo(type);
            Assert.IsNotNull(info, "Unable to get type info for type '{0}'", type);
            return info;
        }

        public static InjectTypeInfo TryGetInfo<T>()
        {
            return TryGetInfo(typeof(T));
        }

        public static InjectTypeInfo TryGetInfo(Type type)
        {
#if UNITY_EDITOR
            using (ProfileBlock.Start("Zenject Reflection"))
#endif
            {
                InjectTypeInfo info;

#if ZEN_MULTITHREADING
                lock (_typeInfo)
#endif
                {
                    if (_typeInfo.TryGetValue(type, out info))
                    {
                        return info;
                    }
                }

                info = GetInfoInternal(type);

#if ZEN_MULTITHREADING
                lock (_typeInfo)
#endif
                {
                    _typeInfo.Add(type, info);
                }

                return info;
            }
        }

        static InjectTypeInfo GetInfoInternal(Type type)
        {
            if (!ShouldAnalyzeType(type))
            {
                return null;
            }

            var getInfoMethod = type.GetMethod(
                ReflectionBakingGetInjectInfoMethodName, BindingFlags.Static | BindingFlags.NonPublic);

            // Try to get the reflection info from the reflection baked method first
            // before resorting to more detailed reflection
            if (getInfoMethod != null)
            {
                var infoGetter = ((Func<InjectTypeInfo>)Delegate.CreateDelegate(
                    typeof(Func<InjectTypeInfo>), getInfoMethod));

                if (infoGetter != null)
                {
                    return infoGetter();
                }
            }

            if (ReflectionBakingCoverageMode == ReflectionBakingCoverageModes.NoCheckAssumeFullCoverage)
            {
                // If we are confident that the reflection baking supplies all the injection information,
                // then we can avoid the costs of doing reflection on types that were not covered
                // by the baking
                return null;
            }

            if (ReflectionBakingCoverageMode == ReflectionBakingCoverageModes.FallbackToDirectReflectionWithWarning)
            {
                Log.Warn("No reflection baking information found for type '{0}' - using more costly direct reflection instead", type);
            }

            return CreateTypeInfoFromReflection(type);
        }

        public static bool ShouldAnalyzeType(Type type)
        {
            if (type == null || type.IsEnum || type.IsArray || type.IsInterface()
                || type.ContainsGenericParameters || IsStaticType(type) 
                || type == typeof(object))
            {
                return false;
            }

            return ShouldAnalyzeNamespace(type.Namespace);
        }

        static bool IsStaticType(Type type)
        {
            // Apparently this is unique to static classes
            return type.IsAbstract && type.IsSealed;
        }

        public static bool ShouldAnalyzeNamespace(string ns)
        {
            return ns != null && ns != "System" && !ns.StartsWith("System.")
                && ns != "UnityEngine" && !ns.StartsWith("UnityEngine.")
                && ns != "UnityEditor" && !ns.StartsWith("UnityEditor.")
                && ns != "UnityStandardAssets" && !ns.StartsWith("UnityStandardAssets.");
        }

        static InjectTypeInfo CreateTypeInfoFromReflection(Type type)
        {
            var reflectionInfo = ReflectionTypeAnalyzer.GetReflectionInfo(type);

            InjectTypeInfo baseTypeInfo = null;

            if (reflectionInfo.BaseType != null && ShouldAnalyzeType(reflectionInfo.BaseType))
            {
                baseTypeInfo = TypeAnalyzer.TryGetInfo(reflectionInfo.BaseType);
            }

            var injectConstructor = ReflectionInfoTypeInfoConverter.ConvertConstructor(
                reflectionInfo.InjectConstructor, type);

            var injectMethods = reflectionInfo.InjectMethods.Select(
                ReflectionInfoTypeInfoConverter.ConvertMethod).ToArray();

            var memberInfos = reflectionInfo.InjectFields.Select(
                x => ReflectionInfoTypeInfoConverter.ConvertField(type, x)).Concat(
                    reflectionInfo.InjectProperties.Select(
                        x => ReflectionInfoTypeInfoConverter.ConvertProperty(type, x))).ToArray();

            return new InjectTypeInfo(
                type, injectConstructor, injectMethods, memberInfos, baseTypeInfo);
        }
    }
}
