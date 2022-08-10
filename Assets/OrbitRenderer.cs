using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitRenderer : MonoBehaviour
{
    public float gravConstant = 667.4f;
    public Rigidbody[] gravityObjects;
    public Vector3 startingVelocity = Vector3.up;
    Vector3 dir;
    Vector3[] orbitPoints;
    public int maxCount= 100000;
    public int countPerFrame = 1000;
    public int simplify = 100;
    int privateMaxCount;
    LineRenderer lineRenderer;
    public bool rebuildTrajectory = false;


    // Start is called before the first frame update
    void Start()
    {
        BuildTrajectory();
    }

    void BuildTrajectory()
    {
        if (lineRenderer)
            Destroy(lineRenderer);

        
        StartCoroutine(ComputeTrajectory());
    }

    void Init()
    {
        startingVelocity = GetComponent<Rigidbody>().velocity;
        privateMaxCount = maxCount;
        orbitPoints = new Vector3[privateMaxCount];
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 5f;
        lineRenderer.endWidth = 5f;
    }

    IEnumerator ComputeTrajectory()
    {
        yield return new WaitUntil(() => lineRenderer == null);
        Init();

        float angle = 0;
        float dt = Time.fixedDeltaTime;
        Vector3 s = transform.position;
        Vector3 lastS = s;

        Vector3 v = startingVelocity;
        Vector3 a = AccelerationCalc(gravityObjects, s);
        float tempAngleSum = 0;
        int step = 0;
        int subCount = 0;
        int iSteps = 0;

        while (angle < 360 && step < privateMaxCount* simplify){
         if(step % simplify == 0){
             orbitPoints[iSteps] = s;
             angle += tempAngleSum;
             tempAngleSum = 0;
             iSteps++;
         }
            a = AccelerationCalc(gravityObjects, s);
            v += a * dt;
            s += v * dt;
            if (gravityObjects.Length == 1)
            {
                tempAngleSum += Mathf.Abs(Vector3.Angle(s, lastS));
            }
            lastS = s;
            step++;
            subCount++;
            if (subCount == countPerFrame)
            {
                lineRenderer.positionCount = iSteps;
                for (int i = 0; i < iSteps; i++){
                    lineRenderer.SetPosition(i, orbitPoints[i]);
                }
                yield return null;
                subCount = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(rebuildTrajectory)
        {
            rebuildTrajectory = false;
            BuildTrajectory();
        }
    }


    public Vector3 AccelerationCalc(Rigidbody[] goArray,Vector3 simPos)
    {
        Vector3 a  = Vector3.zero;
        for (int i = 0; i < goArray.Length; i++){
            dir = goArray[i].position - simPos;
            a += dir.normalized * (goArray[i].mass * gravConstant) / dir.sqrMagnitude;
        }
        return a;
    }
}


