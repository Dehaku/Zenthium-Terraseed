using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spherize : MonoBehaviour
{
    // https://answers.unity.com/questions/929567/how-do-you-make-a-sphere-from-6-planes.html
    public GameObject planePF;
    public float localScaleOffset = 1;
    public float radius;
    public float radiusOffset;
    public bool bend = false;
    public List<GameObject> faces;
    public List<GameObject> virginFaces;
    public Texture2D heightMap;
    public RenderTexture heightMapRT;
    public bool useRT = false;
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
            newPlane.transform.SetParent(transform);
            newPlane.transform.localScale = new Vector3(0.1f * localScaleOffset, 0.1f * localScaleOffset, 0.1f * localScaleOffset);
            newPlane.transform.position = positionArr[p];
            newPlane.transform.eulerAngles = rotationArr[p];
            newPlane.AddComponent<MeshCollider>();
            newPlane.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            faces.Add(newPlane);
            newPlane.name = "Face" + p;
            var ps = newPlane.AddComponent<PlanetSide>();
            if(p == 0)
                ps.side = PlanetSide.Side.center;
            if (p == 1)
                ps.side = PlanetSide.Side.ddown;
            if (p == 2)
                ps.side = PlanetSide.Side.up;
            if (p == 3)
                ps.side = PlanetSide.Side.down;
            if (p == 4)
                ps.side = PlanetSide.Side.right;
            if (p == 5)
                ps.side = PlanetSide.Side.left;
            ps.planetParent = this.gameObject;




            GameObject virginPlane = Instantiate(planePF, transform);
            virginPlane.transform.parent = null;
            virginPlane.transform.localScale = newPlane.transform.localScale;
            virginPlane.transform.position = newPlane.transform.position;
            virginPlane.transform.eulerAngles = newPlane.transform.eulerAngles;
            virginFaces.Add(virginPlane);
            virginPlane.SetActive(false);

            newPlane.transform.position = newPlane.transform.position + transform.position;
        }
    }

    void SetMeshColliders()
    {
        foreach (var face in faces)
        {
            face.GetComponent<MeshCollider>().sharedMesh = face.GetComponent<MeshFilter>().mesh;
        }
    }

    public bool autoUpdateFaces = false;
    public bool shapeFaces = false;

    

    private void Update()
    {
        if(shapeFaces || autoUpdateFaces)
        {
            shapeFaces = false;
            ShapeFacesHeightMap();
        }
    }


    float GetPercentOfRange(float value, float min, float max)
    {
        // Credit to Biocava for negative percentage range checks.
        return (value - min) / (max - min);
    }

    Color32 SampleHeightMap(Vector2 pointInBounds, Vector4 bounds, Color32[] heightPixels, int heightPixelsWidth)
    {

        Texture heightTexture;
        if(!useRT)
            heightTexture = heightMap;
        else
            heightTexture = heightMapRT;

        //Debug.Log("Width: " + heightTexture.width);
        var pointX = 1 - GetPercentOfRange(pointInBounds.x, bounds.w, bounds.x);
        var pointY = 1 - GetPercentOfRange(pointInBounds.y, bounds.y, bounds.z);

        int hTx = (int)Mathf.Clamp(heightTexture.width * pointX,0,heightPixelsWidth-1);
        int hTy = (int)Mathf.Clamp(heightTexture.height * pointY, 0, heightPixelsWidth - 1);
        



        if (Random.Range(0, 500) == 250)
        {
            //Debug.Log(pointInBounds.x + "," + bounds.w + "," + bounds.x);
            //Debug.Log(pointInBounds.y + "," + bounds.y + "," + bounds.z);
            //Debug.Log("PointX: " + pointX + ", pointY:" + pointY + ", hTx:" + hTx + ", hTy:" + hTy);
        }
        float timer = Time.realtimeSinceStartup;
        //var pixel = heightMap.GetPixel(hTx, hTy);
        //if (hTy < 3 || hTx < 3)
        //    Debug.Log("Coords: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms" + ", " + hTx + ":" + hTy);
        //if (hTy > 510 || hTx > 510)
        //    Debug.Log("Coords: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms" + ", " + hTx + ":" + hTy);
        var pixel = heightPixels[hTy + heightPixelsWidth * hTx];
        //if (hTy < 3 || hTx < 3)
        //    Debug.Log("Get Pixel Time: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms" + ", " + hTx + ":" + hTy);
        //if (hTy > 510 || hTx > 510)
        //    Debug.Log("Get Pixel Time: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms" + ", " + hTx + ":" + hTy);
        //bounds.w bounds.x

        return pixel;

    }

    public bool once = false;

    void ShapeFacesHeightMap()
    {
        float timer = Time.realtimeSinceStartup;
        float offset = (Mathf.Sin(Time.realtimeSinceStartup) + 1) * 0.5f;

        bool first = false;

        

        int iteration = 0;
        foreach (var newPlane in faces)
        {
            if (first)
                continue;
            if(once)
            first = true;


            Vector3[] vertices = virginFaces[iteration].GetComponent<MeshFilter>().mesh.vertices;
            List<Vector2> uvs = new List<Vector2>();
            virginFaces[iteration].GetComponent<MeshFilter>().mesh.GetUVs(0,uvs);

            for(int i = 0; i < uvs.Count; i++)
            {
                //Debug.Log("UVS:" + i + ", " + uvs[i]);
            }
            for(int x = 0; x < 33; x++)
            {
                for (int y = 0; y < 33; y++)
                {
                    //Debug.Log("UVSxy:" +x+":"+y+", ult:"+ uvs[x + 32 * y]);
                }
            }



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
                //Debug.Log("Bounds: " + bounds.w + ": " + bounds.x + ": " + bounds.y + ": " + bounds.z + ": ");
                var mbounds = newPlane.GetComponent<MeshFilter>().mesh.bounds;
                //Debug.Log("MeshBounds: " + mbounds);

                Color32[] heightPixels;
                int heightWidth = 0;
                
                if (!useRT)
                {
                    heightPixels = heightMap.GetPixels32();
                    heightWidth = heightMap.width;
                }
                    
                else
                {
                    Texture2D tex = new Texture2D(heightMapRT.width, heightMapRT.height, TextureFormat.RGBA32, false);


                    var old_rt = RenderTexture.active;
                    
                    RenderTexture.active = heightMapRT;
                    // ReadPixels looks at the active RenderTexture.
                    tex.ReadPixels(new Rect(0, 0, heightMapRT.width, heightMapRT.height), 0, 0);
                    tex.Apply();
                    
                    RenderTexture.active = old_rt;
                    heightPixels = tex.GetPixels32();
                    heightWidth = tex.width;

                    Destroy(tex);

                }
                


                for (var i = 0; i < vertices.Length; i++)
                {

                    Vector2 samplePoint = new Vector2(vertices[i].z, vertices[i].x);
                    //if(i == 3 || i == 9 || i == 27 || i == 81 || i == 243 || i == 511)
                    //{
                    //    Debug.Log("i:" + i + ", xy: " + vertices[i].x + ":" + vertices[i].y);
                    //}
                    //Debug.Log("Pre-Vert " + i + ": " + vertices[i].x + "," + vertices[i].y + "," + vertices[i].z);
                    vertices[i] = virginFaces[iteration].transform.InverseTransformPoint(
                        (
                        virginFaces[iteration].transform.TransformPoint(vertices[i]).normalized 
                        * (this.radius + (SampleHeightMap(samplePoint, bounds, heightPixels, heightWidth).b * heightDamp))
                       // + transform.position
                        ) );
                    //Debug.Log("Aft-Vert " + i + ": " + vertices[i].x + "," + vertices[i].y + "," + vertices[i].z);
                    
                }
            }


            newPlane.GetComponent<MeshFilter>().mesh.vertices = vertices;
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            newPlane.GetComponent<MeshFilter>().mesh.RecalculateBounds();



            iteration++;
        }

        SetMeshColliders();

        // Debug.Log("Time to heightmap terrain: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms");
    }
   
}