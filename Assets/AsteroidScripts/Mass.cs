using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mass : MonoBehaviour
{
    public Asteroid asteroid;
    public int bodies = 1; // For tracking how many collisions there've been, mostly for fun.
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform trans;

    
    [SerializeField] float _mass;
    public float _energy; // Temperature in C
    public float _energyLossOverTime; // Temperature in C
    [SerializeField] float _gasPressure;
    [SerializeField] float _GWP; // Global Warming Potential
    [SerializeField] float _oceanLevel;
    public float distanceFromNearestStar;
    [SerializeField] public List<Substance.sub> substances;

    public float scaleOffset = 1;
    [SerializeField] float _physicsMassOffset = 1000f;

    public float starEnergyOutput = 10000;
    public float starSurfaceDist = 0;
    public bool starIgnited = false;

    public float GetStarOutput()
    {
        if(starIgnited)
        {
            return starEnergyOutput;
        }

        // We're not ignited, output 0.
        return 0;
    }

    public float GetGlobalWarmingPotential(bool recalc = true)
    {
        if (!recalc)
            return _GWP;

        float gwpCalc = 0;
        foreach (var item in substances)
        {
            gwpCalc += item.SO.CalculateGlobalWarmingPotential(GetEnergy()) * item.amount;
        }
        _GWP = gwpCalc;

        return gwpCalc;
    }

    public float GetOceanLevel(bool recalc = true)
    {
        if (!recalc)
            return _oceanLevel;

        float massSolid = 0;
        float massLiquid = 0;
        

        foreach (var item in substances)
        {
            if (item.SO.isSolid(GetEnergy()))
                massSolid += item.amount;
            else if (item.SO.isLiquid(GetEnergy()))
                massLiquid += item.amount;
        }
        // Divide by 0 protection
        if(massLiquid == 0 || massSolid == 0)
        {
            _oceanLevel = massLiquid;
            return _oceanLevel;
        }
        _oceanLevel = massLiquid / massSolid;
        return _oceanLevel;
    }

    public void AddEnergy(float energy)
    {
        _energy += energy / GetMass(false);
    }

    public float arbitrarySunMultiplier = 1; // For White/Black surface terraform tools.

    public void AddSolarEnergy(float energy)
    {
        _energy += (energy * arbitrarySunMultiplier) / GetMass(false);
    }

    public float GetEnergy(bool recalc = true)
    {
        if (!recalc)
            return _energy;

        return _energy;
    }

    public float GetPressure(bool recalc = true)
    {
        if (!recalc)
            return _gasPressure;

        float pressureCalc = 0;
        foreach (var item in substances)
        {
            // Most gases that I could find were in the 800-900 range, except steam which was half 800, and half 1600.
            // I have no idea, and I'd rather avoid extra variables for needless, and obtuse detail.
            if(item.SO.isGas(GetEnergy()))
                pressureCalc += item.amount * 800; 
        }
        _gasPressure = pressureCalc;

        return pressureCalc;
    }

    public float GetMass(bool recalc = true)
    {
        if (!recalc)
            return _mass;

        float massCalc = 0;
        foreach (var item in substances)
        {
            massCalc += item.amount * item.weightPerUnit;
        }
        _mass = massCalc;

        return massCalc;
    }

    public void Reset() // Reset ourselves after being pooled.
    {
        bodies = 1;

        substances.Clear();

        var rMass = GetComponent<RandomMass>();
        if (rMass)
            rMass.AddRandomSubstances();

        StartCoroutine(ForceOrbitScript(1f,Random.Range(0.7f,1.3f),1f));

        AffectTransforms();
    }

    public IEnumerator ForceOrbitScript(float initialWaitTime, float velocityMultiply, float destroyTime)
    {
        yield return new WaitForSeconds(initialWaitTime);
        var fO = gameObject.AddComponent<ForceOrbit>();
        yield return new WaitForSeconds(0.25f);
        rb.velocity *= velocityMultiply;

        Destroy(fO, destroyTime);
    }

    public void AffectTransforms()
    {
        if (!rb)
        {
            Debug.LogError(" No Rigidbody: " + gameObject.name);
            return;
        }
            

        rb.mass = GetMass()/_physicsMassOffset;
        var newScale = (Vector3.one * scaleOffset) * Mathf.Pow(rb.mass * _physicsMassOffset, 1f / 3f);
        if (transform.parent)
            transform.parent.localScale = newScale;
        else
            transform.localScale = newScale;
    }

    public void AddSubstance(Substance sub, float amount)
    {
        bool itemExists = false;
        foreach (var item in substances)
        {
            if(item.id == sub.id)
            {
                item.amount += amount;
                itemExists = true;
            }    
        }
        if(!itemExists)
        {
            var subby = new Substance.sub();
            subby.SO = sub;
            subby.init();
            subby.amount = amount;

            substances.Add(subby);
        }
        GetMass();
        AffectTransforms();
    }

    // Start is called before the first frame update
    void Start()
    {

        if(substances == null)
            substances = new List<Substance.sub>();


        foreach (var item in substances)
        {
            if(item.id == "")
            {
                item.init();
            }
        }

        GetMass();

        //Debug.Log("Math: " + Mathf.Pow(125, 1f / 3f));
        for (int i = 0; i < 60; i++)
            temperatureStorage.Add(60);
    }

    public float energyAverage = 0;
    public float energyDelta = 0;
    public float rawSolarEnergy = 0;
    List<float> temperatureStorage = new List<float>();
    public void EnergyDelta()
    {
        temperatureStorage.RemoveAt(0);
        temperatureStorage.Add(_energy);
        energyDelta = 0;
        foreach (var item in temperatureStorage)
        {
            energyDelta += item;
        }
        
        energyDelta = energyDelta / temperatureStorage.Count;
        energyAverage = energyDelta;

        energyDelta -= temperatureStorage[0]; // Oldest temperature.
    }


    // Update is called once per frame
    void Update()
    {
        EnergyDelta();
    }
}

