using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RollingHills : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        UpdateMesh();
    }

    void CreateMesh()
    {
        // Create vertices
        vertices = new Vector3[100];
        for (int i = 0; i < vertices.Length; i++)
        {
            float x = i * 0.1f;
            float y = Mathf.Sin(i * 0.2f) * 3f;  // Use sine wave to create rolling effect
            vertices[i] = new Vector3(x, y, 0);
        }

        // Create triangles
        int[] triangles = new int[(vertices.Length - 1) * 6];
        for (int i = 0, vert = 0; i < triangles.Length; i += 6, vert += 2)
        {
            triangles[i] = vert;
            triangles[i + 1] = triangles[i + 4] = vert + 1;
            triangles[i + 2] = triangles[i + 3] = vert + 2;
            triangles[i + 5] = vert + 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
