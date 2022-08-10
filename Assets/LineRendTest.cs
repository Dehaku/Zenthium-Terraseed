using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendTest : MonoBehaviour
{
    public LineRendererContainerSO lineRendSO;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(lineRendSO);
        if (lineRendSO)
            Debug.Log(lineRendSO.lineRenderer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
