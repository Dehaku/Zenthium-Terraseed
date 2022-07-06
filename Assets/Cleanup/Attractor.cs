using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{

	public float GravityConstant = 667.4f;

	public static List<Attractor> Attractors;

	public Rigidbody rb;

	public bool useSoIRange = false;
	public float sphereOfInfluenceRange = 1000;

	void FixedUpdate()
	{
		foreach (Attractor attractor in Attractors)
		{
			if(useSoIRange)
				if (Vector3.Distance(attractor.transform.position, transform.position) > sphereOfInfluenceRange)
					continue;

			if (attractor != this)
				Attract(attractor);
		}
	}

	void OnEnable()
	{
		if (Attractors == null)
			Attractors = new List<Attractor>();

		Attractors.Add(this);
	}

	void OnDisable()
	{
		Attractors.Remove(this);
	}

	void Attract(Attractor objToAttract)
	{
		Rigidbody rbToAttract = objToAttract.rb;

		Vector3 direction = rb.position - rbToAttract.position;
		float distance = direction.sqrMagnitude;

		if (distance == 0f)
			return;

		float forceMagnitude = GravityConstant * (rb.mass * rbToAttract.mass) / distance;
		Vector3 force = direction.normalized * forceMagnitude;

		rbToAttract.AddForce(force);
	}

}
