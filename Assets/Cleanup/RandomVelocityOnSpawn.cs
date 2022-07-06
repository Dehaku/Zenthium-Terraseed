using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVelocityOnSpawn : MonoBehaviour
{
    public Vector2 LaunchPowerRange;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {


        Vector3 direction = new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f));
        float power = (Random.Range(LaunchPowerRange.x, LaunchPowerRange.y))*100;

        if (power == 0f)
            return;

        float forceMagnitude = power * rb.mass;
        //Vector3 force = direction.normalized * forceMagnitude;
        Vector3 force = direction * forceMagnitude;

        rb.AddForce(force);

        Destroy(this);
    }

    
}
