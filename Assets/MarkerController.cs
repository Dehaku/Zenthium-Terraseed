using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    public GameObject onScreen;
    public GameObject offScreen;

    public bool isOnScreen = true;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnScreen)
        {
            onScreen.SetActive(true);
            offScreen.SetActive(false);
        }
        else
        {
            onScreen.SetActive(false);
            offScreen.SetActive(true);
        }
            

    }
}
