using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Toolkit
{
    public class FilterSelector : EditorWindow
    {
        private string searchPrefix;
        private GameObject parent;

        /*
        [MenuItem("Tools/FilterSelector")]
        static void Init()
        {
            EditorWindow window = GetWindow(typeof(FilterSelector));
            window.Show();

        }
        */

        void OnGUI()
        {
            EditorGUILayout.LabelField("Select parent object");

            parent = (GameObject)EditorGUILayout.ObjectField(parent, typeof(GameObject), true);

            EditorGUILayout.LabelField("Prefix");

            searchPrefix = EditorGUILayout.TextField("Select prefix", searchPrefix);


            if (GUILayout.Button("Select"))
            {
                Transform[] children = null;

                if (Selection.activeGameObject != null)
                {
                    children = Selection.activeGameObject.GetComponentsInChildren<Transform>();
                }
                else if(parent != null)
                {
                    children = parent.GetComponentsInChildren<Transform>();
                }
                else
                {
                    return;
                }

                Selection.activeGameObject = null;

                List<GameObject> selected = new List<GameObject>();

                foreach (Transform child in children)
                {
                    if (child.gameObject.name.StartsWith(searchPrefix))
                    {
                        selected.Add(child.gameObject);
                    }
                }

                Selection.objects = selected.ToArray<GameObject>();


            }




        }


    }
}
