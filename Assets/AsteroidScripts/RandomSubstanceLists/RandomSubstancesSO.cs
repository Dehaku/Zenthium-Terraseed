using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "PlanetGen/RandomSubstanceList")]
public class RandomSubstancesSO : ScriptableObject
{
    public string nameSO;
    public List<RandomRSubstanceEntry> rEntries = new List<RandomRSubstanceEntry>();

    public List<Substance> RollSubstances()
    {
        List<Substance> subReturn = new List<Substance>();
        float roll = 0;
        foreach (var item in rEntries)
        {
            roll = Random.Range(0, 100f);
            if (roll <= item.chance)
            {
                subReturn.Add(item.substanceSO);
                subReturn[subReturn.Count-1].amount = Random.Range(item.amountMinMax.x, item.amountMinMax.y);
            }
        }
        return subReturn;
    }
}

[System.Serializable]
public class RandomRSubstanceEntry
{
    public Substance substanceSO;
    [Header("Chance: 0-100f")]
    [Range(0, 100)]
    public float chance;

    public Vector2 amountMinMax;
}
