using System.Linq.Expressions;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zenject
{
    public static class TypeAnalyzer
    {
        static readonly HashSet<Type> _injectAttributeTypes;
        static Dictionary<Type, ZenjectTypeInfo> _typeInfo = new Dictionary<Type, ZenjectTypeInfo>();

#if UNITY_EDITOR
        // We store this separately from ZenjectTypeInfo because this flag is needed for contract
        // types whereas ZenjectTypeInfo is only needed for types that are instantiated, and
        // we want to minimize the types that generate ZenjectTypeInfo for
        static Dictionary<Type, bool> _allowDuringValidation = new Dictionary<Type, bool>();
#endif

        static TypeAnalyzer()
        {
            _injectAttributeTypes = new HashSet<Type>();
            _injectAttributeTypes.Add(typeof(InjectAttributeBase));
        }

        public static ZenjectTypeInfo GetInfo<T>()
        {
            return GetInfo(typeof(T));
        }

        public static void AddCustomInjectAttribute<T>()
            where T : Attribute
        {
            AddCustomInjectAttribute(typeof(T));
        }

        public static void AddCustomInjectAttribute(Type type)
        {
            Assert.That(type.DerivesFrom<Attribute>());
            _injectAttributeTypes.Add(type);
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

        public static ZenjectTypeInfo GetInfo(Type type)
        {
#if UNITY_EDITOR
            using (ProfileBlock.Start("Zenject Reflection"))
#endif
            {
                Assert.That(!type.IsAbstract(),
                    "Tried to analyze abstract type '{0}'.  This is not currently allowed.", type);

                ZenjectTypeInfo info;

#if ZEN_MULTITHREADING
                lock (_typeInfo)
#endif
                {
                    if (!_typeInfo.TryGetValue(type, out info))
                    {
                        info = CreateTypeInfo(type);
                        _typeInfo.Add(type, info);
                    }
                }

                return info;
            }
        }

        static ZenjectTypeInfo CreateTypeInfo(Type type)
        {
            var constructor = GetInjectConstructor(type);

            return new ZenjectTypeInfo(
                type,
                GetPostInjectMethods(type),
                constructor,
                CreateFactoryMethod(type, constructor),
                GetFieldInjectables(type).ToList(),
                GetPropertyInjectables(type).ToList(),
                GetConstructorInjectables(type, constructor).ToList());
        }

        static Func<object[], object> CreateFactoryMethod(
            Type type, ConstructorInfo constructor)
        {
            ParameterInfo[] par = constructor.GetParameters();
            Expression[] args = new Expression[par.Length];
            ParameterExpression param = Expression.Parameter(typeof(object[]));

            for (int i = 0; i != par.Length; ++i)
            {
                args[i] = Expression.Convert(
                    Expression.ArrayIndex(
                        param, Expression.Constant(i)), par[i].ParameterType);
            }

            return Expression.Lambda<Func<object[], object>>(
                Expression.Convert(
                    Expression.New(constructor, args), typeof(object)), param).Compile();
        }

        static IEnumerable<InjectableInfo> GetConstructorInjectables(Type parentType, ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                return Enumerable.Empty<InjectableInfo>();
            }

            return constructorInfo.GetParameters().Select(
                paramInfo => CreateInjectableInfoForParam(parentType, paramInfo));
        }

        static InjectableInfo CreateInjectableInfoForParam(
            Type parentType, ParameterInfo paramInfo)
        {
            var injectAttributes = paramInfo.AllAttributes<InjectAttributeBase>().ToList();

            Assert.That(injectAttributes.Count <= 1,
                "Found multiple 'Inject' attributes on type parameter '{0}' of type '{1}'.  Parameter should only have one", paramInfo.Name, parentType);

            var injectAttr = injectAttributes.SingleOrDefault();

            object identifier = null;
            bool isOptional = false;
            InjectSources sourceType = InjectSources.Any;

            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
                isOptional = injectAttr.Optional;
                sourceType = injectAttr.Source;
            }

            bool isOptionalWithADefaultValue = (paramInfo.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;

            return new InjectableInfo(
                isOptionalWithADefaultValue || isOptional,
                identifier,
                paramInfo.Name,
                paramInfo.ParameterType,
                parentType,
                null,
                isOptionalWithADefaultValue ? paramInfo.DefaultValue : null,
                sourceType);
        }

        static List<PostInjectableInfo> GetPostInjectMethods(Type type)
        {
            // Note that unlike with fields and properties we use GetCustomAttributes
            // This is so that we can ignore inherited attributes, which is necessary
            // otherwise a base class method marked with [Inject] would cause all overridden
            // derived methods to be added as well
            var methods = type.GetAllInstanceMethods()
                .Where(x => _injectAttributeTypes.Any(a => x.GetCustomAttributes(a, false).Any())).ToList();

            var heirarchyList = type.Yield().Concat(type.GetParentTypes()).Reverse().ToList();

            // Order by base classes first
            // This is how constructors work so it makes more sense
            var values = methods.OrderBy(x => heirarchyList.IndexOf(x.DeclaringType));

            var postInjectInfos = new List<PostInjectableInfo>();

            foreach (var methodInfo in values)
            {
                var paramsInfo = methodInfo.GetParameters();

                var injectAttr = methodInfo.AllAttributes<InjectAttributeBase>().SingleOrDefault();

                if (injectAttr != null)
                {
                    Assert.That(!injectAttr.Optional && injectAttr.Id == null && injectAttr.Source == InjectSources.Any,
                        "Parameters of InjectAttribute do not apply to constructors and methods");
                }

                postInjectInfos.Add(
                    new PostInjectableInfo(
                        methodInfo,
                        CreateActionForMethod(methodInfo),
                        paramsInfo.Select(paramInfo =>
                            CreateInjectableInfoForParam(type, paramInfo)).ToList()));
            }

            return postInjectInfos;
        }

        static Action<object[], object> CreateActionForMethod(MethodInfo methodInfo)
        {
            ParameterInfo[] par = methodInfo.GetParameters();
            Expression[] args = new Expression[par.Length];
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]));
            ParameterExpression instanceParam = Expression.Parameter(typeof(object));

            for (int i = 0; i != par.Length; ++i)
            {
                args[i] = Expression.Convert(
                    Expression.ArrayIndex(
                        argsParam, Expression.Constant(i)), par[i].ParameterType);
            }

            return Expression.Lambda<Action<object[], object>>(
                Expression.Call(
                    Expression.Convert(instanceParam, methodInfo.DeclaringType), methodInfo, args),
                argsParam, instanceParam).Compile();
        }

        static IEnumerable<InjectableInfo> GetPropertyInjectables(Type type)
        {
            var propInfos = type.GetAllInstanceProperties()
                .Where(x => _injectAttributeTypes.Any(a => x.HasAttribute(a)));

            foreach (var propInfo in propInfos)
            {
                yield return CreateForMember(propInfo, type);
            }
        }

        static IEnumerable<InjectableInfo> GetFieldInjectables(Type type)
        {
            var fieldInfos = type.GetAllInstanceFields()
                .Where(x => _injectAttributeTypes.Any(a => x.HasAttribute(a)));

            foreach (var fieldInfo in fieldInfos)
            {
                yield return CreateForMember(fieldInfo, type);
            }
        }

