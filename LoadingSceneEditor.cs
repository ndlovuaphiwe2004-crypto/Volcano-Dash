csharp Assets/Loading Scene/Editor/LoadingSceneEditor.cs
// Pseudocode / Plan:
// 1. Create a CustomEditor for the `LoadingScene` MonoBehaviour so users can pick a scene from a Build Settings dropdown.
// 2. OnEnable: cache the serialized `sceneToLoad` property and build an array of scene names from EditorBuildSettings.scenes.
// 3. In OnInspectorGUI: draw the original `sceneToLoad` string field for manual edits, then draw a popup populated with build scene names.
// 4. When the popup selection changes, write the selected scene name back to the `sceneToLoad` serialized property and apply changes.
// 5. Show helpful warnings if there are no scenes in Build Settings.
// This file must live in an `Editor` folder so Unity compiles it into the editor assembly.

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoadingScene))]
[CanEditMultipleObjects]
internal class LoadingSceneEditor : Editor
{
    private SerializedProperty sceneToLoadProp;
    private string[] sceneNames = Array.Empty<string>();
    private int selectedIndex = 0;

    private void OnEnable()
    {
        sceneToLoadProp = serializedObject.FindProperty("sceneToLoad");
        BuildSceneNameList();
        SyncSelectedIndexWithProperty();
    }

    private void BuildSceneNameList()
    {
        var buildScenes = EditorBuildSettings.scenes;
        var names = new List<string>(buildScenes.Length);
        foreach (var buildScene in buildScenes)
        {
            // Include even disabled scenes so user sees everything in Build Settings.
            if (string.IsNullOrEmpty(buildScene.path))
                continue;

            string name = Path.GetFileNameWithoutExtension(buildScene.path);
            names.Add(name);
        }

        sceneNames = names.ToArray();
    }

    private void SyncSelectedIndexWithProperty()
    {
        string current = sceneToLoadProp.stringValue?.Trim() ?? string.Empty;
        selectedIndex = Array.IndexOf(sceneNames, current);
        if (selectedIndex < 0)
            selectedIndex = 0;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Show default string field so user can still type arbitrary scene names.
        EditorGUILayout.PropertyField(sceneToLoadProp, new GUIContent("Scene To Load"));

        EditorGUILayout.Space();

        if (sceneNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No scenes found in Build Settings. Add scenes via File > Build Settings...", MessageType.Warning);
        }
        else
        {
            // Refresh the list in case Build Settings changed while inspector is open.
            if (GUILayout.Button("Refresh Scenes from Build Settings"))
            {
                BuildSceneNameList();
                SyncSelectedIndexWithProperty();
            }

            // Draw popup of scenes found in Build Settings.
            int newIndex = EditorGUILayout.Popup(new GUIContent("Select Scene (Build Settings)"), selectedIndex, sceneNames);
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                sceneToLoadProp.stringValue = sceneNames[selectedIndex];
            }

            EditorGUILayout.HelpBox($"Selecting from the list will overwrite the `sceneToLoad` string. Current: '{sceneToLoadProp.stringValue}'", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}