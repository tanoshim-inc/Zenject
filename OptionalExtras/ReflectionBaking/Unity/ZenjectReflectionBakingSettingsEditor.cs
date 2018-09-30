using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Zenject.ReflectionBaking
{
    [CustomEditor(typeof(ZenjectReflectionBakingSettings))]
    public class ZenjectReflectionBakingSettingsEditor : Editor
    {
        SerializedProperty _weavedAssemblies;
        SerializedProperty _namespacePatterns;
        SerializedProperty _isEnabledInBuilds;
        SerializedProperty _isEnabledInEditor;
        SerializedProperty _allGeneratedAssemblies;

        // Lists
        ReorderableList _weavedAssembliesList;
        ReorderableList _namespacePatternsList;

        // Layouts
        Vector2 _logScrollPosition;
        int _selectedLogIndex;

        bool _hasModifiedProperties;

        static GUIContent _weavedAssembliesListHeaderContent = new GUIContent
        {
            text = "Weaved Assemblies",
            tooltip = "The list of all the assemblies that will be editted to have reflection information directly embedded"
        };

        static GUIContent _namespacePatternListHeaderContent = new GUIContent
        {
            text = "Namespace Patterns",
            tooltip = "This list of Regex patterns will be compared to the name of each type in the given assemblies, and when a match is found that type will be editting to directly contain reflection information"
        };

        void OnEnable()
        {
            _weavedAssemblies = serializedObject.FindProperty("_weavedAssemblies");
            _namespacePatterns = serializedObject.FindProperty("_namespacePatterns");
            _isEnabledInEditor = serializedObject.FindProperty("_isEnabledInEditor");
            _isEnabledInBuilds = serializedObject.FindProperty("_isEnabledInBuilds");
            _allGeneratedAssemblies = serializedObject.FindProperty("_allGeneratedAssemblies");

            _namespacePatternsList = new ReorderableList(serializedObject, _namespacePatterns);
            _namespacePatternsList.drawHeaderCallback += OnNamespacePatternsDrawHeader;
            _namespacePatternsList.drawElementCallback += OnNamespacePatternsDrawElement;

            _weavedAssembliesList = new ReorderableList(serializedObject, _weavedAssemblies);
            _weavedAssembliesList.drawHeaderCallback += OnWeavedAssemblyDrawHeader;
            _weavedAssembliesList.onAddCallback += OnWeavedAssemblyElementAdded;
            _weavedAssembliesList.drawElementCallback += OnAssemblyListDrawElement;
        }

        void OnNamespacePatternsDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty indexProperty = _namespacePatterns.GetArrayElementAtIndex(index);
            indexProperty.stringValue = EditorGUI.TextField(rect, indexProperty.stringValue);
        }

        void OnAssemblyListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty indexProperty = _weavedAssemblies.GetArrayElementAtIndex(index);
            EditorGUI.LabelField(rect, indexProperty.stringValue, EditorStyles.textArea);
        }

        void OnNamespacePatternsDrawHeader(Rect rect)
        {
            GUI.Label(rect, _namespacePatternListHeaderContent);
        }

        void OnWeavedAssemblyDrawHeader(Rect rect)
        {
            GUI.Label(rect, _weavedAssembliesListHeaderContent);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Label("Settings", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(_isEnabledInBuilds, true);
                EditorGUILayout.PropertyField(_isEnabledInEditor, true);

#if !UNITY_2018
                if (_isEnabledInEditor.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Reflection baking inside unity editor requires Unity 2018+!  It is however supported for builds", MessageType.Error);
                }
#endif
                EditorGUILayout.PropertyField(_allGeneratedAssemblies, true);

                if (_allGeneratedAssemblies.boolValue)
                {
                    GUI.enabled = false;

                    try
                    {
                        _weavedAssembliesList.DoLayoutList();
                    }
                    finally
                    {
                        GUI.enabled = true;
                    }
                }
                else
                {
                    _weavedAssembliesList.DoLayoutList();
                }

                _namespacePatternsList.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _hasModifiedProperties = true;
            }

            if (_hasModifiedProperties)
            {
                _hasModifiedProperties = false;
                ApplyModifiedProperties();
            }
        }

        void ApplyModifiedProperties()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        void OnWeavedAssemblyElementAdded(ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            var paths = AssemblyPathRegistry.GetAllGeneratedAssemblyRelativePaths();

            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];

                bool foundMatch = false;

                for (int k = 0; k < _weavedAssemblies.arraySize; k++)
                {
                    SerializedProperty current = _weavedAssemblies.GetArrayElementAtIndex(k);

                    if (path == current.stringValue)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    GUIContent content = new GUIContent(path);
                    menu.AddItem(content, false, OnWeavedAssemblyAdded, path);
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("[All Assemblies Added]"));
            }

            menu.ShowAsContext();
        }

        void OnWeavedAssemblyAdded(object path)
        {
            _weavedAssemblies.arraySize++;
            SerializedProperty weaved = _weavedAssemblies.GetArrayElementAtIndex(_weavedAssemblies.arraySize - 1);
            weaved.stringValue = ((string)path).Replace("\\", "/");
            ApplyModifiedProperties();
        }
    }
}
