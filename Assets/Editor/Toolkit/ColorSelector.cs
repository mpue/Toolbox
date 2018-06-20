using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Toolkit
{
    public class ColorSelector : EditorWindow
    {
        private string shaderName = "_Tint";
        Color matColor = Color.white;
        GameObject target;
        private string searchPrefix;
        float smoothness = 1.0f;

        /*
        [MenuItem("Tools/ShaderTuner")]
        static void Init()
        {
            EditorWindow window = GetWindow(typeof(ColorSelector));
            window.Show();

        }
        */

        void OnGUI()
        {
            EditorGUILayout.LabelField("Shader name");

            shaderName = EditorGUILayout.TextField("Shader name", shaderName);
            matColor = EditorGUILayout.ColorField("New Color", matColor);
            target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);


            if (GUILayout.Button("Batch change colors"))
                ChangeColors();

            searchPrefix = EditorGUILayout.TextField("Shader prefix", searchPrefix);
            smoothness = EditorGUILayout.FloatField("Smoothness", smoothness);

            if (GUILayout.Button("Batch change smoothness"))
                SetSmooth(smoothness);



        }

        private void ChangeColors()
        {
            foreach (Material mat in target.GetComponent<Renderer>().sharedMaterials)
            {
                mat.SetColor(shaderName, matColor);
            }          
        }

        private void SetSmooth(float value)
        {
            foreach (Material mat in target.GetComponent<Renderer>().sharedMaterials)
            {
                if(mat.name.StartsWith(searchPrefix))
                {
                    mat.SetFloat("_Smoothness", value);
                }
            }
        }
    }
}
