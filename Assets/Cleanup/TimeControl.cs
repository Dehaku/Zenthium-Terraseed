using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControl : MonoBehaviour
{

    [Range(0, 100)] public float timeScale = 1;
    float _previousTimeScale = 1;

    public bool affectPhysicsToo = false;

    private float fixedDeltaTime;

    // Start is called before the first frame update
    void Start()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeScale != _previousTimeScale)
        {
            _previousTimeScale = timeScale;

            Time.timeScale = timeScale;

            if(affectPhysicsToo)
            {
                Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
            }
            else
            {
                Time.fixedDeltaTime = this.fixedDeltaTime;
            }
        }
    }
}
