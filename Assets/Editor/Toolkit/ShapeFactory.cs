using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

namespace Toolkit
{
    public class ShapeFactory
    {
        private class MaterialComparer : IEqualityComparer<Material>
        {
            public bool Equals(Material x, Material y)
            {
                return x.name.Equals(y.name);
            }

            public int GetHashCode(Material obj)
            {
                return obj.GetHashCode();
            }
        }

		public static void combineObjects(GameObject[] objects)
		{
			MeshFilter[] meshFilters = new MeshFilter[objects.Length];

			for (var i = 0; i < objects.Length;i++) {
				meshFilters[i] = objects[i].GetComponent<MeshFilter>();
			}

			ArrayList materials = new ArrayList();
			ArrayList combineInstanceArrays = new ArrayList();

			GameObject gameObject = new GameObject();
			gameObject.name = Selection.gameObjects[0].name+"_merged";

			foreach (MeshFilter meshFilter in meshFilters)
			{
				MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

				if (!meshRenderer ||
					!meshFilter.sharedMesh ||
					meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
				{
					continue;
				}

				for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
				{
					int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
					if (materialArrayIndex == -1)
					{
						materials.Add(meshRenderer.sharedMaterials[s]);
						materialArrayIndex = materials.Count - 1;
					}
					combineInstanceArrays.Add(new ArrayList());

					CombineInstance combineInstance = new CombineInstance();
					combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
					combineInstance.subMeshIndex = s;
					combineInstance.mesh = meshFilter.sharedMesh;
					(combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
				}
			}

			// Get / Create mesh filter & renderer
			MeshFilter meshFilterCombine = gameObject.GetComponent<MeshFilter>();
			if (meshFilterCombine == null)
			{
				meshFilterCombine = gameObject.AddComponent<MeshFilter>();
			}
			MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer>();
			if (meshRendererCombine == null)
			{
				meshRendererCombine = gameObject.AddComponent<MeshRenderer>();
			}

			// Combine by material index into per-material meshes
			// also, Create CombineInstance array for next step
			Mesh[] meshes = new Mesh[materials.Count];
			CombineInstance[] combineInstances = new CombineInstance[materials.Count];

			for (int m = 0; m < materials.Count; m++)
			{
				CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
				meshes[m] = new Mesh();
				meshes[m].CombineMeshes(combineInstanceArray, true, true);

				combineInstances[m] = new CombineInstance();
				combineInstances[m].mesh = meshes[m];
				combineInstances[m].subMeshIndex = 0;
			}

			// Combine into one
			meshFilterCombine.sharedMesh = new Mesh();
			meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

			string name = "combined_" + Guid.NewGuid();
			meshFilterCombine.sharedMesh.name = name;
			AssetDatabase.CreateAsset(meshFilterCombine.sharedMesh,"Assets/"+name+".asset");


			// Assign materials
			Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
			meshRendererCombine.materials = materialsArray;

			foreach (GameObject o in objects) {
				o.SetActive(false);
			}

		}

		private static int Contains(ArrayList searchList, string searchName)
		{
			for (int i = 0; i < searchList.Count; i++)
			{
				if (((Material)searchList[i]).name == searchName)
				{
					return i;
				}
			}
			return -1;
		}

	    public static GameObject createTube(float height, int nbSides, float topRadius1, float topRadius2, float bottomRadius1, float bottomRadius2) {

		    GameObject gameObject = new GameObject();
		    gameObject.name = "Tube";

		    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
		    filter.sharedMesh = new Mesh();

		    int nbVerticesCap = nbSides * 2 + 2;
		    int nbVerticesSides = nbSides * 2 + 2;
		    #region Vertices

		    // bottom + top + sides
		    Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
		    int vert = 0;
		    float _2pi = Mathf.PI * 2f;

		    // Bottom cap
		    int sideCounter = 0;
		    while( vert < nbVerticesCap )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;
			    float cos = Mathf.Cos(r1);
			    float sin = Mathf.Sin(r1);
			    vertices[vert] = new Vector3( cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
			    vertices[vert+1] = new Vector3( cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
			    vert += 2;
		    }

		    // Top cap
		    sideCounter = 0;
		    while( vert < nbVerticesCap * 2 )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;
			    float cos = Mathf.Cos(r1);
			    float sin = Mathf.Sin(r1);
			    vertices[vert] = new Vector3( cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
			    vertices[vert+1] = new Vector3( cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
			    vert += 2;
		    }

		    // Sides (out)
		    sideCounter = 0;
		    while (vert < nbVerticesCap * 2 + nbVerticesSides )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;
			    float cos = Mathf.Cos(r1);
			    float sin = Mathf.Sin(r1);

			    vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
			    vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
			    vert+=2;
		    }

		    // Sides (in)
		    sideCounter = 0;
		    while (vert < vertices.Length )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;
			    float cos = Mathf.Cos(r1);
			    float sin = Mathf.Sin(r1);

			    vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
			    vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
			    vert += 2;
		    }
		    #endregion

		    #region Normales

		    // bottom + top + sides
		    Vector3[] normales = new Vector3[vertices.Length];
		    vert = 0;

		    // Bottom cap
		    while( vert < nbVerticesCap )
		    {
			    normales[vert++] = Vector3.down;
		    }

		    // Top cap
		    while( vert < nbVerticesCap * 2 )
		    {
			    normales[vert++] = Vector3.up;
		    }

		    // Sides (out)
		    sideCounter = 0;
		    while (vert < nbVerticesCap * 2 + nbVerticesSides )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;

			    normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
			    normales[vert+1] = normales[vert];
			    vert+=2;
		    }

		    // Sides (in)
		    sideCounter = 0;
		    while (vert < vertices.Length )
		    {
			    sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			    float r1 = (float)(sideCounter++) / nbSides * _2pi;

			    normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
			    normales[vert+1] = normales[vert];
			    vert+=2;
		    }
		    #endregion

		    #region UVs
		    Vector2[] uvs = new Vector2[vertices.Length];

		    vert = 0;
		    // Bottom cap
		    sideCounter = 0;
		    while( vert < nbVerticesCap )
		    {
			    float t = (float)(sideCounter++) / nbSides;
			    uvs[ vert++ ] = new Vector2( 0f, t );
			    uvs[ vert++ ] = new Vector2( 1f, t );
		    }

		    // Top cap
		    sideCounter = 0;
		    while( vert < nbVerticesCap * 2 )
		    {
			    float t = (float)(sideCounter++) / nbSides;
			    uvs[ vert++ ] = new Vector2( 0f, t );
			    uvs[ vert++ ] = new Vector2( 1f, t );
		    }

		    // Sides (out)
		    sideCounter = 0;
		    while (vert < nbVerticesCap * 2 + nbVerticesSides )
		    {
			    float t = (float)(sideCounter++) / nbSides;
			    uvs[ vert++ ] = new Vector2( t, 0f );
			    uvs[ vert++ ] = new Vector2( t, 1f );
		    }

		    // Sides (in)
		    sideCounter = 0;
		    while (vert < vertices.Length )
		    {
			    float t = (float)(sideCounter++) / nbSides;
			    uvs[ vert++ ] = new Vector2( t, 0f );
			    uvs[ vert++ ] = new Vector2( t, 1f );
		    }
		    #endregion

		    #region Triangles
		    int nbFace = nbSides * 4;
		    int nbTriangles = nbFace * 2;
		    int nbIndexes = nbTriangles * 3;
		    int[] triangles = new int[nbIndexes];

		    // Bottom cap
		    int i = 0;
		    sideCounter = 0;
		    while (sideCounter < nbSides)
		    {
			    int current = sideCounter * 2;
			    int next = sideCounter * 2 + 2;

			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = next;
			    triangles[ i++ ] = current;

			    triangles[ i++ ] = current + 1;
			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = current;

			    sideCounter++;
		    }

		    // Top cap
		    while (sideCounter < nbSides * 2)
		    {
			    int current = sideCounter * 2 + 2;
			    int next = sideCounter * 2 + 4;

			    triangles[ i++ ] = current;
			    triangles[ i++ ] = next;
			    triangles[ i++ ] = next + 1;

			    triangles[ i++ ] = current;
			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = current + 1;

			    sideCounter++;
		    }

		    // Sides (out)
		    while( sideCounter < nbSides * 3 )
		    {
			    int current = sideCounter * 2 + 4;
			    int next = sideCounter * 2 + 6;

			    triangles[ i++ ] = current;
			    triangles[ i++ ] = next;
			    triangles[ i++ ] = next + 1;

			    triangles[ i++ ] = current;
			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = current + 1;

			    sideCounter++;
		    }


		    // Sides (in)
		    while( sideCounter < nbSides * 4 )
		    {
			    int current = sideCounter * 2 + 6;
			    int next = sideCounter * 2 + 8;

			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = next;
			    triangles[ i++ ] = current;

			    triangles[ i++ ] = current + 1;
			    triangles[ i++ ] = next + 1;
			    triangles[ i++ ] = current;

			    sideCounter++;
		    }
		    #endregion

		    filter.sharedMesh.vertices = vertices;
		    filter.sharedMesh.normals = normales;
		    filter.sharedMesh.uv = uvs;
		    filter.sharedMesh.triangles = triangles;
		    filter.sharedMesh.name = "Tube";

		    filter.sharedMesh.RecalculateBounds();

		    gameObject.AddComponent<MeshRenderer>();
		    gameObject.GetComponent<MeshRenderer>().material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

		    AssetDatabase.CreateAsset(filter.sharedMesh,"Assets/cylinder.asset");

		    return gameObject;

	    }

        public static GameObject createTorus(float radius1, float radius2, int nbRadSeg, int nbSides)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Torus";

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = new Mesh();

            #region Vertices		
            Vector3[] vertices = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
            float _2pi = Mathf.PI * 2f;
            for (int seg = 0; seg <= nbRadSeg; seg++)
            {
                int currSeg = seg == nbRadSeg ? 0 : seg;

                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

                for (int side = 0; side <= nbSides; side++)
                {
                    int currSide = side == nbSides ? 0 : side;

                    Vector3 normale = Vector3.Cross(r1, Vector3.up);
                    float t2 = (float)currSide / nbSides * _2pi;
                    Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

                    vertices[side + seg * (nbSides + 1)] = r1 + r2;
                }
            }
            #endregion

            #region Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for (int seg = 0; seg <= nbRadSeg; seg++)
            {
                int currSeg = seg == nbRadSeg ? 0 : seg;

                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

                for (int side = 0; side <= nbSides; side++)
                {
                    normales[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)] - r1).normalized;
                }
            }
            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int seg = 0; seg <= nbRadSeg; seg++)
                for (int side = 0; side <= nbSides; side++)
                    uvs[side + seg * (nbSides + 1)] = new Vector2((float)seg / nbRadSeg, (float)side / nbSides);
            #endregion

            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            int i = 0;
            for (int seg = 0; seg <= nbRadSeg; seg++)
            {
                for (int side = 0; side <= nbSides - 1; side++)
                {
                    int current = side + seg * (nbSides + 1);
                    int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

                    if (i < triangles.Length - 6)
                    {
                        triangles[i++] = current;
                        triangles[i++] = next;
                        triangles[i++] = next + 1;

                        triangles[i++] = current;
                        triangles[i++] = next + 1;
                        triangles[i++] = current + 1;
                    }
                }
            }
            #endregion

            filter.sharedMesh.vertices = vertices;
            filter.sharedMesh.normals = normales;
            filter.sharedMesh.uv = uvs;
            filter.sharedMesh.triangles = triangles;
		    filter.sharedMesh.name = "Torus";

            filter.sharedMesh.RecalculateBounds();

            gameObject.AddComponent<MeshRenderer>();
            gameObject.GetComponent<MeshRenderer>().material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

		    AssetDatabase.CreateAsset(filter.sharedMesh,"Assets/torus.asset");

            return gameObject;
        }

    }


}
