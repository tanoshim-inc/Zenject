using System;
using System.Reflection;
using ModestTree;
using Zenject.Internal;

namespace Zenject.Internal
{
    // An injectable is a field or property with [Inject] attribute
    // Or a constructor parameter
    public class InjectableInfo
    {
        public readonly bool Optional;
        public readonly object Identifier;

        public readonly InjectSources SourceType;

        // The field name or property name from source code
        public readonly string MemberName;
        // The field type or property type from source code
        public readonly Type MemberType;

        public readonly Type ObjectType;

        public readonly object DefaultValue;

        public InjectableInfo(
            bool optional, object identifier, string memberName, Type memberType,
            Type objectType, object defaultValue, InjectSources sourceType)
        {
            Optional = optional;
            ObjectType = objectType;
            MemberType = memberType;
            MemberName = memberName;
            Identifier = identifier;
            DefaultValue = defaultValue;
            SourceType = sourceType;
        }

        public InjectContext SpawnInjectContext(
            DiContainer container, InjectContext currentContext, 
            object targetInstance, Type targetType, object concreteIdentifier)
        {
            var context = ZenPools.SpawnInjectContext(container, MemberType);

            Assert.That(targetType.DerivesFromOrEqual(ObjectType));

            context.ObjectType = targetType;
            context.ParentContext = currentContext;
            context.ObjectInstance = targetInstance;
            context.Identifier = Identifier;
            context.MemberName = MemberName;
            context.Optional = Optional;
            context.SourceType = SourceType;
            context.FallBackValue = DefaultValue;
            context.ConcreteIdentifier = concreteIdentifier;

            return context;
        }
    }
}
