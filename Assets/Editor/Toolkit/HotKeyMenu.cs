using UnityEditor;
using UnityEngine;
using System.Collections;


namespace Toolkit
{
    [InitializeOnLoad]
    public class HotkeyMenu : EditorWindow
    {

		static bool ortho = false;
        static bool wireframe = false;
        static Toolbox toolbox = null;
		static bool grab = false;
        static Texture toolboxIcon;

		private static ShapePropertyEditor propertyEditor = null;

        static DrawCameraMode[] modes;

        static float progress = 1f;

        static void Callback(System.Object obj)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (obj.ToString().Equals("front")) {
                sceneView.rotation = new Quaternion(0, 0, 0, 1);
            }
            else if (obj.ToString().Equals("top")) {
                sceneView.rotation = new Quaternion(1, 0, 0, 1);
            }
            else if (obj.ToString().Equals("left")) {
                sceneView.rotation = new Quaternion(0, 1, 0, 1);
            }
            else if (obj.ToString().Equals("ortho")) {
                toggleOrtho(sceneView);
            }
            /*
            else if (obj.ToString().Equals("toolbox")) {

                toolbox = ScriptableObject.CreateInstance<Toolbox>();
                toolbox.titleContent = new GUIContent("Toolbox", toolboxIcon);
                toolbox.ShowUtility();
                // window.Show();
            }
            */
            else if (obj.ToString().Equals("moveToCursor"))
            {
                toolbox = EditorWindow.GetWindow<Toolbox>();
                if (toolbox != null)
                {
					sceneView.LookAt(toolbox.gizmoPosition);
                }

            }
			else if (obj.ToString().Equals("snapSelectionToCursor"))
			{
				if (Selection.activeGameObject != null)
				{
					Selection.activeGameObject.transform.position = toolbox.gizmoPosition;
				}

			}
            else if (obj.ToString().Equals("snapCursorToSelection"))
            {
                if (Selection.activeGameObject != null)
                {
                    toolbox.gizmoPosition = Selection.activeGameObject.transform.position;
                }

            }

            else if (obj.ToString().Equals("snapCursorToGrid"))
            {
                int gridSize = EditorPrefs.GetInt("Toolkit_GridSize", 1);

                Vector3 pos = toolbox.gizmoPosition;

                pos.x = EditorHelper.snap((int)pos.x, gridSize);
                pos.z = EditorHelper.snap((int)pos.z, gridSize);

                toolbox.gizmoPosition = pos;
            }
            else if (obj.ToString().Equals("mergeObjects"))
            {
                if (Selection.gameObjects.Length > 0)
                {
                	ShapeFactory.combineObjects(Selection.gameObjects);
                }
            }
            else if (obj.ToString().Equals("drawMode"))
            {

                wireframe = !wireframe;

                if (wireframe)
                {
                    SceneView.lastActiveSceneView.renderMode = DrawCameraMode.Wireframe;
                }
                else
                {
                    SceneView.lastActiveSceneView.renderMode = DrawCameraMode.Textured;
                }
                    
				sceneView.Repaint();
            }
				
        }



        static HotkeyMenu()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            
        }

        static Event e;


        public static void OnSceneGUI(UnityEditor.SceneView myview) {

            //if (Event.current.type == EventType.layout)
            //	HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

            e = Event.current;

			if (toolboxIcon == null) {
				toolboxIcon = (Texture)Resources.Load("wrench");
			}

            if (e.isKey && e.keyCode == KeyCode.M)
            {
                // Now create the menu, add items and show it

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Front view"), false, Callback, "front");
                menu.AddItem(new GUIContent("Top view"), false, Callback, "top");
                menu.AddItem(new GUIContent("Left view"), false, Callback, "left");
                menu.AddItem(new GUIContent("Toggle orthographic"), false, Callback, "ortho");
                menu.AddItem(new GUIContent("Show toolbox", toolboxIcon), false, Callback, "toolbox");
                menu.AddItem(new GUIContent("Merge objects"), false, Callback, "mergeObjects");

                menu.ShowAsContext();
            }
            else if (e.isKey && e.keyCode == KeyCode.S && e.modifiers == EventModifiers.Shift)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Move view to cursor"), false, Callback, "moveToCursor");
                menu.AddItem(new GUIContent("Move view to selection"), false, Callback, "moveToSelection");
                menu.AddItem(new GUIContent("Snap selection to cursor"), false, Callback, "snapSelectionToCursor");
                menu.AddItem(new GUIContent("Snap cursor to selection"), false, Callback, "snapCursorToSelection");
                menu.AddItem(new GUIContent("Snap cursor to grid"), false, Callback, "snapCursorToGrid");
                menu.ShowAsContext();
            }
            else if (e.isKey && e.keyCode == KeyCode.O && e.type == EventType.KeyDown && e.modifiers == EventModifiers.Shift)
            {
                toggleOrtho(myview);
            }
            else if (e.isKey && e.keyCode == KeyCode.X && e.type == EventType.KeyDown)
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.transform.Rotate(new Vector3(90, 0, 0));
                }
            }

            else if (e.isKey && e.keyCode == KeyCode.Y && e.type == EventType.KeyDown)
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.transform.Rotate(new Vector3(0, 90, 0));
                }
            }
            else if (e.isKey && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.transform.Rotate(new Vector3(0, 0, 90));
                }
            }

            else if (e.isKey && e.keyCode == KeyCode.G && e.type == EventType.KeyDown)
            {
                toggleGrab();
            }

            else if (e.isKey && e.keyCode == KeyCode.Keypad1 && e.type == EventType.KeyDown)
            {
                Callback("front");
            }
            else if (e.isKey && e.keyCode == KeyCode.Keypad3 && e.type == EventType.KeyDown)
            {
                Callback("left");
            }
            else if (e.isKey && e.keyCode == KeyCode.Keypad5 && e.type == EventType.KeyDown)
            {
                Callback("ortho");
            }
            else if (e.isKey && e.keyCode == KeyCode.Keypad7 && e.type == EventType.KeyDown)
            {
                Callback("top");
            }
			else if (e.isKey && e.keyCode == KeyCode.Period && e.type == EventType.KeyDown)
            {
                Callback("drawMode");
            }


            if (grab && Selection.activeGameObject != null) {
				Vector3 pos = Selection.activeGameObject.transform.localPosition;
				Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Event.current.mousePosition);
				pos.x = mousePos.x;
				Selection.activeGameObject.transform.localPosition = pos;
			}


			if (propertyEditor == null)
				propertyEditor = ScriptableObject.CreateInstance<ShapePropertyEditor>();

			Handles.BeginGUI();
			int w = SceneView.currentDrawingSceneView.camera.pixelWidth / 4;
			GUILayout.BeginArea(new Rect(w - 150,0,300,48));
			GUILayout.EndArea();	
			Handles.EndGUI();
        }

        private void OnDestroy()
        {
            if (toolbox != null)
            {
                toolbox.Close();
            }
        }
			
        private void OnGUI()
        {
			
        }

        private static void toggleOrtho(SceneView sceneView) {
            ortho = !ortho;
            sceneView.orthographic = ortho;
        }

        private static void toggleWireframw(SceneView sceneView) {
            wireframe = !wireframe;
        }

		private static void toggleGrab() {
			grab = !grab;
		}

		static void SaveTextureToFile (Texture2D texture, string filename) { 
			System.IO.File.WriteAllBytes (filename, texture.EncodeToPNG());
		}


    }
}