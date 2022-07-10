using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSide : MonoBehaviour
{
    public enum Side
    {
        unassigned,
        center,
        left,
        up,
        right,
        down,
        ddown
    }
    public Side side;

    public GameObject planetParent;
    
}
