using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mass : MonoBehaviour
{
    public int bodies = 1; // For tracking how many collisions there've been, mostly for fun.
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform trans;

    
    [SerializeField] float _mass;
    [SerializeField] public List<Substance.sub> substances;

    public float scaleOffset = 1;
    [SerializeField] float _physicsMassOffset = 100f;

    public float GetMass()
    {
        float massCalc = 0;
        foreach (var item in substances)
        {
            massCalc += item.amount * item.weightPerUnit;
        }
        _mass = massCalc;
        return massCalc;
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
        //if (transform.parent)
        //    transform.parent.localScale = newScale;
        //else
            
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

