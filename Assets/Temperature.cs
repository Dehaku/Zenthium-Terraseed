using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : MonoBehaviour
{

    public static float CelsiusToFahrenheit(float celsius) 
    {
        return (celsius * 18f / 10f + 32f);
    }

    public static float CelsiusToKelvin(float celsius)
    {
        return (celsius + 273f);
    }

    public static float FahrenheitToCelsius(float fahrenheit)
    {
        return (fahrenheit - 32f) * 5f / 9f;
    }

    public static float FahrenheitToKelvin(float fahrenheit)
    {
        return ((fahrenheit - 32f) * 5f / 9f)+273f;
    }

    public static float KelvinToCelsius(float kelvin)
    {
        return (kelvin - 273f);
    }

    public static float KelvinToFahrenheit(float kelvin)
    {
        return 1.8f*(kelvin - 273f) + 32f;
    }



    // Start is called before the first frame update
    void Start()
    {
        

    }

    public float temperature;
    public bool runTemp = false;
    // Update is called once per frame
    void Update()
    {
        if(runTemp)
        {
            runTemp = false;
            Debug.Log("==== Temp of " + temperature + " ====");
            Debug.Log("C to F" + CelsiusToFahrenheit(temperature));
            Debug.Log("C to K" + CelsiusToKelvin(temperature));
            Debug.Log("F to C" + FahrenheitToCelsius(temperature));
            Debug.Log("F to K" + FahrenheitToKelvin(temperature));
            Debug.Log("K to F" + KelvinToFahrenheit(temperature));
            Debug.Log("K to C" + KelvinToCelsius(temperature));
            Debug.Log("==== ====T==== ====");
        }
    }
}
