using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "PlanetGen/Substance")]
public class Substance : ScriptableObject
{
    public float amount;
    public string id;
    public string description;
    public float weightPerUnit; // kg/m3 // https://www.engineeringtoolbox.com/gas-density-d_158.html
    public Color color;
    public Sprite icon;

    // https://www.engineeringtoolbox.com/material-properties-t_24.html

    [SerializeField] float _meltingPoint = float.MaxValue; // 
    [SerializeField] float _boilingPoint = float.MaxValue; // https://www.engineeringtoolbox.com/boiling-temperature-metals-d_1267.html 
    [SerializeField] float _globalWarmingPotential; // GWP
                                                    // https://en.wikipedia.org/wiki/Global_warming_potential
                                                    // A lot of numbers had to be dug up in other locations, there may be inconsistencies.


    public float CalculateGlobalWarmingPotential(float celsius) // GWP
    {
        // Only return above 0 if this element is a gas.
        if(isGas(celsius))
            return _globalWarmingPotential;

        return 0;
    }

    public float GetGlobalWarmingPotential() // GWP
    {
        return _globalWarmingPotential;
    }

    private void OnValidate()
    {
        if (_meltingPoint == float.MaxValue)
                Debug.LogWarning(name + " doesn't have set melting/boiling points");
    }

    public bool isSolid(float celsius)
    {
        if (celsius < _meltingPoint)
            return true;
        return false;
    }
    public bool isLiquid(float celsius)
    {
        if (celsius > _meltingPoint && celsius < _boilingPoint)
            return true;
        return false;
    }
    public bool isGas(float celsius)
    {
        if (celsius > _boilingPoint)
            return true;
        return false;
    }

    public sub Subify()
    {
        sub returnSub = new sub();
        returnSub.SO = this;
        returnSub.init();
        return returnSub;
    }

    [System.Serializable]
    public class sub
    {
        public Substance SO;
        [SerializeField] public float amount;
        [SerializeField] public string id;
        [SerializeField] public float weightPerUnit;
        [SerializeField] public Color color;
        [SerializeField] public Sprite icon;

        public void init()
        {
            if (SO == null)
                return;
            id = SO.id;
            weightPerUnit = SO.weightPerUnit;
            color = SO.color;
            icon = SO.icon;
        }
    }

}


