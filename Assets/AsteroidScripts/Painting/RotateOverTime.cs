using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Transform target;
    public Vector3 rotateDir;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        target.eulerAngles += (rotateDir * Time.deltaTime);
    }
}
