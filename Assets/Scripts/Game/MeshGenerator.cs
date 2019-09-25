using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
	public Material aerialMaterial, slideMaterial;
	private Mesh mesh;
	// Start is called before the first frame update
	void Start () { }

	// Update is called once per frame
	void Update () { }

	public void generateQuadMesh (Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3) {
		mesh = new Mesh ();
		Vector3[] newVertices = new Vector3[4];
		Vector2[] newUV = new Vector2[4];
		int[] newTriangles = new int[2 * 3];

		// 頂点座標の指定.
		newVertices[0] = v3;
		newVertices[1] = v0;
		newVertices[2] = v1;
		newVertices[3] = v2;

		// UVの指定 (頂点数と同じ数を指定すること).
		newUV[0] = new Vector2 (0.0f, 0.0f);
		newUV[1] = new Vector2 (0.0f, 1.0f);
		newUV[2] = new Vector2 (1.0f, 1.0f);
		newUV[3] = new Vector2 (1.0f, 0.0f);

		// 三角形ごとの頂点インデックスを指定.
		newTriangles[0] = 2;
		newTriangles[1] = 1;
		newTriangles[2] = 0;
		newTriangles[3] = 0;
		newTriangles[4] = 3;
		newTriangles[5] = 2;

		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.triangles = newTriangles;

		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();

		GetComponent<MeshFilter> ().sharedMesh = mesh;
		GetComponent<MeshFilter> ().sharedMesh.name = "myMesh";
		this.gameObject.GetComponent<Renderer> ().material = slideMaterial;
	}

	public void generateAerialMesh (Vector3 v0, Vector3 v1) {
		mesh = new Mesh ();
		Vector3[] newVertices = new Vector3[8];
		Vector2[] newUV = new Vector2[8];
		int[] newTriangles = new int[6 * 2 * 3 + 2 * 2 * 3];

		// 頂点座標の指定.
		newVertices[0] = v1 + new Vector3 (-0.2f, 0, 0);
		newVertices[1] = v0 + new Vector3 (-0.2f, 0, 0);
		newVertices[2] = v0 + new Vector3 (0, 0, -0.2f); //    23  
		newVertices[3] = v1 + new Vector3 (0, 0, -0.2f); //  01  45
		newVertices[4] = v1 + new Vector3 (0.2f, 0, 0); //     67  
		newVertices[5] = v0 + new Vector3 (0.2f, 0, 0);
		newVertices[6] = v0 + new Vector3 (0, 0, 0.2f);
		newVertices[7] = v1 + new Vector3 (0, 0, 0.2f);

		// UVの指定 (頂点数と同じ数を指定すること).
		newUV[0] = new Vector2 (0.0f, 0.0f); //  0--37--4
		newUV[1] = new Vector2 (0.0f, 1.0f); //  |  ||  |
		newUV[2] = new Vector2 (1.0f, 1.0f); //  |  ||  |
		newUV[3] = new Vector2 (1.0f, 0.0f); //  1--26--5(UVの貼り付け)
		newUV[4] = new Vector2 (0.0f, 0.0f);
		newUV[5] = new Vector2 (0.0f, 1.0f);
		newUV[6] = new Vector2 (1.0f, 1.0f);
		newUV[7] = new Vector2 (1.0f, 0.0f);

		// 三角形ごとの頂点インデックスを指定.
		newTriangles[0] = 2;
		newTriangles[1] = 1;
		newTriangles[2] = 0;
		newTriangles[3] = 0;
		newTriangles[4] = 3;
		newTriangles[5] = 2;
		newTriangles[6] = 5;
		newTriangles[7] = 2;
		newTriangles[8] = 3;
		newTriangles[9] = 3;
		newTriangles[10] = 4;
		newTriangles[11] = 5;
		newTriangles[12] = 5;
		newTriangles[13] = 6;
		newTriangles[14] = 7;
		newTriangles[15] = 7;
		newTriangles[16] = 4;
		newTriangles[17] = 5;
		newTriangles[18] = 6;
		newTriangles[19] = 1; //  0--37--4
		newTriangles[20] = 0; //  |  ||  |
		newTriangles[21] = 0; //  |  ||  |
		newTriangles[22] = 7; //  1--26--5(UVの貼り付け)
		newTriangles[23] = 6;
		newTriangles[24] = 0;
		newTriangles[25] = 3;
		newTriangles[26] = 7;
		newTriangles[27] = 3;
		newTriangles[28] = 7;
		newTriangles[29] = 4;
		newTriangles[30] = 1;
		newTriangles[31] = 2;
		newTriangles[32] = 6;
		newTriangles[33] = 2;
		newTriangles[34] = 6;
		newTriangles[35] = 5;
		// newTriangles[36] = 3;
		// newTriangles[37] = 2;
		// newTriangles[38] = 5;
		// newTriangles[39] = 5;
		// newTriangles[40] = 4;
		// newTriangles[41] = 3;
		// newTriangles[42] = 0;
		// newTriangles[43] = 1;
		// newTriangles[44] = 2;
		// newTriangles[45] = 2;
		// newTriangles[46] = 3;
		// newTriangles[47] = 0;

		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.triangles = newTriangles;

		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();

		GetComponent<MeshFilter> ().sharedMesh = mesh;
		GetComponent<MeshFilter> ().sharedMesh.name = "myMesh";
		this.gameObject.GetComponent<Renderer> ().material = aerialMaterial;
	}
}