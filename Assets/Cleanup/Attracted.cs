using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


public class Attracted : MonoBehaviour
{

	public float GravityConstant = 667.4f;

	public static List<Attracted> Attracteds;

	public Rigidbody rb;

	public bool useSoIRange = false;
	public float sphereOfInfluenceRange = 1000;

	public bool showDebugTime = false;
	public bool useJob = false;
	public int threadinstancethings = 100;



	NativeArray<float3> positionArray;
	NativeArray<float3> positionAltArray;
	NativeArray<float> massArray;
	NativeArray<float3> outputForceArray;

	NativeArray<float3> outputForce;

	void UpdateNativeArrays()
    {
		if (positionArray.IsCreated)
			positionArray.Dispose();
		if (positionAltArray.IsCreated)
			positionAltArray.Dispose();
		if (massArray.IsCreated)
			massArray.Dispose();
		if (outputForceArray.IsCreated)
			outputForceArray.Dispose();
		if (outputForce.IsCreated)
			outputForce.Dispose();

		positionArray = new NativeArray<float3>(Attracteds.Count, Allocator.Persistent);
		positionAltArray = new NativeArray<float3>(Attracteds.Count, Allocator.Persistent);
		massArray = new NativeArray<float>(Attracteds.Count, Allocator.Persistent);
		outputForceArray = new NativeArray<float3>(Attracteds.Count, Allocator.Persistent);

		outputForce = new NativeArray<float3>(1, Allocator.Persistent); ;

		//Debug.Log("Bug: This doesn't update planet positions as they move.");
	}

	JobHandle jobHandle;
	bool firstRun = true;

	void FixedUpdate()
	{
		var timeTracker = Time.realtimeSinceStartup;

		if (useJob)
        {
			if(!firstRun)
            {
				//jobHandle.Complete();
				//rb.AddForce(outputForce[0]);
			}
			


			if (Attracteds.Count != positionArray.Length)
				UpdateNativeArrays();
			

			for (int i = 0; i<Attracteds.Count; i++)
            {
				positionArray[i] = Attracteds[i].transform.position;
				positionAltArray[i] = Attracteds[i].transform.position;
				massArray[i] = Attracteds[i].rb.mass;
			}

			AttractedJobPar attractJobPar = new AttractedJobPar
			{

				position = rb.transform.position,
				positionAltArray = positionAltArray,
				massArray = massArray,
				gravityConstant = this.GravityConstant,
				outputVelocity = outputForce
			};

			/* Group version
			AttractedJobPar attractJobPar = new AttractedJobPar
			{

				positionArray = positionArray,
				positionAltArray = positionAltArray,
				massArray = massArray,
				gravityConstant = this.GravityConstant,
				outputVelocityArray = outputForceArray
			};
			*/
			timeTracker = Time.realtimeSinceStartup;
			jobHandle = attractJobPar.Schedule(1, threadinstancethings);
			if(firstRun)
				firstRun = false;
			
			
			//Debug.Log("Force:" + outputForce[0]);


			/* Group version
			for (int i = 0; i < Attracteds.Count; i++)
            {
				Attracteds[i].rb.AddForce(outputForceArray[i]);
            }
			*/

		}
			
		else
			Attract();

		if (showDebugTime)
			Debug.Log("Time: " + (Time.realtimeSinceStartup - timeTracker) * 1000f + "ms");
	}

    private void LateUpdate()
    {
		if (!firstRun)
        {
			jobHandle.Complete();
			rb.AddForce(outputForce[0]);
		}
			
	}

    void OnEnable()
	{
		if (Attracteds == null)
			Attracteds = new List<Attracted>();

		Attracteds.Add(this);
	}

	void OnDisable()
	{
		Attracteds.Remove(this);
	}

	void Attract()
	{
		Vector3 sumOfForces = new Vector3(0, 0, 0);

		foreach (Attracted attracted in Attracteds)
		{
			if (useSoIRange)
				if (Vector3.Distance(attracted.transform.position, transform.position) > sphereOfInfluenceRange)
					continue;

			if (attracted != this)
			{
				Rigidbody rbToAttract = attracted.rb;

				Vector3 direction = rbToAttract.position - rb.position;
				float distance = direction.sqrMagnitude;

				if (distance == 0f)
					return;

				float forceMagnitude = GravityConstant * (rb.mass * rbToAttract.mass) / distance;
				Vector3 force = direction.normalized * forceMagnitude;

				sumOfForces += force;


			}
		}
		rb.AddForce(sumOfForces);
	}


    private void OnDestroy()
    {
		if (positionArray.IsCreated)
			positionArray.Dispose();
		if (positionAltArray.IsCreated)
			positionAltArray.Dispose();
		if (massArray.IsCreated)
			massArray.Dispose();
		if (outputForceArray.IsCreated)
			outputForceArray.Dispose();

	}
}


[BurstCompile]
public struct AttractedJobPar : IJobParallelFor
{
	public float3 position;
	[ReadOnly] public NativeArray<float3> positionAltArray;
	[ReadOnly] public NativeArray<float> massArray;
	public NativeArray<float3> outputVelocity;
	public float gravityConstant;

	Vector3 sumOfForces;
	Vector3 direction;
	float distance;
	float forceMagnitude;
	Vector3 force;

	public void Execute(int index)
	{
		sumOfForces = new Vector3(0, 0, 0);
		for(int i = 0; i < positionAltArray.Length; i++)
		{
			
			direction = positionAltArray[i] - position;
			distance = direction.sqrMagnitude;

			if (distance == 0f)
				continue;

			forceMagnitude = gravityConstant * (massArray[index] * massArray[i]) / distance;
			force = direction.normalized * forceMagnitude;

			sumOfForces += force;
		}

		outputVelocity[0] = sumOfForces;
	}
}

public struct AttractedJobParAllAtOnceOops : IJobParallelFor
{
	[ReadOnly] public NativeArray<float3> positionArray;
	[ReadOnly] public NativeArray<float3> positionAltArray;
	[ReadOnly] public NativeArray<float> massArray;
	public NativeArray<float3> outputVelocityArray;
	public float gravityConstant;

	Vector3 sumOfForces;
	Vector3 direction;
	float distance;
	float forceMagnitude;
	Vector3 force;

	public void Execute(int index)
	{

		sumOfForces = new Vector3(0, 0, 0);

		for (int i = 0; i < positionAltArray.Length; i++)
		{
			if (index != i)
			{
				direction = positionAltArray[i] - positionArray[index];
				distance = direction.sqrMagnitude;

				if (distance == 0f)
					continue;

				forceMagnitude = gravityConstant * (massArray[index] * massArray[i]) / distance;
				force = direction.normalized * forceMagnitude;

				sumOfForces += force;


			}
		}

		outputVelocityArray[index] = sumOfForces;
	}
}
