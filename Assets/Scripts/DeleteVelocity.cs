using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteVelocity : MonoBehaviour
{
    public bool deleteVelocity = false;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (deleteVelocity)
        {
            if(rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            if(rb)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                Debug.Log("No RB found in " + gameObject.name);
            }
            
        }
            
    }
}
