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

        public Vector3 gizmoPosition = new Vector3(0, 0, 0);

        private static Vector2 scrollPosition = Vector2.zero;

        private static bool loaded = false;
        private static int selected;


		private static int selectedPath = 0;
        private static bool autoRefresh = false;

        private ObjectCloner objectCloner = null;
        private FilterSelector selector = null;
        private ColorSelector colorSelector = null;
        private WaypointGenerator waypointGenerator = null;

        
        [MenuItem("Window/Toolbox")]
        private static void OpenWindow()
        {
            Toolbox window = GetWindow<Toolbox>();
            // window.titleContent = new GUIContent("Toolbox", (Texture)Resources.Load("wrench"));

        }
        

        void OnEnable()
        {
            autoRefresh = EditorPrefs.GetBool("Toolkit_AutoRefresh", false);

            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnFocus()
        {
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        private void OnDisable()
        {
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.RepaintAll();

        }

        void OnSceneGUI(SceneView sceneView)
        {

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

        void drawGizmo(float size)
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

        void OnGUI()
        {

            SceneView sceneView = SceneView.lastActiveSceneView;

            if (GUILayout.Button("View to cursor"))
            {
                sceneView.LookAt(gizmoPosition);
            }

            else if (GUILayout.Button("Snap selected to cursor"))
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.transform.position = gizmoPosition;
                }

            }

            else if (GUILayout.Button("Snap cursor to selected"))
            {
                if (Selection.activeGameObject != null)
                {
                    gizmoPosition = Selection.activeGameObject.transform.position;
                }

            }

            else if (GUILayout.Button("Snap cursor to grid"))
            {
                int gridSize = EditorPrefs.GetInt("Toolkit_GridSize", 1);

                Vector3 pos = gizmoPosition;

                pos.x = EditorHelper.snap((int)pos.x, gridSize);
                pos.z = EditorHelper.snap((int)pos.z, gridSize);

                gizmoPosition = pos;
            }

            else if (GUILayout.Button("Merge selected"))
            {
                if (Selection.gameObjects.Length > 0)
                {
                    ShapeFactory.combineObjects(Selection.gameObjects);
                }
            }

            else if (GUILayout.Button("Activate children"))
            {
                Selection.activeGameObject.SetActive(true);
            }

            else if (GUILayout.Button("Deactivate"))
            {
                Selection.activeGameObject.SetActive(false);
            }

            else if (GUILayout.Button("Object placer"))
            {
                if (objectCloner == null)
                {
                    objectCloner = ScriptableObject.CreateInstance<ObjectCloner>();                
                }
                objectCloner.Show();
            }


            else if (GUILayout.Button("Filter selector"))
            {
                if (selector == null)
                {
                    selector = ScriptableObject.CreateInstance<FilterSelector>();
                }
                selector.Show();
            }

            else if (GUILayout.Button("Shader batch modify"))
            {
                if (colorSelector == null)
                {
                    colorSelector = ScriptableObject.CreateInstance<ColorSelector>();
                }
                colorSelector.Show();
            }


            else if (GUILayout.Button("Waypoint from OBJ"))
            {
                if (waypointGenerator == null)
                {
                    waypointGenerator = ScriptableObject.CreateInstance<WaypointGenerator>();
                }
                waypointGenerator.Show();
            }

        }


    }
}