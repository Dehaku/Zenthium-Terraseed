using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGFXGenner : MonoBehaviour
{
    public Transform trans;
    public AsteroidLogic astLogic;
    public PaintBoss painter;

    public float checkRange;

    public float iterateTime;
    public float iterateTimeVariance; // Helping space out spikes.
    float _iterateTimer = 0;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    void CheckAsteroids()
    {
        Transform hitParent = null;
        foreach (var item in astLogic.GetObjectRBs())
        {
            hitParent = item.transform.parent;

            if (Vector3.Distance(trans.position,item.position) <= checkRange)
            {
                PlanetSide ps = hitParent.GetComponentInChildren<PlanetSide>();
                if(ps != null)
                {
                    var paintTag = ps.GetComponent<PaintTag>();
                    if (paintTag)
                    {
                        
                    }
                    else
                    {
                        painter.MakePlanetPaintable(ps.planetParent);
                    }
                }

                StartCoroutine(EnableSpherize(hitParent, item, 4f));

                
            }
        }
    }

    IEnumerator EnableSpherize(Transform obj, Rigidbody item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj)
        {
            var spherize = item.transform.parent.GetComponentInChildren<Spherize>();
            if (spherize)
            {
                spherize.EnableFaceColliders(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        _iterateTimer += Time.deltaTime;
        if(_iterateTimer > iterateTime)
        {
            _iterateTimer = 0f-Random.Range(0f,iterateTimeVariance);
            CheckAsteroids();
        }
    }


}
