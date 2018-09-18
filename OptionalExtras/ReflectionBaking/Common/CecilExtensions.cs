using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Mono.Cecil;
using System;
using Mono.Collections.Generic;

namespace Zenject.ReflectionBaking
{
    public static class CecilExtensions
    {
        public static Type TryGetActualType(this TypeReference typeRef, System.Reflection.Assembly assembly)
        {
            var reflectionName = GetReflectionName(typeRef);
            return assembly.GetType(reflectionName);
        }

        static string GetReflectionName(this TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)type;

                return string.Format(
                    "{0}.{1}[{2}]", genericInstance.Namespace, type.Name,
                    String.Join(",", genericInstance.GenericArguments.Select(p => GetReflectionName(p)).ToArray()));
            }

            return type.FullName;
        }

        public static List<TypeDefinition> LoopupAllTypes(this ModuleDefinition module)
        {
            var allTypes = new List<TypeDefinition>();

            foreach (var type in module.Types)
            {
                LoopupAllTypesInternal(type, allTypes);
            }

            return allTypes;
        }

        static void LoopupAllTypesInternal(TypeDefinition type, List<TypeDefinition> buffer)
        {
            buffer.Add(type);

            foreach (var nestedType in type.NestedTypes)
            {
                LoopupAllTypesInternal(nestedType, buffer);
            }
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.CompareOrdinal(methodDef.Name, name) == 0)
                {
                    return methodDef;
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params Type[] parameterTypes)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDefinition = instance.Methods[i];

                if (!string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) ||
                    parameterTypes.Length != methodDefinition.Parameters.Count)
                {
                    continue;
                }

                MethodDefinition result = methodDefinition;
                for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                {
                    ParameterDefinition parameter = methodDefinition.Parameters[x];
                    if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                    {
                        break;
                    }

                    if (x == 0)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params TypeReference[] parameterTypes)
        {
            if (instance.Methods != null)
            {
                for (int i = 0; i < instance.Methods.Count; i++)
                {
                    MethodDefinition methodDefinition = instance.Methods[i];
                    if (string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) // Names Match
                        && parameterTypes.Length == methodDefinition.Parameters.Count) // The same number of parameters
                    {
                        MethodDefinition result = methodDefinition;
                        for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                        {
                            ParameterDefinition parameter = methodDefinition.Parameters[x];
                            if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                            {
                                break;
                            }

                            if (x == 0)
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, int argCount)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.CompareOrdinal(methodDef.Name, name) == 0 && methodDef.Parameters.Count == argCount)
                {
                    return methodDef;
                }
            }
            return null;
        }

        public static PropertyDefinition GetProperty(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Properties.Count; i++)
            {
                PropertyDefinition preopertyDef = instance.Properties[i];

                // Properties can only have one argument or they are an indexer.
                if (string.CompareOrdinal(preopertyDef.Name, name) == 0 && preopertyDef.Parameters.Count == 0)
                {
                    return preopertyDef;
                }
            }
            return null;
        }

        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider instance)
        {
            if (!instance.HasCustomAttributes) return false;

            Collection<CustomAttribute> attributes = instance.CustomAttributes;

            for(int i = 0;  i < attributes.Count; i++)
            {
                if(attributes[i].AttributeType.FullName.Equals(typeof(T).FullName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        public static CustomAttribute GetCustomAttribute<T>(this ICustomAttributeProvider instance)
        {
            if (!instance.HasCustomAttributes) return null;

            Collection<CustomAttribute> attributes = instance.CustomAttributes;

            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].AttributeType.FullName.Equals(typeof(T).FullName, StringComparison.Ordinal))
                {
                    return attributes[i];
                }
            }
            return null;
        }
    }
}
