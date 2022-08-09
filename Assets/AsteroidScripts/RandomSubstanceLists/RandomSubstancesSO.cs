using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "PlanetGen/RandomSubstanceList")]
public class RandomSubstancesSO : ScriptableObject
{
    public string nameSO;
    public List<RandomEntry> rEntries = new List<RandomEntry>();


}
