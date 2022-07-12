using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class AsteroidLogic : MonoBehaviour
{
    
    
    
    [Header("===Initial Spawn===")]
    public List<GameObject> spawnPrefabs;
    public float spawnRadius = 10;
    public int spawnAmount = 1;
    public int spawnPerTick = 1;

    [SerializeField] int _amountSpawned = 0;

    List<Rigidbody> objectsRB = new List<Rigidbody>();

    public void RemoveObjectRB(Rigidbody obj)
    {
        objectsRB.Remove(obj);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (spawnPrefabs.Count == 0)
            Debug.LogWarning("Spawner has no prefab setup.");

        foreach (var item in OtherObjectsToGrav)
        {
            objectsRB.Add(item);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i != spawnPerTick; i++)
        {
            if (_amountSpawned < spawnAmount)
            {
                Vector3 spawnPos = transform.position + (UnityEngine.Random.insideUnitSphere * spawnRadius);
                var obj = Instantiate(spawnPrefabs[UnityEngine.Random.Range(0, spawnPrefabs.Count)], spawnPos, transform.rotation);
                objectsRB.Add(obj.GetComponentInChildren<Rigidbody>());
                refreshArrays = true;

                _amountSpawned++;
            }
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }



    [Header("===Gravity Calculations===")]
    public List<Rigidbody> OtherObjectsToGrav = new List<Rigidbody>();
    public float GravityConstant = 667.4f;

    public bool useSoIRange = false;
    public float sphereOfInfluenceRange = 1000;

    public bool showDebugTime = false;
    public bool stopGravityCalcs = false;
    public bool useJob = false;
    public int threadinstancethings = 100;




    NativeArray<float3> positionArray;
    NativeArray<float> massArray;
    NativeArray<float3> outputForceArray;

    void UpdateNativeArrays()
    {
        for (int i = 0; i < objectsRB.Count; i++)
        {
            positionArray[i] = objectsRB[i].position;
            massArray[i] = objectsRB[i].mass;
        }
    }

    public bool refreshArrays = false;
    void RefreshNativeArrays()
    {
        if (positionArray.IsCreated)
            positionArray.Dispose();
        if (massArray.IsCreated)
            massArray.Dispose();
        if (outputForceArray.IsCreated)
            outputForceArray.Dispose();

        positionArray = new NativeArray<float3>(objectsRB.Count, Allocator.Persistent);
        massArray = new NativeArray<float>(objectsRB.Count, Allocator.Persistent);
        outputForceArray = new NativeArray<float3>(objectsRB.Count, Allocator.Persistent);

        refreshArrays = false;
    }

    JobHandle jobHandle;

    private void FixedUpdate()
    {
        var timeTracker = Time.realtimeSinceStartup;
        if (stopGravityCalcs)
            return;

        if (useJob)
        {

            if (refreshArrays || objectsRB.Count != positionArray.Length)
                RefreshNativeArrays();

            UpdateNativeArrays();

            AttractedJobParAllAtOnce attractJobPar = new AttractedJobParAllAtOnce
            {

				positionArray = positionArray,
				massArray = massArray,
				gravityConstant = this.GravityConstant,
				outputVelocityArray = outputForceArray
			};
			
            timeTracker = Time.realtimeSinceStartup;
            jobHandle = attractJobPar.Schedule(objectsRB.Count, threadinstancethings);
            

            jobHandle.Complete();
            for (int i = 0; i < objectsRB.Count; i++)
                objectsRB[i].AddForce(outputForceArray[i]);
        }

        else
            Attract();

        if (showDebugTime)
            Debug.Log("Time: " + (Time.realtimeSinceStartup - timeTracker) * 1000f + "ms");
    }
    private void LateUpdate()
    {
        
    }

    void Attract()
    {

        
        foreach (var rb in objectsRB)
        {
            
            Vector3 sumOfForces = new Vector3(0, 0, 0);
            Vector3 direction;
            float distance;
            float forceMagnitude;
            Vector3 force;
            foreach (var attracted in objectsRB)
            {
                if (useSoIRange)
                    if (Vector3.Distance(attracted.transform.position, transform.position) > sphereOfInfluenceRange)
                        continue;

                if (attracted != rb)
                {
                    direction = attracted.position - rb.position;
                    distance = direction.sqrMagnitude;

                    if (distance == 0f)
                        return;

                    forceMagnitude = GravityConstant * (rb.mass * attracted.mass) / distance;
                    force = direction.normalized * forceMagnitude;

                    sumOfForces += force;


                }
            }
            rb.AddForce(sumOfForces);
        }
        
    }


    private void OnDestroy()
    {
        if (positionArray.IsCreated)
            positionArray.Dispose();
        if (massArray.IsCreated)
            massArray.Dispose();
        if (outputForceArray.IsCreated)
            outputForceArray.Dispose();

    }

}






[BurstCompile]
public struct AttractedJobParAllAtOnce : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positionArray;
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
        float3 myPos;
        sumOfForces = new Vector3(0, 0, 0);
        myPos = positionArray[index];

        for (int i = 0; i < positionArray.Length; i++)
        {
            if (index != i)
            {
                direction = positionArray[i] - myPos;
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
