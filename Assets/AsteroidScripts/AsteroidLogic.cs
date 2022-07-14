using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Pool;

public class AsteroidLogic : MonoBehaviour
{
    [SerializeField] int activeAsteroids, inactiveAsteroids, minimumAsteroids;

    
    
    [Header("===Initial Spawn===")]
    public List<GameObject> spawnPrefabs;
    public float spawnRadius = 10;
    public float velocityDeleteRadius = 10000; // If a rigidbody is further than this distance, it's velocity is deleted so it can return.
    public int spawnAmount = 1;
    public int spawnPerTick = 1;

    [SerializeField] int _amountSpawned = 0;

    List<GameObject> objectsGO = new List<GameObject>();
    List<Rigidbody> objectsRB = new List<Rigidbody>();

    static GameObject _planetContainer;

    ObjectPool<Asteroid> _asteroidPool;

    private void Awake()
    {
        if (!_planetContainer)
        {
            _planetContainer = new GameObject();
            _planetContainer.name = "Planet Container";
        }

        _asteroidPool = new ObjectPool<Asteroid>(CreateAsteroid, OnTakeAsteroidFromPool, OnReturnBallToPool);
    }
    Asteroid CreateAsteroid()
    {
        var obj = Instantiate(spawnPrefabs[UnityEngine.Random.Range(0, spawnPrefabs.Count)]);
        var ast = obj.GetComponent<Asteroid>();
        ast.SetPool(_asteroidPool);
        return ast;
    }

    void OnTakeAsteroidFromPool(Asteroid asteroid)
    {
        Debug.Log(asteroid.name + " has been taken out.");
        asteroid.gameObject.SetActive(true);
        refreshArrays = true; // Refresh Gravity Job Cache

        // Remove momentum.
        asteroid.mass.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // Reset substances and the like.
        asteroid.mass.Reset();
    }

    void OnReturnBallToPool(Asteroid asteroid)
    {
        Debug.Log(asteroid.name + " is going in.");
        asteroid.gameObject.SetActive(false);
        refreshArrays = true;  // Refresh Gravity Job Cache
    }



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


    void UpdateActiveAsteroidCount()
    {
        activeAsteroids = 0;
        inactiveAsteroids = 0;

        for(int i = 0; i < objectsGO.Count; i++)
        {
            if (objectsGO[i].activeInHierarchy)
                activeAsteroids++;
            else
                inactiveAsteroids++;
        }
    }

    void RespawnAsteroidLogic()
    {
        if (activeAsteroids >= minimumAsteroids)
            return;

        if ((activeAsteroids + inactiveAsteroids) < minimumAsteroids)
            return;

        Debug.Log("Respawning!");

        Vector3 spawnPos = transform.position + (UnityEngine.Random.insideUnitSphere * spawnRadius);
        var obj = _asteroidPool.Get();
        var objRB = obj.GetComponentInChildren<Rigidbody>();
        objectsRB.Add(objRB);
        obj.transform.position = spawnPos;
        obj.transform.rotation = transform.rotation;
        refreshArrays = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateActiveAsteroidCount();
        RespawnAsteroidLogic();

        for(int i = 0; i != spawnPerTick; i++)
        {
            if (_amountSpawned < spawnAmount)
            {
                Vector3 spawnPos = transform.position + (UnityEngine.Random.insideUnitSphere * spawnRadius);
                //var obj = Instantiate(spawnPrefabs[UnityEngine.Random.Range(0, spawnPrefabs.Count)], spawnPos, transform.rotation);
                var obj = _asteroidPool.Get();
                //var obj = CreateAsteroid();
                objectsGO.Add(obj.gameObject);
                obj.transform.parent = _planetContainer.transform;

                var objRB = obj.GetComponentInChildren<Rigidbody>();
                obj.transform.position = spawnPos;
                obj.transform.rotation = transform.rotation;
                objectsRB.Add(objRB);

                obj.name = obj.name + _amountSpawned;
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
            if(objectsRB[i].gameObject.activeInHierarchy)
            {
                positionArray[i] = objectsRB[i].position;
                massArray[i] = objectsRB[i].mass;
            }
            else
            {
                positionArray[i] = objectsRB[i].position;
                massArray[i] = 0;
            }
            
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
        KeepBodiesClose();
    }

    void KeepBodiesClose()
    {
        foreach (var item in objectsRB)
        {
            if (Vector3.Distance(item.position, transform.position) > velocityDeleteRadius)
            {
                item.velocity = -(item.velocity*0.9f);
                //item.position += item.velocity * 0.5f;

                var dir = transform.position - item.position;
                item.position = item.position + (dir.normalized * 10);
                //Debug.Log(item.gameObject.name + " has left the area! Reversing.");
            }
        }
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
