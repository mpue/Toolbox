//C# Example
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toolkit
{
	public class Line {
		public Vector3 start {get; set;}
		public Vector3 end {get; set;}

	}


    public class ObjectCloner : EditorWindow
    {
        // private Vector3 gizmoPosition = new Vector3(0, 0, 0);

        bool showLinearPlacement = true;
 
        float distance = 1f;
		bool pickRandom = false;
		bool useRay = false;
		bool normalForRotation = false;
		bool keepInSphere;
		float sphereRadius = 10;
		Vector3 spherePosition = Vector3.zero;

        bool limitHeight = false;
        float maxHeight = 100;

		bool randomizePlacement = false;
		private float randomDistance = 1f;
		float targetProbability = 0.5f;

		bool randomizeRotation = false;

        bool useObjectBounds = true;
        bool groupObjects = true;
		bool combineObjects = false;

		bool showPlacementGrid;

        //float fuzzyness = 0.0f;
        GameObject original;
        int selectedAxis = 0;

        private string targetStructureName = "Group";

        public string targetName;

        public string[] folders = { "Assets/Prefabs"};

		private string[] xaxis = {"-","+"};
		private string[] yaxis = {"-","+"};
		private string[] zaxis = {"-","+"};

		private int x_dir = 0;
		private int y_dir = 0;
		private int z_dir = 0;

		private int x_count = 1;
		private int y_count = 1;
		private int z_count = 1;

		float _minSize = 1f;
		float _maxSize = 2f;

	    private Texture preview;

		[SerializeField]
		public List<GameObject> prefabs;

		private int numObjects;

		private Vector2 scrollPosition = Vector2.zero;

		private Toolbox toolbox = null;

		private List<Line> gridLines = new List<Line>();

        [MenuItem("Tools/ObjectPlacer")]
        private static void OpenWindow()
        {
            ObjectCloner window = GetWindow<ObjectCloner>();
            window.titleContent = new GUIContent("Object placer");

        }
        private void Update()
        {
            Repaint();
        }

        void Callback(object obj)
        {
            Debug.Log("Selected: " + obj);
        }

        void OnGUI()
        {

            showLinearPlacement = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showLinearPlacement, "Linear placement");

            if (showLinearPlacement)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
				scrollPosition =  EditorGUILayout.BeginScrollView(scrollPosition);
                showLinearPlacementPanel();
				EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--; 
            }

			
        }

        private void showLinearPlacementPanel()
        {
        
            EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("X-Axis dir");
			x_dir = EditorGUILayout.Popup(x_dir, xaxis,GUILayout.Width(50));
			x_count = EditorGUILayout.IntField("X count",x_count);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Y-Axis dir");
			y_dir = EditorGUILayout.Popup(y_dir, yaxis,GUILayout.Width(50));
			y_count = EditorGUILayout.IntField("Y count",y_count);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Z-Axis dir");
			z_dir = EditorGUILayout.Popup(z_dir, zaxis,GUILayout.Width(50));
			z_count = EditorGUILayout.IntField("Z count",z_count);
			EditorGUILayout.EndHorizontal();

			showPlacementGrid = EditorGUILayout.Toggle("Show grid", showPlacementGrid);

			useRay = EditorGUILayout.Toggle("Use raycast for bottom", useRay);

			if (useRay) {
				normalForRotation = EditorGUILayout.Toggle("Use normal for rotation", normalForRotation);
			}

			randomizePlacement = EditorGUILayout.Toggle("Randomize placement", randomizePlacement);

			if (randomizePlacement) {
				
				randomDistance = EditorGUILayout.FloatField("Random distance", randomDistance);
				targetProbability = EditorGUILayout.FloatField("Placement probability", targetProbability);
			}
            
			keepInSphere = EditorGUILayout.Toggle("Keep inside sphere", keepInSphere);

			if (keepInSphere) {

				sphereRadius = EditorGUILayout.FloatField("Sphere radius", sphereRadius);
				spherePosition = EditorGUILayout.Vector3Field("Sphere position", spherePosition);

			}

            limitHeight = EditorGUILayout.Toggle("Limit placement haight", limitHeight);

            if (limitHeight)
            {
                maxHeight = EditorGUILayout.FloatField("Maximum height", maxHeight);

            }

            randomizeRotation = EditorGUILayout.Toggle("Randomize rotation", randomizeRotation);
			useObjectBounds = EditorGUILayout.Toggle("Use object bounds", useObjectBounds);

            if (!useObjectBounds)
            {
                distance = EditorGUILayout.FloatField("Distance", distance);
				GUILayout.Label("Size range");
				EditorGUILayout.MinMaxSlider(ref _minSize,ref _maxSize,1f,10f);			               
			}

  
            groupObjects = EditorGUILayout.Toggle("Group objects", groupObjects);

            if (groupObjects)
            {
                targetStructureName = EditorGUILayout.TextField("Group name", targetStructureName);
            }

			combineObjects = EditorGUILayout.Toggle("Combine objects", combineObjects);

			pickRandom = EditorGUILayout.Toggle("Pick random object", pickRandom);

			if (pickRandom) {

				ScriptableObject target = this;
				SerializedObject so = new SerializedObject(target);
				SerializedProperty prefabsProp = so.FindProperty("prefabs");

				EditorGUILayout.PropertyField(prefabsProp, true); // True means show children
				so.ApplyModifiedProperties(); // Remember to apply modified properties

			}
			else {
				original = (GameObject)EditorGUILayout.ObjectField(original, typeof(GameObject), true);
			}


            if (original == null)
            {
                if (Selection.transforms.Length == 1)
                {
                    original = Selection.transforms[0].gameObject;

                }
            }

			if (original != null || (prefabs != null && prefabs.Count > 0))
            {
                // EditorGUILayout.LabelField("Selection : " + original.name);

			    if (preview == null) {
				    preview = AssetPreview.GetAssetPreview(original);
			    }

			    if (preview != null) {

				    if (GUILayout.Button(preview, GUILayout.Width(128), GUILayout.Height(128))) {
					    GameObject orig = GameObject.Find(targetStructureName);

					    GameObject empty = new GameObject();
					    empty.name = targetStructureName;

					    if (orig != null)
					    {
						    DestroyImmediate(orig);
					    }
						EditorCoroutine.start(placeOnAxis(empty));
				    }
			    }

                else if (GUILayout.Button("Place object(s)"))
                {
                    GameObject orig = GameObject.Find(targetStructureName);

                    GameObject empty = new GameObject();
                    empty.name = targetStructureName;


					Undo.RegisterCreatedObjectUndo(empty, empty.name);

                    if (orig != null)
                    {
                        DestroyImmediate(orig);
                    }
                    //placeOnAxis(empty, selectedAxis);

					EditorCoroutine.start(placeOnAxis(empty));
					// Selection.activeGameObject.SetActive(false);

                }
            
            }

            else
            {
				EditorGUILayout.LabelField("Please select or choose one or more object(s).");
            }

            EditorGUILayout.EndVertical();

			if (GUI.changed && showPlacementGrid) {

				if (toolbox != null) {
					updateGrid(x_count,z_count,(int)distance, toolbox.gizmoPosition);
					SceneView.RepaintAll();
				}

			}
        }

        private void showGridPlacementPanel()
        {
            // EditorGUILayout.LabelField("Insane option");
        }


		private IEnumerator placeOnAxis(GameObject parent)
        {
			if (Selection.transforms.Length == 1 || prefabs.Count > 0)
            {

				List<GameObject> objects = new List<GameObject>();

				int xdir = 1;
				int ydir = 1;
				int zdir = 1;

				if (x_dir == 0)
					xdir = -1;
				if (y_dir == 0)
					ydir = -1;
				if (z_dir == 0)
					zdir = -1;

				var xsize = distance;
				var ysize = distance;
				var zsize = distance;

				Vector3 current = Vector3.zero;

				for (var x = 0; x < (int)x_count; x++)
                {
					for (var y = 0; y < (int)y_count; y++)
					{
						for (var z = 0; z < (int)z_count; z++)
						{
							SceneView.RepaintAll();

							if (pickRandom) {
								int index = Random.Range(0, prefabs.Count);
								original = prefabs[index];

								toolbox = EditorWindow.GetWindow<Toolbox>();
								if (toolbox != null)
								{
									current = toolbox.gizmoPosition;
								}
								else {
									current = original.transform.position;
								}
							}
							else {
								current = Selection.transforms[0].position;
							}
							

							float probability = Random.Range(0,1f);

							if (randomizePlacement &&  probability < targetProbability) {
								continue;
							}
								
							float randomDistX = distance;
							float randomDistY = distance;
							float randomDistZ = distance;

							if (randomizePlacement) {
								randomDistX = Random.Range(0,randomDistance);
								randomDistY = Random.Range(0,randomDistance);
								randomDistZ = Random.Range(0,randomDistance);
							}

							float _y = current.y;
							Vector3 normal = Vector3.zero;

							if (useRay) {
								_y = getCurrentHeight((current.x - x * xdir * xsize) + randomDistX,(current.z - z * zdir * zsize) + randomDistZ, out normal);
							}
							else {
								_y = (current.y - y * ydir * ysize) + randomDistY;
							}

							Vector3 targetPos = 
								new Vector3(
									(current.x - x * xdir * xsize) + randomDistX, 
									_y, 
									(current.z - z * zdir * zsize) + randomDistZ);

							float dist = (spherePosition - targetPos).magnitude;
							bool inSphere = dist < sphereRadius;

							if(!occupiedPlace(targetPos,parent.transform)) {

								if (keepInSphere && !inSphere) {
									continue;
								}

                                if (limitHeight && targetPos.y > maxHeight)
                                {
                                    continue;
                                }

								GameObject g = null;
								g = Instantiate(original);

								if (randomizePlacement) {
									float scale = Random.Range(_minSize,_maxSize);
									g.transform.localScale = new Vector3(scale,scale,scale);
								}

								if (randomizeRotation) {
									g.transform.Rotate(new Vector3(0,Random.Range(0,90f),0));
								}
								if (normalForRotation) {
									g.transform.rotation = Quaternion.FromToRotation(g.transform.up, normal) * g.transform.rotation;
								}
								
								Undo.RegisterCreatedObjectUndo(g, g.name);
								g.transform.position = targetPos;
								
								objects.Add(g);
								
								if (groupObjects && !combineObjects)
								{
									g.transform.SetParent(parent.transform);
								}

								yield return new WaitForSeconds(0.01f);
							}
						}
					}
                }
				if (!groupObjects && combineObjects) {
					ShapeFactory.combineObjects(objects.ToArray());

					foreach(GameObject g in objects) {
						DestroyImmediate(g);
					}

					DestroyImmediate(parent);
				}
            }

			yield return null;
        }
        
		private bool occupiedPlace(Vector3 location, Transform parent) {

			foreach(Collider c in parent.GetComponentsInChildren<Collider>()) {
				if (c.bounds.Contains(location)) {
					return true;
				}
			}
			return false;
        }

        private Bounds getBounds(Transform t)
        {
            if (t.GetComponentsInChildren<Collider>().Length > 0)
            {
                return t.GetComponentsInChildren<Collider>()[0].bounds;
            }
            return new Bounds();
        }

		private float getCurrentHeight(float x, float z, out Vector3 normal) {
		
			//cast a ray against mouse position
			Ray worldRay = new Ray(new Vector3(x,100,z), Vector3.down);
			RaycastHit hitInfo;

            normal = Vector3.zero;

			if (Physics.Raycast(worldRay, out hitInfo))
			{
				normal = hitInfo.normal;
				return  hitInfo.point.y;
			}
			else
			{
				Ray ray = new Ray(new Vector3(x,100,z), Vector3.down);
				// create a plane at 0,0,0 whose normal points to +Y:
				Plane hPlane = new Plane(Vector3.up, Vector3.zero);
				// Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
				float distance = 0;
				// if the ray hits the plane...
				if (hPlane.Raycast(ray, out distance)) {
					// get the hit point:
					normal = Vector3.up;
					return ray.GetPoint(distance).y;
				}

			}
			return 0;
		
		}



        private void OnInspectorUpdate()
        {


        }

        private void OnSelectionChange()
        {

            if (Selection.transforms.Length == 1)
            {
                original = Selection.transforms[0].gameObject;
			    preview = AssetPreview.GetAssetPreview(original);
            }
            else
            {
                original = null;
            }

        }

        void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        
        }

        public void OnSceneGUI(SceneView scnView)
        {
            /*
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
            */


            if (original !=  null)
            {
                Quaternion orientation = Quaternion.Euler(0,0,0);

                if (selectedAxis == 0)
                {
					Handles.color = Color.red;
                    orientation = Quaternion.Euler(0, 90, 0);
                }
                else if (selectedAxis == 1)
                {
					Handles.color = Color.red;
                    orientation = Quaternion.Euler(0, -90, 0);
                }
				if (selectedAxis == 2)
				{
					Handles.color = Color.green;
					orientation = Quaternion.Euler(-90, 0, 0);
				}
				else if (selectedAxis == 3)
				{
					Handles.color = Color.green;
					orientation = Quaternion.Euler(90, 0, 0);
				}
                else if (selectedAxis == 4)
                {
					Handles.color = Color.blue;
                    orientation = Quaternion.Euler(0, 0, 0);
                }
                else if (selectedAxis == 5)
                {
					Handles.color = Color.blue;
                    orientation = Quaternion.Euler(0, 180, 0);
                }

                // Handles.ArrowCap(0, original.transform.position, orientation,5);

            }

            /*
            Event e = Event.current;

            //Check the event type and make sure it's left click.
            if ((e.type == EventType.MouseDown) && e.button == 0)
            {
                //cast a ray against mouse position
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    gizmoPosition = hitInfo.transform.position;
                    Debug.Log(gizmoPosition);
                    Handles.CubeCap(1,gizmoPosition, Quaternion.Euler(0, 0, 0),5);
                    Selection.activeGameObject = hitInfo.collider.gameObject;
                }

                e.Use();

            }
            */

			Event e = Event.current;

			if ((e.type == EventType.MouseUp) && e.button == 1 && e.shift) {
				toolbox = EditorWindow.GetWindow<Toolbox>();
				if (showPlacementGrid && toolbox != null) {
					updateGrid(x_count,z_count,(int)distance, toolbox.gizmoPosition);
					drawGrid();
				}
				scnView.Repaint();
			}

			if (keepInSphere) {
				Handles.color = Color.red;	
				drawGizmo(sphereRadius,spherePosition);
			}

			if (toolbox != null && showPlacementGrid) {
				drawGrid();
			}

            if (toolbox != null && limitHeight)
            {
                drawMaxHeight(toolbox.gizmoPosition);
            }

        }

        void drawGizmo(float size, Vector3 gizmoPosition)
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

			Handles.color = Color.yellow;
			Handles.DrawWireDisc(gizmoPosition, Vector3.up, size);
			Handles.DrawWireDisc(gizmoPosition, Vector3.left, size);
			Handles.DrawWireDisc(gizmoPosition, Vector3.forward, size );
			Handles.SphereHandleCap(0, gizmoPosition, Quaternion.LookRotation(Vector3.up), 0.1f, EventType.Repaint);


		}

		void drawQuad(Vector3 pos, float size) {
			var y = 0;
			Handles.DrawAAPolyLine(new Vector3(pos.x,pos.y,pos.z), new Vector3(pos.x + size,y,pos.z),new Vector3(pos.x + size,y,pos.z+size),new Vector3(pos.x,y,pos.z+size));
		}

		void updateGrid(int xcount,int zcount, int gridSize, Vector3 pos) {
			gridLines.Clear();
			Vector3 norm;
			var y = 0;
			for (var x = 0; x < (int)xcount; x++ )
			{
				for (var z = 0; z <= (int)zcount; z++) 
				{
					
					Line line1 = new Line();
					line1.start = new Vector3(x*distance,getCurrentHeight(x*distance+pos.x, z*distance+pos.z, out norm),z*distance) + pos;
					line1.end   = new Vector3(x*distance+distance,getCurrentHeight(x*distance+distance+pos.x, z*distance+pos.z, out norm),z*distance)+pos;

					Line line2 = new Line();
					line2.start = new Vector3(x*distance,getCurrentHeight(x*distance+pos.x, z*distance+pos.z, out norm),z*distance) + pos;
					line2.end = new Vector3(x*distance,getCurrentHeight(x*distance+pos.x, z*distance+distance+pos.z, out norm),z*distance+distance)+pos;

					if (keepInSphere) {
						if (isInSphere(line1.start) && isInSphere(line1.end) && isInSphere(line2.start) && isInSphere(line2.end)) {
							gridLines.Add(line1);
							gridLines.Add(line2);
						}
					}
					else {
						gridLines.Add(line1);
						gridLines.Add(line2);
					}


				}
			}
		}

		private bool isInSphere(Vector3 targetPos) {
			float dist = (spherePosition - targetPos).magnitude;
			return dist < sphereRadius;
		}

		void drawGrid() {
			Handles.color = Color.white;
			foreach(Line line in gridLines) {
				Handles.DrawLine(line.start, line.end);
			}
		}

        void drawMaxHeight(Vector3 pos)
        {
            Handles.color = new Color(1.0f, 0.2f, 0.2f, 0.3f);
            Rect r = new Rect(0, 0, 100, maxHeight);

            Vector3 discpos = new Vector3(pos.x, maxHeight, pos.y);

            Handles.DrawSolidDisc(discpos, Vector3.up, 50);
        }

		void OnDrawGizmos() {
			if (keepInSphere) {
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(spherePosition,sphereRadius);			
			}
		}

    }
}