using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Toolkit
{
    public class Toolbox : EditorWindow
    {

        public static Vector3 gizmoPosition = new Vector3(0, 0, 0);

        private static Vector2 scrollPosition = Vector2.zero;

        private static bool loaded = false;
        private static int selected;


        private static int selectedPath = 0;
        private static bool autoRefresh = false;

        private static ObjectCloner objectCloner = null;
        private static FilterSelector selector = null;
        private static ColorSelector colorSelector = null;
        private static WaypointGenerator waypointGenerator = null;

        private static List<GUIContent> contents = null;

        private static GUIContent _viewToCursorContent = null;
        private static GUIContent _selectedToCursorContent = null;
        private static GUIContent _cursorToSelectedContent = null;
        private static GUIContent _cursorToGridContent = null;

        static Toolbox() {
            contents = new List<GUIContent>();

            AddContent("cursor","Snap view to cursor");
            AddContent("snap_selected", "Snap selected to cursor");
            AddContent("snap_cursor_selected", "Snap cursor to selected");
            AddContent("cursor_grid", "Snap cursor to grid");
            AddContent("merge", "Merge selected");
            AddContent("activate_children", "Activate children");
            AddContent("disable", "Disable");
            AddContent("object_placer", "Place objects");
            AddContent("filter", "Filter select");
            AddContent("batch", "Shader batch modify");
            AddContent("path", "Waypoint from OBJ");
        }

        private static void AddContent(string icon, string hint)
        {
            contents.Add(new GUIContent((Texture)Resources.Load(icon),hint));
        }

        [MenuItem("Window/Toolbox")]
        private static void OpenWindow()
        {
            // Toolbox window = GetWindow<Toolbox>();
            // window.titleContent = new GUIContent("Toolbox", (Texture)Resources.Load("wrench"));

            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }
        

        void OnEnable()
        {
            autoRefresh = EditorPrefs.GetBool("Toolkit_AutoRefresh", false);

        }

        void OnFocus()
        {
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnDisable()
        {
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.RepaintAll();

        }

        static void OnSceneGUI(SceneView sceneView)
        {
            createToolbar();

            Event e = Event.current;

			if ((e.type == EventType.MouseUp) && e.button == 1 && e.shift)
            {
                //cast a ray against mouse position
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;
			
                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    gizmoPosition = hitInfo.point;
                    Debug.Log(gizmoPosition);
                }
                else
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    // create a plane at 0,0,0 whose normal points to +Y:
                    Plane hPlane = new Plane(Vector3.up, Vector3.zero);
                    // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
                    float distance = 0;
                    // if the ray hits the plane...
                    if (hPlane.Raycast(ray, out distance))
                    {
                        // get the hit point:
                        gizmoPosition = ray.GetPoint(distance);
                    }

                }

                bool autoSnap = EditorPrefs.GetBool("Toolkit_AutoSnap", false);

                if (autoSnap) {
                    int gridSize = EditorPrefs.GetInt("Toolkit_GridSize", 1);

                    Vector3 pos = gizmoPosition;

                    pos.x = EditorHelper.snap((int)pos.x, gridSize);
                    pos.z = EditorHelper.snap((int)pos.z, gridSize);

                    gizmoPosition = pos;
                }

				sceneView.Repaint();
            }
			else if ((e.type == EventType.MouseDown) && e.button == 1 && e.command)
            {
                gizmoPosition = Vector3.zero;
            }
            
            drawGizmo(0.5f);
            
        }



        static void drawGizmo(float size)
        {

            Handles.color = Color.black;

            Vector3 gizmoStart = gizmoPosition;
            Vector3 gizmoEnd = gizmoPosition;

            gizmoStart.x -= size;
            gizmoEnd.x += size;

			Handles.DrawAAPolyLine(4.0f,gizmoStart, gizmoEnd);

            gizmoStart = gizmoPosition;
            gizmoEnd = gizmoPosition;

            gizmoStart.z -= size;
            gizmoEnd.z += size;

			Handles.DrawAAPolyLine(4.0f,gizmoStart, gizmoEnd);

            gizmoStart = gizmoPosition;
            gizmoEnd = gizmoPosition;

            gizmoStart.y -= size;
            gizmoEnd.y += size;

			Handles.DrawAAPolyLine(4.0f,gizmoStart, gizmoEnd);

            Handles.color = Color.red;
            Handles.DrawWireDisc(gizmoPosition, Vector3.up, size * 0.8f);
			Handles.DrawWireDisc(gizmoPosition, Vector3.left, size * 0.8f);
			Handles.DrawWireDisc(gizmoPosition, Vector3.forward, size * 0.8f);
            Handles.SphereHandleCap(0, gizmoPosition, Quaternion.LookRotation(Vector3.up), 0.1f, EventType.Repaint);

        }

        static void createToolbar()
        {

            SceneView sceneView = SceneView.lastActiveSceneView;

            Handles.BeginGUI();
            GUILayout.BeginVertical();
            


            if (GUILayout.Button(contents[0], GUILayout.Width(32), GUILayout.Height(32)))
            {
                sceneView.LookAt(gizmoPosition);
            }

            else if (GUILayout.Button(contents[1], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.transform.position = gizmoPosition;
                }

            }

            else if (GUILayout.Button(contents[2], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (Selection.activeGameObject != null)
                {
                    gizmoPosition = Selection.activeGameObject.transform.position;
                }

            }

            else if (GUILayout.Button(contents[3], GUILayout.Width(32), GUILayout.Height(32)))
            {
                int gridSize = EditorPrefs.GetInt("Toolkit_GridSize", 1);

                Vector3 pos = gizmoPosition;

                pos.x = EditorHelper.snap((int)pos.x, gridSize);
                pos.z = EditorHelper.snap((int)pos.z, gridSize);

                gizmoPosition = pos;
            }

            else if (GUILayout.Button(contents[4], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (Selection.gameObjects.Length > 0)
                {
                    ShapeFactory.combineObjects(Selection.gameObjects);
                }
            }

            else if (GUILayout.Button(contents[5], GUILayout.Width(32), GUILayout.Height(32)))
            {
                Selection.activeGameObject.SetActive(true);
            }

            else if (GUILayout.Button(contents[6], GUILayout.Width(32), GUILayout.Height(32)))
            {
                Selection.activeGameObject.SetActive(false);
            }

            else if (GUILayout.Button(contents[7], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (objectCloner == null)
                {
                    objectCloner = ScriptableObject.CreateInstance<ObjectCloner>();                
                }
                objectCloner.Show();
            }


            else if (GUILayout.Button(contents[8], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (selector == null)
                {
                    selector = ScriptableObject.CreateInstance<FilterSelector>();
                }
                selector.Show();
            }

            else if (GUILayout.Button(contents[9], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (colorSelector == null)
                {
                    colorSelector = ScriptableObject.CreateInstance<ColorSelector>();
                }
                colorSelector.Show();
            }


            else if (GUILayout.Button(contents[10], GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (waypointGenerator == null)
                {
                    waypointGenerator = ScriptableObject.CreateInstance<WaypointGenerator>();
                }
                waypointGenerator.Show();
            }

            GUILayout.EndVertical();
            Handles.EndGUI();

        }


    }
}