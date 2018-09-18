using System;
using System.Reflection;
using ModestTree;
using UnityEngine;

namespace Zenject.ReflectionBaking
{
    public static class ReflectionBakingInternalUtil
    {
        public static string ConvertAbsoluteToAssetPath(string systemPath)
        {
            var projectPath = Application.dataPath;

            // Remove 'Assets'
            projectPath = projectPath.Substring(0, projectPath.Length - /* Assets */ 6);

            int systemPathLength = systemPath.Length;
            int assetPathLength = systemPathLength - projectPath.Length;

            Assert.That(assetPathLength > 0, "Unexpect path '{0}'", systemPath);

            return systemPath.Substring(projectPath.Length, assetPathLength);
        }

        public static void DirtyAllScripts()
        {
            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;

            Type compilationInterface = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");

            if (compilationInterface != null)
            {
                BindingFlags staticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                MethodInfo dirtyAllScriptsMethod = compilationInterface.GetMethod("DirtyAllScripts", staticBindingFlags);
                dirtyAllScriptsMethod.Invoke(null, null);
            }

            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
