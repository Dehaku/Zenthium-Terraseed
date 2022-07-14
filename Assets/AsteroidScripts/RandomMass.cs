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
        
        AddRandomSubstances();
    }
    //[EButton]

    public List<RandomEntry> rEntries = new List<RandomEntry>();
    public void AddRandomSubstances()
    {
        foreach (var item in rEntries)
        {
            float roll = Random.Range(0, 100f);
            //Debug.Log(item.substanceSO.id + "Rolled: " + roll + ", against " + item.chance);
            if(roll <= item.chance)
            {
                //Debug.Log("Success!");
                mass.AddSubstance(item.substanceSO, Random.Range(item.amountMinMax.x, item.amountMinMax.y));
            }

                //Random.Range(item.amountMinMax.x, item.amountMinMax.y);
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
