using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "PlanetGen/RandomSubstanceListCollection")]
public class RandomSubstanceListCollectionSO : ScriptableObject
{
    public string nameSO;
    public List<RandomSubstancesSO> rEntries = new List<RandomSubstancesSO>();

    public RandomSubstancesSO GetRandomList() 
    {
        return rEntries[Random.Range(0, rEntries.Count)];
    }
}

