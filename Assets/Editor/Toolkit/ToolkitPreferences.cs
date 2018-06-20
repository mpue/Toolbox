using UnityEngine;
using UnityEditor;

namespace Toolkit
{
    public class ToolkitPreferences
    {
	    // Have we loaded the prefs yet
	    private static bool prefsLoaded = false;

	    // The Preferences
	    private static string modelDirectory = "Models";
        private static bool autoRefresh = false;
        private static int gridSize = 1;
        private static bool autoSnap = false;
        private static int thumbnailSize = 64;

        [PreferenceItem("Toolkit")]
	    private static void ToolkitPreferencesGUI()
	    {
		    if (!prefsLoaded)
		    {
			    modelDirectory = EditorPrefs.GetString("Toolkit_ModelPath", "Models");
                autoRefresh = EditorPrefs.GetBool("Toolkit_AutoRefresh", false);
                autoSnap = EditorPrefs.GetBool("Toolkit_AutoSnap", false);
                gridSize = EditorPrefs.GetInt("Toolkit_GridSize", 1);
                prefsLoaded = true;
		    }

		    EditorGUILayout.LabelField("Model directory");
		    modelDirectory = EditorGUILayout.TextField(modelDirectory);

            EditorGUILayout.LabelField("Auto refresh");
            autoRefresh = EditorGUILayout.Toggle(autoRefresh);

            EditorGUILayout.LabelField("Grid size");
            gridSize = EditorGUILayout.IntField(gridSize);

            EditorGUILayout.LabelField("Thumb size");
            thumbnailSize = EditorGUILayout.IntField(thumbnailSize);

            EditorGUILayout.LabelField("Auto snap");
            autoSnap = EditorGUILayout.Toggle(autoSnap);

            if (GUI.changed)
		    {
			    EditorPrefs.SetString("Toolkit_ModelPath", modelDirectory);
                EditorPrefs.SetBool("Toolkit_AutoRefresh", autoRefresh);
                EditorPrefs.SetInt("Toolkit_GridSize", gridSize);
                EditorPrefs.SetInt("Toolkit_ThumbnailSize", thumbnailSize);
                EditorPrefs.SetBool("Toolkit_AutoSnap", autoSnap);
            }
	    }
    }

}
