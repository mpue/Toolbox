using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Toolkit
{
    public class WaypointGenerator : EditorWindow
    {
        
        private GameObject parent;
        private String inputData;
        private float scale = 0.5f;

        string text = "Nothing Opened...";
        TextAsset txtAsset;
        Vector2 scroll;

        /*
        [MenuItem("Tools/WayPoinGen")]
        static void Init()
        {
            EditorWindow window = GetWindow(typeof(WaypointGenerator));
            window.Show();

        }
        */

        UnityEngine.Object source;

        void OnGUI()
        {
            EditorGUILayout.LabelField("Select parent object");

            parent = (GameObject)EditorGUILayout.ObjectField(parent, typeof(GameObject), true);

            source = EditorGUILayout.ObjectField(source, typeof(UnityEngine.Object), true);

            if (source != null)
            {
                TextAsset newTxtAsset = (TextAsset)source;

                if (newTxtAsset != txtAsset)
                    ReadTextAsset(newTxtAsset);


                if (GUILayout.Button("Create"))
                {
                    createWayPoints(text);
                }

                scroll = EditorGUILayout.BeginScrollView(scroll);
                text = EditorGUILayout.TextArea(text, GUILayout.Height(position.height - 30));
                EditorGUILayout.EndScrollView();

            }

        }

        void ReadTextAsset(TextAsset txt)
        {
            text = txt.text;
            txtAsset = txt;
        }

        void createWayPoints(String data)
        {

            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length;i++)
            {
                string[] vectorData = lines[i].Split(null);

                float x = float.Parse(vectorData[2]);
                float y = float.Parse(vectorData[1]);
                float z = float.Parse(vectorData[3]);

                Vector3 pos = new Vector3(x * scale, y * scale, z * scale);

                GameObject g = new GameObject();
                g.name = "Waypoint " + i;
                g.transform.position = pos;

                g.transform.parent = parent.transform;

            }

        }
    }
}
