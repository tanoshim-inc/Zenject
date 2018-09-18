using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ModestTree;
using UnityEngine;
using UnityEditor;

namespace Zenject.ReflectionBaking
{
    [CreateAssetMenu(menuName = "Zenject/Reflection Baking Settings", fileName = "Zenject Reflection Baking Settings")]
    public class ZenjectReflectionBakingSettings : ScriptableObject
    {
        [SerializeField]
        ExecutionModes _executionMode;

        [SerializeField]
        bool _allAssemblies;

        [SerializeField]
        List<string> _weavedAssemblies;

        [SerializeField]
        List<string> _namespacePatterns;

        public bool AllAssemblies
        {
            get { return _allAssemblies; }
        }

        public List<string> NamespacePatterns
        {
            get { return _namespacePatterns; }
        }

        public List<string> WeavedAssemblies
        {
            get { return _weavedAssemblies; }
        }

        public ExecutionModes ExecutionMode
        {
            get { return _executionMode; }
        }

        [UsedImplicitly]
        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        void OnBeforeAssemblyReload()
        {
#if UNITY_2018
            if (_executionMode == ExecutionModes.InEditorAndBuilds || _executionMode == ExecutionModes.InEditorOnly)
            {
                UnityReflectionBakingRunner.Run(this);
            }
#endif
        }

        public enum ExecutionModes
        {
            Disabled,
            InEditorAndBuilds,
            InEditorOnly,
            BuildsOnly,
        }
    }
}
