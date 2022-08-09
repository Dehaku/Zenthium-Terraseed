using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mass))]
public class RandomMass : MonoBehaviour
{
    Mass mass;

    private void Awake()
    {
        mass = GetComponent<Mass>();
        if (!mass)
            Debug.LogError("No Mass in randomMass somehow");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!randomSubstancesSO)
            Debug.LogError("Missing SO listing.");
        AddRandomSubstances();
    }
    //[EButton]

    public RandomSubstancesSO randomSubstancesSO;
    public void AddRandomSubstances()
    {
        foreach (var item in randomSubstancesSO.rEntries)
        {
            float roll = Random.Range(0, 100f);
            if(roll <= item.chance)
            {
                //Debug.Log("Success!");
                mass.AddSubstance(item.substanceSO, Random.Range(item.amountMinMax.x, item.amountMinMax.y));
            }
        }
    }


}

[System.Serializable]
public class RandomEntry
{
    public Substance substanceSO;
    [Header("Chance: 0-100f")]
    [Range(0, 100)]
    public float chance;
    
    public Vector2 amountMinMax;
}
