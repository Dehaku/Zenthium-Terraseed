using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Taken and modified from https://forum.unity.com/threads/orbital-physics-maintaining-a-circular-orbit.403077/

public class ForceOrbit : MonoBehaviour
{
    AsteroidLogic astLogic;
    public float forceAmount = 1000;
    public Vector3 pushVec;
    private Rigidbody objectToPush;
    public float _G = 667.4f; // increased gravitational constant
    public float _M = 100000f; // temp number : will be replaced by Dominant Gravity Source's Mass
    public float _r = 10000f; // temp number : will be replaced by distance between mass centers
    public float _F;

    public bool _Circularizing = true; // set this to TRUE in the inspector to create a circular orbit
    private Rigidbody biggestMass; // this is a public class that keeps track of the dominant gravity source / mass // etc for a given object

    public bool randomRotation = false; // can ignore the random raotation functions for this
    private float xseed;
    private float yseed;
    private float zfeed;

    // Use this for initialization
    void Start()
    {
        SetBiggestMass();
        //biggestMass = GetComponent<GravityObject>();
        objectToPush = gameObject.GetComponent<Rigidbody>();

        StartCoroutine("WaitingForInfo");
    }
    void SetBiggestMass()
    {
        astLogic = FindObjectOfType<AsteroidLogic>();
        float biggest = 0;
        Rigidbody biggestRB = null;
        foreach (var item in astLogic.GetObjectRBs())
        {
            if (!biggestRB)
            {
                biggestRB = item;
                biggest = item.mass;
                continue;
            }

            if(item.mass > biggest)
            {
                biggestRB = item;
                biggest = item.mass;
            }
        }

        biggestMass = biggestRB;
    }

    private void StartPush()
    {
        objectToPush.mass = objectToPush.mass * transform.localScale.x;
        Vector3 gravity = biggestMass.position - transform.position;
        _M = biggestMass.mass; //the Mass attracting
        Vector3 pushDirection = Vector3.ProjectOnPlane(transform.forward, -gravity.normalized).normalized; //gets the correct direction to push for a circular orbit

        _r = gravity.magnitude;
        //Debug.Log("Has Radius");

        //Debug.Log("Has Gravity Direction");

        pushVec = pushDirection;

        if (_Circularizing)
        {
            GetCircularizingForce();
        }
        else
        {
            pushVec = pushVec * forceAmount;
            objectToPush.velocity = pushVec;
        }

        if (randomRotation) // this is can be ignored ..
        {
            xseed = Random.Range(2f, 89f);
            yseed = Random.Range(2f, 89f);
            zfeed = Random.Range(2f, 89f);
            transform.rotation = Quaternion.Euler(xseed, yseed, zfeed);
        }
    }

    private IEnumerator WaitingForInfo() //get the dominant gravity source : this is a two body equation
    {
        yield return new WaitUntil(() => biggestMass != null); // get the largest mass exerting force on this

        StartPush();
    }

    void GetCircularizingForce()
    {
        _F = Mathf.Sqrt((_G * _M) / _r); // here is the trick .. only the larger Mass's value is included in the equation
        pushVec = pushVec * _F;
        objectToPush.velocity = pushVec;
    }
}
