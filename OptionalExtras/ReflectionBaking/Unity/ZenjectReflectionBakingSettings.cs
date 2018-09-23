using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ModestTree;
using UnityEngine;
using UnityEditor;

namespace Zenject.ReflectionBaking
{
    public class ZenjectReflectionBakingSettings : ScriptableObject
    {
        [SerializeField]
        bool _isEnabled;

        [SerializeField]
        bool _allGeneratedAssemblies;

        [SerializeField]
        List<string> _weavedAssemblies;

        [SerializeField]
        List<string> _namespacePatterns;

        public List<string> NamespacePatterns
        {
            get { return _namespacePatterns; }
        }

        public List<string> WeavedAssemblies
        {
            get { return _weavedAssemblies; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        public bool AllGeneratedAssemblies
        {
            get { return _allGeneratedAssemblies; }
        }
    }
}
