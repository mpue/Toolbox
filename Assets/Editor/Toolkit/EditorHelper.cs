using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Toolkit
{
    class EditorHelper : MonoBehaviour
    {
	    //-------------------------------------------------------------------------------------------------------
	    static List<string> list = new List<string>();

	    //-------------------------------------------------------------------------------------------------------
		public static void __CollectAll(string path, bool pathsOnly)
	    {
		    string[] files = Directory.GetFiles( path );
		    foreach(string file in files)
		    {
			    // if(file.Contains(".meta")) continue;

				if (!pathsOnly) {
					//if (file.Contains(".prefab")){
						// GameObject asset = (GameObject)AssetDatabase.LoadAssetAtPath(file, typeof(GameObject));
						list.Add(file);
					// }
					
				}	
			    //if (asset == null) 
			    //	Debug.Log("Asset is not " + typeof(Object) + ": " + file);

			    //Debug.Log("Adding File:" + asset.name);
		    }

		    string[] folders = Directory.GetDirectories( path );
		    foreach(string folder in folders)
		    {
			    string name = Path.GetFileName( folder );
			    string dest = Path.Combine( path, name );

				if (pathsOnly) {
					list.Add(dest);
				}

			    //Debug.Log("List Count:" + list.Count + ", dest:" + dest);
				__CollectAll( dest, pathsOnly );
		    }		
	    }

	    //-------------------------------------------------------------------------------------------------------
		public static List<string> CollectAll(string path, bool pathsOnly)
	    {
		    list.Clear();
			__CollectAll(path,pathsOnly);

		    //Debug.Log("List Count:" + list.Count + ", path:" + path);
		    return list;
	    }

		public static T[] GetAtPath<T>(string path)
		{

			ArrayList al = new ArrayList();
			string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path, "*.*", SearchOption.AllDirectories);
			//string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

			foreach (string fileName in fileEntries)
			{
				int assetPathIndex = fileName.IndexOf("Assets");
				string localPath = fileName.Substring(assetPathIndex);

				UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

				if (t != null)
					al.Add(t);
			}
			T[] result = new T[al.Count];
			for (int i = 0; i < al.Count; i++)
				result[i] = (T)al[i];

			return result;
		}

        /**
         * Snaps the current given location to a given grid
         * 
         * @param location the location to be snapped
         * @param raster the raster size
         * @param tolerance the tolerance to cosult while snapping
         * @return
         */
        public static int snap(int location, int raster)
        {

            int toleranceWindow = (raster / 2);

            if (location > 0)
            {
                if ((location % raster) > toleranceWindow)
                {
                    location = location + (raster - (location % raster));
                }
                else
                {
                    location = location - (location % raster);
                }
            }
            else
            {
                if ((location % raster) < toleranceWindow)
                {
                    location = location + (raster - (location % raster)) - raster;
                }
                else
                {
                    location = location - (location % raster) - raster;
                }
            }
            return location;
        }

        static Bounds CalculateBounds(GameObject go)
        {
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer r in rList)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

		public static void FocusCameraOnGameObject(Camera c, GameObject g, Vector3 initialPosition, bool randomize)
        {
            Bounds b = CalculateBounds(g);
            Vector3 max = b.size;
            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
            float dist = radius / (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
            Debug.Log("Radius = " + radius + " dist = " + dist);
            
			if (randomize) {
				Vector3 pos = UnityEngine.Random.onUnitSphere * dist + b.center;
				if (pos.y < 2)
					pos.y = 2; 
				c.transform.position = pos;
				c.transform.LookAt(b.center);
			}
			else {
				Vector3 pos = initialPosition;
				
				initialPosition.z -= -dist;
				
				c.transform.position = initialPosition;
				c.transform.LookAt(b.center);
			}

        }

	}

}