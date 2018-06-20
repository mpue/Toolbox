using System;
using UnityEngine;
using UnityEditor;

namespace Toolkit
{ 
    public class ShapePropertyEditor : EditorWindow
    {
	    private GameObject editingObject = null;
	    private Primitives primitive;

	    private float radius = 0.5f;
	    private float width = 1.0f;
	    private float height = 1.0f;
	    private float length = 1.0f;
	    private float size = 1.0f;

	    private float outerRadius = 1f;
	    private float innerRadius = .3f;
	    private int nbRadSeg = 24;
	    private int nbSides = 18;

        private static void OpenWindow()
	    {
		    ShapePropertyEditor window = GetWindow<ShapePropertyEditor>();
		    window.titleContent = new GUIContent("Shape properties");
		    window.position = new Rect(100,100,100,100);
	    }

	    void OnGUI()
	    {


		    switch (primitive) {
			    case Primitives.Capsule :
				    GUILayout.Label("Capsule");
				    showCapsuleEditor();
				    break;
			    case Primitives.Sphere :
				    GUILayout.Label("Sphere");
				    showSphereEditor();
				    break;	
			    case Primitives.Cube :
				    GUILayout.Label("Cube");
				    showCubeEditor();
				    break;
			    case Primitives.Cylinder :
				    GUILayout.Label("Cylinder");
				    showCylinderEditor();
				    break;			
			    case Primitives.Plane :
				    GUILayout.Label("Plane");
				    showPlaneEditor();
				    break;
			    case Primitives.Torus :
				    GUILayout.Label("Torus");
				    showTorusEditor();
				    break;
			    default:
				    break;
		    }
	    }

	    private void showPlaneEditor() {
		    width = EditorGUILayout.FloatField("Width", width);
		    length = EditorGUILayout.FloatField("Length", length);
		    Vector3 scale = new Vector3(width,1,length);
		    editingObject.transform.localScale = scale;
	    }

	    private void showCubeEditor() {
		    size = EditorGUILayout.FloatField("Size", size);
		    Vector3 scale = new Vector3(size,size,size);
		    editingObject.transform.localScale = scale;
	    }

	    private void showCapsuleEditor() {
		    radius = EditorGUILayout.FloatField("Radius", radius);
		    height = EditorGUILayout.FloatField("Height", height);
		    Vector3 scale = new Vector3(radius*2f,height,radius*2f);
		    editingObject.transform.localScale = scale;
	    }

	    private void showCylinderEditor() {

            innerRadius = EditorGUILayout.FloatField("Inner radius", innerRadius);
            outerRadius = EditorGUILayout.FloatField("Outer radius", outerRadius);

            height = EditorGUILayout.FloatField("Height", height);
            nbSides = EditorGUILayout.IntField("Sides", nbSides);

            if (GUILayout.Button("Update"))
            {
                Vector3 oldPos = editingObject.transform.position;
			    Quaternion oldRot = editingObject.transform.rotation;
                DestroyImmediate(editingObject);
                editingObject = ShapeFactory.createTube(height, nbSides, outerRadius, innerRadius, outerRadius, innerRadius);
                editingObject.transform.position = oldPos;
			    editingObject.transform.rotation = oldRot;
            }

	    }

	    private void showTorusEditor() {

		    innerRadius = EditorGUILayout.FloatField("Inner radius", innerRadius);
		    outerRadius = EditorGUILayout.FloatField("Outer radius", outerRadius);
		    nbSides = EditorGUILayout.IntField("Sides", nbSides);
		    nbRadSeg = EditorGUILayout.IntField("Segments", nbRadSeg);

		    if (GUILayout.Button("Update"))
		    {
			    Vector3 oldPos = editingObject.transform.position;
			    Quaternion oldRot = editingObject.transform.rotation;
			    DestroyImmediate(editingObject);
			    editingObject = ShapeFactory.createTorus(outerRadius,innerRadius,nbRadSeg,nbSides);
			    editingObject.transform.position = oldPos;
			    editingObject.transform.rotation = oldRot;
		    }
	    }

	    private void showSphereEditor() {
		    radius = EditorGUILayout.FloatField("Radius", radius);
		    Vector3 scale = new Vector3(radius*2f,radius*2f,radius*2f);
		    editingObject.transform.localScale = scale;
	    }

	    public void setEditingObject(GameObject obj, Primitives type) {
		    this.editingObject = obj;
		    this.primitive = type;
	    }

        public void setEditingObject(GameObject obj)
        {
            this.editingObject = obj;
        }
    }
}
