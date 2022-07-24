using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UISpeedMeter : MonoBehaviour
{
    public Rigidbody rb;
    
    [Space]
    public string prefix;
    public string suffix;
    public TextMeshProUGUI text;
    [Header("Speed Color")]
    public float stopSpeed = 0.1f;
    public Color stopColor;
    public Color goingColor;
    public float tooFastSpeed;
    public Color tooFastColor;
    [Space]
    public Color boostColor;
    public bool isBoosting = false;
    public bool isInHyperspace = false;
    [Header("Box Color - Optional")]
    public bool colorBoxWhenHyper = true;
    public Image box;
    public Color boxColor;

    float _speed;
    

    // Update is called once per frame
    void Update()
    {
        if(!rb)
        {
            Debug.Log("No Rigidbody set for UISpeedMeter");
            return;
        }

        // Getting magnitude and setting speed
        _speed = rb.velocity.magnitude;
        
        

        // Text Colors based on speeds
        if (isBoosting)
            text.color = boostColor;
        else if (_speed > tooFastSpeed)
            text.color = tooFastColor;
        else if (_speed > stopSpeed)
            text.color = goingColor;
        else
            text.color = stopColor;


        // Warping to other star systems.
        if(isInHyperspace)
        {
            float hyperSpeed = 0;
            hyperSpeed += Random.Range(0, 10);
            hyperSpeed += Random.Range(0, 10) * 10;
            hyperSpeed += Random.Range(0, 10) * 100;
            hyperSpeed += Random.Range(0, 10) * 1000;
            hyperSpeed += Random.Range(0, 10) * 0.1f;

            _speed = hyperSpeed;
            text.color = tooFastColor;

            if(box && colorBoxWhenHyper)
            {
                box.color = tooFastColor;
            }
        }
        else
        {
            if (box)
                box.color = boxColor;
        }

        // Setting the numeric display with the prefixes and suffixes, allowing for language support.
        text.text = prefix + _speed.ToString("F1") + suffix;

        // Eastereggs
        if (_speed > 69 && _speed < 69.01f)
            text.text = "nice";
        if (_speed > 420 && _speed < 420.01f)
            text.text = "Mary Jane";
        if (_speed > 636 && _speed < 636.01f)
            text.text = "DCXXXVI";
    }
}