#if !(UNITY_WSA && ENABLE_DOTNET) || UNITY_EDITOR
        private static IEnumerable<FieldInfo> GetAllFields(Type t, BindingFlags flags)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            return t.GetFields(flags).Concat(GetAllFields(t.BaseType, flags)).Distinct();
        }

        private static Action<object, object> GetOnlyPropertySetter(
            Type parentType,
            string propertyName)
        {
            Assert.That(parentType != null);
            Assert.That(!string.IsNullOrEmpty(propertyName));

            var allFields = GetAllFields(
                parentType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();

            var writeableFields = allFields.Where(f => f.Name == string.Format("<{0}>k__BackingField", propertyName)).ToList();

            if (!writeableFields.Any())
            {
                throw new ZenjectException(string.Format(
                    "Can't find backing field for get only property {0} on {1}.\r\n{2}",
                    propertyName, parentType.FullName, string.Join(";", allFields.Select(f => f.Name).ToArray())));
            }

            return (injectable, value) => writeableFields.ForEach(f => f.SetValue(injectable, value));
        }
#endif

        static InjectableInfo CreateForMember(MemberInfo memInfo, Type parentType)
        {
            var injectAttributes = memInfo.AllAttributes<InjectAttributeBase>().ToList();

            Assert.That(injectAttributes.Count <= 1,
            "Found multiple 'Inject' attributes on type field '{0}' of type '{1}'.  Field should only container one Inject attribute", memInfo.Name, parentType);

            var injectAttr = injectAttributes.SingleOrDefault();

            object identifier = null;
            bool isOptional = false;
            InjectSources sourceType = InjectSources.Any;

            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
                isOptional = injectAttr.Optional;
                sourceType = injectAttr.Source;
            }

            Type memberType = memInfo is FieldInfo
                ? ((FieldInfo)memInfo).FieldType : ((PropertyInfo)memInfo).PropertyType;

            var setter = GetSetter(parentType, memInfo);

            return new InjectableInfo(
                isOptional,
                identifier,
                memInfo.Name,
                memberType,
                parentType,
                setter,
                null,
                sourceType);
        }

        static Action<object, object> GetSetter(Type parentType, MemberInfo memInfo)
        {
            var fieldInfo = memInfo as FieldInfo;
            var propInfo = memInfo as PropertyInfo;

            Type memberType = fieldInfo != null
                ? fieldInfo.FieldType : propInfo.PropertyType;

            // It seems that for readonly fields, we have to use the slower approach below
            // As discussed here: https://www.productiverage.com/trying-to-set-a-readonly-autoproperty-value-externally-plus-a-little-benchmarkdotnet
            if (fieldInfo == null || !fieldInfo.IsInitOnly)
            {
                var typeParam = Expression.Parameter(typeof(object));
                var valueParam = Expression.Parameter(typeof(object));

                return Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.MakeMemberAccess(Expression.Convert(typeParam, parentType), memInfo),
                        Expression.Convert(valueParam, memberType)),
                        typeParam, valueParam).Compile();
            }

            if (fieldInfo != null)
            {
                return ((object injectable, object value) => fieldInfo.SetValue(injectable, value));
            }

            Assert.IsNotNull(propInfo);

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return ((object injectable, object value) => propInfo.SetValue(injectable, value, null));
#else
            if (propInfo.CanWrite)
            {
                return ((object injectable, object value) => propInfo.SetValue(injectable, value, null));
            }

            return GetOnlyPropertySetter(parentType, propInfo.Name);
