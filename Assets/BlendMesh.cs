using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendMesh : MonoBehaviour
{

    public GameObject objectA;
    public GameObject objectB;

    private Vector3[] newVertices;
    private int[] newTriangles;

    private Mesh meshA;
    private Mesh meshB;
    private Mesh meshC;

    void Start()
    {

        meshA = objectA.GetComponent<MeshFilter>().mesh;
        meshB = objectB.GetComponent<MeshFilter>().mesh;

        newVertices = meshA.vertices; //we'll overwrite these in the forloop
        newTriangles = meshA.triangles;

        for (int i = 0; i < meshA.vertices.Length; i++)
        {

            Vector3 start = meshA.vertices[i];
            Vector3 end = NearestVertexTo(start, meshB); ;
            newVertices[i] = ((end - start) * 0.5f) + start;

        }

        GameObject newObj = new GameObject(); //a new gameobject for creating the mesh
        newObj.AddComponent<MeshRenderer>();
        newObj.AddComponent<MeshFilter>();

        meshC = new Mesh();
        newObj.GetComponent<MeshFilter>().mesh = meshC;
        meshC.vertices = newVertices;
        meshC.triangles = newTriangles;

        

    }

    void Update()
    {
        GetComponent<MeshFilter>().mesh = meshC;
    }

    public Vector3 NearestVertexTo(Vector3 point, Mesh mesh)
    {
        // convert point to local space
        point = transform.InverseTransformPoint(point);

        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestVertex = Vector3.zero;
        // scan all vertices to find nearest
        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 diff = point - vertex;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }
        // convert nearest vertex back to world space
        return transform.TransformPoint(nearestVertex);

    }
}