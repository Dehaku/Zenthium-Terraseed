using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetPainter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;
                Vector2 pixelUV = hit.textureCoord2;

                //Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
                //uvPosition.x = pixelUV.x;
                //uvPosition.y = pixelUV.y;
                //uvPosition.z = 0.0f;

                //var tex = hit.collider.GetComponent<MeshRenderer>().material.GetTexture();
                //return true;


                Renderer rend = hit.transform.GetComponent<Renderer>();

                //MeshCollider meshCollider = hit.collider as MeshCollider;
                Debug.Log("Hit: " + pixelUV);

                if (rend == null || rend.sharedMaterial == null ||
                    rend.sharedMaterial.mainTexture == null || meshCollider == null)
                {
                    if (rend == null)
                        Debug.Log("Con1");
                    if (rend.sharedMaterial == null)
                        Debug.Log("Con2");
                    if (rend.sharedMaterial.mainTexture == null)
                        Debug.Log("Con3");
                    if (meshCollider == null)
                        Debug.Log("Con4");

                    var mainTex = rend.sharedMaterial.GetTexture("_MainTex");

                    if(mainTex == null)
                    { Debug.Log("wtf"); }
                    else
                    {
                        Texture2D newTex = mainTex as Texture2D;

                        pixelUV.x *= newTex.width;
                        pixelUV.y *= newTex.height;

                        newTex.SetPixel(Mathf.FloorToInt(pixelUV.x), Mathf.FloorToInt(pixelUV.y), Color.black);

                        newTex.Apply();
                    }

                    Debug.Log("Condition1, return");
                    
                    return;
                }

                // Now draw a pixel where we hit the object
                Texture2D tex = rend.material.mainTexture as Texture2D;
                
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                tex.SetPixel(Mathf.FloorToInt(pixelUV.x), Mathf.FloorToInt(pixelUV.y), Color.black);

                tex.Apply();

            }


        }
    }
}
