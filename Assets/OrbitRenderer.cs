using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitRenderer : MonoBehaviour
{
    public float gravConstant = 667.4f;
    public List<Rigidbody> gravityObjects;
    public Vector3 startingVelocity = Vector3.up;
    Vector3 dir;
    Vector3[] orbitPoints;
    public int maxCount= 10000;
    public int countPerFrame = 1000;
    public int simplify = 100;
    int privateMaxCount;
    public LineRenderer lineRendererPF;
    LineRenderer lineRenderer;
    public bool rebuildTrajectory = false;
    public float fadeRate = 0.01f;

    AsteroidLogic asteroidLogic;

    // Start is called before the first frame update
    void Start()
    {
        asteroidLogic = GameObject.FindGameObjectWithTag("AsteroidManager").GetComponent<AsteroidLogic>();
        BuildTrajectory();
    }


    void CacheStars()
    {
        gravityObjects.Clear();
        foreach (var item in asteroidLogic.GetStarRigidbodies())
        {
            gravityObjects.Add(item);
        }
    }

    Color fadeColor;
    void BuildTrajectory()
    {
        CacheStars();

        StopAllCoroutines();

        if(lineRenderer)
            StartCoroutine(FadeDestroyOldLineRenderer());
        var co = StartCoroutine(ComputeTrajectory());
    }

    
    IEnumerator FadeDestroyOldLineRenderer()
    {
        /*
        fadeColor.a = lineRenderer.colorGradient.colorKeys[0].color.a-(fadeRate*Time.deltaTime);
        // Verbose
        //lineRenderer.colorGradient.colorKeys[0].color = fadeColor;
        //lineRenderer.colorGradient.colorKeys[lineRenderer.colorGradient.colorKeys.Length-1].color = fadeColor;
        
        
        Color c = lineRenderer.colorGradient.colorKeys[0].color;
        for (float alpha = 1f; alpha >= 0; alpha -= fadeRate)
        {
            c.a = alpha;
            for(int i = 0; i < lineRenderer.colorGradient.colorKeys.Length; i++)
            {
                lineRenderer.colorGradient.colorKeys[i].color = c;
            }
            //lineRenderer.colorGradient.colorKeys[0].color = c;
            //lineRenderer.colorGradient.colorKeys[lineRenderer.colorGradient.colorKeys.Length - 1].color = c;
            Debug.Log("Woot");
            yield return null;
        }

        Debug.Log("Woopie");
        //yield return new WaitUntil(() => lineRenderer.colorGradient.colorKeys[0].color.a <= 0.01f);

        */
        yield return null;
        if (lineRenderer)
            Destroy(lineRenderer.gameObject);
    }

    void Init()
    {
        startingVelocity = transform.parent.GetComponent<Rigidbody>().velocity;
        privateMaxCount = maxCount;
        orbitPoints = new Vector3[privateMaxCount];
        LineRenderer lR = Instantiate(lineRendererPF, this.transform);
        lineRenderer = lR;
    }

    IEnumerator ComputeTrajectory()
    {
        yield return new WaitUntil(() => lineRenderer == null);
        Init();

        float angle = 0;
        float dt = Time.fixedDeltaTime;
        Vector3 s = transform.parent.position;
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
            if (gravityObjects.Count == 1)
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


    public Vector3 AccelerationCalc(List<Rigidbody> goArray,Vector3 simPos)
    {
        Vector3 a  = Vector3.zero;
        for (int i = 0; i < goArray.Count; i++){
            dir = goArray[i].position - simPos;
            a += dir.normalized * (goArray[i].mass * gravConstant) / dir.sqrMagnitude;
        }
        return a;
    }
}


