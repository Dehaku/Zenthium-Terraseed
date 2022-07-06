using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpherizeBackup : MonoBehaviour
{
    // https://answers.unity.com/questions/929567/how-do-you-make-a-sphere-from-6-planes.html
    public GameObject planePF;
    public float localScaleOffset = 1;
    public float radius;
    public float radiusOffset;
    public bool bend = false;
    public List<GameObject> faces;
    public Texture2D heightMap;
    public List<Vector3[]> planeVertsUntouched;

    public float heightDamp;

    void Start()
    {
        
        Vector3[] positionArr = {
             new Vector3(0,0.5f,0),
             new Vector3(0,-0.5f,0),
             new Vector3(0,0,0.5f),
             new Vector3(0,0,-0.5f),
             new Vector3(0.5f,0,0),
             new Vector3(-0.5f,0,0)
         };

        Vector3[] rotationArr = {
             Vector3.zero,
             new Vector3(180,0,0),
             new Vector3(90,0,0),
             new Vector3(-90,0,0),
             new Vector3(0,0,-90),
             new Vector3(0,0,90)
         };

        for (int p = 0; p < 6; p++)
        {
            //make 6 planes for the sphere in this loop

            GameObject newPlane = Instantiate(planePF,transform);
            faces.Add(newPlane);
                //GameObject.CreatePrimitive(PrimitiveType.Plane);
            newPlane.transform.SetParent(transform);
            newPlane.transform.localScale = new Vector3(0.1f * localScaleOffset, 0.1f * localScaleOffset, 0.1f * localScaleOffset);
            newPlane.transform.position = positionArr[p];
            newPlane.transform.eulerAngles = rotationArr[p];
            
        }
        //ShapeFaces();
    }

    public bool autoUpdateFaces = false;
    public bool shapeFaces = false;

    

    private void Update()
    {
        if(shapeFaces || autoUpdateFaces)
        {
            shapeFaces = false;
            ShapeFacesHeightMap();
            Debug.Log("Sample Calls: " + sampleCalls);
        }
    }


    float GetPercentOfRange(float value, float min, float max)
    {
        // Credit to Biocava for negative percentage range checks.
        return (value - min) / (max - min);
    }



    int sampleCalls = 0;
    Vector3 SampleHeightMap(Vector2 pointInBounds, Vector4 bounds)
    {
        sampleCalls++;
        var heightPixels = heightMap.GetPixels();
        Texture heightTexture = heightMap;
        //Debug.Log("Width: " + heightTexture.width);
        var pointX = GetPercentOfRange(pointInBounds.x, bounds.w, bounds.x);
        var pointY = GetPercentOfRange(pointInBounds.y, bounds.y, bounds.z);

        int hTx = (int)(heightTexture.width * pointX);
        int hTy = (int)(heightTexture.height * pointY);

        if (Random.Range(0,500) == 250)
        {
            Debug.Log(pointInBounds.x + "," + bounds.w + "," + bounds.x);
            Debug.Log(pointInBounds.y + "," + bounds.y + "," + bounds.z);
            Debug.Log("PointX: " + pointX + ", pointY:" + pointY + ", hTx:" + hTx + ", hTy:" + hTy);
        }
        float timer = Time.realtimeSinceStartup;
        var pixel = heightMap.GetPixel(hTx, hTy);
        if(hTy < 3 || hTx < 3)    
        Debug.Log("Get Pixel Time: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms" + ", " + hTx + ":" + hTy);
        //bounds.w bounds.x

        return new Vector3(0,0,pixel.b);

    }


    void ShapeFacesHeightMap()
    {
        float timer = Time.realtimeSinceStartup;
        float offset = (Mathf.Sin(Time.realtimeSinceStartup) + 1) * 0.5f;

        bool first = false;

        foreach (var newPlane in faces)
        {
            if (first)
                continue;
            first = true;


            Vector3[] vertices = newPlane.GetComponent<MeshFilter>().mesh.vertices;
            var heightPixels = heightMap.GetPixels();
            if (bend)
            {
                Vector4 bounds = new Vector4();
                for (var i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i].x < bounds.w)
                        bounds.w = vertices[i].x;
                    if (vertices[i].x > bounds.x)
                        bounds.x = vertices[i].x;
                    if (vertices[i].z < bounds.y)
                        bounds.y = vertices[i].z;
                    if (vertices[i].z > bounds.z)
                        bounds.z = vertices[i].z;
                }
                Debug.Log("Bounds: " + bounds.w + ": " + bounds.x + ": " + bounds.y + ": " + bounds.z + ": ");
                var mbounds = newPlane.GetComponent<MeshFilter>().mesh.bounds;
                Debug.Log("MeshBounds: " + mbounds);
                
                //Debug.Log("MeshBoundsEx: " +mbounds. + ": " + mbounds.x + ": " + mbounds.y + ": " + mbounds.z + ": ");

                for (var i = 0; i < vertices.Length; i++)
                {

                    //vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * this.radius);
                    Vector2 samplePoint = new Vector2(vertices[i].x, vertices[i].z);
                    Debug.Log("Pre-Vert " + i + ": " + vertices[i].x + "," + vertices[i].y + "," + vertices[i].z);
                    vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * (this.radius + (SampleHeightMap(samplePoint, bounds).z * heightDamp)) );
                    Debug.Log("Aft-Vert " + i + ": " + vertices[i].x + "," + vertices[i].y + "," + vertices[i].z);
                    //vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * (this.radius + radiusOffset));

                    //int minArea = (int)Mathf.Clamp((vertices.Length * offset) - (vertices.Length * 0.1f), 0, vertices.Length);
                    //int maxArea = (int)Mathf.Clamp((vertices.Length * offset) + (vertices.Length * 0.1f), 0, vertices.Length);
                    //newPlane.GetComponent<MeshFilter>().mesh.index
                    //if (i > minArea && i < maxArea)
                    //    vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * (this.radius + radiusOffset));
                }
            }


            newPlane.GetComponent<MeshFilter>().mesh.vertices = vertices;
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateBounds();

            newPlane.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            
            
        }

        Debug.Log("Time to heightmap terrain: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms");
    }

    void ShapeFaces()
    {
        float offset = (Mathf.Sin(Time.realtimeSinceStartup)+1)*0.5f;
        
        foreach (var newPlane in faces)
        {
            Vector3[] vertices = newPlane.GetComponent<MeshFilter>().mesh.vertices;

            if (bend)
            {
                Debug.Log("Vert Length: " + vertices.Length);
                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * this.radius);
                    //vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * (this.radius + radiusOffset));

                    int minArea = (int)Mathf.Clamp((vertices.Length * offset)-(vertices.Length*0.1f), 0, vertices.Length);
                    int maxArea = (int)Mathf.Clamp((vertices.Length * offset) + (vertices.Length * 0.1f), 0, vertices.Length);
                    
                    if (i > minArea && i < maxArea)
                        vertices[i] = newPlane.transform.InverseTransformPoint(newPlane.transform.TransformPoint(vertices[i]).normalized * (this.radius + radiusOffset));
                }
            }


            newPlane.GetComponent<MeshFilter>().mesh.vertices = vertices;
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateBounds();

            newPlane.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            /*
            Texture2D tex = new Texture2D(128,128);
            newPlane.GetComponent<MeshRenderer>().material.mainTexture = tex;
            */
        }
    }
}