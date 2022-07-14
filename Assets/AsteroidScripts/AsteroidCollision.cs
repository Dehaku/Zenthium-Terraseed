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
        //Debug.Log("myMass:" + name + ", " + myMass.bodies + ", vs " + collision.collider.name);
        if (myMass.bodies < 0.5f) // We've already been consumed, don't run any logic. 1 didn't seem to work. Floating issues?
            return;

        var theirMass = collision.gameObject.GetComponent<Mass>();
        if (!theirMass)
            return;

        var myMassAmount = myMass.GetMass();
        var theirMassAmount = theirMass.GetMass();

        if (myMassAmount == 0 || theirMassAmount == 0)
            return;

        // Smaller mass gets eaten.
        if (myMassAmount >= theirMass.GetMass())
        {
            AbsorbBody(myMass,theirMass,gameObject,collision.gameObject);
        }
        else
            AbsorbBody(theirMass,myMass, collision.gameObject, gameObject);
    }

    void AbsorbBody(Mass eaterMass,Mass victimMass, GameObject eaterGO, GameObject victimGO)
    { // Replace destroy with a pool later. >>>Maybe give half material each hit instead of removing, removing after below 1 substance unit

        eaterMass.bodies += victimMass.bodies;
        victimMass.bodies = 0;

        foreach (var item in victimMass.substances)
        {
            eaterMass.AddSubstance(item.SO, item.amount);
        }
        victimMass.substances.Clear();

        var eaterRB = eaterGO.GetComponent<Rigidbody>();
        var victimRB = victimGO.GetComponent<Rigidbody>();


        var relativeVelocity = eaterRB.velocity - victimRB.velocity;
        var impactMagnitude = relativeVelocity.magnitude;

        //Debug.Log("Velocities: " + eaterRB.velocity + " : " + victimRB.velocity + ", Impact: " + relativeVelocity + " : " + impactMagnitude);

        eaterRB.AddForce(victimRB.velocity * victimRB.mass, ForceMode.Impulse);

        if (victimMass.asteroid) // release from pool.
            victimMass.asteroid.Release();
        else if (victimGO.transform.parent) // Legacy code, JIC
            victimGO.transform.parent.gameObject.SetActive(false);
        else
            victimGO.SetActive(false);
    }

    private void OnDisable()
    {
        //var timer = Time.realtimeSinceStartup;
        aLogic.RemoveObjectRB(GetComponent<Rigidbody>());
        //Debug.Log("Time Taken: " + (Time.realtimeSinceStartup - timer) * 1000 + "ms");
    }
}
