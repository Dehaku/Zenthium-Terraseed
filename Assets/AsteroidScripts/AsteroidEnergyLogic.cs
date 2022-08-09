using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidEnergyLogic : MonoBehaviour
{
    public float energyDistanceMultiplier = 100;
    public float blackbodyCooling = 0.01f;
    public float backgroundTemperature = -270.425f; // 2.725 kelvin
    [Header("References")]
    public AsteroidLogic astLogic;
    List<Rigidbody> objectsRB;
    public List<Mass> stars;

    // Start is called before the first frame update
    void Start()
    {
        objectsRB = astLogic.GetObjectRBs();
    }

    public void CacheStars()
    {

    }

    List<Mass> _asteroids = new List<Mass>();

    public void CacheAsteroids()
    {
        List<Mass> newAsteroids = new List<Mass>();
        foreach (var ast in objectsRB)
        {
            if (ast.gameObject.activeInHierarchy)
                newAsteroids.Add(ast.GetComponent<Mass>());
        }
        _asteroids = newAsteroids;
    }

    public void RunAsteroidEnergyLogic()
    {
        float processTime = Time.realtimeSinceStartup;
            

        foreach (var star in stars)
        {
            Vector3 starPos = star.transform.position;
            float dist = 0;
            float starEnergy = 0;

            foreach (var ast in _asteroids)
            {
                dist = Vector3.Distance(starPos, ast.transform.position);
                dist -= star.starSurfaceDist;
                ast.distanceFromNearestStar = dist;


                if (dist > 1) // Divide by 0 protection... probably.
                    starEnergy = star.GetStarOutput() * (1 / Mathf.Sqrt(dist * energyDistanceMultiplier));
                else
                    starEnergy = star.GetStarOutput();

                ast.AddEnergy(starEnergy);
            }
        }

        //Debug.Log(((Time.realtimeSinceStartup - processTime) * 1000f) + "ms");
    }
    void RunAsteroidBlackbodyCooling()
    {
        float temp = 0;
        foreach (var ast in _asteroids)
        {
            temp = Temperature.CelsiusToKelvin(ast._energy);
            temp *= (1 - blackbodyCooling);
            ast._energy = Temperature.KelvinToCelsius(temp);
        }
    }

    float _cacheRate = 1.25f;
    float _cacheTimer = 0;
    void CacheTimer()
    {
        _cacheTimer += Time.deltaTime;
        if(_cacheTimer > _cacheRate)
        {
            _cacheTimer = 0;
            CacheStars();
            CacheAsteroids();
        }
    }

    public void CalculateAsteroidSubstances()
    {
        foreach (var ast in _asteroids)
        {
            ast.GetPressure();
            ast.GetGlobalWarmingPotential();
            ast.GetOceanLevel();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CacheTimer();
        RunAsteroidEnergyLogic();
        RunAsteroidBlackbodyCooling();
        CalculateAsteroidSubstances();
    }
}
