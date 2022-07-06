using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public GameObject faceGO;
    public Camera faceCanvasCam;
    public RenderTexture canvasTexture;
    public Material baseMaterial;
    public Material canvasMaterial;
    public Face neighborLeft;
    public Face neighborUp;
    public Face neighborRight;
    public Face neighborDown;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
