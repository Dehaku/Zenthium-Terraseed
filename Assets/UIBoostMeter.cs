using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIBoostMeter : MonoBehaviour
{
    
    
    public TextMeshProUGUI boostText;
    public Image meter;
    public Gradient meterGradient;
    public ShipMovementRB shipStats;

    public string prefix;
    public string suffix;



    // Start is called before the first frame update
    void Start()
    {
        if (!shipStats)
            Debug.LogError("Shipstats not assigned");
    }

    // Update is called once per frame
    void Update()
    {
        float boostPercent = shipStats.GetBoostPercentFull();


        // **Boost Meter**
        // Meter Color
        meter.color = meterGradient.Evaluate(boostPercent);

        // Drain Meter
        meter.rectTransform.localScale = new Vector3(boostPercent,1,1);

        // **Boost Text**
        boostText.text = prefix + shipStats.GetBoost().ToString("F1") + suffix;
    }
}
