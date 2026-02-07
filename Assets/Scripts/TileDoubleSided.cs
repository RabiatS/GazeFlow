using UnityEngine;

// Makes tiles always visible from both sides by duplicating the mesh
public class TileDoubleSided : MonoBehaviour
{
    void Start()
    {
        MakeDoubleSided();
    }
    
    void MakeDoubleSided()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;
        
        Mesh originalMesh = meshFilter.sharedMesh;
        if (originalMesh == null) return;
        
        // Create a new mesh that's double-sided
        Mesh doubleSidedMesh = new Mesh();
        doubleSidedMesh.name = originalMesh.name + "_DoubleSided";
        
        // Get original vertices and triangles
        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;
        Vector2[] uvs = originalMesh.uv;
        Vector3[] normals = originalMesh.normals;
        
        // Duplicate vertices and flip normals for back faces
        Vector3[] newVertices = new Vector3[vertices.Length * 2];
        int[] newTriangles = new int[triangles.Length * 2];
        Vector2[] newUvs = new Vector2[uvs.Length * 2];
        Vector3[] newNormals = new Vector3[normals.Length * 2];
        
        // Copy front faces
        for (int i = 0; i < vertices.Length; i++)
        {
            newVertices[i] = vertices[i];
            newUvs[i] = uvs[i];
            newNormals[i] = normals[i];
        }
        
        // Copy back faces with flipped normals
        for (int i = 0; i < vertices.Length; i++)
        {
            newVertices[vertices.Length + i] = vertices[i];
            newUvs[vertices.Length + i] = uvs[i];
            newNormals[vertices.Length + i] = -normals[i];
        }
        
        // Copy front triangles
        for (int i = 0; i < triangles.Length; i++)
        {
            newTriangles[i] = triangles[i];
        }
        
        // Copy back triangles (reversed order)
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int baseIndex = triangles.Length + i;
            newTriangles[baseIndex] = triangles[i + 2] + vertices.Length;
            newTriangles[baseIndex + 1] = triangles[i + 1] + vertices.Length;
            newTriangles[baseIndex + 2] = triangles[i] + vertices.Length;
        }
        
        // Assign to mesh
        doubleSidedMesh.vertices = newVertices;
        doubleSidedMesh.triangles = newTriangles;
        doubleSidedMesh.uv = newUvs;
        doubleSidedMesh.normals = newNormals;
        doubleSidedMesh.RecalculateBounds();
        
        meshFilter.mesh = doubleSidedMesh;
    }
}