#endif
        }

        static ConstructorInfo GetInjectConstructor(Type parentType)
        {
            var constructors = parentType.Constructors();

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            // WP8 generates a dummy constructor with signature (internal Classname(UIntPtr dummy))
            // So just ignore that
            constructors = constructors.Where(c => !IsWp8GeneratedConstructor(c)).ToArray();
#endif

            if (constructors.IsEmpty())
            {
                return null;
            }

            if (constructors.HasMoreThan(1))
            {
                var explicitConstructor = (from c in constructors where _injectAttributeTypes.Any(a => c.HasAttribute(a)) select c).SingleOrDefault();

                if (explicitConstructor != null)
                {
                    return explicitConstructor;
                }

                // If there is only one public constructor then use that
                // This makes decent sense but is also necessary on WSA sometimes since the WSA generated
                // constructor can sometimes be private with zero parameters
                var singlePublicConstructor = constructors.Where(x => x.IsPublic).OnlyOrDefault();

                if (singlePublicConstructor != null)
                {
                    return singlePublicConstructor;
                }

                // Choose the one with the least amount of arguments
                // This might result in some non obvious errors like null reference exceptions
                // but is probably the best trade-off since it allows zenject to be more compatible
                // with libraries that don't depend on zenject at all
                // Discussion here - https://github.com/modesttree/Zenject/issues/416
                return constructors.OrderBy(x => x.GetParameters().Count()).First();
            }

            return constructors[0];
        }

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
        static bool IsWp8GeneratedConstructor(ConstructorInfo c)
        {
            ParameterInfo[] args = c.GetParameters();

            if (args.Length == 1)
            {
                return args[0].ParameterType == typeof(UIntPtr)
                    && (string.IsNullOrEmpty(args[0].Name) || args[0].Name == "dummy");
            }

            if (args.Length == 2)
            {
                return args[0].ParameterType == typeof(UIntPtr)
                    && args[1].ParameterType == typeof(Int64*)
                    && (string.IsNullOrEmpty(args[0].Name) || args[0].Name == "dummy")
                    && (string.IsNullOrEmpty(args[1].Name) || args[1].Name == "dummy");
            }

            return false;
        }
#endif
    }
}
