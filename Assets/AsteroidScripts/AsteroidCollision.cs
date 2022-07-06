using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mass))]
public class AsteroidCollision : MonoBehaviour
{
    
    public Mass myMass;

    AsteroidLogic aLogic;

    private void Start()
    {
        //var timer = Time.realtimeSinceStartup;
        // Probably a better way to do this.
        var aLogicThing = GameObject.FindGameObjectsWithTag("AsteroidManager");
        aLogic = aLogicThing[0].GetComponent<AsteroidLogic>();
        //Debug.Log("Time Taken: " + (Time.realtimeSinceStartup - timer) * 1000 + "ms");
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision! " + gameObject.name + ":" + collision.gameObject.name);

        var theirMass = collision.gameObject.GetComponent<Mass>();
        if (!theirMass)
            return;
        
        // Smaller mass gets eaten.
        if(myMass.GetMass() >= theirMass.GetMass())
        {
            AbsorbBody(myMass,theirMass,gameObject,collision.gameObject);
        }
        else
            AbsorbBody(theirMass, myMass, collision.gameObject, gameObject);
    }

    void AbsorbBody(Mass eaterMass, Mass victimMass, GameObject eaterGO, GameObject victimGO)
    { // Replace destroy with a pool later. >>>Maybe give half material each hit instead of removing, removing after below 1 substance unit
        
        foreach (var item in victimMass.substances)
        {
            eaterMass.AddSubstance(item.SO, item.amount);
            eaterMass.bodies += victimMass.bodies;
            victimMass.bodies = 0;
        }
        victimMass.substances.Clear();

        var eaterRB = eaterGO.GetComponent<Rigidbody>();
        var victimRB = victimGO.GetComponent<Rigidbody>();


        var relativeVelocity = eaterRB.velocity - victimRB.velocity;
        var impactMagnitude = relativeVelocity.magnitude;

        Debug.Log("Velocities: " + eaterRB.velocity + " : " + victimRB.velocity + ", Impact: " + relativeVelocity + " : " + impactMagnitude);

        eaterRB.AddForce(victimRB.velocity * victimRB.mass, ForceMode.Impulse);
        victimGO.SetActive(false);
        //Destroy(victimGO); // Replace this with a pool later.
    }

    private void OnDisable()
    {
        //var timer = Time.realtimeSinceStartup;
        aLogic.RemoveObjectRB(GetComponent<Rigidbody>());
        //Debug.Log("Time Taken: " + (Time.realtimeSinceStartup - timer) * 1000 + "ms");
    }
}
