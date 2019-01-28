using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


public class Toolbar : EditorWindow
{
    [MenuItem("Window/Scene GUI/Enable")]
    public static void Enable()
    {
        SceneView.onSceneGUIDelegate += OnScene;
        Debug.Log("Scene GUI : Enabled");
    }

    [MenuItem("Window/Scene GUI/Disable")]
    public static void Disable()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
        Debug.Log("Scene GUI : Disabled");
    }

    private static void OnScene(SceneView sceneview)
    {
        Handles.BeginGUI();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Button 1", GUILayout.Width(64), GUILayout.Height(64)))
            Debug.Log("I am Button 1.");
        if (GUILayout.Button("Button 2", GUILayout.Width(64), GUILayout.Height(64)))
            Debug.Log("I am Button 2.");
        EditorGUILayout.EndHorizontal();
        Handles.EndGUI();
    }
}